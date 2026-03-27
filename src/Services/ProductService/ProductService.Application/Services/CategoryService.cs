using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;

    public CategoryService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _categories.GetByIdAsync(id, ct);
        return c?.ToDto();
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var c = await _categories.GetBySlugAsync(slug, ct);
        return c?.ToDto();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var list = await _categories.GetAllAsync(includeInactive, ct);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetRootCategoriesAsync(CancellationToken ct = default)
    {
        var list = await _categories.GetRootCategoriesAsync(ct);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default)
    {
        var list = await _categories.GetSubCategoriesAsync(parentId, ct);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        await ValidateParentAsync(null, request.ParentCategoryId, ct);

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? request.Slug.Trim().ToLowerInvariant()
            : await SlugGenerator.GetUniqueSlugAsync(request.Name, (s, c) => _categories.SlugExistsAsync(s, null, c), ct);

        if (await _categories.SlugExistsAsync(slug, null, ct))
            throw new InvalidOperationException($"Slug '{slug}' already exists.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            ParentCategoryId = request.ParentCategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            MetaTitle = request.MetaTitle?.Trim(),
            MetaDescription = request.MetaDescription?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _categories.AddAsync(category, ct);
        var loaded = await _categories.GetByIdAsync(category.Id, ct);
        return loaded!.ToDto();
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _categories.GetByIdAsync(id, ct);
        if (category == null) return null;

        await ValidateParentAsync(id, request.ParentCategoryId, ct);

        if (request.Name != null) category.Name = request.Name.Trim();
        if (request.Slug != null) category.Slug = request.Slug.Trim().ToLowerInvariant();
        if (request.Description != null) category.Description = request.Description.Trim();
        if (request.ImageUrl != null) category.ImageUrl = request.ImageUrl.Trim();
        category.ParentCategoryId = request.ParentCategoryId;
        if (request.DisplayOrder.HasValue) category.DisplayOrder = request.DisplayOrder.Value;
        if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;
        if (request.MetaTitle != null) category.MetaTitle = request.MetaTitle.Trim();
        if (request.MetaDescription != null) category.MetaDescription = request.MetaDescription.Trim();

        category.UpdatedAt = DateTime.UtcNow;
        await _categories.UpdateAsync(category, ct);
        var loaded = await _categories.GetByIdAsync(id, ct);
        return loaded?.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categories.GetByIdAsync(id, ct);
        if (category == null) return false;

        var hasProducts = await _categories.HasProductsAsync(id, ct);
        if (hasProducts)
            throw new InvalidOperationException("Cannot delete category that has products. Move or remove products first.");

        var hasSubs = (await _categories.GetSubCategoriesAsync(id, ct)).Count > 0;
        if (hasSubs)
            throw new InvalidOperationException("Cannot delete category that has subcategories. Delete or move subcategories first.");

        await _categories.DeleteAsync(category, ct);
        return true;
    }

    private async Task ValidateParentAsync(Guid? categoryId, Guid? parentCategoryId, CancellationToken ct)
    {
        if (!parentCategoryId.HasValue) return;

        var parent = await _categories.GetByIdAsync(parentCategoryId.Value, ct);
        if (parent == null)
            throw new InvalidOperationException("Parent category does not exist.");

        if (categoryId.HasValue && parentCategoryId.Value == categoryId.Value)
            throw new InvalidOperationException("Category cannot be its own parent.");

        if (!categoryId.HasValue) return;

        var all = await _categories.GetAllAsync(includeInactive: true, ct);
        var childIdsByParent = all
            .Where(c => c.ParentCategoryId.HasValue)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        var stack = new Stack<Guid>();
        stack.Push(categoryId.Value);
        var descendants = new HashSet<Guid>();
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!childIdsByParent.TryGetValue(current, out var children)) continue;
            foreach (var ch in children)
            {
                if (!descendants.Add(ch)) continue;
                stack.Push(ch);
            }
        }

        if (descendants.Contains(parentCategoryId.Value))
            throw new InvalidOperationException("Category cannot be moved under one of its descendants.");
    }
}
