import { NavLink, Outlet } from "react-router-dom";
import "./AdminLayout.css";

const NAV: (
  | { to: string; icon: string; label: string; end?: boolean }
  | "sep"
)[] = [
  { to: "/admin",              icon: "📊", label: "Dashboard",        end: true },
  { to: "/admin/products",     icon: "📦", label: "Products" },
  { to: "/admin/categories",   icon: "🗂️", label: "Categories" },
  { to: "/admin/sales",        icon: "🏷️", label: "Sales & Discounts" },
  "sep",
  { to: "/admin/testimonials", icon: "💬", label: "Testimonials" },
  { to: "/admin/contacts",     icon: "📩", label: "Contact Messages" },
  { to: "/admin/newsletter",   icon: "📧", label: "Newsletter" },
];

export function AdminLayout() {
  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-sidebar__header">
          <h2 className="admin-sidebar__title">AmCart Admin</h2>
          <div className="admin-sidebar__subtitle">Management Console</div>
        </div>
        <ul className="admin-nav">
          {NAV.map((item, i) =>
            item === "sep" ? (
              <li key={i} className="admin-nav__sep" />
            ) : (
              <li key={item.to} className="admin-nav__item">
                <NavLink
                  to={item.to}
                  end={item.end ?? false}
                  className={({ isActive }) =>
                    `admin-nav__link${isActive ? " admin-nav__link--active" : ""}`
                  }
                >
                  <span className="admin-nav__icon">{item.icon}</span>
                  {item.label}
                </NavLink>
              </li>
            ),
          )}
        </ul>
      </aside>

      <div className="admin-content">
        <Outlet />
      </div>
    </div>
  );
}
