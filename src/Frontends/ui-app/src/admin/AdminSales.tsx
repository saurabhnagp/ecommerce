import { useState } from "react";

type Sale = {
  id: string;
  name: string;
  discount: number;
  category: string;
  startDate: string;
  endDate: string;
  active: boolean;
};

const SEED: Sale[] = [
  { id: "1", name: "Summer Clearance",  discount: 25, category: "Women",       startDate: "2026-01-01", endDate: "2026-03-31", active: true },
  { id: "2", name: "Accessory Bonanza", discount: 15, category: "Accessories", startDate: "2026-02-01", endDate: "2026-02-28", active: false },
  { id: "3", name: "Men's Weekend",     discount: 50, category: "Men",         startDate: "2026-01-20", endDate: "2026-04-30", active: true },
];

const EMPTY: Sale = { id: "", name: "", discount: 0, category: "Men", startDate: "", endDate: "", active: true };

export function AdminSales() {
  const [sales, setSales] = useState<Sale[]>(SEED);
  const [editing, setEditing] = useState<Sale | null>(null);
  const [msg, setMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  function openNew() { setEditing({ ...EMPTY, id: crypto.randomUUID() }); setMsg(null); }
  function openEdit(s: Sale) { setEditing({ ...s }); setMsg(null); }

  function handleDelete(id: string) {
    if (!confirm("Delete this sale?")) return;
    setSales((prev) => prev.filter((s) => s.id !== id));
    setMsg({ type: "success", text: "Sale deleted." });
  }

  function handleSave(e: React.FormEvent) {
    e.preventDefault();
    if (!editing) return;
    setSales((prev) => {
      const idx = prev.findIndex((s) => s.id === editing.id);
      if (idx >= 0) { const n = [...prev]; n[idx] = editing; return n; }
      return [...prev, editing];
    });
    setMsg({ type: "success", text: "Sale saved." });
    setEditing(null);
  }

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Sales & Discounts</h1>
        <button className="admin-btn admin-btn--primary" onClick={openNew}>+ Create Sale</button>
      </div>

      {msg && <div className={`admin-msg admin-msg--${msg.type}`}>{msg.text}</div>}

      {editing && (
        <div className="admin-card" style={{ marginBottom: "1.5rem" }}>
          <h3 style={{ margin: "0 0 1rem", fontSize: "0.95rem", color: "#555" }}>
            {sales.find((s) => s.id === editing.id) ? "Edit Sale" : "New Sale"}
          </h3>
          <form className="admin-form" onSubmit={handleSave}>
            <div className="admin-field">
              <label>Sale Name *</label>
              <input value={editing.name} onChange={(e) => setEditing({ ...editing, name: e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>Discount % *</label>
              <input type="number" min={1} max={100} value={editing.discount} onChange={(e) => setEditing({ ...editing, discount: +e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>Category</label>
              <select value={editing.category} onChange={(e) => setEditing({ ...editing, category: e.target.value })}>
                <option>Men</option>
                <option>Women</option>
                <option>Accessories</option>
                <option>All</option>
              </select>
            </div>
            <div className="admin-field">
              <label>Active</label>
              <select value={editing.active ? "yes" : "no"} onChange={(e) => setEditing({ ...editing, active: e.target.value === "yes" })}>
                <option value="yes">Yes</option>
                <option value="no">No</option>
              </select>
            </div>
            <div className="admin-field">
              <label>Start Date *</label>
              <input type="date" value={editing.startDate} onChange={(e) => setEditing({ ...editing, startDate: e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>End Date *</label>
              <input type="date" value={editing.endDate} onChange={(e) => setEditing({ ...editing, endDate: e.target.value })} required />
            </div>
            <div className="admin-form__actions">
              <button type="submit" className="admin-btn admin-btn--primary">Save</button>
              <button type="button" className="admin-btn admin-btn--ghost" onClick={() => setEditing(null)}>Cancel</button>
            </div>
          </form>
        </div>
      )}

      <div className="admin-card" style={{ padding: 0 }}>
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Sale Name</th>
                <th>Discount</th>
                <th>Category</th>
                <th>Start</th>
                <th>End</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {sales.map((s) => (
                <tr key={s.id}>
                  <td style={{ fontWeight: 600 }}>{s.name}</td>
                  <td>{s.discount}%</td>
                  <td>{s.category}</td>
                  <td>{s.startDate}</td>
                  <td>{s.endDate}</td>
                  <td>
                    <span className={s.active ? "admin-badge admin-badge--green" : "admin-badge admin-badge--red"}>
                      {s.active ? "active" : "inactive"}
                    </span>
                  </td>
                  <td>
                    <div className="admin-table__actions">
                      <button className="admin-btn admin-btn--ghost" onClick={() => openEdit(s)}>Edit</button>
                      <button className="admin-btn admin-btn--danger" onClick={() => handleDelete(s.id)}>Delete</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
