# ShopFlow – E-commerce Microservices

Portfolio-grade e-commerce microservices system demonstrating **service boundaries**, **database-per-service**, **API Gateway with YARP**, and **asynchronous messaging with MassTransit and RabbitMQ**.

**Tech Stack:** .NET 9 · ASP.NET Core · YARP · MassTransit · RabbitMQ · PostgreSQL · Redis · EF Core · JWT · Docker Compose

---

## Why This Project?

ShopFlow demonstrates how a modern distributed e-commerce system can be designed around independently deployable services with clear ownership of data and responsibilities.

| Concern                    | How ShopFlow Addresses It                                             |
| -------------------------- | --------------------------------------------------------------------- |
| Independent deployability  | Separate API projects for each service                                |
| Data ownership             | Identity, Catalog, and Orders each have their own PostgreSQL database |
| Polyglot persistence       | Basket uses Redis                                                     |
| Asynchronous integration   | Orders publishes `OrderCreated` through MassTransit/RabbitMQ          |
| Single client entry point  | YARP API Gateway                                                      |
| Service boundaries         | Each service owns its business logic and data                         |
| Event-driven communication | Integration events are published through RabbitMQ                     |

---

# Architecture

```text
                           ┌───────────────────────┐
                           │   ShopFlow Gateway    │
                           │       YARP :5100      │
                           └───────────┬───────────┘
                                       │
             ┌─────────────────────────┼─────────────────────────┐
             │                         │                         │
             ▼                         ▼                         ▼
   ┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
   │     Identity    │       │     Catalog     │       │      Basket     │
   │      :5101      │       │      :5102      │       │      :5103      │
   │                 │       │                 │       │                 │
   │   PostgreSQL    │       │   PostgreSQL    │       │      Redis      │
   │      :5433      │       │      :5434      │       │      :6380      │
   └─────────────────┘       └─────────────────┘       └─────────────────┘
             │
             │
             │                         ┌─────────────────┐
             │                         │      Orders     │
             └────────────────────────►│      :5104      │
                                       │                 │
                                       │   PostgreSQL    │
                                       │      :5435      │
                                       └────────┬────────┘
                                                │
                                                │ OrderCreated
                                                ▼
                                       ┌─────────────────┐
                                       │    RabbitMQ     │
                                       │      :5672      │
                                       │ Management :15672│
                                       └─────────────────┘
```

---

## Services

| Service  |   Port | Data Store         | Responsibility                        |
| -------- | -----: | ------------------ | ------------------------------------- |
| Gateway  | `5100` | —                  | Routing and single entry point        |
| Identity | `5101` | PostgreSQL `:5433` | Registration, authentication and JWT  |
| Catalog  | `5102` | PostgreSQL `:5434` | Product management                    |
| Basket   | `5103` | Redis `:6380`      | Shopping cart management              |
| Orders   | `5104` | PostgreSQL `:5435` | Order management and event publishing |
| RabbitMQ | `5672` | —                  | Asynchronous message broker           |

---

# Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)

Docker Desktop is required to run the PostgreSQL databases, Redis, and RabbitMQ infrastructure.

---

# Quick Start

## 1. Start Infrastructure

From the project root:

```bash
cd docker
docker compose up -d
```

This starts the required infrastructure services.

### Infrastructure

| Resource            | URL / Connection       |
| ------------------- | ---------------------- |
| RabbitMQ Management | http://localhost:15672 |
| RabbitMQ            | `localhost:5672`       |
| Redis               | `localhost:6380`       |
| Identity Database   | `localhost:5433`       |
| Catalog Database    | `localhost:5434`       |
| Orders Database     | `localhost:5435`       |

### RabbitMQ Management Credentials

```text
Username: shopflow
Password: shopflow_secret
```

---

# Run the Services

From the solution root, run each service in a separate terminal.

### Gateway

```bash
dotnet run --project src/Gateways/ShopFlow.Gateway
```

### Identity

```bash
dotnet run --project src/Services/Identity/ShopFlow.Identity
```

### Catalog

```bash
dotnet run --project src/Services/Catalog/ShopFlow.Catalog
```

### Basket

```bash
dotnet run --project src/Services/Basket/ShopFlow.Basket
```

### Orders

```bash
dotnet run --project src/Services/Orders/ShopFlow.Orders
```

---

# Service URLs

| Entry Point         | URL                                           |
| ------------------- | --------------------------------------------- |
| Gateway             | http://localhost:5100                         |
| Identity Swagger    | http://localhost:5101/swagger                 |
| Catalog Swagger     | http://localhost:5102/swagger                 |
| Basket Swagger      | http://localhost:5103/swagger                 |
| Orders Swagger      | http://localhost:5104/swagger                 |
| Catalog via Gateway | http://localhost:5100/catalog/api/v1/products |

The Gateway provides the main entry point for client applications while the individual service URLs can be used for development and Swagger testing.

---

# Project Structure

```text
ShopFlow/
│
├── src/
│   │
│   ├── Gateways/
│   │   └── ShopFlow.Gateway
│   │
│   ├── Services/
│   │   ├── Identity/
│   │   │   └── ShopFlow.Identity
│   │   │
│   │   ├── Catalog/
│   │   │   └── ShopFlow.Catalog
│   │   │
│   │   ├── Basket/
│   │   │   └── ShopFlow.Basket
│   │   │
│   │   └── Orders/
│   │       └── ShopFlow.Orders
│   │
│   └── BuildingBlocks/
│       └── ShopFlow.BuildingBlocks
│           └── Shared integration events
│
├── docker/
│   └── docker-compose.yml
│
├── docs/
│   └── architecture.md
│
└── README.md
```

---

# API Overview

All service APIs use versioned routes under:

```text
/api/v1
```

---

## Identity

**Base Route:** `/api/v1/auth`

| Method | Endpoint    | Auth   | Description                    |
| ------ | ----------- | ------ | ------------------------------ |
| POST   | `/register` | Public | Register a new customer        |
| POST   | `/login`    | Public | Login and receive JWT          |
| GET    | `/me`       | Bearer | Get current authenticated user |

### Registration Example

```json
{
  "email": "customer@example.com",
  "password": "SecureP@ss1",
  "firstName": "Ada",
  "lastName": "Lovelace"
}
```

The registration endpoint creates users with the `Customer` role.

---

# Catalog

**Base Route:** `/api/v1/products`

| Method | Endpoint | Auth      | Description                                        |
| ------ | -------- | --------- | -------------------------------------------------- |
| GET    | `/`      | Public    | List products with search, category and pagination |
| GET    | `/{id}`  | Public    | Get product by ID                                  |
| POST   | `/`      | Admin JWT | Create product                                     |
| PUT    | `/{id}`  | Admin JWT | Update product                                     |
| DELETE | `/{id}`  | Admin JWT | Soft-deactivate product                            |

---

# Basket

**Base Route:** `/api/v1/basket`

Basket data is stored in **Redis**.

| Method | Endpoint             | Auth   | Description               |
| ------ | -------------------- | ------ | ------------------------- |
| GET    | `/`                  | Bearer | Get current user's basket |
| POST   | `/items`             | Bearer | Add or increase an item   |
| PUT    | `/items/{productId}` | Bearer | Update item quantity      |
| DELETE | `/items/{productId}` | Bearer | Remove an item            |
| DELETE | `/`                  | Bearer | Clear basket              |

---

# Orders

**Base Route:** `/api/v1/orders`

| Method | Endpoint | Auth   | Description                             |
| ------ | -------- | ------ | --------------------------------------- |
| POST   | `/`      | Bearer | Create order and publish `OrderCreated` |
| GET    | `/`      | Bearer | List current user's orders              |
| GET    | `/{id}`  | Bearer | Get order by ID                         |

After an order is successfully persisted, the Orders service publishes an `OrderCreated` integration event to RabbitMQ through MassTransit.

A demonstration consumer receives the event and logs the message.

---

# Asynchronous Messaging

ShopFlow uses **MassTransit + RabbitMQ** for asynchronous communication between services.

The primary integration event is:

```text
OrderCreated
```

### Event Flow

```text
Client
   │
   ▼
Gateway
   │
   ▼
Orders Service
   │
   ├── Save Order
   │
   ▼
PostgreSQL
   │
   │ Local commit succeeds
   ▼
MassTransit
   │
   ▼
RabbitMQ
   │
   ▼
OrderCreated Consumer
```

The architecture deliberately avoids distributed transactions between services.

---

# End-to-End Flow

A typical customer journey through ShopFlow looks like this:

```text
1. Register
      ↓
2. Login
      ↓
3. Receive JWT
      ↓
4. Browse products
      ↓
5. Add products to basket
      ↓
6. Basket stored in Redis
      ↓
7. Create order
      ↓
8. Order stored in Orders PostgreSQL
      ↓
9. Publish OrderCreated
      ↓
10. RabbitMQ delivers event
      ↓
11. Consumer processes event
```

---

# Design Principles

## Database per Service

Each service owns its own data.

```text
Identity ──► Identity PostgreSQL

Catalog  ──► Catalog PostgreSQL

Orders   ──► Orders PostgreSQL

Basket   ──► Redis
```

Services do **not** directly read or write another service's database tables.

This keeps service boundaries explicit and allows each service to evolve independently.

---

## Authentication

Identity is responsible for issuing JWT access tokens.

Other services validate the same:

* Signing key
* Issuer
* Audience

The Gateway provides the external entry point while individual services remain responsible for authorization within their own boundaries.

---

## Synchronous Communication

Normal client requests follow this path:

```text
Client
   ↓
YARP Gateway
   ↓
Target Microservice
   ↓
Database / Redis
```

For example:

```text
GET /catalog/api/v1/products
        ↓
ShopFlow Gateway
        ↓
Catalog Service
        ↓
Catalog PostgreSQL
```

---

## Asynchronous Communication

Integration events use RabbitMQ:

```text
Orders Service
      ↓
MassTransit
      ↓
RabbitMQ
      ↓
Consumer
```

This allows services to react to business events without requiring direct synchronous calls between them.

---

# Integration Contracts

Shared integration events are located in:

```text
src/BuildingBlocks/ShopFlow.BuildingBlocks
```

Currently, the primary event is:

```text
OrderCreated
```

The BuildingBlocks project contains **contracts/events**, not shared business logic or shared databases.

---

# Key Microservices Concepts Demonstrated

ShopFlow is designed as a portfolio project to demonstrate practical understanding of:

* Microservice architecture
* Service boundaries
* Database-per-service
* API Gateway pattern
* YARP reverse proxy
* JWT authentication
* Role-based authorization
* Redis-based persistence
* PostgreSQL
* Entity Framework Core
* Asynchronous messaging
* RabbitMQ
* MassTransit
* Integration events
* Docker Compose
* Independent service deployment
* Synchronous vs asynchronous communication
* Event-driven architecture

---

# Non-Goals

The current implementation intentionally does not include:

* Payment provider integration
* Full inventory management
* Inventory reservation
* Distributed transactions
* Full order-processing saga
* Kubernetes deployment
* Event sourcing across the entire system

These areas can be introduced later as the system evolves.

---

# Architecture Documentation

Additional architecture and failure-mode discussions are available in:

```text
docs/architecture.md
```

This document covers design decisions, service communication, failure scenarios, and discussion points relevant to understanding the system architecture.
