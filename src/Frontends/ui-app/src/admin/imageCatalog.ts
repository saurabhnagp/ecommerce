/** Static image catalog served from public/product-images/. */

export type CatalogImage = {
  label: string;
  path: string;
};

export const PRODUCT_IMAGES: CatalogImage[] = [
  { label: "Smartphone",       path: "/product-images/products/smartphone.svg" },
  { label: "Laptop",           path: "/product-images/products/laptop.svg" },
  { label: "Tablet",           path: "/product-images/products/tablet.svg" },
  { label: "Headphones",       path: "/product-images/products/headphones.svg" },
  { label: "Speaker",          path: "/product-images/products/speaker.svg" },
  { label: "Keyboard",         path: "/product-images/products/keyboard.svg" },
  { label: "Mouse",            path: "/product-images/products/mouse.svg" },
  { label: "Camera",           path: "/product-images/products/camera.svg" },
  { label: "Watch",            path: "/product-images/products/watch.svg" },
  { label: "Game Controller",  path: "/product-images/products/gaming-controller.svg" },
  { label: "T-Shirt",          path: "/product-images/products/tshirt.svg" },
  { label: "Shoes",            path: "/product-images/products/shoes.svg" },
  { label: "Backpack",         path: "/product-images/products/backpack.svg" },
  { label: "Sunglasses",       path: "/product-images/products/sunglasses.svg" },
  { label: "Wallet",           path: "/product-images/products/wallet.svg" },
  { label: "Perfume",          path: "/product-images/products/perfume.svg" },
  { label: "Book",             path: "/product-images/products/book.svg" },
  { label: "Water Bottle",     path: "/product-images/products/water-bottle.svg" },
];

export const CATEGORY_IMAGES: CatalogImage[] = [
  { label: "Electronics",      path: "/product-images/categories/electronics.svg" },
  { label: "Clothing",         path: "/product-images/categories/clothing.svg" },
  { label: "Accessories",      path: "/product-images/categories/accessories.svg" },
  { label: "Footwear",         path: "/product-images/categories/footwear.svg" },
  { label: "Books",            path: "/product-images/categories/books.svg" },
  { label: "Home & Kitchen",   path: "/product-images/categories/home-kitchen.svg" },
  { label: "Sports",           path: "/product-images/categories/sports.svg" },
  { label: "Beauty",           path: "/product-images/categories/beauty.svg" },
];

export const BRAND_IMAGES: CatalogImage[] = [
  { label: "Generic Brand",    path: "/product-images/brands/generic-brand.svg" },
  { label: "Tech Brand",       path: "/product-images/brands/tech-brand.svg" },
  { label: "Fashion Brand",    path: "/product-images/brands/fashion-brand.svg" },
  { label: "Sports Brand",     path: "/product-images/brands/sports-brand.svg" },
  { label: "Luxury Brand",     path: "/product-images/brands/luxury-brand.svg" },
];

export const AVATAR_IMAGES: CatalogImage[] = [
  { label: "Avatar 1 (Blue)",   path: "/product-images/avatars/avatar-1.svg" },
  { label: "Avatar 2 (Pink)",   path: "/product-images/avatars/avatar-2.svg" },
  { label: "Avatar 3 (Green)",  path: "/product-images/avatars/avatar-3.svg" },
  { label: "Avatar 4 (Amber)",  path: "/product-images/avatars/avatar-4.svg" },
  { label: "Avatar 5 (Violet)", path: "/product-images/avatars/avatar-5.svg" },
  { label: "Avatar 6 (Red)",    path: "/product-images/avatars/avatar-6.svg" },
];
