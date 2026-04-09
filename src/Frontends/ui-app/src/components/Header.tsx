import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { fetchPublicCategories, type CategoryDto } from "../api/products";
import { useCart } from "../cart/CartContext";
import { getStoredUser } from "../auth/storage";
import { NavMegaMenu } from "./NavMegaMenu";
import { UserMenuDropdown } from "./UserMenuDropdown";
import "./Header.css";

type Props = { signedIn: boolean; onAuthRefresh: () => void };

export function Header({ signedIn, onAuthRefresh }: Props) {
  const navigate = useNavigate();
  const { cart, loading: cartLoading } = useCart();
  const [query, setQuery] = useState("");
  const [searchError, setSearchError] = useState("");
  const [categoryRoots, setCategoryRoots] = useState<CategoryDto[]>([]);
  const [catNavLoading, setCatNavLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const res = await fetchPublicCategories();
        if (!cancelled) setCategoryRoots(res.data ?? []);
      } catch {
        if (!cancelled) setCategoryRoots([]);
      } finally {
        if (!cancelled) setCatNavLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const cartCount = cart?.items.reduce((n, i) => n + i.quantity, 0) ?? 0;
  const cartSym = cart?.currency === "USD" ? "$" : "₹";
  const cartTotal = cart?.total ?? 0;
  const cartTotalFmt = cartTotal.toLocaleString(cart?.currency === "USD" ? "en-US" : "en-IN", {
    minimumFractionDigits: 2,
  });

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    const q = query.trim();
    if (!q) {
      setSearchError("Please enter a search term.");
      return;
    }
    setSearchError("");
    navigate(`/search?q=${encodeURIComponent(q)}`);
  }

  return (
    <header className="site-header">
      {/* ——— Top Bar ——— */}
      <div className="top-bar">
        <div className="top-bar__inner">
          <div className="top-bar__left">
            7a85e16b-df0a-4f51-a713-3dbd51d15c0d
            {signedIn && (() => {
              const user = getStoredUser();
              return (
                <>
                  {user?.phone && (
                    <>
                      <span className="top-bar__item">{user.phone}</span>
                      <span className="top-bar__sep">|</span>
                    </>
                  )}
                  <span className="top-bar__item">{user?.email ?? ""}</span>
                </>
              );
            })()}
          </div>
          <div className="top-bar__right">
            {signedIn ? (
              <>
                <UserMenuDropdown onLogout={onAuthRefresh} />
                <span className="top-bar__sep">|</span>
                <Link to="/wishlist" className="top-bar__link">
                  Wishlist
                </Link>
              </>
            ) : (
              <>
                <Link to="/login" className="top-bar__link">
                  Login
                </Link>
                <span className="top-bar__sep">|</span>
                <Link to="/register" className="top-bar__link">
                  Register
                </Link>
              </>
            )}
          </div>
        </div>
      </div>

      {/* ——— Middle Bar ——— */}
      <div className="mid-bar">
        <div className="mid-bar__inner">
          <div className="search-box-wrap">
            <form className="search-box" onSubmit={handleSearch}>
              <input
                type="text"
                className="search-box__input"
                placeholder="Search products..."
                value={query}
                onChange={(e) => {
                  setQuery(e.target.value);
                  if (searchError) setSearchError("");
                }}
                aria-label="Search products"
                aria-invalid={!!searchError}
                aria-describedby={searchError ? "header-search-error" : undefined}
              />
              <button type="submit" className="search-box__btn" aria-label="Search">
                &#x1F50D;
              </button>
            </form>
            {searchError && (
              <p id="header-search-error" className="search-box__error" role="alert">
                {searchError}
              </p>
            )}
          </div>

          <Link to="/" className="mid-bar__logo">
            AmCart
          </Link>

          <Link to="/cart" className="cart-widget">
            <span className="cart-widget__icon">&#x1F6D2;</span>
            <span className="cart-widget__text">
              {cartLoading ? (
                "…"
              ) : (
                <>
                  {cartCount} item(s) / <b>{cartSym}</b> <b>{cartTotalFmt}</b>
                </>
              )}
            </span>
            <span className="cart-widget__arrow">&rarr;</span>
          </Link>
        </div>
      </div>

      {/* ——— Navigation Bar ——— */}
      <nav className="nav-bar" aria-label="Main navigation">
        <ul className="nav-bar__list">
          <li className="nav-bar__item">
            <Link to="/" className="nav-bar__link">
              Home
            </Link>
          </li>
          {catNavLoading ? (
            <li className="nav-bar__item">
              <span className="nav-bar__link nav-bar__link--muted">Categories…</span>
            </li>
          ) : (
            categoryRoots.map((root) => (
              <li key={root.id} className="nav-bar__item nav-bar__item--mega">
                <span className="nav-bar__link">
                  {root.name} <span className="nav-bar__caret">&#9662;</span>
                </span>
                <NavMegaMenu root={root} />
              </li>
            ))
          )}
          <li className="nav-bar__item">
            <Link to="/popular" className="nav-bar__link">
              Popular
            </Link>
          </li>
          <li className="nav-bar__item">
            <Link to="/contact" className="nav-bar__link">
              Contact Us
            </Link>
          </li>
        </ul>
      </nav>
    </header>
  );
}
