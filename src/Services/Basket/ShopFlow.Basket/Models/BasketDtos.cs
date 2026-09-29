namespace ShopFlow.Basket.Models;

public record BasketItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);

public record CustomerBasketDto(
    string UserId,
    IReadOnlyList<BasketItemDto> Items,
    decimal Total
);

public record AddBasketItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);

public record UpdateBasketItemRequest(
    int Quantity
);
