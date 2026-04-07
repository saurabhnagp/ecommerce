namespace AmCart.ProductService.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
