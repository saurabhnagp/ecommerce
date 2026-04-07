import { useEffect, useState } from "react";
import {
  fetchCategories,
  createCategory,
  updateCategory,
  deleteCategory,
} from "./productApi";
import type { CategoryDto } from "./productApi";
import { CATEGORY_IMAGES } from "./imageCatalog";

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
      const parentCategoryId = form.parentCategoryId || null;
      const body: Record<string, unknown> = {
        name: form.name,
        description: form.description || undefined,
        imageUrl: form.imageUrl || undefined,
        parentCategoryId,
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

  function sortByOrderThenName(a: CategoryDto, b: CategoryDto) {
    if (a.displayOrder !== b.displayOrder) return a.displayOrder - b.displayOrder;
    return a.name.localeCompare(b.name, undefined, { sensitivity: "base" });
  }

  /** Tree order (DFS): flat API sort made child rows appear next to unrelated parents. */
  type CatRow = { c: CategoryDto; depth: number };
  const rowsOrdered: CatRow[] = [];
  function visit(cat: CategoryDto, depth: number) {
    rowsOrdered.push({ c: cat, depth });
    const children = categories
      .filter((x) => x.parentCategoryId === cat.id)
      .sort(sortByOrderThenName);
    for (const ch of children) visit(ch, depth + 1);
  }
  for (const root of [...rootCategories].sort(sortByOrderThenName)) {
    visit(root, 0);
  }
  const idsShown = new Set(rowsOrdered.map((r) => r.c.id));
  const orphans = categories
    .filter((c) => !idsShown.has(c.id))
    .sort(sortByOrderThenName);
  for (const c of orphans) {
    rowsOrdered.push({ c, depth: 0 });
  }

  const editingCategoryId = editingId && editingId !== "__new__" ? editingId : null;
  const descendantsOfEditing = new Set<string>();
  if (editingCategoryId) {
    const stack = [editingCategoryId];
    while (stack.length > 0) {
      const current = stack.pop()!;
      const children = categories.filter((x) => x.parentCategoryId === current);
      for (const ch of children) {
        if (descendantsOfEditing.has(ch.id)) continue;
        descendantsOfEditing.add(ch.id);
        stack.push(ch.id);
      }
    }
  }
  const parentOptions = rowsOrdered.filter(
    ({ c }) => c.id !== editingCategoryId && !descendantsOfEditing.has(c.id)
  );

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
                {parentOptions.map(({ c, depth }) => (
                  <option key={c.id} value={c.id}>
                    {`${depth > 0 ? `${"  ".repeat(depth)}- ` : ""}${c.name}`}
                  </option>
                ))}
              </select>
            </div>
            <div className="admin-field admin-field--full">
              <label>Description</label>
              <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            </div>
            <div className="admin-field">
              <label>Image</label>
              <select value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })}>
                <option value="">— Select an image —</option>
                {CATEGORY_IMAGES.map((img) => (
                  <option key={img.path} value={img.path}>{img.label}</option>
                ))}
              </select>
              {form.imageUrl && (
                <img
                  src={form.imageUrl}
                  alt="Preview"
                  style={{ marginTop: "0.5rem", width: 80, height: 80, objectFit: "contain", borderRadius: 6, border: "1px solid #e2e8f0" }}
                />
              )}
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
                {rowsOrdered.map(({ c, depth }) => (
                  <tr key={c.id}>
                    <td
                      style={{
                        fontWeight: depth === 0 ? 600 : 400,
                        paddingLeft: depth > 0 ? `${0.5 + depth * 1.5}rem` : undefined,
                      }}
                    >
                      {depth > 0 && (
                        <span style={{ color: "#ccc", marginRight: "0.3rem" }} aria-hidden>
                          └
                        </span>
                      )}
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
                {rowsOrdered.length === 0 && (
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
