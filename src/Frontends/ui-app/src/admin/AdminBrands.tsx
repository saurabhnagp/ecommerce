import { useEffect, useState } from "react";
import {
  fetchBrands,
  createBrand,
  updateBrand,
  deleteBrand,
} from "./productApi";
import type { BrandDto } from "./productApi";

type FormData = {
  name: string;
  slug: string;
  description: string;
  logoUrl: string;
  websiteUrl: string;
  isActive: boolean;
};

function slugifyFromName(name: string): string {
  const s = name
    .trim()
    .toLowerCase()
    .replace(/\s+/g, "-")
    .replace(/[^a-z0-9-]/g, "")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");
  return s || "brand";
}

const EMPTY_FORM: FormData = {
  name: "",
  slug: "",
  description: "",
  logoUrl: "",
  websiteUrl: "",
  isActive: true,
};

export function AdminBrands() {
  const [brands, setBrands] = useState<BrandDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FormData>(EMPTY_FORM);
  const [msg, setMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  async function load() {
    try {
      const res = await fetchBrands(true);
      setBrands(res.data);
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Failed to load." });
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  function openNew() {
    setEditingId("__new__");
    setForm(EMPTY_FORM);
    setMsg(null);
  }

  function openEdit(b: BrandDto) {
    setEditingId(b.id);
    setForm({
      name: b.name,
      slug: b.slug ?? "",
      description: b.description ?? "",
      logoUrl: b.logoUrl ?? "",
      websiteUrl: b.websiteUrl ?? "",
      isActive: b.isActive,
    });
    setMsg(null);
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    try {
      const body: Record<string, unknown> = {
        name: form.name,
        description: form.description || undefined,
        logoUrl: form.logoUrl || undefined,
        websiteUrl: form.websiteUrl || undefined,
        isActive: form.isActive,
      };
      const slugTrim = form.slug.trim();
      if (slugTrim) body.slug = slugTrim;

      if (editingId === "__new__") {
        await createBrand(body);
        setMsg({ type: "success", text: "Brand created." });
      } else {
        await updateBrand(editingId!, body);
        setMsg({ type: "success", text: "Brand saved." });
      }
      setEditingId(null);
      await load();
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Save failed." });
    }
  }

  async function handleDelete(id: string) {
    if (!confirm("Delete this brand? Products may lose brand association.")) return;
    try {
      await deleteBrand(id);
      setMsg({ type: "success", text: "Brand deleted." });
      await load();
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Delete failed." });
    }
  }

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Brands</h1>
        <button type="button" className="admin-btn admin-btn--primary" onClick={openNew}>
          + Add Brand
        </button>
      </div>

      {msg && <div className={`admin-msg admin-msg--${msg.type}`}>{msg.text}</div>}

      {editingId && (
        <div className="admin-card" style={{ marginBottom: "1.5rem" }}>
          <h3 style={{ margin: "0 0 1rem", fontSize: "0.95rem", color: "#555" }}>
            {editingId === "__new__" ? "New Brand" : "Edit Brand"}
          </h3>
          <form className="admin-form" onSubmit={handleSave}>
            <div className="admin-field">
              <label>Brand Name *</label>
              <input
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                required
              />
            </div>
            <div className="admin-field">
              <label>URL slug (SEO)</label>
              <input
                value={form.slug}
                onChange={(e) => setForm({ ...form, slug: e.target.value })}
                placeholder="e.g. lacoste"
                autoComplete="off"
              />
              <p className="admin-field-hint">
                Leave empty when creating to auto-generate from the name. Uniqueness is enforced on the server.
              </p>
              <button
                type="button"
                className="admin-btn admin-btn--ghost"
                style={{ marginTop: "0.35rem" }}
                onClick={() => setForm((f) => ({ ...f, slug: slugifyFromName(f.name) }))}
              >
                Generate from name
              </button>
            </div>
            <div className="admin-field admin-field--full">
              <label>Description</label>
              <textarea
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
              />
            </div>
            <div className="admin-field">
              <label>Logo URL</label>
              <input
                value={form.logoUrl}
                onChange={(e) => setForm({ ...form, logoUrl: e.target.value })}
                placeholder="https://..."
              />
            </div>
            <div className="admin-field">
              <label>Website URL</label>
              <input
                value={form.websiteUrl}
                onChange={(e) => setForm({ ...form, websiteUrl: e.target.value })}
                placeholder="https://..."
              />
            </div>
            <div className="admin-field">
              <label>Active</label>
              <select
                value={form.isActive ? "yes" : "no"}
                onChange={(e) => setForm({ ...form, isActive: e.target.value === "yes" })}
              >
                <option value="yes">Yes</option>
                <option value="no">No</option>
              </select>
            </div>
            <div className="admin-form__actions">
              <button type="submit" className="admin-btn admin-btn--primary">
                Save
              </button>
              <button
                type="button"
                className="admin-btn admin-btn--ghost"
                onClick={() => setEditingId(null)}
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {loading ? (
        <p style={{ color: "#888" }}>Loading brands...</p>
      ) : (
        <div className="admin-card" style={{ padding: 0 }}>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Slug</th>
                  <th>Website</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {brands.map((b) => (
                  <tr key={b.id}>
                    <td style={{ fontWeight: 600 }}>{b.name}</td>
                    <td>
                      <code style={{ fontSize: "0.78rem", color: "#555" }}>{b.slug}</code>
                    </td>
                    <td>
                      {b.websiteUrl ? (
                        <a href={b.websiteUrl} target="_blank" rel="noreferrer">
                          link
                        </a>
                      ) : (
                        "—"
                      )}
                    </td>
                    <td>
                      <span
                        className={
                          b.isActive
                            ? "admin-badge admin-badge--green"
                            : "admin-badge admin-badge--red"
                        }
                      >
                        {b.isActive ? "active" : "inactive"}
                      </span>
                    </td>
                    <td>
                      <div className="admin-table__actions">
                        <button
                          type="button"
                          className="admin-btn admin-btn--ghost"
                          onClick={() => openEdit(b)}
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          className="admin-btn admin-btn--danger"
                          onClick={() => handleDelete(b.id)}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {brands.length === 0 && (
                  <tr>
                    <td colSpan={5} style={{ textAlign: "center", color: "#999", padding: "2rem" }}>
                      No brands yet.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
