import { useEffect, useState } from "react";
import {
  fetchProducts,
  fetchCategories,
  fetchBrands,
  createProduct,
  updateProduct,
  deleteProduct,
} from "./productApi";
import type { ProductDto, CategoryDto, BrandDto } from "./productApi";

type FormData = {
  name: string;
  slug: string;
  sku: string;
  price: number;
  compareAtPrice: number;
  quantity: number;
  status: string;
  categoryId: string;
  brandId: string;
  description: string;
  imageUrl: string;
  isFeatured: boolean;
  currency: string;
};

/** Client-side preview of URL slug; server uses the same rules when slug is omitted. */
function slugifyFromName(name: string): string {
  const s = name
    .trim()
    .toLowerCase()
    .replace(/\s+/g, "-")
    .replace(/[^a-z0-9-]/g, "")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");
  return s || "item";
}

const EMPTY_FORM: FormData = {
  name: "",
  slug: "",
  sku: "",
  price: 0,
  compareAtPrice: 0,
  quantity: 0,
  status: "draft",
  categoryId: "",
  brandId: "",
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
  const [brands, setBrands] = useState<BrandDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FormData>(EMPTY_FORM);
  const [search, setSearch] = useState("");
  const [msg, setMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  async function load(p = page, s = search) {
    try {
      const [prodRes, catRes, brandRes] = await Promise.all([
        fetchProducts({ page: p, pageSize: 20, search: s || undefined }),
        fetchCategories(),
        fetchBrands(true),
      ]);
      setProducts(prodRes.data.items);
      setTotalPages(prodRes.data.totalPages);
      setCategories(catRes.data);
      setBrands(brandRes.data);
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
      slug: p.slug ?? "",
      sku: p.sku,
      price: p.price,
      compareAtPrice: p.compareAtPrice ?? 0,
      quantity: p.quantity ?? 0,
      status: p.status,
      categoryId: p.categoryId ?? "",
      brandId: p.brandId ?? "",
      description: p.description ?? "",
      imageUrl: primaryImg?.url ?? "",
      isFeatured: p.isFeatured,
      currency: p.currency ?? "INR",
    });
    setMsg(null);
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    try {
      const emptyGuid = "00000000-0000-0000-0000-000000000000";
      const body: Record<string, unknown> = {
        name: form.name,
        sku: form.sku,
        price: form.price,
        compareAtPrice: form.compareAtPrice || undefined,
        quantity: form.quantity,
        status: form.status,
        description: form.description || undefined,
        isFeatured: form.isFeatured,
        currency: form.currency,
      };
      const slugTrim = form.slug.trim();
      if (slugTrim) body.slug = slugTrim;
      if (form.imageUrl) {
        body.images = [{ url: form.imageUrl, isPrimary: true, displayOrder: 0 }];
      }

      if (editingId === "__new__") {
        if (form.categoryId) body.categoryId = form.categoryId;
        if (form.brandId) body.brandId = form.brandId;
        await createProduct(body);
        setMsg({ type: "success", text: "Product created." });
      } else {
        body.categoryId = form.categoryId || emptyGuid;
        body.brandId = form.brandId || emptyGuid;
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
              <label>URL slug (SEO)</label>
              <input
                value={form.slug}
                onChange={(e) => setForm({ ...form, slug: e.target.value })}
                placeholder="e.g. lacoste-cotton-shirt-slim-fit"
                autoComplete="off"
              />
              <p className="admin-field-hint">
                Used in product URLs (<code>/products/…</code>). Leave empty when creating to auto-generate from the name.
                Uniqueness is enforced on the server.
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
              <label>Brand</label>
              <select value={form.brandId} onChange={(e) => setForm({ ...form, brandId: e.target.value })}>
                <option value="">— None —</option>
                {brands.map((b) => (
                  <option key={b.id} value={b.id}>{b.name}</option>
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
              <label>Compare-at price (MSRP / was price)</label>
              <input type="number" min={0} step="0.01" value={form.compareAtPrice} onChange={(e) => setForm({ ...form, compareAtPrice: +e.target.value })} />
              <p className="admin-field-hint">
                Optional “original” or list price. When higher than <strong>Price</strong>, the storefront can show it struck through so the sale price looks like a discount.
              </p>
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
                    <th>Slug</th>
                    <th>SKU</th>
                    <th>Category</th>
                    <th>Brand</th>
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
                    const thumb = p.primaryImageUrl ?? img?.url;
                    const cur = p.currency ?? "INR";
                    return (
                      <tr key={p.id}>
                        <td>
                          {thumb ? (
                            <img className="admin-table__img" src={thumb} alt="" />
                          ) : (
                            <span style={{ color: "#ccc" }}>—</span>
                          )}
                        </td>
                        <td style={{ fontWeight: 600 }}>{p.name}</td>
                        <td>
                          <code style={{ fontSize: "0.78rem", color: "#555" }}>{p.slug}</code>
                        </td>
                        <td>{p.sku}</td>
                        <td>{p.categoryId ? catMap.get(p.categoryId) ?? "—" : "—"}</td>
                        <td>{p.brandName ?? "—"}</td>
                        <td>
                          {cur === "INR" ? "₹" : "$"}{p.price.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                          {p.compareAtPrice != null && p.compareAtPrice > p.price && (
                            <span style={{ color: "#aaa", textDecoration: "line-through", marginLeft: "0.4rem", fontSize: "0.78rem" }}>
                              {cur === "INR" ? "₹" : "$"}{p.compareAtPrice.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                            </span>
                          )}
                        </td>
                        <td>{p.quantity ?? 0}</td>
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
                      <td colSpan={10} style={{ textAlign: "center", color: "#999", padding: "2rem" }}>
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
