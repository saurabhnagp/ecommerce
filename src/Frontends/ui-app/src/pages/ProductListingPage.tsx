import { useState, useMemo } from "react";
import { FilterSidebar } from "../components/FilterSidebar";
import { ProductGrid } from "../components/ProductGrid";
import type { Filters } from "../components/FilterSidebar";
import type { Product } from "../components/ProductCard";
import "./ProductListingPage.css";

const SAMPLE_PRODUCTS: Product[] = [
  { id: "1",  name: "Lacoste Cotton Shirt Slim Fit",     price: 900,  originalPrice: 950,  image: "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=400&h=460&fit=crop", badge: "NEW",   tab: "new",         rating: 4 },
  { id: "2",  name: "Striped Polo Collar T-Shirt",       price: 900,  originalPrice: 1800, image: "https://images.unsplash.com/photo-1625910513413-5fc421e0fd4f?w=400&h=460&fit=crop", badge: "- 50%", tab: "new",         rating: 5 },
  { id: "3",  name: "Classic Crew-Neck Tee",             price: 900,  image: "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400&h=460&fit=crop",                                      tab: "new",         rating: 4 },
  { id: "4",  name: "Full-Sleeve Knit Pullover",         price: 900,  image: "https://images.unsplash.com/photo-1614975059251-992f11792571?w=400&h=460&fit=crop",                                      tab: "new",         rating: 3 },
  { id: "5",  name: "Leather Textured Wallet",           price: 1200, image: "https://images.unsplash.com/photo-1627123424574-724758594e93?w=400&h=460&fit=crop",                                      tab: "new",         rating: 5 },
  { id: "6",  name: "Designer Clutch Bag – Red",         price: 1500, originalPrice: 2000, image: "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=400&h=460&fit=crop", badge: "- 25%", tab: "new",         rating: 4 },
  { id: "7",  name: "Elegant Chain Handbag",             price: 2800, image: "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=400&h=460&fit=crop",                                      tab: "new",         rating: 4 },
  { id: "8",  name: "Gucci Classic Round Watch",         price: 4500, originalPrice: 5500, image: "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=400&h=460&fit=crop", badge: "- 18%", tab: "new",         rating: 5 },

  { id: "9",  name: "Casual Denim Jacket",               price: 2200, image: "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=400&h=460&fit=crop",                                      tab: "featured",    rating: 4 },
  { id: "10", name: "Women's Summer Dress",              price: 1800, originalPrice: 2400, image: "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=400&h=460&fit=crop", badge: "- 25%", tab: "featured",    rating: 5 },
  { id: "11", name: "Premium Aviator Sunglasses",        price: 1300, image: "https://images.unsplash.com/photo-1511499767150-a48a237f0083?w=400&h=460&fit=crop",                                      tab: "featured",    rating: 4 },
  { id: "12", name: "Slim Fit Chino Trousers",           price: 1600, image: "https://images.unsplash.com/photo-1624378439575-d8705ad7ae80?w=400&h=460&fit=crop",                                      tab: "featured",    rating: 3 },

  { id: "13", name: "Running Sneakers – Blue",           price: 2500, originalPrice: 3200, image: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=460&fit=crop", badge: "- 22%", tab: "special",     rating: 5 },
  { id: "14", name: "Leather Belt – Brown",              price: 650,  image: "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400&h=460&fit=crop",                                      tab: "special",     rating: 4 },
  { id: "15", name: "Formal Oxford Shoes",               price: 3400, image: "https://images.unsplash.com/photo-1614252369475-531eba835eb1?w=400&h=460&fit=crop",                                      tab: "special",     rating: 4 },
  { id: "16", name: "Canvas Tote Bag – Navy",            price: 800,  originalPrice: 1100, image: "https://images.unsplash.com/photo-1622560480605-d83c853bc5c3?w=400&h=460&fit=crop", badge: "- 27%", tab: "special",     rating: 3 },

  { id: "17", name: "Oversized Cotton Hoodie",           price: 1400, image: "https://images.unsplash.com/photo-1556821840-3a63f95609a7?w=400&h=460&fit=crop",                                      tab: "bestsellers", rating: 5 },
  { id: "18", name: "Puma Sports Watch – Black",         price: 3800, originalPrice: 4500, image: "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&h=460&fit=crop", badge: "- 16%", tab: "bestsellers", rating: 4 },
  { id: "19", name: "Graphic Print Backpack",            price: 1100, image: "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400&h=460&fit=crop",                                      tab: "bestsellers", rating: 4 },
  { id: "20", name: "Classic Linen Shirt – White",       price: 1250, image: "https://images.unsplash.com/photo-1598033129183-c4f50c736c10?w=400&h=460&fit=crop", badge: "NEW",                       tab: "bestsellers", rating: 5 },
];

const DEFAULT_FILTERS: Filters = {
  category: null,
  subcategory: null,
  priceMin: 0,
  priceMax: 5000,
  sizes: [],
  colors: [],
  brands: [],
  sort: "newest",
};

export function ProductListingPage() {
  const [filters, setFilters] = useState<Filters>(DEFAULT_FILTERS);

  const visibleProducts = useMemo(() => {
    let list = [...SAMPLE_PRODUCTS];

    if (filters.priceMin > 0) list = list.filter((p) => p.price >= filters.priceMin);
    if (filters.priceMax < 5000) list = list.filter((p) => p.price <= filters.priceMax);

    switch (filters.sort) {
      case "price-asc":
        list.sort((a, b) => a.price - b.price);
        break;
      case "price-desc":
        list.sort((a, b) => b.price - a.price);
        break;
      case "name-asc":
        list.sort((a, b) => a.name.localeCompare(b.name));
        break;
    }

    return list;
  }, [filters]);

  return (
    <div className="product-listing">
      <FilterSidebar filters={filters} onChange={setFilters} />

      <section className="product-listing__main">
        <nav className="listing-breadcrumb">
          <a href="/">Home</a>
          <span className="listing-breadcrumb__sep">/</span>
          <span>Products</span>
        </nav>

        <ProductGrid products={visibleProducts} />
      </section>
    </div>
  );
}
