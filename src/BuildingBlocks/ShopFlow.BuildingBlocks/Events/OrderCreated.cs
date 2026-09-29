namespace ShopFlow.BuildingBlocks.Events;

/// <summary>
/// Published by Orders service after an order is successfully persisted.
/// Other services can subscribe without coupling to Orders' database.
/// </summary>
public record OrderCreated(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    DateTime OccurredAtUtc,
    IReadOnlyList<OrderCreatedItem> Items
);

public record OrderCreatedItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
