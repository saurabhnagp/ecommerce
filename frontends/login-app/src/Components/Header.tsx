import { Link, useLocation } from "react-router-dom";
import logoImg from "../assets/images/logo.png";
import chevronDown from "../assets/images/chevronDown.png";
import search from "../assets/images/search.png";
import heart from "../assets/images/heart.png";
import bag from "../assets/images/bag.png";

function Header() {
  const { pathname } = useLocation();
  const isLoginPage = pathname === "/login";

  return (
    <header className="bg-white">
      {/* Top utility bar - light gray */}
      {/* <div className="bg-gray-100 border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 py-2 flex items-center justify-between text-sm text-gray-600">
          <div className="flex items-center gap-6">
            <a href="/ops" className="flex items-center gap-1.5 hover:text-blue-600">
              <img src={ICONS.globe} alt="" className="w-4 h-4" />
              Amazon /ops
            </a>
            <a href="/finance" className="flex items-center gap-1.5 hover:text-blue-600">
              <img src={ICONS.cart} alt="" className="w-4 h-4" />
              Finance items
            </a>
          </div>
          <div className="flex items-center gap-4">
            <span className="text-gray-300">|</span>
            <a href="/login" className="hover:text-blue-600">Log in</a>
            <span className="text-gray-300">|</span>
            <a href="/contact" className="hover:text-blue-600">Contact us</a>
            <span className="text-gray-300">|</span>
            <a href="/account" className="hover:text-blue-600">Account</a>
          </div>
        </div>
      </div> */}

      {/* Main header: logo, search, user/cart */}
      <div className="max-w-7xl mx-auto px-4 py-4 flex flex-wrap items-center gap-6">
        <Link to="/" className="flex items-center gap-2 shrink-0">
          <img src={logoImg} alt="AmCart" className="h-9 w-auto" />
          {/* <span className="text-xl font-bold text-blue-800">AmCart</span> */}
        </Link>
        <div className="flex-1 min-w-[200px] max-w-2xl">
          <div className="flex rounded-lg overflow-hidden border border-gray-300">
            <input
              type="search"
              placeholder="Search for products..."
              className="flex-1 px-4 py-2.5 text-gray-700 placeholder-gray-400 focus:outline-none focus:ring-0"
              aria-label="Search"
            />
            <button
              type="button"
              className="px-5 bg-blue-600 hover:bg-blue-700 transition-colors flex items-center justify-center"
              aria-label="Search"
            >
              <img src={search} alt="" className="w-5 h-5" />
            </button>
          </div>
        </div>
        <div className="flex items-center gap-6 shrink-0">
          {isLoginPage ? (
            <Link to="/register" className="text-sm font-medium text-gray-800 hover:text-blue-600">
              Register
            </Link>
          ) : (
            <Link to="/login" className="text-sm font-medium text-gray-800 hover:text-blue-600">
              Login
            </Link>
          )}
          <a href="/wishlist" className="text-gray-600 hover:text-blue-600" aria-label="Wishlist">
            <img src={heart} alt="" className="w-6 h-6" />
          </a>
          <a href="/cart" className="relative text-gray-600 hover:text-blue-600 inline-block" aria-label="Cart">
            <img src={bag} alt="" className="w-6 h-6" />
            <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-medium text-white">
              0
            </span>
          </a>
        </div>
      </div>

      {/* Bottom navigation */}
      <nav className="border-t border-gray-200" aria-label="Main navigation">
        <div className="max-w-7xl mx-auto px-4 py-3">
          <ul className="list-none p-0 m-0 flex flex-wrap items-center gap-6 text-sm text-gray-700">
            <li>
              <a href="/" className="flex items-center gap-1 hover:text-blue-600">
                Home
                <img src={chevronDown} alt="" className="w-4 h-4" />
              </a>
            </li>
            <li>
              <a href="/shopping" className="flex items-center gap-1 hover:text-blue-600">
                Shopping
                <img src={chevronDown} alt="" className="w-4 h-4" />
              </a>
            </li>
            <li>
              <a href="/chairbox" className="hover:text-blue-600">Chairbox</a>
            </li>
            <li>
              <a href="/gendims" className="hover:text-blue-600">Gendims</a>
            </li>
            <li>
              <a href="/sare" className="flex items-center gap-1 hover:text-blue-600">
                Sare
                <img src={chevronDown} alt="" className="w-4 h-4" />
              </a>
            </li>
            <li>
              <a href="/contact" className="hover:text-blue-600">Contact us</a>
            </li>
          </ul>
        </div>
      </nav>
    </header>
  );
}

export default Header;
