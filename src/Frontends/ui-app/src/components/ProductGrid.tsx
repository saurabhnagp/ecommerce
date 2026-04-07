import { ProductCard } from "./ProductCard";
import type { ProductDto } from "../api/products";
import "./ProductGrid.css";

type Props = {
  products: ProductDto[];
  totalCount: number;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  loading?: boolean;
  /** When false, hide pagination and “Showing X of Y” (e.g. home featured strip). */
  showListingMeta?: boolean;
  wishlistProductIds?: Set<string>;
  onWishlistToggle?: (productId: string, nextInWishlist: boolean) => void | Promise<void>;
  onAddToCart?: (productId: string, quantity?: number) => void | Promise<void>;
  /** Shown when not loading and there are no products (default: "No products found."). */
  emptyMessage?: string;
};

export function ProductGrid({
  products,
  totalCount,
  page,
  totalPages,
  onPageChange,
  loading,
  showListingMeta = true,
  wishlistProductIds,
  onWishlistToggle,
  onAddToCart,
  emptyMessage = "No products found.",
}: Props) {
  return (
    <div>
      {/* Page arrows */}
      {showListingMeta && totalPages > 1 && (
        <div className="pgrid-nav">
          <button
            className="pgrid-nav__btn"
            disabled={page <= 1}
            onClick={() => onPageChange(page - 1)}
          >
            &#8249;
          </button>
          <span className="pgrid-nav__page">
            {page} / {totalPages}
          </span>
          <button
            className="pgrid-nav__btn"
            disabled={page >= totalPages}
            onClick={() => onPageChange(page + 1)}
          >
            &#8250;
          </button>
        </div>
      )}

      {/* Info bar */}
      {showListingMeta && (
        <div className="pgrid-info">
          <span>
            Showing{" "}
            <span className="pgrid-info__count">
              {products.length} of {totalCount}
            </span>{" "}
            products
          </span>
        </div>
      )}

      {/* Grid */}
      <div className="pgrid-grid">
        {loading ? (
          <p className="pgrid-empty">Loading products…</p>
        ) : products.length > 0 ? (
          products.map((p) => (
            <ProductCard
              key={p.id}
              product={p}
              wishlistProductIds={wishlistProductIds}
              onWishlistToggle={onWishlistToggle}
              onAddToCart={onAddToCart}
            />
          ))
        ) : (
          <p className="pgrid-empty">{emptyMessage}</p>
        )}
      </div>
    </div>
  );
}
