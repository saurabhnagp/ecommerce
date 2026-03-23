import { useState } from "react";
import "./FilterSidebar.css";

const CATEGORIES = [
  {
    name: "Women",
    subs: ["Dresses", "Tops", "Skirts", "Jackets", "Accessories"],
  },
  {
    name: "Men",
    subs: ["Shirts", "T-Shirts", "Jeans", "Jackets", "Watches"],
  },
  {
    name: "Accessories",
    subs: ["Bags", "Sunglasses", "Wallets", "Belts"],
  },
];

const SIZES = ["XS", "S", "M", "L", "XL", "XXL"];

const COLORS: { name: string; hex: string }[] = [
  { name: "Black", hex: "#222" },
  { name: "White", hex: "#f5f5f5" },
  { name: "Red", hex: "#d32f2f" },
  { name: "Blue", hex: "#1976d2" },
  { name: "Green", hex: "#388e3c" },
  { name: "Beige", hex: "#d4c5a9" },
  { name: "Navy", hex: "#1a237e" },
  { name: "Grey", hex: "#9e9e9e" },
];

const BRANDS = ["Lacoste", "Gucci", "Puma", "Nike", "Adidas", "Levi's"];

export type Filters = {
  category: string | null;
  subcategory: string | null;
  priceMin: number;
  priceMax: number;
  sizes: string[];
  colors: string[];
  brands: string[];
  sort: string;
};

type Props = {
  filters: Filters;
  onChange: (f: Filters) => void;
};

export function FilterSidebar({ filters, onChange }: Props) {
  const [openCat, setOpenCat] = useState<string | null>(filters.category);

  function set<K extends keyof Filters>(key: K, value: Filters[K]) {
    onChange({ ...filters, [key]: value });
  }

  function toggleArr(key: "sizes" | "colors" | "brands", val: string) {
    const arr = filters[key];
    set(key, arr.includes(val) ? arr.filter((v) => v !== val) : [...arr, val]);
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
        <ul className="filter-cat-list">
          {CATEGORIES.map((cat) => (
            <li key={cat.name}>
              <button
                className={`filter-cat-btn${filters.category === cat.name ? " filter-cat-btn--active" : ""}`}
                onClick={() => {
                  const next = openCat === cat.name ? null : cat.name;
                  setOpenCat(next);
                  set("category", next);
                  set("subcategory", null);
                }}
              >
                <span
                  className={`filter-cat-btn__arrow${openCat === cat.name ? " filter-cat-btn__arrow--open" : ""}`}
                >
                  &#9654;
                </span>
                {cat.name}
              </button>
              {openCat === cat.name && (
                <ul className="filter-cat-sublist">
                  {cat.subs.map((sub) => (
                    <li key={sub}>
                      <button
                        className={filters.subcategory === sub ? "active" : ""}
                        onClick={() => set("subcategory", filters.subcategory === sub ? null : sub)}
                      >
                        {sub}
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </li>
          ))}
        </ul>
      </div>

      {/* Price Range */}
      <div className="filter-section">
        <h4 className="filter-section__title">Price</h4>
        <div className="price-range">
          <input
            type="range"
            className="price-range__slider"
            min={0}
            max={5000}
            step={50}
            value={filters.priceMax}
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

      {/* Size */}
      <div className="filter-section">
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
      </div>

      {/* Color */}
      <div className="filter-section">
        <h4 className="filter-section__title">Color</h4>
        <div className="filter-colors">
          {COLORS.map((c) => (
            <button
              key={c.name}
              className={`color-swatch${filters.colors.includes(c.name) ? " color-swatch--active" : ""}`}
              style={{ background: c.hex }}
              title={c.name}
              onClick={() => toggleArr("colors", c.name)}
            />
          ))}
        </div>
      </div>

      {/* Brand */}
      <div className="filter-section">
        <h4 className="filter-section__title">Brand</h4>
        <div className="filter-checks">
          {BRANDS.map((b) => (
            <label key={b} className="filter-check">
              <input
                type="checkbox"
                checked={filters.brands.includes(b)}
                onChange={() => toggleArr("brands", b)}
              />
              {b}
            </label>
          ))}
        </div>
      </div>
    </aside>
  );
}
