import { useState, useEffect, useCallback, useRef } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { ProductGrid } from "../components/ProductGrid";
import { fetchPublicProducts } from "../api/products";
import type { ProductDto } from "../api/products";
import { addCartItem } from "../api/cart";
import {
  addWishlistItem,
  fetchWishlistProductIds,
  removeWishlistItem,
} from "../api/wishlist";
import { useCart } from "../cart/CartContext";
import { onAuthChange } from "../auth/notify";
import { getAccessToken } from "../auth/storage";
import "./ProductListingPage.css";

const PAGE_SIZE = 12;

export function SearchResultsPage() {
  const [searchParams] = useSearchParams();
  const q = (searchParams.get("q") ?? "").trim();
  const hasQuery = q.length > 0;

  const { refreshCart } = useCart();
  const [sessionRev, setSessionRev] = useState(0);
  const [wishlistIds, setWishlistIds] = useState<Set<string>>(() => new Set());

  const [page, setPage] = useState(1);

  const [products, setProducts] = useState<ProductDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);

  useEffect(() => onAuthChange(() => setSessionRev((n) => n + 1)), []);

  useEffect(() => {
    setPage(1);
  }, [q]);

  useEffect(() => {
    const t = getAccessToken();
    if (!t) {
      setWishlistIds(new Set());
      return;
    }
    let cancelled = false;
    fetchWishlistProductIds(t)
      .then((res) => {
        if (!cancelled) setWishlistIds(new Set(res.data ?? []));
      })
      .catch(() => {
        if (!cancelled) setWishlistIds(new Set());
      });
    return () => {
      cancelled = true;
    };
  }, [sessionRev]);

  const handleAddToCart = useCallback(
    async (productId: string, quantity = 1) => {
      await addCartItem(productId, quantity);
      await refreshCart();
    },
    [refreshCart]
  );

  const handleWishlistToggle = useCallback(
    async (productId: string, nextInWishlist: boolean) => {
      const t = getAccessToken();
      if (!t) return;
      setWishlistIds((prev) => {
        const next = new Set(prev);
        if (nextInWishlist) next.add(productId);
        else next.delete(productId);
        return next;
      });
      try {
        if (nextInWishlist) await addWishlistItem(productId, t);
        else await removeWishlistItem(productId, t);
      } catch {
        setWishlistIds((prev) => {
          const next = new Set(prev);
          if (nextInWishlist) next.delete(productId);
          else next.add(productId);
          return next;
        });
      }
    },
    []
  );

  const fetchRef = useRef(0);

  const loadProducts = useCallback(async (p: number, search: string) => {
    const id = ++fetchRef.current;
    setLoading(true);
    try {
      const res = await fetchPublicProducts({
        page: p,
        pageSize: PAGE_SIZE,
        search,
        sortBy: "createdAt",
        sortDesc: true,
      });
      if (id === fetchRef.current) {
        setProducts(res.data.items);
        setTotalCount(res.data.totalCount);
        setTotalPages(res.data.totalPages);
      }
    } catch {
      if (id === fetchRef.current) {
        setProducts([]);
        setTotalCount(0);
        setTotalPages(1);
      }
    } finally {
      if (id === fetchRef.current) setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!hasQuery) {
      setProducts([]);
      setTotalCount(0);
      setTotalPages(1);
      setLoading(false);
      return;
    }
    loadProducts(page, q);
  }, [page, q, hasQuery, loadProducts]);

  return (
    <div className="product-listing">
      <section className="product-listing__main">
        <nav className="listing-breadcrumb">
          <Link to="/">Home</Link>
          <span className="listing-breadcrumb__sep">/</span>
          <span>Search results</span>
          {hasQuery && (
            <>
              <span className="listing-breadcrumb__sep">/</span>
              <span>&ldquo;{q}&rdquo;</span>
            </>
          )}
        </nav>

        {!hasQuery ? (
          <p className="search-results__validation" role="alert">
            Please enter a search term.
          </p>
        ) : (
          <ProductGrid
            products={products}
            totalCount={totalCount}
            page={page}
            totalPages={totalPages}
            onPageChange={setPage}
            loading={loading}
            wishlistProductIds={wishlistIds}
            onWishlistToggle={handleWishlistToggle}
            onAddToCart={handleAddToCart}
            emptyMessage="No products found for your search."
          />
        )}
      </section>
    </div>
  );
}
