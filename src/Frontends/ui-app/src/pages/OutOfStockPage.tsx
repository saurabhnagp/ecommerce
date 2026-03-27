import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { fetchPublicProducts, type ProductDto } from "../api/products";
import { addCartItem } from "../api/cart";
import { useCart } from "../cart/CartContext";
import { ProductGrid } from "../components/ProductGrid";
import "./ProductListingPage.css";

const PAGE_SIZE = 12;

/**
 * Active catalog items that are tracked and currently have no sellable quantity.
 * (They remain visible for transparency; add-to-cart is disabled on cards.)
 */
export function OutOfStockPage() {
  const { refreshCart } = useCart();
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const load = useCallback(async (p: number) => {
    setLoading(true);
    setErr("");
    try {
      const res = await fetchPublicProducts({
        page: p,
        pageSize: PAGE_SIZE,
        stockFilter: "outOfStock",
        sortBy: "createdAt",
        sortDesc: true,
      });
      setProducts(res.data.items);
      setTotalCount(res.data.totalCount);
      setTotalPages(res.data.totalPages);
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Could not load products");
      setProducts([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load(page);
  }, [page, load]);

  const handleAddToCart = useCallback(
    async (productId: string, quantity = 1) => {
      await addCartItem(productId, quantity);
      await refreshCart();
    },
    [refreshCart]
  );

  return (
    <div className="product-listing">
      <section className="product-listing__main" style={{ maxWidth: "100%" }}>
        <nav className="listing-breadcrumb">
          <Link to="/">Home</Link>
          <span className="listing-breadcrumb__sep">/</span>
          <span>Out of stock</span>
        </nav>
        <h1 style={{ fontSize: "1.35rem", margin: "0 0 0.5rem", color: "#222" }}>Currently unavailable</h1>
        <p className="muted" style={{ margin: "0 0 1rem", maxWidth: "640px" }}>
          These active products are tracked for inventory and have no units available right now. You can still see
          them here; use the stock indicator on other pages (green = in stock, red = out of stock).
        </p>
        {err && (
          <p className="search-results__validation" role="alert">
            {err}
          </p>
        )}
        <ProductGrid
          products={products}
          totalCount={totalCount}
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
          loading={loading}
          onAddToCart={handleAddToCart}
          emptyMessage="No out-of-stock listings right now — everything tracked is in stock."
        />
      </section>
    </div>
  );
}
