import { useState, useEffect, useCallback, useRef } from "react";
import { FilterSidebar } from "../components/FilterSidebar";
import { ProductGrid } from "../components/ProductGrid";
import type { Filters } from "../components/FilterSidebar";
import {
  fetchPublicProducts,
  fetchPublicCategories,
  fetchPublicBrands,
} from "../api/products";
import type { ProductDto, CategoryDto, BrandDto } from "../api/products";
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

const DEFAULT_FILTERS: Filters = {
  selectedCategoryId: null,
  priceMin: 0,
  priceMax: 0,
  sizes: [],
  colors: [],
  brandId: null,
  sort: "newest",
};

function sortToApi(sort: string): { sortBy?: string; sortDesc?: boolean } {
  switch (sort) {
    case "price-asc":
      return { sortBy: "price", sortDesc: false };
    case "price-desc":
      return { sortBy: "price", sortDesc: true };
    case "name-asc":
      return { sortBy: "name", sortDesc: false };
    case "popular":
      return { sortBy: "rating", sortDesc: true };
    case "newest":
    default:
      return { sortBy: "createdAt", sortDesc: true };
  }
}

export function ProductListingPage() {
  const { refreshCart } = useCart();
  const [sessionRev, setSessionRev] = useState(0);
  const [wishlistIds, setWishlistIds] = useState<Set<string>>(() => new Set());

  const [filters, setFilters] = useState<Filters>(DEFAULT_FILTERS);
  const [page, setPage] = useState(1);

  const [products, setProducts] = useState<ProductDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);

  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [brands, setBrands] = useState<BrandDto[]>([]);
  const [metaLoading, setMetaLoading] = useState(true);

  useEffect(() => onAuthChange(() => setSessionRev((n) => n + 1)), []);

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

  // Load categories + brands once
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [catRes, brandRes] = await Promise.all([
          fetchPublicCategories(),
          fetchPublicBrands(),
        ]);
        if (!cancelled) {
          setCategories(catRes.data);
          setBrands(brandRes.data);
        }
      } catch {
        /* sidebar shows empty state */
      } finally {
        if (!cancelled) setMetaLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const fetchRef = useRef(0);

  const loadProducts = useCallback(
    async (f: Filters, p: number) => {
      const id = ++fetchRef.current;
      setLoading(true);
      try {
        const { sortBy, sortDesc } = sortToApi(f.sort);
        const res = await fetchPublicProducts({
          page: p,
          pageSize: PAGE_SIZE,
          categoryId: f.selectedCategoryId ?? undefined,
          brandId: f.brandId ?? undefined,
          minPrice: f.priceMin > 0 ? f.priceMin : undefined,
          maxPrice: f.priceMax > 0 ? f.priceMax : undefined,
          sortBy,
          sortDesc,
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
    },
    []
  );

  // Re-fetch when filters or page change
  useEffect(() => {
    loadProducts(filters, page);
  }, [filters, page, loadProducts]);

  function handleFilterChange(next: Filters) {
    setFilters(next);
    setPage(1);
  }

  return (
    <div className="product-listing">
      <FilterSidebar
        filters={filters}
        onChange={handleFilterChange}
        categories={categories}
        brands={brands}
        loading={metaLoading}
      />

      <section className="product-listing__main">
        <nav className="listing-breadcrumb">
          <a href="/">Home</a>
          <span className="listing-breadcrumb__sep">/</span>
          <span>Products</span>
        </nav>

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
        />
      </section>
    </div>
  );
}
