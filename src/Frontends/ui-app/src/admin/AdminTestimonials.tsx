import { useEffect, useState } from "react";
import {
  fetchTestimonials,
  createTestimonial,
  updateTestimonial,
  deleteTestimonial,
} from "./api";
import type { Testimonial } from "./api";

type Form = {
  customerName: string;
  photoUrl: string;
  comment: string;
  rating: number;
  sortOrder: number;
  isActive: boolean;
};

const EMPTY: Form = { customerName: "", photoUrl: "", comment: "", rating: 5, sortOrder: 0, isActive: true };

export function AdminTestimonials() {
  const [items, setItems] = useState<Testimonial[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<Form>(EMPTY);
  const [msg, setMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  async function load() {
    try {
      const res = await fetchTestimonials();
      setItems(res.data);
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Failed to load." });
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { load(); }, []);

  function openNew() {
    setEditingId("__new__");
    setForm(EMPTY);
    setMsg(null);
  }

  function openEdit(t: Testimonial) {
    setEditingId(t.id);
    setForm({
      customerName: t.customerName,
      photoUrl: t.photoUrl ?? "",
      comment: t.comment,
      rating: t.rating,
      sortOrder: t.sortOrder,
      isActive: t.isActive ?? true,
    });
    setMsg(null);
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    try {
      if (editingId === "__new__") {
        await createTestimonial(form);
        setMsg({ type: "success", text: "Testimonial created." });
      } else {
        await updateTestimonial(editingId!, form);
        setMsg({ type: "success", text: "Testimonial updated." });
      }
      setEditingId(null);
      await load();
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Save failed." });
    }
  }

  async function handleDelete(id: string) {
    if (!confirm("Delete this testimonial?")) return;
    try {
      await deleteTestimonial(id);
      setMsg({ type: "success", text: "Testimonial deleted." });
      await load();
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Delete failed." });
    }
  }

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Testimonials</h1>
        <button className="admin-btn admin-btn--primary" onClick={openNew}>+ Add Testimonial</button>
      </div>

      {msg && <div className={`admin-msg admin-msg--${msg.type}`}>{msg.text}</div>}

      {editingId && (
        <div className="admin-card" style={{ marginBottom: "1.5rem" }}>
          <h3 style={{ margin: "0 0 1rem", fontSize: "0.95rem", color: "#555" }}>
            {editingId === "__new__" ? "New Testimonial" : "Edit Testimonial"}
          </h3>
          <form className="admin-form" onSubmit={handleSave}>
            <div className="admin-field">
              <label>Customer Name *</label>
              <input value={form.customerName} onChange={(e) => setForm({ ...form, customerName: e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>Photo URL</label>
              <input value={form.photoUrl} onChange={(e) => setForm({ ...form, photoUrl: e.target.value })} placeholder="https://..." />
            </div>
            <div className="admin-field admin-field--full">
              <label>Comment *</label>
              <textarea value={form.comment} onChange={(e) => setForm({ ...form, comment: e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>Rating (1-5)</label>
              <input type="number" min={1} max={5} value={form.rating} onChange={(e) => setForm({ ...form, rating: +e.target.value })} />
            </div>
            <div className="admin-field">
              <label>Sort Order</label>
              <input type="number" value={form.sortOrder} onChange={(e) => setForm({ ...form, sortOrder: +e.target.value })} />
            </div>
            <div className="admin-field">
              <label>Active</label>
              <select value={form.isActive ? "yes" : "no"} onChange={(e) => setForm({ ...form, isActive: e.target.value === "yes" })}>
                <option value="yes">Yes</option>
                <option value="no">No</option>
              </select>
            </div>
            <div className="admin-form__actions">
              <button type="submit" className="admin-btn admin-btn--primary">Save</button>
              <button type="button" className="admin-btn admin-btn--ghost" onClick={() => setEditingId(null)}>Cancel</button>
            </div>
          </form>
        </div>
      )}

      {loading ? (
        <p style={{ color: "#888" }}>Loading testimonials...</p>
      ) : (
        <div className="admin-card" style={{ padding: 0 }}>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Customer</th>
                  <th>Comment</th>
                  <th>Rating</th>
                  <th>Order</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((t) => (
                  <tr key={t.id}>
                    <td style={{ fontWeight: 600 }}>{t.customerName}</td>
                    <td style={{ maxWidth: 300, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{t.comment}</td>
                    <td>{"★".repeat(t.rating)}{"☆".repeat(5 - t.rating)}</td>
                    <td>{t.sortOrder}</td>
                    <td>
                      <div className="admin-table__actions">
                        <button className="admin-btn admin-btn--ghost" onClick={() => openEdit(t)}>Edit</button>
                        <button className="admin-btn admin-btn--danger" onClick={() => handleDelete(t.id)}>Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
                {items.length === 0 && (
                  <tr><td colSpan={5} style={{ textAlign: "center", color: "#999", padding: "2rem" }}>No testimonials yet.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
