# PaymentGateway

A pet project payment API built in C#/.NET, integrated with a Telegram Web App (TWA) frontend for creating and tracking payments.

## Tech Stack

- **.NET 9** (ASP.NET Core, Minimal APIs)
- **PostgreSQL 16** + **EF Core 9** (Npgsql)
- **FluentValidation** — request validation
- **Serilog** — structured logging with `LogContext` and a custom `CorrelationIdMiddleware` for end-to-end request tracing
- **Telegram.Bot 22.10.3** — bot commands + Telegram WebApp `initData` HMAC-SHA256 validation
- **xUnit** — unit tests (`PaymentGateway.Tests`)
- **Docker Compose** — API + PostgreSQL

## Features

- CRUD-style payment API with pagination
- Global error handling (`IExceptionHandler` + `ProblemDetails`)
- Custom exceptions mapped to HTTP status codes (404, 409, etc.)
- Idempotency key support (unique index, duplicate-request protection)
- Payment status history tracking
- Fake PSP provider simulating payment processing
- Background service for async payment processing
- Retry logic on simulated PSP failure
- Telegram bot (`/start`, Web App button)
- Telegram WebApp `initData` signature validation, wired into auth
- Frontend (TWA) served from `wwwroot`, using Telegram WebApp SDK (theming, haptic feedback, `close()`)
- Unit tests for `PaymentService`
- CORS policy configured
- Dockerized API + database via `docker-compose`

## API Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/telegram` | Validates Telegram `initData`, returns parsed user |
| POST | `/api/payments` | Creates a payment (user id resolved from Telegram `initData` header if present) |
| GET | `/api/payments/{id}` | Gets a payment by id |
| GET | `/api/payments` | Lists payments, paginated, optionally filtered by `telegramUserId` |

## Project Structure

```
PaymentGateway.sln
├── PaymentGateway/          # API project (.NET 9, wwwroot = TWA frontend)
└── PaymentGateway.Tests/    # xUnit tests
```

## Running Locally

Set required environment variables (`.env` or shell):

```
POSTGRES_PASSWORD=your_password
TELEGRAM_BOT_TOKEN=your_bot_token
```

From the repository root:

```bash
docker compose up -d --build
```

The API will be available at `http://localhost:8080`, PostgreSQL at `localhost:5432`.

## Status

Backend, Telegram bot, and TWA frontend integration are complete and dockerized.