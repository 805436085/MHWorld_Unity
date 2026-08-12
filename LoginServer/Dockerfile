FROM python:3.13.15-slim

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    LOGIN_DATA_DIR=/app/data \
    LOGIN_JWT_SECRET=change-me-in-production \
    LOGIN_JWT_EXPIRE_HOURS=168

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY app ./app

RUN mkdir -p /app/data

EXPOSE 8080

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8080"]
