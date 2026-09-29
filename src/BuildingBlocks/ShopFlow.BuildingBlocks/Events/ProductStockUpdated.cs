namespace ShopFlow.BuildingBlocks.Events;

/// <summary>
/// Optional event for future inventory reactions.
/// </summary>
public record ProductStockUpdated(
    Guid ProductId,
    int AvailableQuantity,
    DateTime OccurredAtUtc
);
