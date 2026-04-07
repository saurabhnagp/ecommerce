namespace AmCart.ProductService.Application.DTOs;

public class OrderHistoryItemDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "";
    public DateTime? ShippedAt { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "USD";
    public string? PreviewProductName { get; set; }
    public string? PreviewImageUrl { get; set; }
    public int LineItemCount { get; set; }
}

public class PagedOrdersDto
{
    public IReadOnlyList<OrderHistoryItemDto> Items { get; set; } = Array.Empty<OrderHistoryItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
