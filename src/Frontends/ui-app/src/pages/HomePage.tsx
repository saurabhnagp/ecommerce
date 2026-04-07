import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { addCartItem } from "../api/cart";
import {
  fetchFeaturedProducts,
  fetchPublicProducts,
  type ProductDto,
} from "../api/products";
import {
  addWishlistItem,
  fetchWishlistProductIds,
  removeWishlistItem,
} from "../api/wishlist";
import { useCart } from "../cart/CartContext";
import { ProductGrid } from "../components/ProductGrid";
import { TestimonialsSection } from "../components/TestimonialsSection";
import { onAuthChange } from "../auth/notify";
import { getAccessToken } from "../auth/storage";
import "./HomePage.css";

const HOME_GRID_COUNT = 12;

export function HomePage() {
  const { refreshCart } = useCart();
  const [sessionRev, setSessionRev] = useState(0);
  const [wishlistIds, setWishlistIds] = useState<Set<string>>(() => new Set());

  const [products, setProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [sectionTitle, setSectionTitle] = useState("Featured products");
  const [loadError, setLoadError] = useState("");

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

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setLoadError("");
    (async () => {
      try {
        const featuredRes = await fetchFeaturedProducts(HOME_GRID_COUNT);
        let list: ProductDto[] = featuredRes.data ?? [];
        if (list.length > 0) {
          if (!cancelled) setSectionTitle("Featured products");
        } else {
          const paged = await fetchPublicProducts({
            page: 1,
            pageSize: HOME_GRID_COUNT,
            sortBy: "createdAt",
            sortDesc: true,
          });
          list = paged.data.items;
          if (!cancelled) setSectionTitle("New arrivals");
        }
        if (!cancelled) setProducts(list);
      } catch (e) {
        if (!cancelled) {
          setProducts([]);
          setLoadError(e instanceof Error ? e.message : "Could not load products.");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

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

  return (
    <div className="home-page">
      <section className="home-page__products" aria-labelledby="home-products-heading">
        <div className="home-page__products-head">
          <h2 id="home-products-heading" className="home-page__title">
            {sectionTitle}
          </h2>
          <Link to="/products" className="home-page__shop-all">
            Shop all →
          </Link>
        </div>
        {loadError && <p className="home-page__err">{loadError}</p>}
        <ProductGrid
          products={products}
          totalCount={products.length}
          page={1}
          totalPages={1}
          onPageChange={() => {}}
          loading={loading}
          showListingMeta={false}
          wishlistProductIds={wishlistIds}
          onWishlistToggle={handleWishlistToggle}
          onAddToCart={handleAddToCart}
        />
      </section>

      <TestimonialsSection />
    </div>
  );
}
