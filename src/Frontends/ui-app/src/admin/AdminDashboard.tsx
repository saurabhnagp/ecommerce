const STATS = [
  { label: "Total Products", value: "248", icon: "📦", color: "#c9a962" },
  { label: "Categories",     value: "12",  icon: "🗂️", color: "#1976d2" },
  { label: "Active Sales",   value: "3",   icon: "🏷️", color: "#388e3c" },
  { label: "Contact Messages", value: "17", icon: "📩", color: "#ef5350" },
  { label: "Subscribers",    value: "1,024", icon: "📧", color: "#7b1fa2" },
  { label: "Testimonials",   value: "8",   icon: "💬", color: "#f57c00" },
];

export function AdminDashboard() {
  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Dashboard</h1>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(200px, 1fr))", gap: "1rem" }}>
        {STATS.map((s) => (
          <div className="admin-card" key={s.label} style={{ borderLeft: `4px solid ${s.color}` }}>
            <div style={{ display: "flex", alignItems: "center", gap: "0.7rem" }}>
              <span style={{ fontSize: "1.6rem" }}>{s.icon}</span>
              <div>
                <div style={{ fontSize: "1.4rem", fontWeight: 700, color: "#222" }}>{s.value}</div>
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
          <a href="/admin/sales?action=new" className="admin-btn admin-btn--ghost">Create Sale</a>
          <a href="/admin/contacts" className="admin-btn admin-btn--ghost">View Messages</a>
        </div>
      </div>
    </div>
  );
}
