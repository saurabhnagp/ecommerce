import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { addCartItem } from "../api/cart";
import {
  fetchProductBySlug,
  fetchProductNeighbors,
  type ProductDto,
  type ProductImage,
  type ProductNeighborsDto,
} from "../api/products";
import {
  createProductReview,
  fetchProductReviews,
  voteOnProductReview,
  type ProductReviewDto,
} from "../api/reviews";
import {
  addWishlistItem,
  fetchWishlistProductIds,
  removeWishlistItem,
} from "../api/wishlist";
import { useCart } from "../cart/CartContext";
import { onAuthChange } from "../auth/notify";
import { getAccessToken, getStoredUser } from "../auth/storage";
import { productInStock, productIsLowStock } from "../utils/stock";
import "./ProductDetailPage.css";

const PLACEHOLDER =
  "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=800&h=800&fit=crop";

const REVIEW_PAGE_SIZE = 5;

function Stars({ count = 0 }: { count?: number }) {
  const n = Math.round(Math.min(5, Math.max(0, count)));
  return (
    <span className="product-detail__stars" aria-label={`${n} of 5 stars`}>
      {"★".repeat(n)}
      {"☆".repeat(5 - n)}
    </span>
  );
}

function sortImages(images: ProductImage[] | undefined): ProductImage[] {
  if (!images?.length) return [];
  return [...images].sort((a, b) => a.displayOrder - b.displayOrder);
}

function reviewerInitials(name: string | null | undefined) {
  const s = (name ?? "C").trim();
  const parts = s.split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "C";
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

export function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { refreshCart } = useCart();

  const [sessionRev, setSessionRev] = useState(0);
  const [wishlistIds, setWishlistIds] = useState<Set<string>>(() => new Set());
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [neighbors, setNeighbors] = useState<ProductNeighborsDto | null>(null);
  const [reviews, setReviews] = useState<ProductReviewDto[]>([]);
  const [reviewsTotal, setReviewsTotal] = useState(0);
  const [reviewsPage, setReviewsPage] = useState(1);
  const [reviewsLoading, setReviewsLoading] = useState(false);
  const [reviewsLoadError, setReviewsLoadError] = useState<string | null>(null);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewTitle, setReviewTitle] = useState("");
  const [reviewComment, setReviewComment] = useState("");
  const [reviewPhotoUrl, setReviewPhotoUrl] = useState("");
  const [reviewSubmitBusy, setReviewSubmitBusy] = useState(false);
  const [reviewBanner, setReviewBanner] = useState<{ type: "ok" | "err"; text: string } | null>(null);
  const [voteBusyId, setVoteBusyId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [mainIndex, setMainIndex] = useState(0);
  const [zoomOpen, setZoomOpen] = useState(false);
  const [qty, setQty] = useState(1);
  const [cartBusy, setCartBusy] = useState(false);
  const [copyHint, setCopyHint] = useState("");

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
    if (!slug?.trim()) {
      setLoading(false);
      setError("Product not found.");
      setProduct(null);
      return;
    }

    let cancelled = false;
    setLoading(true);
    setError("");
    setMainIndex(0);
    setQty(1);
    setNeighbors(null);
    setReviews([]);
    setReviewsTotal(0);
    setReviewsPage(1);
    setReviewBanner(null);
    setReviewsLoadError(null);

    (async () => {
      try {
        const res = await fetchProductBySlug(slug);
        const p = res.data;
        if (cancelled || !p) return;
        setProduct(p);
        document.title = `${p.name} · AmCart`;

        const nRes = await fetchProductNeighbors(p.id).catch(() => null);
        if (cancelled) return;
        if (nRes?.data) setNeighbors(nRes.data);
      } catch (e) {
        if (!cancelled) {
          setProduct(null);
          setError(e instanceof Error ? e.message : "Could not load product.");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
      document.title = "AmCart";
    };
  }, [slug]);

  useEffect(() => {
    if (!product?.id) return;
    let cancelled = false;
    setReviewsLoading(true);
    setReviewsLoadError(null);
    const token = getAccessToken();
    fetchProductReviews(product.id, reviewsPage, REVIEW_PAGE_SIZE, token)
      .then((r) => {
        if (!cancelled) {
          setReviews(r.items);
          setReviewsTotal(r.totalCount);
          setReviewsLoadError(null);
        }
      })
      .catch((e) => {
        if (!cancelled) {
          setReviews([]);
          setReviewsTotal(0);
          setReviewsLoadError(e instanceof Error ? e.message : "Could not load reviews.");
        }
      })
      .finally(() => {
        if (!cancelled) setReviewsLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [product?.id, reviewsPage, sessionRev]);

  const hasMyReview = useMemo(() => {
    const uid = getStoredUser()?.id;
    if (!uid) return false;
    return reviews.some((r) => r.userId.toLowerCase() === uid.toLowerCase());
  }, [reviews, sessionRev]);

  const reviewTotalPages = Math.max(1, Math.ceil(reviewsTotal / REVIEW_PAGE_SIZE));

  const orderedImages = useMemo(() => sortImages(product?.images), [product?.images]);
  const mainImage = orderedImages[mainIndex] ?? orderedImages[0];
  const mainUrl =
    mainImage?.url?.trim() || product?.primaryImageUrl?.trim() || PLACEHOLDER;

  const inStock = product ? productInStock(product) : false;
  const lowStock = product ? productIsLowStock(product) : false;
  const maxQty = useMemo(() => {
    if (!product) return 1;
    if (!product.trackInventory) return 99;
    const q = product.quantity ?? 0;
    return Math.max(1, Math.min(99, q));
  }, [product]);

  useEffect(() => {
    setQty((q) => Math.min(Math.max(1, q), maxQty));
  }, [maxQty]);

  const inWishlist = product ? wishlistIds.has(product.id) : false;

  const isSale =
    product &&
    product.compareAtPrice != null &&
    product.compareAtPrice > product.price;

  const handleWishlist = useCallback(async () => {
    if (!product) return;
    const t = getAccessToken();
    if (!t) {
      navigate("/login");
      return;
    }
    if (inWishlist) {
      await removeWishlistItem(product.id, t);
      setWishlistIds((prev) => {
        const next = new Set(prev);
        next.delete(product.id);
        return next;
      });
    } else {
      await addWishlistItem(product.id, t);
      setWishlistIds((prev) => new Set(prev).add(product.id));
    }
  }, [product, inWishlist, navigate]);

  const handleAddToCart = useCallback(async () => {
    if (!product || !inStock) return;
    setCartBusy(true);
    try {
      await addCartItem(product.id, qty);
      await refreshCart();
    } finally {
      setCartBusy(false);
    }
  }, [product, inStock, qty, refreshCart]);

  const handleSubmitReview = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();
      if (!product) return;
      const token = getAccessToken();
      if (!token) {
        navigate("/login");
        return;
      }
      const title = reviewTitle.trim();
      const comment = reviewComment.trim();
      if (!title && !comment) {
        setReviewBanner({ type: "err", text: "Add a title or a comment." });
        return;
      }
      setReviewSubmitBusy(true);
      setReviewBanner(null);
      try {
        await createProductReview(
          product.id,
          {
            rating: reviewRating,
            title: title || undefined,
            comment: comment || undefined,
            reviewerPhotoUrl: reviewPhotoUrl.trim() || undefined,
          },
          token
        );
        setReviewTitle("");
        setReviewComment("");
        setReviewPhotoUrl("");
        setReviewRating(5);
        setReviewBanner({ type: "ok", text: "Thanks! Your review was posted." });
        setReviewsPage(1);
        const r = await fetchProductReviews(product.id, 1, REVIEW_PAGE_SIZE, token);
        setReviews(r.items);
        setReviewsTotal(r.totalCount);
      } catch (err) {
        setReviewBanner({
          type: "err",
          text: err instanceof Error ? err.message : "Could not submit review.",
        });
      } finally {
        setReviewSubmitBusy(false);
      }
    },
    [product, navigate, reviewRating, reviewTitle, reviewComment, reviewPhotoUrl]
  );

  const handleVote = useCallback(
    async (reviewId: string, action: "like" | "dislike") => {
      if (!product) return;
      const token = getAccessToken();
      if (!token) {
        navigate("/login");
        return;
      }
      setVoteBusyId(reviewId);
      try {
        const updated = await voteOnProductReview(product.id, reviewId, action, token);
        setReviews((prev) => prev.map((x) => (x.id === updated.id ? updated : x)));
      } catch {
        /* ignore */
      } finally {
        setVoteBusyId(null);
      }
    },
    [product, navigate]
  );

  const share = useCallback(async () => {
    const url = window.location.href;
    try {
      if (navigator.share) {
        await navigator.share({ title: product?.name ?? "Product", url });
      } else {
        await navigator.clipboard.writeText(url);
        setCopyHint("Link copied");
        window.setTimeout(() => setCopyHint(""), 2000);
      }
    } catch {
      /* user cancelled share sheet */
    }
  }, [product?.name]);

  const printPage = useCallback(() => {
    window.print();
  }, []);

  const hasToken = !!getAccessToken();

  if (loading) {
    return (
      <div className="product-detail product-detail--loading">
        <p className="muted">Loading product…</p>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="product-detail product-detail--error">
        <p>{error || "Product not found."}</p>
        <Link to="/products" className="btn-outline">
          Back to shop
        </Link>
      </div>
    );
  }

  const stockLabel = !product.trackInventory
    ? "In stock"
    : inStock
      ? lowStock
        ? "Low stock"
        : "In stock"
      : "Out of stock";

  return (
    <div className="product-detail" id="product-detail-print-root">
      <nav className="product-detail__breadcrumb" aria-label="Breadcrumb">
        <Link to="/">Home</Link>
        <span className="product-detail__bc-sep">/</span>
        {product.categorySlug && product.categoryName ? (
          <>
            <Link to={`/category/${encodeURIComponent(product.categorySlug)}`}>
              {product.categoryName}
            </Link>
            <span className="product-detail__bc-sep">/</span>
          </>
        ) : null}
        <span className="product-detail__bc-current">{product.name}</span>
      </nav>

      <div className="product-detail__grid">
        <div className="product-detail__gallery">
          <button
            type="button"
            className="product-detail__main-img-btn"
            onClick={() => setZoomOpen(true)}
            aria-label="Open image zoom"
          >
            <img
              className="product-detail__main-img"
              src={mainUrl}
              alt={mainImage?.altText || product.name}
            />
            <span className="product-detail__zoom-hint">Click to zoom</span>
          </button>
          {orderedImages.length > 1 && (
            <div className="product-detail__thumbs" role="list">
              {orderedImages.map((im, i) => (
                <button
                  key={im.id ?? `${im.url}-${i}`}
                  type="button"
                  role="listitem"
                  className={`product-detail__thumb${i === mainIndex ? " product-detail__thumb--active" : ""}`}
                  onClick={() => setMainIndex(i)}
                >
                  <img src={im.url} alt="" />
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="product-detail__info">
          <h1 className="product-detail__title">{product.name}</h1>
          {product.brandName && (
            <p className="product-detail__brand">{product.brandName}</p>
          )}

          <div className="product-detail__rating-row">
            {product.averageRating > 0 && (
              <>
                <Stars count={product.averageRating} />
                <span className="muted">
                  {product.averageRating.toFixed(1)} ({product.reviewCount} reviews)
                </span>
              </>
            )}
          </div>

          <div className="product-detail__price-row">
            {isSale && (
              <span className="product-detail__price-compare">
                &#8377;{" "}
                {product.compareAtPrice!.toLocaleString("en-IN", {
                  minimumFractionDigits: 2,
                })}
              </span>
            )}
            <span className="product-detail__price">
              &#8377;{" "}
              {product.price.toLocaleString("en-IN", {
                minimumFractionDigits: 2,
              })}
            </span>
            {product.currency && product.currency !== "INR" && (
              <span className="muted product-detail__currency">({product.currency})</span>
            )}
          </div>

          <p
            className={`product-detail__stock${inStock ? "" : " product-detail__stock--out"}${lowStock && inStock ? " product-detail__stock--low" : ""}`}
          >
            {stockLabel}
            {product.trackInventory && inStock && product.quantity != null && (
              <span className="muted"> · {product.quantity} available</span>
            )}
          </p>

          {product.shortDescription && (
            <p className="product-detail__short">{product.shortDescription}</p>
          )}

          <div className="product-detail__actions product-detail__actions-no-print">
            <div className="product-detail__qty">
              <label htmlFor="pd-qty">Qty</label>
              <input
                id="pd-qty"
                type="number"
                min={1}
                max={maxQty}
                value={qty}
                disabled={!inStock}
                onChange={(e) => {
                  const v = Number(e.target.value);
                  if (Number.isFinite(v))
                    setQty(Math.min(maxQty, Math.max(1, Math.floor(v))));
                }}
              />
            </div>
            <button
              type="button"
              className="btn-primary product-detail__add"
              disabled={!inStock || cartBusy}
              onClick={() => void handleAddToCart()}
            >
              {cartBusy ? "Adding…" : "Add to cart"}
            </button>
            <button
              type="button"
              className="btn-outline"
              onClick={() => void handleWishlist()}
            >
              {inWishlist ? "♥ Saved" : "♡ Wishlist"}
            </button>
          </div>

          <div className="product-detail__secondary-actions product-detail__actions-no-print">
            <button type="button" className="btn-outline" onClick={() => void share()}>
              Share
            </button>
            <button type="button" className="btn-outline" onClick={printPage}>
              Print
            </button>
            {copyHint && <span className="product-detail__toast">{copyHint}</span>}
          </div>

          <div className="product-detail__neighbors product-detail__actions-no-print">
            {neighbors?.previous && (
              <Link
                className="product-detail__neighbor product-detail__neighbor--prev"
                to={`/products/${encodeURIComponent(neighbors.previous.slug)}`}
              >
                ← {neighbors.previous.name}
              </Link>
            )}
            {neighbors?.next && (
              <Link
                className="product-detail__neighbor product-detail__neighbor--next"
                to={`/products/${encodeURIComponent(neighbors.next.slug)}`}
              >
                {neighbors.next.name} →
              </Link>
            )}
          </div>

          <p className="product-detail__sku muted">SKU: {product.sku}</p>
        </div>
      </div>

      {product.attributes && product.attributes.length > 0 && (
        <section className="product-detail__section">
          <h2>Features</h2>
          <table className="product-detail__attrs">
            <tbody>
              {[...product.attributes]
                .sort((a, b) => a.displayOrder - b.displayOrder)
                .map((a) => (
                  <tr key={a.id}>
                    <th scope="row">{a.name}</th>
                    <td>{a.value}</td>
                  </tr>
                ))}
            </tbody>
          </table>
        </section>
      )}

      {product.variants && product.variants.length > 0 && (
        <section className="product-detail__section">
          <h2>Variants</h2>
          <ul className="product-detail__variants">
            {product.variants.map((v) => (
              <li key={v.id}>
                <strong>{v.name}</strong> — &#8377;{" "}
                {v.price.toLocaleString("en-IN", { minimumFractionDigits: 2 })}{" "}
                <span className="muted">({v.sku})</span>
              </li>
            ))}
          </ul>
        </section>
      )}

      {product.description?.trim() && (
        <section className="product-detail__section">
          <h2>Details</h2>
          <div className="product-detail__description">
            {product.description.split(/\n\s*\n/).map((para, i) => (
              <p key={i}>{para.trim()}</p>
            ))}
          </div>
        </section>
      )}

      {product.tagNames && product.tagNames.length > 0 && (
        <section className="product-detail__section product-detail__tags">
          <h2>Tags</h2>
          <p>{product.tagNames.join(" · ")}</p>
        </section>
      )}

      <section className="product-detail__section product-detail__section--reviews">
        <h2>Reviews {reviewsTotal > 0 && <span className="muted">({reviewsTotal})</span>}</h2>

        {reviewBanner && (
          <p
            className={
              reviewBanner.type === "ok"
                ? "product-detail__review-banner product-detail__review-banner--success"
                : "product-detail__review-banner product-detail__review-banner--error"
            }
            role="status"
          >
            {reviewBanner.text}
          </p>
        )}

        {reviewsLoadError && (
          <p className="product-detail__review-banner product-detail__review-banner--error" role="alert">
            {reviewsLoadError}
          </p>
        )}

        {!hasToken ? (
          <p className="muted product-detail__review-hint">
            <Link to="/login">Sign in</Link> to write a review or vote on helpfulness.
          </p>
        ) : hasMyReview ? (
          <p className="muted product-detail__review-hint">You have already reviewed this product.</p>
        ) : (
          <form className="product-detail__review-form" onSubmit={(e) => void handleSubmitReview(e)}>
            <h3 className="product-detail__review-form-title">Write a review</h3>
            <div className="product-detail__review-form-row">
              <label htmlFor="pd-review-rating">Rating</label>
              <select
                id="pd-review-rating"
                value={reviewRating}
                onChange={(e) => setReviewRating(+e.target.value)}
              >
                {[5, 4, 3, 2, 1].map((n) => (
                  <option key={n} value={n}>
                    {n} stars
                  </option>
                ))}
              </select>
            </div>
            <div className="product-detail__review-form-row">
              <label htmlFor="pd-review-title">Title (optional)</label>
              <input
                id="pd-review-title"
                value={reviewTitle}
                onChange={(e) => setReviewTitle(e.target.value)}
                maxLength={300}
                placeholder="Short summary"
              />
            </div>
            <div className="product-detail__review-form-row">
              <label htmlFor="pd-review-comment">Comment (optional if title set)</label>
              <textarea
                id="pd-review-comment"
                value={reviewComment}
                onChange={(e) => setReviewComment(e.target.value)}
                maxLength={5000}
                rows={4}
                placeholder="Share your experience"
              />
            </div>
            <div className="product-detail__review-form-row">
              <label htmlFor="pd-review-photo">Profile photo URL (optional)</label>
              <input
                id="pd-review-photo"
                value={reviewPhotoUrl}
                onChange={(e) => setReviewPhotoUrl(e.target.value)}
                placeholder="https://..."
              />
            </div>
            <button type="submit" className="btn-primary" disabled={reviewSubmitBusy}>
              {reviewSubmitBusy ? "Submitting…" : "Submit review"}
            </button>
          </form>
        )}

        {reviewsLoading ? (
          <p className="muted">Loading reviews…</p>
        ) : reviewsLoadError ? null : reviews.length === 0 ? (
          <p className="muted">No reviews yet.</p>
        ) : (
          <>
            <ul className="product-detail__reviews">
              {reviews.map((r) => (
                <li key={r.id} className="product-detail__review">
                  <div className="product-detail__review-meta">
                    {r.reviewerPhotoUrl?.trim() ? (
                      <img
                        className="product-detail__review-avatar"
                        src={r.reviewerPhotoUrl.trim()}
                        alt=""
                      />
                    ) : (
                      <span className="product-detail__review-avatar product-detail__review-avatar--text">
                        {reviewerInitials(r.reviewerDisplayName)}
                      </span>
                    )}
                    <div className="product-detail__review-who">
                      <span className="product-detail__review-name">
                        {r.reviewerDisplayName?.trim() || "Customer"}
                      </span>
                      <time dateTime={r.createdAt}>
                        {new Date(r.createdAt).toLocaleString(undefined, {
                          dateStyle: "medium",
                          timeStyle: "short",
                        })}
                      </time>
                    </div>
                  </div>
                  <div className="product-detail__review-head">
                    <Stars count={r.rating} />
                  </div>
                  {r.title && <p className="product-detail__review-title">{r.title}</p>}
                  {r.comment && <p className="product-detail__review-comment">{r.comment}</p>}
                  {r.isVerifiedPurchase && (
                    <span className="product-detail__verified">Verified purchase</span>
                  )}
                  <div className="product-detail__review-votes">
                    <button
                      type="button"
                      className={`btn-outline product-detail__vote${r.myVote === "like" ? " product-detail__vote--active" : ""}`}
                      disabled={!hasToken || voteBusyId === r.id}
                      onClick={() => void handleVote(r.id, "like")}
                    >
                      Helpful ({r.helpfulCount})
                    </button>
                    <button
                      type="button"
                      className={`btn-outline product-detail__vote${r.myVote === "dislike" ? " product-detail__vote--active" : ""}`}
                      disabled={!hasToken || voteBusyId === r.id}
                      onClick={() => void handleVote(r.id, "dislike")}
                    >
                      Not helpful ({r.notHelpfulCount})
                    </button>
                  </div>
                </li>
              ))}
            </ul>
            {reviewTotalPages > 1 && (
              <div className="product-detail__review-pager">
                <button
                  type="button"
                  className="btn-outline"
                  disabled={reviewsPage <= 1}
                  onClick={() => setReviewsPage((p) => Math.max(1, p - 1))}
                >
                  Previous
                </button>
                <span className="muted">
                  Page {reviewsPage} of {reviewTotalPages}
                </span>
                <button
                  type="button"
                  className="btn-outline"
                  disabled={reviewsPage >= reviewTotalPages}
                  onClick={() => setReviewsPage((p) => p + 1)}
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </section>

      {zoomOpen && (
        <button
          type="button"
          className="product-detail__lightbox"
          onClick={() => setZoomOpen(false)}
          aria-label="Close zoom"
        >
          <img src={mainUrl} alt="" />
        </button>
      )}
    </div>
  );
}
