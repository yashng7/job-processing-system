# Job Processing System

A full-stack distributed job processing system built with .NET 8, Angular, PostgreSQL, Redis, and SignalR.

## Architecture Overview

```
┌─────────────┐     HTTP      ┌─────────────┐     EF Core    ┌──────────────┐
│   Angular   │ ──────────── ▶│  ASP.NET    │ ─────────────▶ │  PostgreSQL  │
│  Frontend   │               │  Core API   │                 │   Database   │
│  (Port 80)  │ ◀──SignalR─── │  (Port 5000)│ ─── Redis ────▶│              │
└─────────────┘               └─────────────┘     Queue       └──────────────┘
                                                      │               ▲
                                                      ▼               │
                                               ┌─────────────┐        │
                                               │   .NET      │ EF Core│
                                               │   Worker    │ ───────┘
                                               │   Service   │
                                               └─────────────┘
```

### Components

| Service    | Technology          | Port  | Responsibility                        |
|------------|---------------------|-------|---------------------------------------|
| Frontend   | Angular 17 + Nginx  | 80    | UI, real-time updates via SignalR     |
| API        | ASP.NET Core 8      | 5000  | REST API, SignalR Hub, job enqueue    |
| Worker     | .NET Worker Service | -     | Consume queue, process jobs           |
| PostgreSQL | PostgreSQL 16       | 5432  | Persistent job storage                |
| Redis      | Redis 7             | 6379  | Job queue (LPUSH/BRPOP)               |

## Quick Start

### Prerequisites
- Docker Desktop installed
- Ports 80 and 5000 available

### Run

```bash
git clone <repo-url>
cd job-processing-system
docker compose up --build
```

Open browser: **http://localhost**

API: **http://localhost:5000**

## API Usage

### Create a Job
```bash
curl -X POST http://localhost:5000/api/jobs \
  -H "Content-Type: application/json" \
  -d '{"name": "Email Job", "payload": "{\"to\": \"user@example.com\"}"}'
```

### List All Jobs
```bash
curl http://localhost:5000/api/jobs
```

### Get Job Details
```bash
curl http://localhost:5000/api/jobs/{id}
```

### Retry Failed Job
```bash
curl -X POST http://localhost:5000/api/jobs/{id}/retry
```

### Health Check
```bash
curl http://localhost:5000/health
```

## Scaling Workers

To run multiple worker instances:

```bash
docker compose up --build --scale worker=3
```

## Job Lifecycle

```
Created → Queued → Processing → Completed
                             ↘ Failed (retry up to 3 times)
```

## Environment Variables

### API
| Variable | Description | Default |
|----------|-------------|---------|
| ConnectionStrings__DefaultConnection | PostgreSQL connection | - |
| Redis__ConnectionString | Redis host:port | - |
| Cors__AllowedOrigins | Allowed CORS origins | - |

### Worker
| Variable | Description |
|----------|-------------|
| ConnectionStrings__DefaultConnection | PostgreSQL connection |
| Redis__ConnectionString | Redis host:port |