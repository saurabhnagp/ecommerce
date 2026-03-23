import { useEffect, useState } from "react";
import { fetchSubscribers } from "./api";
import type { Subscriber } from "./api";

export function AdminNewsletter() {
  const [items, setItems] = useState<Subscriber[]>([]);
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const [search, setSearch] = useState("");

  async function load() {
    try {
      const res = await fetchSubscribers();
      setItems(res.data);
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Failed to load." });
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { load(); }, []);

  const filtered = items.filter((s) => s.email.includes(search.toLowerCase()));
  const activeCount = items.filter((s) => s.isActive).length;

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Newsletter Subscribers</h1>
        <span style={{ fontSize: "0.85rem", color: "#888" }}>
          <strong style={{ color: "#333" }}>{activeCount}</strong> active / {items.length} total
        </span>
      </div>

      {msg && <div className={`admin-msg admin-msg--${msg.type}`}>{msg.text}</div>}

      {loading ? (
        <p style={{ color: "#888" }}>Loading subscribers...</p>
      ) : (
        <>
          <div className="admin-toolbar">
            <input
              className="admin-search"
              placeholder="Search by email..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>

          <div className="admin-card" style={{ padding: 0 }}>
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Email</th>
                    <th>Subscribed On</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.map((s) => (
                    <tr key={s.id}>
                      <td style={{ fontWeight: 600 }}>{s.email}</td>
                      <td>{new Date(s.subscribedAt).toLocaleDateString()}</td>
                      <td>
                        <span className={s.isActive ? "admin-badge admin-badge--green" : "admin-badge admin-badge--red"}>
                          {s.isActive ? "active" : "unsubscribed"}
                        </span>
                      </td>
                    </tr>
                  ))}
                  {filtered.length === 0 && (
                    <tr><td colSpan={3} style={{ textAlign: "center", color: "#999", padding: "2rem" }}>No subscribers found.</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
