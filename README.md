# ShopFlow – E-commerce Microservices

Portfolio microservices system that demonstrates **service boundaries**, **database-per-service**, **API Gateway (YARP)**, and **async messaging (MassTransit + RabbitMQ)**.

**Stack:** .NET 9 · ASP.NET Core · YARP · MassTransit · RabbitMQ · PostgreSQL · Redis · EF Core · Docker Compose

---

## Why this project exists

A monolith is enough for a small shop. ShopFlow is structured like a distributed system so you can show:

| Concern | How ShopFlow addresses it |
|--------|---------------------------|
| Independent deployability | Separate API projects + containers |
| Data ownership | Identity, Catalog, Orders each have their own PostgreSQL database |
| Polyglot persistence | Basket uses Redis |
| Async integration | Orders publishes `OrderCreated` via MassTransit/RabbitMQ |
| Single client entry | YARP API Gateway |

---

## Architecture (MVP)

```
                    ┌─────────────────┐
                    │  ShopFlow.Gateway│  :5100
                    │      (YARP)      │
                    └────────┬────────┘
           ┌─────────────────┼─────────────────┐
           ▼                 ▼                 ▼
    /identity/*        /catalog/*         /basket/*      /orders/*
    Identity:5101      Catalog:5102       Basket:5103    Orders:5104
    PostgreSQL:5433    PostgreSQL:5434    Redis:6380     PostgreSQL:5435
                              │                               │
                              └─────────── RabbitMQ ──────────┘
                                         :5672 / :15672
```

**Services**

| Service    | Port | Database              | Role                          |
|-----------|------|------------------------|-------------------------------|
| Gateway   | 5100 | —                      | Routing, single entry         |
| Identity  | 5101 | postgres-identity:5433 | Auth, JWT                     |
| Catalog   | 5102 | postgres-catalog:5434  | Products                      |
| Basket    | 5103 | Redis:6380             | Shopping cart                 |
| Orders    | 5104 | postgres-orders:5435   | Place orders, publish events  |
| RabbitMQ  | 5672 | —                      | Messaging (UI :15672)         |

---

## Prerequisites

- .NET 9 SDK
- Docker Desktop (for infrastructure)

---

## Quick Start – Infrastructure

```bash
cd docker
docker compose up -d
```

| Resource              | URL / Port                                      |
|-----------------------|-------------------------------------------------|
| RabbitMQ Management   | `http://localhost:15672` (shopflow / shopflow_secret) |
| Redis                 | `localhost:6380`                                |
| Identity DB           | `localhost:5433`                                |
| Catalog DB            | `localhost:5434`                                |
| Orders DB             | `localhost:5435`                                |

---

## Run services (local)

From solution root, in separate terminals:

```bash
dotnet run --project src/Gateways/ShopFlow.Gateway
dotnet run --project src/Services/Identity/ShopFlow.Identity
dotnet run --project src/Services/Catalog/ShopFlow.Catalog
dotnet run --project src/Services/Basket/ShopFlow.Basket
dotnet run --project src/Services/Orders/ShopFlow.Orders
```

Gateway: `http://localhost:5100`  
Example: `http://localhost:5100/catalog/health` → Catalog service health  

---

## Project structure

```
ShopFlow/
├── src/
│   ├── Gateways/ShopFlow.Gateway
│   ├── Services/
│   │   ├── Identity/ShopFlow.Identity
│   │   ├── Catalog/ShopFlow.Catalog
│   │   ├── Basket/ShopFlow.Basket
│   │   └── Orders/ShopFlow.Orders
│   └── BuildingBlocks/ShopFlow.BuildingBlocks   # shared events (e.g. OrderCreated)
├── docker/docker-compose.yml
└── README.md
```

---

## Implementation roadmap

1. **Foundation** (this release) – solution, Compose, Gateway, service shells, shared events  
2. **Identity** – register/login/JWT  
3. **Catalog** – products CRUD  
4. **Basket** – Redis cart  
5. **Orders** – create order + publish `OrderCreated`  
6. **Messaging** – MassTransit consumer  
7. **Polish** – seed data, health, README diagram  

---

## Non-goals (by design)

- Payment provider integration  
- Full inventory saga / distributed transactions  
- Kubernetes  
- Event sourcing everywhere  

These are documented so the project stays honest and interview-defensible.

---

## License

MIT – portfolio demonstration.
