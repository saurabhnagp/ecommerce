import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { fetchProducts } from "./productApi";
import type { ProductDto } from "./productApi";

export function AdminLowStock() {
  const [items, setItems] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setErr("");
    fetchProducts({ page, pageSize: 25, stockFilter: "lowStock" })
      .then((res) => {
        if (!cancelled) {
          setItems(res.data.items);
          setTotalPages(res.data.totalPages);
        }
      })
      .catch((e) => {
        if (!cancelled)
          setErr(e instanceof Error ? e.message : "Failed to load low-stock products (admin only).");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [page]);

  return (
    <div>
      <div className="admin-content__header">
        <h1 className="admin-content__title">Low stock</h1>
      </div>
      <p style={{ color: "#666", margin: "0 0 1rem", fontSize: "0.9rem" }}>
        Tracked products with quantity between 1 and the low-stock threshold (per product, default 5). Requires admin.
      </p>
      {err && <div className="admin-msg admin-msg--error">{err}</div>}
      {loading ? (
        <p style={{ color: "#888" }}>Loading…</p>
      ) : items.length === 0 ? (
        <p style={{ color: "#888" }}>No low-stock items.</p>
      ) : (
        <div className="admin-card" style={{ padding: 0 }}>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>SKU</th>
                  <th>Qty</th>
                  <th>Threshold</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {items.map((p) => (
                  <tr key={p.id}>
                    <td>{p.name}</td>
                    <td>{p.sku}</td>
                    <td>{p.quantity ?? "—"}</td>
                    <td>{p.lowStockThreshold ?? "—"}</td>
                    <td>
                      <Link to="/admin/products">Products</Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
      {totalPages > 1 && (
        <div style={{ marginTop: "1rem", display: "flex", gap: "0.75rem", alignItems: "center" }}>
          <button
            type="button"
            className="admin-btn admin-btn--ghost"
            disabled={page <= 1}
            onClick={() => setPage((n) => Math.max(1, n - 1))}
          >
            Previous
          </button>
          <span style={{ color: "#888", fontSize: "0.85rem" }}>
            Page {page} / {totalPages}
          </span>
          <button
            type="button"
            className="admin-btn admin-btn--ghost"
            disabled={page >= totalPages}
            onClick={() => setPage((n) => n + 1)}
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
