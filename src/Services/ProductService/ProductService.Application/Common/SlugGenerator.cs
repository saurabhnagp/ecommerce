using System.Text.RegularExpressions;

namespace AmCart.ProductService.Application.Common;

public static class SlugGenerator
{
    private static readonly Regex SlugInvalidChars = new(@"[^a-z0-9\-]", RegexOptions.Compiled);
    private static readonly Regex MultipleDashes = new(@"\-+", RegexOptions.Compiled);

    /// <summary>Generates a URL-friendly slug from the given text. Trims, lowercases, replaces spaces with dashes, removes invalid characters.</summary>
    public static string FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var slug = name.Trim().ToLowerInvariant();
        slug = slug.Replace(' ', '-');
        slug = SlugInvalidChars.Replace(slug, "");
        slug = MultipleDashes.Replace(slug, "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }

    /// <summary>Returns a unique slug by appending -2, -3, ... until exists returns false.</summary>
    public static async Task<string> GetUniqueSlugAsync(string name, Func<string, CancellationToken, Task<bool>> slugExistsAsync, CancellationToken ct = default)
    {
        var baseSlug = FromName(name);
        var slug = baseSlug;
        var counter = 2;
        while (await slugExistsAsync(slug, ct).ConfigureAwait(false))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        return slug;
    }
}
