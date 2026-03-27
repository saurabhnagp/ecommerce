import { useState } from "react";
import type { CategoryDto, BrandDto } from "../api/products";
import "./FilterSidebar.css";

// const SIZES = ["XS", "S", "M", "L", "XL", "XXL"];

// const COLORS: { name: string; hex: string }[] = [
//   { name: "Black", hex: "#222" },
//   { name: "White", hex: "#f5f5f5" },
//   { name: "Red", hex: "#d32f2f" },
//   { name: "Blue", hex: "#1976d2" },
//   { name: "Green", hex: "#388e3c" },
//   { name: "Beige", hex: "#d4c5a9" },
//   { name: "Navy", hex: "#1a237e" },
//   { name: "Grey", hex: "#9e9e9e" },
// ];

export type Filters = {
  /** Category id sent to the product API (leaf or root with no children). */
  selectedCategoryId: string | null;
  priceMin: number;
  priceMax: number;
  sizes: string[];
  colors: string[];
  brandId: string | null;
  sort: string;
};

type Props = {
  filters: Filters;
  onChange: (f: Filters) => void;
  categories: CategoryDto[];
  brands: BrandDto[];
  maxPrice?: number;
  loading?: boolean;
};

export function FilterSidebar({
  filters,
  onChange,
  categories,
  brands,
  maxPrice = 50000,
  loading,
}: Props) {
  /** Expanded category rows for recursive tree UI. */
  const [expandedIds, setExpandedIds] = useState<Set<string>>(() => new Set());

  function set<K extends keyof Filters>(key: K, value: Filters[K]) {
    onChange({ ...filters, [key]: value });
  }

  function subs(cat: CategoryDto) {
    return cat.subCategories ?? [];
  }

  function collectDescendantIds(cat: CategoryDto, out: Set<string>) {
    const children = subs(cat);
    for (const ch of children) {
      out.add(ch.id);
      collectDescendantIds(ch, out);
    }
  }

  function toggleExpanded(id: string, open: boolean) {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (open) next.add(id);
      else next.delete(id);
      return next;
    });
  }

  function renderCategoryNode(cat: CategoryDto, depth = 0) {
    const children = subs(cat);
    const hasChildren = children.length > 0;
    const isExpanded = expandedIds.has(cat.id);
    const isSelected = filters.selectedCategoryId === cat.id;

    return (
      <li key={cat.id}>
        <button
          type="button"
          className={`filter-cat-btn${isSelected ? " filter-cat-btn--active" : ""}${hasChildren && isExpanded ? " filter-cat-btn--expanded" : ""}`}
          style={{ paddingLeft: `${0.35 + depth * 0.85}rem` }}
          onClick={() => {
            if (!hasChildren) {
              set(
                "selectedCategoryId",
                filters.selectedCategoryId === cat.id ? null : cat.id
              );
              return;
            }

            if (isExpanded) {
              toggleExpanded(cat.id, false);
              const desc = new Set<string>();
              collectDescendantIds(cat, desc);
              const sel = filters.selectedCategoryId;
              if (sel === cat.id || (sel != null && desc.has(sel))) {
                set("selectedCategoryId", null);
              }
            } else {
              toggleExpanded(cat.id, true);
              set("selectedCategoryId", cat.id);
            }
          }}
        >
          {hasChildren ? (
            <span
              className={`filter-cat-btn__arrow${isExpanded ? " filter-cat-btn__arrow--open" : ""}`}
              aria-hidden
            >
              &#9654;
            </span>
          ) : (
            <span className="filter-cat-btn__arrow filter-cat-btn__arrow--spacer" aria-hidden />
          )}
          {cat.name}
        </button>

        {hasChildren && isExpanded && (
          <ul className="filter-cat-sublist">
            {children.map((child) => renderCategoryNode(child, depth + 1))}
          </ul>
        )}
      </li>
    );
  }

  return (
    <aside className="filter-sidebar">
      {/* Sort */}
      <div className="filter-sort">
        <h4 className="filter-section__title">Sort By</h4>
        <select value={filters.sort} onChange={(e) => set("sort", e.target.value)}>
          <option value="newest">Newest First</option>
          <option value="price-asc">Price: Low → High</option>
          <option value="price-desc">Price: High → Low</option>
          <option value="name-asc">Name: A → Z</option>
          <option value="popular">Popularity</option>
        </select>
      </div>

      {/* Categories */}
      <div className="filter-section">
        <h4 className="filter-section__title">Categories</h4>
        {loading ? (
          <p className="filter-loading">Loading…</p>
        ) : categories.length === 0 ? (
          <p className="filter-empty">No categories yet.</p>
        ) : (
          <>
            <ul className="filter-cat-list">
              {categories.map((cat) => renderCategoryNode(cat))}
            </ul>
            {filters.selectedCategoryId && (
              <button
                type="button"
                className="filter-clear-btn"
                onClick={() => set("selectedCategoryId", null)}
              >
                Clear category
              </button>
            )}
          </>
        )}
      </div>

      {/* Price Range */}
      <div className="filter-section">
        <h4 className="filter-section__title">Price</h4>
        <div className="price-range">
          <input
            type="range"
            className="price-range__slider"
            min={0}
            max={maxPrice}
            step={50}
            value={filters.priceMax || maxPrice}
            onChange={(e) => set("priceMax", +e.target.value)}
          />
          <div className="price-range__inputs">
            <input
              type="number"
              className="price-range__input"
              placeholder="Min"
              value={filters.priceMin || ""}
              onChange={(e) => set("priceMin", +e.target.value)}
            />
            <span className="price-range__sep">&ndash;</span>
            <input
              type="number"
              className="price-range__input"
              placeholder="Max"
              value={filters.priceMax || ""}
              onChange={(e) => set("priceMax", +e.target.value)}
            />
          </div>
        </div>
      </div>

      {/* Size (client-side only — kept for UI completeness) */}
      {/* <div className="filter-section">
        <h4 className="filter-section__title">Size</h4>
        <div className="filter-checks">
          {SIZES.map((s) => (
            <label key={s} className="filter-check">
              <input
                type="checkbox"
                checked={filters.sizes.includes(s)}
                onChange={() => toggleArr("sizes", s)}
              />
              {s}
            </label>
          ))}
        </div>
      </div> */}

      {/* Color (client-side only) */}
      {/* <div className="filter-section">
        <h4 className="filter-section__title">Color</h4>
        <div className="filter-colors">
          {COLORS.map((c) => (
            <button
              key={c.name}
              type="button"
              className={`color-swatch${filters.colors.includes(c.name) ? " color-swatch--active" : ""}`}
              style={{ background: c.hex }}
              title={c.name}
              onClick={() => toggleArr("colors", c.name)}
            />
          ))}
        </div>
      </div> */}

      {/* Brand */}
      <div className="filter-section">
        <h4 className="filter-section__title">Brand</h4>
        {loading ? (
          <p className="filter-loading">Loading…</p>
        ) : brands.length === 0 ? (
          <p className="filter-empty">No brands yet.</p>
        ) : (
          <div className="filter-checks">
            {brands.map((b) => (
              <label key={b.id} className="filter-check">
                <input
                  type="radio"
                  name="brand"
                  checked={filters.brandId === b.id}
                  onChange={() =>
                    set("brandId", filters.brandId === b.id ? null : b.id)
                  }
                />
                {b.name}
              </label>
            ))}
            {filters.brandId && (
              <button
                type="button"
                className="filter-clear-btn"
                onClick={() => set("brandId", null)}
              >
                Clear brand
              </button>
            )}
          </div>
        )}
      </div>
    </aside>
  );
}
