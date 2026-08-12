# MH Idle 服务器（Python / FastAPI）

## 这是什么

本作独立的存档/登录后端，**不依赖主游戏服务器**。  
同一微信小游戏 AppID 下，登录凭证仍来自微信；账号与存档数据存在你自己的机器上。

## 飞牛 NAS 部署（推荐）

详见：**[飞牛NAS部署.md](./飞牛NAS部署.md)**

核心命令（在 `server` 目录）：

```bash
mkdir -p data
docker compose up -d --build
```

## 本地启动（电脑调试）

```bash
cd server
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

浏览器打开：http://127.0.0.1:8000/docs

## 接口

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/health` | 健康检查 |
| POST | `/api/login` | `{ "code": "guest", "guest_key": "device-1" }` → token |
| GET | `/api/save` | Header `Authorization: Bearer <token>` |
| PUT | `/api/save` | body `{ "data": { ...存档JSON... }, "revision": 0 }` |

## 环境变量

见 `.env.example`。Docker 里也可写在 `docker-compose.yml` 的 `environment`。

正式微信登录：填 `MH_WECHAT_APP_ID` / `MH_WECHAT_APP_SECRET`，客户端传真正的 `wx.login` code。
