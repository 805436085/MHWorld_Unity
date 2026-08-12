"""
MH Idle 最小存档服务器（FastAPI + SQLite）

本地启动：
  cd server
  python -m venv .venv
  source .venv/bin/activate   # Windows: .venv\\Scripts\\activate
  pip install -r requirements.txt
  uvicorn main:app --host 0.0.0.0 --port 8000 --reload

接口：
  GET  /health
  POST /api/login          # 开发模式可用 guest；正式接微信 code2session
  GET  /api/save           # Header: Authorization: Bearer <token>
  PUT  /api/save           # 上传存档 JSON
"""

from __future__ import annotations

import hashlib
import json
import os
import secrets
import sqlite3
import time
from contextlib import contextmanager
from pathlib import Path
from typing import Any, Optional

import httpx
from fastapi import Depends, FastAPI, Header, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from pydantic_settings import BaseSettings

ROOT = Path(__file__).resolve().parent
DB_PATH = ROOT / "data" / "mh_idle.db"
DB_PATH.parent.mkdir(parents=True, exist_ok=True)


class Settings(BaseSettings):
    # 微信小程序配置（正式环境填写；开发期可留空走 guest 登录）
    wechat_app_id: str = ""
    wechat_app_secret: str = ""
    allow_guest_login: bool = True
    token_ttl_seconds: int = 60 * 60 * 24 * 30  # 30 天

    class Config:
        env_prefix = "MH_"
        env_file = ".env"


settings = Settings()
app = FastAPI(title="MH Idle Server", version="0.1.0")
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


def _connect() -> sqlite3.Connection:
    conn = sqlite3.connect(DB_PATH)
    conn.row_factory = sqlite3.Row
    return conn


@contextmanager
def db():
    conn = _connect()
    try:
        yield conn
        conn.commit()
    finally:
        conn.close()


def init_db() -> None:
    with db() as conn:
        conn.executescript(
            """
            CREATE TABLE IF NOT EXISTS players (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                openid TEXT NOT NULL UNIQUE,
                created_at INTEGER NOT NULL,
                updated_at INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS tokens (
                token TEXT PRIMARY KEY,
                player_id INTEGER NOT NULL,
                created_at INTEGER NOT NULL,
                expires_at INTEGER NOT NULL,
                FOREIGN KEY(player_id) REFERENCES players(id)
            );

            CREATE TABLE IF NOT EXISTS saves (
                player_id INTEGER PRIMARY KEY,
                payload TEXT NOT NULL,
                revision INTEGER NOT NULL DEFAULT 1,
                updated_at INTEGER NOT NULL,
                FOREIGN KEY(player_id) REFERENCES players(id)
            );
            """
        )


@app.on_event("startup")
def on_startup() -> None:
    init_db()


class LoginRequest(BaseModel):
    # 微信 wx.login 拿到的 code；开发期可传 guest / 任意字符串
    code: str = Field(..., min_length=1)
    # 可选：客户端自定义游客标识，保证同一设备稳定
    guest_key: Optional[str] = None


class LoginResponse(BaseModel):
    token: str
    player_id: int
    openid: str
    expires_at: int


class SavePayload(BaseModel):
    data: dict[str, Any]
    revision: Optional[int] = None


class SaveResponse(BaseModel):
    data: dict[str, Any]
    revision: int
    updated_at: int


def _now() -> int:
    return int(time.time())


def _hash_guest(key: str) -> str:
    return "guest_" + hashlib.sha256(key.encode("utf-8")).hexdigest()[:24]


async def _openid_from_wechat(code: str) -> str:
    if not settings.wechat_app_id or not settings.wechat_app_secret:
        raise HTTPException(status_code=500, detail="未配置微信 AppId/Secret")

    url = "https://api.weixin.qq.com/sns/jscode2session"
    params = {
        "appid": settings.wechat_app_id,
        "secret": settings.wechat_app_secret,
        "js_code": code,
        "grant_type": "authorization_code",
    }
    async with httpx.AsyncClient(timeout=10) as client:
        resp = await client.get(url, params=params)
        data = resp.json()

    if "openid" not in data:
        raise HTTPException(status_code=400, detail=f"微信登录失败: {data}")
    return data["openid"]


def _issue_token(conn: sqlite3.Connection, player_id: int) -> tuple[str, int]:
    token = secrets.token_urlsafe(32)
    now = _now()
    exp = now + settings.token_ttl_seconds
    conn.execute(
        "INSERT INTO tokens(token, player_id, created_at, expires_at) VALUES (?, ?, ?, ?)",
        (token, player_id, now, exp),
    )
    return token, exp


def _get_or_create_player(conn: sqlite3.Connection, openid: str) -> int:
    row = conn.execute("SELECT id FROM players WHERE openid = ?", (openid,)).fetchone()
    now = _now()
    if row:
        conn.execute("UPDATE players SET updated_at = ? WHERE id = ?", (now, row["id"]))
        return int(row["id"])

    cur = conn.execute(
        "INSERT INTO players(openid, created_at, updated_at) VALUES (?, ?, ?)",
        (openid, now, now),
    )
    return int(cur.lastrowid)


def require_player(authorization: Optional[str] = Header(default=None)) -> int:
    if not authorization or not authorization.startswith("Bearer "):
        raise HTTPException(status_code=401, detail="缺少 Authorization Bearer token")
    token = authorization[len("Bearer ") :].strip()
    if not token:
        raise HTTPException(status_code=401, detail="无效 token")

    with db() as conn:
        row = conn.execute(
            "SELECT player_id, expires_at FROM tokens WHERE token = ?",
            (token,),
        ).fetchone()
        if not row:
            raise HTTPException(status_code=401, detail="token 不存在")
        if int(row["expires_at"]) < _now():
            raise HTTPException(status_code=401, detail="token 已过期")
        return int(row["player_id"])


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/api/login", response_model=LoginResponse)
async def login(body: LoginRequest) -> LoginResponse:
    code = body.code.strip()

    if code == "guest" or (settings.allow_guest_login and not settings.wechat_app_id):
        guest_src = body.guest_key or code or "default"
        openid = _hash_guest(guest_src)
    else:
        openid = await _openid_from_wechat(code)

    with db() as conn:
        player_id = _get_or_create_player(conn, openid)
        token, exp = _issue_token(conn, player_id)

    return LoginResponse(token=token, player_id=player_id, openid=openid, expires_at=exp)


@app.get("/api/save", response_model=SaveResponse)
def get_save(player_id: int = Depends(require_player)) -> SaveResponse:
    with db() as conn:
        row = conn.execute(
            "SELECT payload, revision, updated_at FROM saves WHERE player_id = ?",
            (player_id,),
        ).fetchone()
        if not row:
            # 尚无云存档：返回空对象，客户端用本地档
            return SaveResponse(data={}, revision=0, updated_at=0)
        return SaveResponse(
            data=json.loads(row["payload"]),
            revision=int(row["revision"]),
            updated_at=int(row["updated_at"]),
        )


@app.put("/api/save", response_model=SaveResponse)
def put_save(body: SavePayload, player_id: int = Depends(require_player)) -> SaveResponse:
    now = _now()
    payload = json.dumps(body.data, ensure_ascii=False)

    with db() as conn:
        row = conn.execute(
            "SELECT revision FROM saves WHERE player_id = ?",
            (player_id,),
        ).fetchone()

        if row is None:
            revision = 1
            conn.execute(
                "INSERT INTO saves(player_id, payload, revision, updated_at) VALUES (?, ?, ?, ?)",
                (player_id, payload, revision, now),
            )
        else:
            current = int(row["revision"])
            # 简单乐观锁：客户端带旧 revision 时校验
            if body.revision is not None and body.revision < current:
                raise HTTPException(
                    status_code=409,
                    detail=f"存档冲突：服务器 revision={current}，客户端={body.revision}",
                )
            revision = current + 1
            conn.execute(
                "UPDATE saves SET payload = ?, revision = ?, updated_at = ? WHERE player_id = ?",
                (payload, revision, now, player_id),
            )

    return SaveResponse(data=body.data, revision=revision, updated_at=now)


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("main:app", host="0.0.0.0", port=int(os.getenv("PORT", "8000")), reload=True)
