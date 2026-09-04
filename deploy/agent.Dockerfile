# Stage 1: Build & Dependencies
FROM python:3.12-slim@sha256:78387bc3881b8273120a12ebe6c1ab22b018ccc2c9adf565ae1ac9b536e184ea AS builder
WORKDIR /app
COPY agent/pyproject.toml agent/README.md agent/requirements.lock ./
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.lock && \
    pip install --no-cache-dir --no-deps .

# Stage 2: Test
FROM builder AS tester
WORKDIR /app
COPY agent/requirements-test.lock ./
RUN pip install --no-cache-dir -r requirements-test.lock
COPY agent/app/ ./app/
COPY agent/tests/ ./tests/
COPY contracts/ ./contracts/
ENV PYTHONPATH=/app
CMD ["pytest"]

# Stage 3: Runtime
FROM python:3.12-slim@sha256:78387bc3881b8273120a12ebe6c1ab22b018ccc2c9adf565ae1ac9b536e184ea AS runtime
WORKDIR /app
COPY --from=builder /usr/local/lib/python3.12/site-packages /usr/local/lib/python3.12/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin
COPY agent/app/ ./app/
COPY contracts/ ./contracts/
ENV PORT=8000 ENVIRONMENT=production USE_FIXTURES=False
EXPOSE 8000
CMD ["sh", "-c", "uvicorn app.api:app --host 0.0.0.0 --port ${PORT}"]
