# 飞牛 NAS 部署 MH Idle 存档服务器

> 我无法直接登录你家的飞牛；下面按飞牛常见 **Docker** 方式，你在 NAS 上操作即可。

## 一、飞牛上先确认

1. 已安装并启用 **Docker**（飞牛应用中心 / 容器相关功能）
2. 知道 NAS 局域网 IP，例如 `192.168.1.50`
3. 若要从外网访问：已做端口转发，或用飞牛自带的外网访问 / 反代

## 二、把服务文件放到 NAS

任选一种：

### 方式 A：Git 克隆（推荐）

在飞牛「终端」或 SSH 里：

```bash
cd /vol1/1000   # 改成你的数据盘实际路径
git clone https://github.com/805436085/MHWorld_Unity.git
cd MHWorld_Unity
git checkout cursor/mh-idle-dev-17dc
cd server
```

### 方式 B：只拷 server 目录

把仓库里的 `server/` 整个文件夹上传到飞牛某个目录，例如：

`/vol1/1000/mh-idle-server/`

需要包含：

- `Dockerfile`
- `docker-compose.yml`
- `main.py`
- `requirements.txt`

## 三、一键启动

在 `server` 目录执行：

```bash
mkdir -p data
docker compose up -d --build
```

看是否起来：

```bash
docker compose ps
docker compose logs -f --tail=50
```

本机测试：

```bash
curl http://127.0.0.1:8000/health
```

局域网测试（把 IP 换成你的飞牛 IP）：

```text
http://192.168.1.50:8000/health
http://192.168.1.50:8000/docs
```

应看到 `{"status":"ok"}` 和接口文档页。

## 四、飞牛 Docker UI 操作（如果你更习惯图形界面）

1. 打开飞牛的 Docker / 容器管理
2. 用「Compose」或「项目」导入本目录的 `docker-compose.yml`
3. 构建并启动 `mh-idle-server`
4. 确认端口映射 `8000:8000`（冲突就改成 `18080:8000`）

## 五、外网访问（给微信小游戏用）

微信小游戏正式环境通常要求 **HTTPS 域名**，不能只用家里局域网 IP。

可选路径：

1. **飞牛反代 + 域名 + HTTPS**（有域名时最正规）
2. 先内网/开发调试用 `http://NAS局域网IP:8000`
3. 临时穿透（仅调试，不建议正式上线）

Unity / 微信里把 `CloudSaveClient` 的 `baseUrl` 改成：

```text
https://你的域名
```

或开发期：

```text
http://192.168.1.50:8000
```

## 六、常用维护

```bash
# 停止
docker compose down

# 更新代码后重建
git pull
docker compose up -d --build

# 备份存档（很重要）
cp data/mh_idle.db data/mh_idle.db.bak-$(date +%F)
```

数据库文件在：`server/data/mh_idle.db`

## 七、验证登录与存档

```bash
# 游客登录
curl -s -X POST http://192.168.1.50:8000/api/login \
  -H 'Content-Type: application/json' \
  -d '{"code":"guest","guest_key":"nas-test"}'

# 把返回的 token 填下面
curl -s http://192.168.1.50:8000/api/save \
  -H 'Authorization: Bearer 这里填token'
```

## 八、我这边做不到的事

- 无法直接 SSH/登录你的飞牛
- 无法替你配置家庭路由器端口转发、域名证书

你如果愿意，下一步可以把这两项发我，我按你的实际情况改配置：

1. 飞牛局域网 IP  
2. 是否已有域名 / 是否已开 Docker  
3. `docker compose up` 的报错原文（如有）
