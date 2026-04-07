using System.Text;

namespace AmCart.UserService.Api.Services;

/// <summary>Forwards catalog and order requests to ProductService (same JWT issuer/audience when auth is forwarded).</summary>
public class ProductServiceProxy
{
    private readonly HttpClient _http;

    public ProductServiceProxy(HttpClient http)
    {
        _http = http;
    }

    public Task<HttpResponseMessage> GetOrdersAsync(int page, int pageSize, string authorizationHeader, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/v1/orders?page={page}&pageSize={pageSize}");
        req.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    public Task<HttpResponseMessage> GetOrderAsync(Guid orderId, string authorizationHeader, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"api/v1/orders/{orderId}");
        req.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    public Task<HttpResponseMessage> GetProductBySlugAsync(string slug, string? authorizationHeader, CancellationToken ct = default)
    {
        var path = $"api/v1/products/by-slug/{Uri.EscapeDataString(slug)}?publicOnly=true";
        var req = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrWhiteSpace(authorizationHeader))
            req.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    public Task<HttpResponseMessage> GetProductNeighborsAsync(Guid productId, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"api/v1/products/{productId}/neighbors");
        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    public Task<HttpResponseMessage> PostProductReviewAsync(
        Guid productId,
        string jsonBody,
        string authorizationHeader,
        CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/v1/products/{productId}/reviews")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
        };
        req.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    public Task<HttpResponseMessage> PostProductReviewVoteAsync(
        Guid productId,
        Guid reviewId,
        string jsonBody,
        string authorizationHeader,
        CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/v1/products/{productId}/reviews/{reviewId}/vote")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
        };
        req.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    }
}
