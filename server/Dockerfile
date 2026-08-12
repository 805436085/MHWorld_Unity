FROM python:3.12-slim

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    MH_DB_PATH=/data/mh_idle.db \
    MH_ALLOW_GUEST_LOGIN=true

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY main.py .

EXPOSE 8000

# 数据目录由 docker-compose 挂载到 /data
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
