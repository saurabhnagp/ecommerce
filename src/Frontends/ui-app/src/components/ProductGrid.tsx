import { useState } from "react";
import { ProductCard } from "./ProductCard";
import type { Product } from "./ProductCard";
import "./ProductGrid.css";

const TABS = [
  { key: "new", label: "New Products" },
  { key: "featured", label: "Featured" },
  { key: "special", label: "Special" },
  { key: "bestsellers", label: "Bestsellers" },
] as const;

type TabKey = (typeof TABS)[number]["key"];

const PAGE_SIZE = 8;

type Props = {
  products: Product[];
};

export function ProductGrid({ products }: Props) {
  const [activeTab, setActiveTab] = useState<TabKey>("new");
  const [page, setPage] = useState(0);

  const filtered = products.filter((p) => p.tab === activeTab);
  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paged = filtered.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE);

  function switchTab(tab: TabKey) {
    setActiveTab(tab);
    setPage(0);
  }

  return (
    <div>
      {/* Tabs */}
      <div className="pgrid-tabs">
        {TABS.map((t) => (
          <button
            key={t.key}
            className={`pgrid-tab${activeTab === t.key ? " pgrid-tab--active" : ""}`}
            onClick={() => switchTab(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* Page arrows */}
      <div className="pgrid-nav">
        <button
          className="pgrid-nav__btn"
          disabled={page === 0}
          onClick={() => setPage((p) => p - 1)}
        >
          &#8249;
        </button>
        <button
          className="pgrid-nav__btn"
          disabled={page >= totalPages - 1}
          onClick={() => setPage((p) => p + 1)}
        >
          &#8250;
        </button>
      </div>

      {/* Info bar */}
      <div className="pgrid-info">
        <span>
          Showing{" "}
          <span className="pgrid-info__count">
            {paged.length} of {filtered.length}
          </span>{" "}
          products
        </span>
        {totalPages > 1 && (
          <span>
            Page {page + 1} / {totalPages}
          </span>
        )}
      </div>

      {/* Grid */}
      <div className="pgrid-grid">
        {paged.length > 0 ? (
          paged.map((p) => <ProductCard key={p.id} product={p} />)
        ) : (
          <p className="pgrid-empty">No products found in this category.</p>
        )}
      </div>
    </div>
  );
}
