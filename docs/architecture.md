# ShopFlow Architecture Notes

## Service boundaries

- **Identity** owns users and credentials. No other service writes to its database.
- **Catalog** owns products and list prices.
- **Basket** is ephemeral cart state in Redis (keyed by user id).
- **Orders** owns order aggregates. After commit, it publishes `OrderCreated`.

## Communication

- **Sync:** Browser/mobile → Gateway (YARP) → service HTTP APIs.
- **Async:** Orders → RabbitMQ (`OrderCreated`) → subscribers (e.g. logging, future inventory).

## Failure modes to discuss in interviews

1. Catalog down while browsing: Gateway returns 502 for `/catalog/*`; Identity/Basket can still work.
2. Order saved but message broker down: document at-least-once publishing strategy (outbox later).
3. No distributed transaction between Basket and Orders: place order reads basket, writes order, then clears basket (compensating action if needed).
