import { useEffect, useState } from "react";
import { fetchCategories, fetchProducts } from "./productApi";
import { fetchContactMessages, fetchSubscribers, fetchTestimonials } from "./api";

type DashboardCounts = {
  totalProducts: number;
  categories: number;
  contactMessages: number;
  subscribers: number;
  testimonials: number;
};

const ZERO_COUNTS: DashboardCounts = {
  totalProducts: 0,
  categories: 0,
  contactMessages: 0,
  subscribers: 0,
  testimonials: 0,
};

export function AdminDashboard() {
  const [counts, setCounts] = useState<DashboardCounts>(ZERO_COUNTS);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError("");
      try {
        const [productsRes, categoriesRes, contactsRes, subscribersRes, testimonialsRes] =
          await Promise.all([
            fetchProducts({ page: 1, pageSize: 1 }),
            fetchCategories(true),
            fetchContactMessages(),
            fetchSubscribers(),
            fetchTestimonials(),
          ]);
        if (cancelled) return;
        setCounts({
          totalProducts: productsRes.data.totalCount ?? 0,
          categories: categoriesRes.data.length ?? 0,
          contactMessages: contactsRes.data.length ?? 0,
          subscribers: subscribersRes.data.length ?? 0,
          testimonials: testimonialsRes.data.length ?? 0,
        });
      } catch (e) {
        if (cancelled) return;
        setError(e instanceof Error ? e.message : "Could not load dashboard metrics.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const stats = [
    { label: "Total Products", value: counts.totalProducts, icon: "📦", color: "#c9a962" },
    { label: "Categories", value: counts.categories, icon: "🗂️", color: "#1976d2" },
    { label: "Contact Messages", value: counts.contactMessages, icon: "📩", color: "#ef5350" },
    { label: "Subscribers", value: counts.subscribers, icon: "📧", color: "#7b1fa2" },
    { label: "Testimonials", value: counts.testimonials, icon: "💬", color: "#f57c00" },
  ];

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Dashboard</h1>
      </div>

      {error && <div className="admin-msg admin-msg--error">{error}</div>}

      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(200px, 1fr))", gap: "1rem" }}>
        {stats.map((s) => (
          <div className="admin-card" key={s.label} style={{ borderLeft: `4px solid ${s.color}` }}>
            <div style={{ display: "flex", alignItems: "center", gap: "0.7rem" }}>
              <span style={{ fontSize: "1.6rem" }}>{s.icon}</span>
              <div>
                <div style={{ fontSize: "1.4rem", fontWeight: 700, color: "#222" }}>
                  {loading ? "…" : s.value.toLocaleString("en-IN")}
                </div>
                <div style={{ fontSize: "0.8rem", color: "#888" }}>{s.label}</div>
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="admin-card" style={{ marginTop: "1.5rem" }}>
        <h3 style={{ margin: "0 0 0.5rem", fontSize: "0.95rem", color: "#555" }}>Quick Actions</h3>
        <div style={{ display: "flex", gap: "0.6rem", flexWrap: "wrap" }}>
          <a href="/admin/products?action=new" className="admin-btn admin-btn--primary">+ Add Product</a>
          <a href="/admin/categories" className="admin-btn admin-btn--ghost">Manage Categories</a>
          <a href="/admin/brands" className="admin-btn admin-btn--ghost">Manage Brands</a>
          <a href="/admin/contacts" className="admin-btn admin-btn--ghost">View Messages</a>
        </div>
      </div>
    </div>
  );
}
