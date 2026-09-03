# Stage 1: Build & Dependencies
FROM python:3.12-slim AS builder
WORKDIR /app
COPY agent/pyproject.toml agent/README.md ./
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir .

# Stage 2: Test
FROM builder AS tester
WORKDIR /app
RUN pip install --no-cache-dir pytest pytest-asyncio
COPY agent/app/ ./app/
COPY agent/tests/ ./tests/
COPY contracts/ ./contracts/
ENV PYTHONPATH=/app
CMD ["pytest"]

# Stage 3: Runtime
FROM python:3.12-slim AS runtime
WORKDIR /app
COPY --from=builder /usr/local/lib/python3.12/site-packages /usr/local/lib/python3.12/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin
COPY agent/app/ ./app/
COPY contracts/ ./contracts/
ENV PORT=8000
EXPOSE 8000
ENV ENVIRONMENT=development
ENV USE_FIXTURES=True
CMD ["sh", "-c", "uvicorn app.api:app --host 0.0.0.0 --port ${PORT}"]
