import { useEffect, useState } from "react";
import { fetchContactMessages, markContactRead } from "./api";
import type { ContactMsg } from "./api";

export function AdminContacts() {
  const [items, setItems] = useState<ContactMsg[]>([]);
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  async function load() {
    try {
      const res = await fetchContactMessages();
      setItems(res.data);
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Failed to load." });
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { load(); }, []);

  async function handleMarkRead(id: string) {
    try {
      await markContactRead(id);
      setItems((prev) => prev.map((m) => (m.id === id ? { ...m, isRead: true } : m)));
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Failed." });
    }
  }

  const unreadCount = items.filter((m) => !m.isRead).length;

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">
          Contact Messages
          {unreadCount > 0 && (
            <span className="admin-badge admin-badge--red" style={{ marginLeft: "0.6rem", fontSize: "0.78rem" }}>
              {unreadCount} unread
            </span>
          )}
        </h1>
      </div>

      {msg && <div className={`admin-msg admin-msg--${msg.type}`}>{msg.text}</div>}

      {loading ? (
        <p style={{ color: "#888" }}>Loading messages...</p>
      ) : (
        <div className="admin-card" style={{ padding: 0 }}>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Subject</th>
                  <th>Comment</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((m) => (
                  <tr key={m.id} style={{ background: m.isRead ? undefined : "#fffde7" }}>
                    <td style={{ whiteSpace: "nowrap" }}>{new Date(m.createdAt).toLocaleDateString()}</td>
                    <td style={{ fontWeight: 600 }}>{m.name}</td>
                    <td><a href={`mailto:${m.email}`}>{m.email}</a></td>
                    <td>{m.subject}</td>
                    <td style={{ maxWidth: 250, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{m.comment}</td>
                    <td>
                      <span className={m.isRead ? "admin-badge admin-badge--green" : "admin-badge admin-badge--yellow"}>
                        {m.isRead ? "read" : "unread"}
                      </span>
                    </td>
                    <td>
                      {!m.isRead && (
                        <button className="admin-btn admin-btn--ghost" onClick={() => handleMarkRead(m.id)}>
                          Mark Read
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
                {items.length === 0 && (
                  <tr><td colSpan={7} style={{ textAlign: "center", color: "#999", padding: "2rem" }}>No messages yet.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
