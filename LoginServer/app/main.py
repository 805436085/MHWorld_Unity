from __future__ import annotations

import sqlite3
from contextlib import asynccontextmanager
from typing import Annotated, AsyncIterator

from fastapi import Depends, FastAPI, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from pydantic import BaseModel, Field

from . import auth, db


@asynccontextmanager
async def lifespan(_app: FastAPI) -> AsyncIterator[None]:
    db.init_db()
    yield


app = FastAPI(title="MH Idle LoginServer", version="0.1.0", lifespan=lifespan)
bearer = HTTPBearer(auto_error=False)


class RegisterRequest(BaseModel):
    username: str = Field(min_length=3, max_length=32, pattern=r"^[A-Za-z0-9_]+$")
    password: str = Field(min_length=6, max_length=72)


class LoginRequest(BaseModel):
    username: str = Field(min_length=3, max_length=32)
    password: str = Field(min_length=6, max_length=72)


class TokenResponse(BaseModel):
    access_token: str
    token_type: str = "bearer"
    user_id: int
    username: str


class UserResponse(BaseModel):
    user_id: int
    username: str


@app.get("/health")
def health() -> dict:
    return {"ok": True, "service": "login"}


@app.post("/register", response_model=TokenResponse)
def register(body: RegisterRequest) -> TokenResponse:
    password_hash = auth.hash_password(body.password)
    try:
        with db.connect() as conn:
            cur = conn.execute(
                "INSERT INTO users (username, password_hash) VALUES (?, ?)",
                (body.username, password_hash),
            )
            user_id = int(cur.lastrowid)
    except sqlite3.IntegrityError as exc:
        raise HTTPException(status_code=409, detail="username already exists") from exc

    token = auth.create_access_token(user_id=user_id, username=body.username)
    return TokenResponse(access_token=token, user_id=user_id, username=body.username)


@app.post("/login", response_model=TokenResponse)
def login(body: LoginRequest) -> TokenResponse:
    with db.connect() as conn:
        row = conn.execute(
            "SELECT id, username, password_hash FROM users WHERE username = ? COLLATE NOCASE",
            (body.username,),
        ).fetchone()

    if row is None or not auth.verify_password(body.password, row["password_hash"]):
        raise HTTPException(status_code=401, detail="invalid username or password")

    user_id = int(row["id"])
    username = str(row["username"])
    token = auth.create_access_token(user_id=user_id, username=username)
    return TokenResponse(access_token=token, user_id=user_id, username=username)


def current_user(
    creds: Annotated[HTTPAuthorizationCredentials | None, Depends(bearer)],
) -> UserResponse:
    if creds is None or creds.scheme.lower() != "bearer":
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="missing token")
    try:
        payload = auth.decode_access_token(creds.credentials)
        return UserResponse(user_id=int(payload["sub"]), username=str(payload["username"]))
    except Exception as exc:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="invalid token") from exc


@app.get("/me", response_model=UserResponse)
def me(user: Annotated[UserResponse, Depends(current_user)]) -> UserResponse:
    return user
