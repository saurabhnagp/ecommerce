import "./ProductCard.css";

export type Product = {
  id: string;
  name: string;
  price: number;
  originalPrice?: number;
  image: string;
  badge?: string;
  rating?: number;
  tab: "new" | "featured" | "special" | "bestsellers";
};

type Props = { product: Product };

function Stars({ count = 0 }: { count?: number }) {
  return (
    <span className="product-card__stars">
      {"★".repeat(Math.round(count))}
      {"☆".repeat(5 - Math.round(count))}
    </span>
  );
}

export function ProductCard({ product }: Props) {
  const isSale = product.originalPrice && product.originalPrice > product.price;

  return (
    <div className="product-card">
      <div className="product-card__img-wrap">
        <img
          className="product-card__img"
          src={product.image}
          alt={product.name}
          loading="lazy"
        />

        {product.badge && (
          <span
            className={`product-card__badge${isSale ? " product-card__badge--sale" : ""}`}
          >
            {product.badge}
          </span>
        )}

        <div className="product-card__actions">
          <button className="product-card__action-btn" title="Quick view">
            &#128065;
          </button>
          <button className="product-card__action-btn" title="Add to wishlist">
            &#9825;
          </button>
          <button className="product-card__action-btn" title="Add to cart">
            &#128722;
          </button>
        </div>
      </div>

      <div className="product-card__info">
        <div className="product-card__price">
          {isSale && (
            <span className="product-card__price--original">
              &#8377; {product.originalPrice!.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
            </span>
          )}
          &#8377; {product.price.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
        </div>
        <div className="product-card__name">{product.name}</div>
        {product.rating != null && <Stars count={product.rating} />}
      </div>
    </div>
  );
}
