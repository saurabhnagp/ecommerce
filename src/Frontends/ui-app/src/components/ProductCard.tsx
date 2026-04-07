import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import type { ProductDto } from "../api/products";
import { getAccessToken } from "../auth/storage";
import { productInStock, productIsLowStock } from "../utils/stock";
import "./ProductCard.css";

type Props = {
  product: ProductDto;
  wishlistProductIds?: Set<string>;
  onWishlistToggle?: (productId: string, nextInWishlist: boolean) => void | Promise<void>;
  onAddToCart?: (productId: string, quantity?: number) => void | Promise<void>;
};

function Stars({ count = 0 }: { count?: number }) {
  return (
    <span className="product-card__stars">
      {"★".repeat(Math.round(count))}
      {"☆".repeat(5 - Math.round(count))}
    </span>
  );
}

const PLACEHOLDER =
  "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&h=460&fit=crop";

export function ProductCard({
  product,
  wishlistProductIds,
  onWishlistToggle,
  onAddToCart,
}: Props) {
  const navigate = useNavigate();
  const [cartBusy, setCartBusy] = useState(false);
  const inWishlist = wishlistProductIds?.has(product.id) ?? false;
  const detailPath = `/products/${encodeURIComponent(product.slug)}`;

  const imgs = product.images ?? [];
  const primaryImg = imgs.find((i) => i.isPrimary) ?? imgs[0];
  const imgUrl =
    product.primaryImageUrl?.trim() || primaryImg?.url || PLACEHOLDER;

  const isSale =
    product.compareAtPrice != null && product.compareAtPrice > product.price;

  const discount = isSale
    ? Math.round(
        ((product.compareAtPrice! - product.price) / product.compareAtPrice!) *
          100
      )
    : 0;

  const badge = isSale ? `- ${discount}%` : product.isFeatured ? "NEW" : null;

  const inStock = productInStock(product);
  const lowStock = productIsLowStock(product);
  const stockTitle = !product.trackInventory
    ? "In stock"
    : inStock
      ? lowStock
        ? "Low stock"
        : "In stock"
      : "Out of stock";

  async function handleWishlistClick(e: React.MouseEvent) {
    e.preventDefault();
    e.stopPropagation();
    if (!getAccessToken()) {
      navigate("/login");
      return;
    }
    await onWishlistToggle?.(product.id, !inWishlist);
  }

  async function handleAddToCartClick(e: React.MouseEvent) {
    e.preventDefault();
    e.stopPropagation();
    if (!inStock) return;
    if (!onAddToCart) return;
    setCartBusy(true);
    try {
      await onAddToCart(product.id, 1);
    } finally {
      setCartBusy(false);
    }
  }

  return (
    <div className="product-card">
      <div className="product-card__img-wrap">
        <Link to={detailPath} className="product-card__img-link" aria-label={product.name}>
          <img
            className="product-card__img"
            src={imgUrl}
            alt={product.name}
            loading="lazy"
          />
        </Link>

        {product.trackInventory === true && (
          <span
            className={`product-card__stock-dot${inStock ? (lowStock ? " product-card__stock-dot--low" : " product-card__stock-dot--in") : " product-card__stock-dot--out"}`}
            title={stockTitle}
            aria-label={stockTitle}
          />
        )}

        {badge && (
          <span
            className={`product-card__badge${isSale ? " product-card__badge--sale" : ""}`}
          >
            {badge}
          </span>
        )}

        <div className="product-card__actions">
          <button
            type="button"
            className="product-card__action-btn"
            title="Quick view"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              navigate(detailPath);
            }}
          >
            &#128065;
          </button>
          <button
            type="button"
            className={`product-card__action-btn${inWishlist ? " product-card__action-btn--wishlist-on" : ""}`}
            title={inWishlist ? "Remove from wishlist" : "Add to wishlist"}
            aria-pressed={inWishlist}
            onClick={handleWishlistClick}
          >
            {inWishlist ? "\u2665" : "\u2661"}
          </button>
          <button
            type="button"
            className="product-card__action-btn"
            title={inStock ? "Add to cart" : "Out of stock"}
            disabled={cartBusy || !inStock}
            onClick={handleAddToCartClick}
          >
            &#128722;
          </button>
        </div>
      </div>

      <div className="product-card__info">
        <div className="product-card__price">
          {isSale && (
            <span className="product-card__price--original">
              &#8377;{" "}
              {product.compareAtPrice!.toLocaleString("en-IN", {
                minimumFractionDigits: 2,
              })}
            </span>
          )}
          &#8377;{" "}
          {product.price.toLocaleString("en-IN", {
            minimumFractionDigits: 2,
          })}
        </div>
        <Link to={detailPath} className="product-card__name-link">
          <div className="product-card__name">{product.name}</div>
        </Link>
        {product.trackInventory === true && !inStock && (
          <div className="product-card__oos">Out of stock</div>
        )}
        {product.trackInventory === true && inStock && lowStock && (
          <div className="product-card__low-stock">Low stock</div>
        )}
        {product.averageRating > 0 && <Stars count={product.averageRating} />}
      </div>
    </div>
  );
}
