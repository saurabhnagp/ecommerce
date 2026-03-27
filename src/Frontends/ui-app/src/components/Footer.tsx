import { useState } from "react";
import { Link } from "react-router-dom";
import { subscribeNewsletter } from "../api/newsletter";
import "./Footer.css";

// Re-add when "Lower Footer" section is uncommented:
// import { Link } from "react-router-dom";
// const PRODUCT_TAGS = ["Men","Women","Watches","Shoes","Jackets","Shirts","Jeans","Accessories","Sunglasses"];

const PAYMENT_LABELS = ["VISA", "RUPAY", "UPI", "MASTER", "PAYPAL"];

export function Footer() {
  const [nlEmail, setNlEmail] = useState("");
  const [nlBusy, setNlBusy] = useState(false);
  const [nlMsg, setNlMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  async function handleSubscribe(e: React.FormEvent) {
    e.preventDefault();
    setNlMsg(null);
    setNlBusy(true);
    try {
      await subscribeNewsletter(nlEmail.trim());
      setNlMsg({ type: "success", text: "Subscribed successfully!" });
      setNlEmail("");
    } catch (err) {
      setNlMsg({
        type: "error",
        text: err instanceof Error ? err.message : "Subscription failed. Please try again.",
      });
    } finally {
      setNlBusy(false);
    }
  }

  return (
    <footer className="site-footer">
      {/* ——— Upper Footer ——— */}
      <div className="footer-upper">
        <div className="footer-upper__inner">
          <div className="footer-about">
            <h3 className="footer-heading">About Us</h3>
            <p className="footer-about__text">
              AmCart is your one-stop destination for fashion and lifestyle products. We bring you the
              latest trends in men&rsquo;s and women&rsquo;s clothing, accessories, and more
              all at competitive prices with a seamless shopping experience.
            </p>
          </div>

          <div className="footer-contact">
            <h3 className="footer-heading">Contact Info</h3>
            <ul className="contact-list">
              <li>
                <span className="contact-list__icon">&#x1F4CD;</span>
                AmCart, Connaught Place, New Delhi, India
              </li>
              <li>
                <span className="contact-list__icon">&#x1F4DE;</span>
                +91 9876543210
              </li>
              <li>
                <span className="contact-list__icon">&#x2709;</span>
                support@amcart.com
              </li>
              <li>
                <span className="contact-list__icon">&#x1F310;</span>
                www.amcart.com
              </li>
            </ul>
          </div>

          <div className="footer-newsletter">
            <h3 className="footer-heading">Newsletter</h3>
            <p className="newsletter__desc">
              Subscribe to get special offers, free giveaways, and exclusive deals.
            </p>
            <form className="newsletter-form" onSubmit={handleSubscribe}>
              <input
                type="email"
                className="newsletter-form__input"
                placeholder="Enter your email..."
                aria-label="Email for newsletter"
                value={nlEmail}
                onChange={(e) => { setNlEmail(e.target.value); setNlMsg(null); }}
                required
              />
              <button type="submit" className="newsletter-form__btn" disabled={nlBusy}>
                {nlBusy ? "..." : "Subscribe"}
              </button>
            </form>
            {nlMsg && (
              <p className={`newsletter__feedback newsletter__feedback--${nlMsg.type}`}>
                {nlMsg.text}
              </p>
            )}
          </div>
        </div>
      </div>

      {/* ——— Lower Footer ——— */}
      {/* <div className="footer-lower">
        <div className="footer-lower__inner">
          <div>
            <h3 className="footer-heading">Information</h3>
            <ul className="footer-link-list">
              <li>
                <Link to="/new-products">New Products</Link>
              </li>
              <li>
                <Link to="/popular">Best Sellers</Link>
              </li>
              <li>
                <Link to="/sale">Special Offers</Link>
              </li>
              <li>
                <Link to="/contact">Contact</Link>
              </li>
            </ul>
          </div>

          <div>
            <h3 className="footer-heading">Product Tags</h3>
            <div className="product-tags">
              {PRODUCT_TAGS.map((tag) => (
                <Link
                  key={tag}
                  to={`/category/${encodeURIComponent(tag.toLowerCase())}`}
                  className="product-tag"
                >
                  {tag}
                </Link>
              ))}
            </div>
          </div>

          <div>
            <h3 className="footer-heading">My Account</h3>
            <ul className="footer-link-list">
              <li>
                <Link to="/account/profile">My Account</Link>
              </li>
              <li>
                <Link to="/wishlist">Wishlist</Link>
              </li>
              <li>
                <Link to="/cart">Cart</Link>
              </li>
              <li>
                <Link to="/account/change-password">Change Password</Link>
              </li>
            </ul>
          </div>

          <div>
            <h3 className="footer-heading">Main Categories</h3>
            <ul className="footer-link-list">
              <li>
                <Link to="/category/men">For Men</Link>
              </li>
              <li>
                <Link to="/category/women">For Women</Link>
              </li>
              <li>
                <Link to="/category/jeans">Jeans</Link>
              </li>
              <li>
                <Link to="/category/jackets">Jackets</Link>
              </li>
              <li>
                <Link to="/category/accessories">Accessories</Link>
              </li>
            </ul>
          </div>
        </div>
      </div> */}

      {/* ——— Bottom Bar ——— */}
      <div className="footer-bottom">
        <div className="footer-bottom__inner">
          <div className="payment-icons">
            {PAYMENT_LABELS.map((label) => (
              <span key={label} className="payment-icon">
                {label}
              </span>
            ))}
          </div>

          <div className="social-links">
            <a href="#" className="social-link" aria-label="Facebook" title="Facebook">
              f
            </a>
            <a href="#" className="social-link" aria-label="Twitter" title="Twitter">
              𝕏
            </a>
            <a href="#" className="social-link" aria-label="Instagram" title="Instagram">
              IG
            </a>
            <a href="#" className="social-link" aria-label="YouTube" title="YouTube">
              ▶
            </a>
          </div>

          <p className="footer-copyright">
            <Link to="/out-of-stock" className="footer-oos-link">
              Out of stock
            </Link>
            <span className="footer-copyright__sep"> · </span>
            &copy; 2026 AmCart. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  );
}
