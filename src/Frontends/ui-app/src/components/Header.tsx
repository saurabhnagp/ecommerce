import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useCart } from "../cart/CartContext";
import { getStoredUser } from "../auth/storage";
import { UserMenuDropdown } from "./UserMenuDropdown";
import "./Header.css";

type Props = { signedIn: boolean; onAuthRefresh: () => void };

const WOMEN_CATS: Record<string, string[]> = {
  "Indian & Western Wear": [
    "Kurtas & Suits",
    "Kurtis & Tunics",
    "Leggings & Salwars",
    "Skirts & Palazzos",
    "Sarees & Blouses",
    "Dress Material",
    "Lehenga Choli",
    "Dupattas & Shawls",
  ],
  "Western Wear": [
    "Dresses & Jumpsuits",
    "Tops & T-Shirts",
    "Jeans & Jeggings",
    "Trousers & Capris",
    "Shorts & Skirts",
    "Shrugs",
    "Sweaters & Sweatshirts",
    "Jackets & Waistcoats",
  ],
  Accessories: ["Watches", "Sunglasses", "Eye Glasses", "Belts"],
};

const MEN_CATS: Record<string, string[]> = {
  Clothing: [
    "T-Shirts",
    "Casual Shirts",
    "Formal Shirts",
    "Suits",
    "Jeans",
    "Casual Trousers",
    "Formal Trousers",
    "Shorts",
    "Track Pants",
    "Sweaters & Sweatshirts",
    "Jackets",
    "Blazers & Coats",
  ],
  Accessories: [
    "Watches & Wearables",
    "Sunglasses & Frames",
    "Bags & Backpacks",
    "Wallets & Belts",
    "Fashion Accessories",
  ],
};

function MegaMenu({ cats }: { cats: Record<string, string[]> }) {
  return (
    <div className="mega-menu">
      {Object.entries(cats).map(([group, items]) => (
        <div key={group} className="mega-menu__col">
          <h4 className="mega-menu__title">{group}</h4>
          <ul className="mega-menu__list">
            {items.map((item) => (
              <li key={item}>
                <Link
                  to={`/category/${encodeURIComponent(item.toLowerCase().replace(/\s+/g, "-"))}`}
                  className="mega-menu__link"
                >
                  {item}
                </Link>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  );
}

export function Header({ signedIn, onAuthRefresh }: Props) {
  const navigate = useNavigate();
  const { cart, loading: cartLoading } = useCart();
  const [query, setQuery] = useState("");

  const cartCount = cart?.items.reduce((n, i) => n + i.quantity, 0) ?? 0;
  const cartSym = cart?.currency === "USD" ? "$" : "₹";
  const cartTotal = cart?.total ?? 0;
  const cartTotalFmt = cartTotal.toLocaleString(cart?.currency === "USD" ? "en-US" : "en-IN", {
    minimumFractionDigits: 2,
  });

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    const q = query.trim();
    if (q) navigate(`/search?q=${encodeURIComponent(q)}`);
  }

  return (
    <header className="site-header">
      {/* ——— Top Bar ——— */}
      <div className="top-bar">
        <div className="top-bar__inner">
          <div className="top-bar__left">
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
          <form className="search-box" onSubmit={handleSearch}>
            <input
              type="text"
              className="search-box__input"
              placeholder="Search products..."
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              aria-label="Search products"
            />
            <button type="submit" className="search-box__btn" aria-label="Search">
              &#x1F50D;
            </button>
          </form>

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
          <li className="nav-bar__item nav-bar__item--mega">
            <span className="nav-bar__link">
              Women <span className="nav-bar__caret">&#9662;</span>
            </span>
            <MegaMenu cats={WOMEN_CATS} />
          </li>
          <li className="nav-bar__item nav-bar__item--mega">
            <span className="nav-bar__link">
              Men <span className="nav-bar__caret">&#9662;</span>
            </span>
            <MegaMenu cats={MEN_CATS} />
          </li>
          <li className="nav-bar__item">
            <Link to="/new-products" className="nav-bar__link">
              New Products
            </Link>
          </li>
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
          <li className="nav-bar__item">
            <Link to="/sale" className="nav-bar__link nav-bar__link--sale">
              Sale
            </Link>
          </li>
        </ul>
      </nav>
    </header>
  );
}
