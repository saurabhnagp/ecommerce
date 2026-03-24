import { useEffect, useState } from "react";
import {
  fetchCategories,
  createCategory,
  updateCategory,
  deleteCategory,
} from "./productApi";
import type { CategoryDto } from "./productApi";

type FormData = {
  name: string;
  description: string;
  imageUrl: string;
  parentCategoryId: string;
  displayOrder: number;
  isActive: boolean;
};

const EMPTY_FORM: FormData = {
  name: "",
  description: "",
  imageUrl: "",
  parentCategoryId: "",
  displayOrder: 0,
  isActive: true,
};

export function AdminCategories() {
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FormData>(EMPTY_FORM);
  const [msg, setMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  async function load() {
    try {
      const res = await fetchCategories(true);
      setCategories(res.data);
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Failed to load." });
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { load(); }, []);

  function openNew() {
    setEditingId("__new__");
    setForm(EMPTY_FORM);
    setMsg(null);
  }

  function openEdit(c: CategoryDto) {
    setEditingId(c.id);
    setForm({
      name: c.name,
      description: c.description ?? "",
      imageUrl: c.imageUrl ?? "",
      parentCategoryId: c.parentCategoryId ?? "",
      displayOrder: c.displayOrder,
      isActive: c.isActive,
    });
    setMsg(null);
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    try {
      const body: Record<string, unknown> = {
        name: form.name,
        description: form.description || undefined,
        imageUrl: form.imageUrl || undefined,
        parentCategoryId: form.parentCategoryId || undefined,
        displayOrder: form.displayOrder,
        isActive: form.isActive,
      };

      if (editingId === "__new__") {
        await createCategory(body);
        setMsg({ type: "success", text: "Category created." });
      } else {
        await updateCategory(editingId!, body);
        setMsg({ type: "success", text: "Category saved." });
      }
      setEditingId(null);
      await load();
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Save failed." });
    }
  }

  async function handleDelete(id: string) {
    if (!confirm("Delete this category?")) return;
    try {
      await deleteCategory(id);
      setMsg({ type: "success", text: "Category deleted." });
      await load();
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Delete failed." });
    }
  }

  const rootCategories = categories.filter((c) => !c.parentCategoryId);

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Categories</h1>
        <button className="admin-btn admin-btn--primary" onClick={openNew}>+ Add Category</button>
      </div>

      {msg && <div className={`admin-msg admin-msg--${msg.type}`}>{msg.text}</div>}

      {editingId && (
        <div className="admin-card" style={{ marginBottom: "1.5rem" }}>
          <h3 style={{ margin: "0 0 1rem", fontSize: "0.95rem", color: "#555" }}>
            {editingId === "__new__" ? "New Category" : "Edit Category"}
          </h3>
          <form className="admin-form" onSubmit={handleSave}>
            <div className="admin-field">
              <label>Category Name *</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>Parent Category</label>
              <select value={form.parentCategoryId} onChange={(e) => setForm({ ...form, parentCategoryId: e.target.value })}>
                <option value="">— Root (top-level) —</option>
                {rootCategories.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
            <div className="admin-field admin-field--full">
              <label>Description</label>
              <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            </div>
            <div className="admin-field">
              <label>Image URL</label>
              <input value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} placeholder="https://..." />
            </div>
            <div className="admin-field">
              <label>Display Order</label>
              <input type="number" value={form.displayOrder} onChange={(e) => setForm({ ...form, displayOrder: +e.target.value })} />
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
        <p style={{ color: "#888" }}>Loading categories...</p>
      ) : (
        <div className="admin-card" style={{ padding: 0 }}>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Slug</th>
                  <th>Subcategories</th>
                  <th>Order</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {categories.map((c) => (
                  <tr key={c.id}>
                    <td style={{ fontWeight: 600, paddingLeft: c.parentCategoryId ? "2rem" : undefined }}>
                      {c.parentCategoryId && <span style={{ color: "#ccc", marginRight: "0.3rem" }}>└</span>}
                      {c.name}
                    </td>
                    <td style={{ color: "#888" }}>{c.slug}</td>
                    <td>
                      {categories.filter((x) => x.parentCategoryId === c.id).length}
                    </td>
                    <td>{c.displayOrder}</td>
                    <td>
                      <span className={c.isActive ? "admin-badge admin-badge--green" : "admin-badge admin-badge--red"}>
                        {c.isActive ? "active" : "inactive"}
                      </span>
                    </td>
                    <td>
                      <div className="admin-table__actions">
                        <button className="admin-btn admin-btn--ghost" onClick={() => openEdit(c)}>Edit</button>
                        <button className="admin-btn admin-btn--danger" onClick={() => handleDelete(c.id)}>Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
                {categories.length === 0 && (
                  <tr><td colSpan={6} style={{ textAlign: "center", color: "#999", padding: "2rem" }}>No categories yet.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
