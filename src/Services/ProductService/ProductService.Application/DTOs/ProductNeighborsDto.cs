namespace AmCart.ProductService.Application.DTOs;

public class ProductNeighborDto
{
    public string Slug { get; set; } = null!;
    public string Name { get; set; } = null!;
}

/// <summary>Previous/next product in the same category (ordered by name, then id).</summary>
public class ProductNeighborsDto
{
    public ProductNeighborDto? Previous { get; set; }
    public ProductNeighborDto? Next { get; set; }
}
