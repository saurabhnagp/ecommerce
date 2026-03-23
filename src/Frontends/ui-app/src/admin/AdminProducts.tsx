import { useEffect, useState } from "react";
import {
  fetchProducts,
  fetchCategories,
  createProduct,
  updateProduct,
  deleteProduct,
} from "./productApi";
import type { ProductDto, CategoryDto } from "./productApi";

type FormData = {
  name: string;
  sku: string;
  price: number;
  compareAtPrice: number;
  quantity: number;
  status: string;
  categoryId: string;
  description: string;
  imageUrl: string;
  isFeatured: boolean;
  currency: string;
};

const EMPTY_FORM: FormData = {
  name: "",
  sku: "",
  price: 0,
  compareAtPrice: 0,
  quantity: 0,
  status: "draft",
  categoryId: "",
  description: "",
  imageUrl: "",
  isFeatured: false,
  currency: "INR",
};

const STATUS_BADGE: Record<string, string> = {
  active: "admin-badge admin-badge--green",
  draft: "admin-badge admin-badge--yellow",
  out_of_stock: "admin-badge admin-badge--red",
  archived: "admin-badge admin-badge--red",
};

export function AdminProducts() {
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FormData>(EMPTY_FORM);
  const [search, setSearch] = useState("");
  const [msg, setMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  async function load(p = page, s = search) {
    try {
      const [prodRes, catRes] = await Promise.all([
        fetchProducts({ page: p, pageSize: 20, search: s || undefined }),
        fetchCategories(),
      ]);
      setProducts(prodRes.data.items);
      setTotalPages(prodRes.data.totalPages);
      setCategories(catRes.data);
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

  function openEdit(p: ProductDto) {
    setEditingId(p.id);
    const imgs = p.images ?? [];
    const primaryImg = imgs.find((i) => i.isPrimary) ?? imgs[0];
    setForm({
      name: p.name,
      sku: p.sku,
      price: p.price,
      compareAtPrice: p.compareAtPrice ?? 0,
      quantity: p.quantity,
      status: p.status,
      categoryId: p.categoryId ?? "",
      description: p.description ?? "",
      imageUrl: primaryImg?.url ?? "",
      isFeatured: p.isFeatured,
      currency: p.currency,
    });
    setMsg(null);
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    try {
      const body: Record<string, unknown> = {
        name: form.name,
        sku: form.sku,
        price: form.price,
        compareAtPrice: form.compareAtPrice || undefined,
        quantity: form.quantity,
        status: form.status,
        categoryId: form.categoryId || undefined,
        description: form.description || undefined,
        isFeatured: form.isFeatured,
        currency: form.currency,
      };
      if (form.imageUrl) {
        body.images = [{ url: form.imageUrl, isPrimary: true, displayOrder: 0 }];
      }

      if (editingId === "__new__") {
        await createProduct(body);
        setMsg({ type: "success", text: "Product created." });
      } else {
        await updateProduct(editingId!, body);
        setMsg({ type: "success", text: "Product saved." });
      }
      setEditingId(null);
      await load();
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Save failed." });
    }
  }

  async function handleDelete(id: string) {
    if (!confirm("Delete this product?")) return;
    try {
      await deleteProduct(id);
      setMsg({ type: "success", text: "Product deleted." });
      await load();
    } catch (err) {
      setMsg({ type: "error", text: err instanceof Error ? err.message : "Delete failed." });
    }
  }

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    setPage(1);
    load(1, search);
  }

  function goPage(p: number) {
    setPage(p);
    load(p, search);
  }

  const catMap = new Map(categories.map((c) => [c.id, c.name]));

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Products</h1>
        <button className="admin-btn admin-btn--primary" onClick={openNew}>+ Add Product</button>
      </div>

      {msg && <div className={`admin-msg admin-msg--${msg.type}`}>{msg.text}</div>}

      {editingId && (
        <div className="admin-card" style={{ marginBottom: "1.5rem" }}>
          <h3 style={{ margin: "0 0 1rem", fontSize: "0.95rem", color: "#555" }}>
            {editingId === "__new__" ? "New Product" : "Edit Product"}
          </h3>
          <form className="admin-form" onSubmit={handleSave}>
            <div className="admin-field">
              <label>Product Name *</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>SKU / Product Number *</label>
              <input value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>Category</label>
              <select value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })}>
                <option value="">— None —</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
            <div className="admin-field">
              <label>Status</label>
              <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
                <option value="draft">Draft</option>
                <option value="active">Active</option>
                <option value="out_of_stock">Out of Stock</option>
                <option value="archived">Archived</option>
              </select>
            </div>
            <div className="admin-field">
              <label>Price *</label>
              <input type="number" min={0} step="0.01" value={form.price} onChange={(e) => setForm({ ...form, price: +e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>Compare-at Price</label>
              <input type="number" min={0} step="0.01" value={form.compareAtPrice} onChange={(e) => setForm({ ...form, compareAtPrice: +e.target.value })} />
            </div>
            <div className="admin-field">
              <label>Quantity *</label>
              <input type="number" min={0} value={form.quantity} onChange={(e) => setForm({ ...form, quantity: +e.target.value })} required />
            </div>
            <div className="admin-field">
              <label>Currency</label>
              <select value={form.currency} onChange={(e) => setForm({ ...form, currency: e.target.value })}>
                <option value="INR">INR</option>
                <option value="USD">USD</option>
              </select>
            </div>
            <div className="admin-field admin-field--full">
              <label>Description</label>
              <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            </div>
            <div className="admin-field">
              <label>Primary Image URL</label>
              <input value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} placeholder="https://..." />
            </div>
            <div className="admin-field">
              <label>Featured</label>
              <select value={form.isFeatured ? "yes" : "no"} onChange={(e) => setForm({ ...form, isFeatured: e.target.value === "yes" })}>
                <option value="no">No</option>
                <option value="yes">Yes</option>
              </select>
            </div>
            <div className="admin-form__actions">
              <button type="submit" className="admin-btn admin-btn--primary">Save Product</button>
              <button type="button" className="admin-btn admin-btn--ghost" onClick={() => setEditingId(null)}>Cancel</button>
            </div>
          </form>
        </div>
      )}

      {/* Toolbar */}
      <form className="admin-toolbar" onSubmit={handleSearch}>
        <input
          className="admin-search"
          placeholder="Search by name or SKU..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button type="submit" className="admin-btn admin-btn--ghost">Search</button>
      </form>

      {loading ? (
        <p style={{ color: "#888" }}>Loading products...</p>
      ) : (
        <>
          <div className="admin-card" style={{ padding: 0 }}>
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Image</th>
                    <th>Name</th>
                    <th>SKU</th>
                    <th>Category</th>
                    <th>Price</th>
                    <th>Qty</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {products.map((p) => {
                    const imgs = p.images ?? [];
                    const img = imgs.find((i) => i.isPrimary) ?? imgs[0];
                    return (
                      <tr key={p.id}>
                        <td>
                          {img ? (
                            <img className="admin-table__img" src={img.url} alt="" />
                          ) : (
                            <span style={{ color: "#ccc" }}>—</span>
                          )}
                        </td>
                        <td style={{ fontWeight: 600 }}>{p.name}</td>
                        <td>{p.sku}</td>
                        <td>{p.categoryId ? catMap.get(p.categoryId) ?? "—" : "—"}</td>
                        <td>
                          {p.currency === "INR" ? "₹" : "$"}{p.price.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                          {p.compareAtPrice != null && p.compareAtPrice > p.price && (
                            <span style={{ color: "#aaa", textDecoration: "line-through", marginLeft: "0.4rem", fontSize: "0.78rem" }}>
                              {p.currency === "INR" ? "₹" : "$"}{p.compareAtPrice.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                            </span>
                          )}
                        </td>
                        <td>{p.quantity}</td>
                        <td>
                          <span className={STATUS_BADGE[p.status] ?? "admin-badge"}>{p.status.replace("_", " ")}</span>
                        </td>
                        <td>
                          <div className="admin-table__actions">
                            <button className="admin-btn admin-btn--ghost" onClick={() => openEdit(p)}>Edit</button>
                            <button className="admin-btn admin-btn--danger" onClick={() => handleDelete(p.id)}>Delete</button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                  {products.length === 0 && (
                    <tr>
                      <td colSpan={8} style={{ textAlign: "center", color: "#999", padding: "2rem" }}>
                        No products found.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {totalPages > 1 && (
            <div style={{ display: "flex", justifyContent: "center", gap: "0.4rem", marginTop: "1rem" }}>
              {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                <button
                  key={p}
                  className={`admin-btn ${p === page ? "admin-btn--primary" : "admin-btn--ghost"}`}
                  onClick={() => goPage(p)}
                >
                  {p}
                </button>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}
