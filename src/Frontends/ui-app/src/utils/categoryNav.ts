import type { CategoryDto } from "../api/products";

/** Match category by slug anywhere in the tree (roots + nested `subCategories`). */
export function findCategoryIdBySlug(
  roots: CategoryDto[],
  slug: string
): string | null {
  const target = slug.trim().toLowerCase();
  function walk(nodes: CategoryDto[]): string | null {
    for (const n of nodes) {
      if ((n.slug ?? "").trim().toLowerCase() === target) return n.id;
      const subs = n.subCategories ?? [];
      if (subs.length) {
        const found = walk(subs);
        if (found) return found;
      }
    }
    return null;
  }
  return walk(roots);
}

export function categoryHref(slug: string) {
  return `/category/${encodeURIComponent(slug)}`;
}

export function findCategoryNameById(
  roots: CategoryDto[],
  id: string | null
): string | null {
  if (!id) return null;
  function walk(nodes: CategoryDto[]): string | null {
    for (const n of nodes) {
      if (n.id === id) return n.name;
      const f = walk(n.subCategories ?? []);
      if (f) return f;
    }
    return null;
  }
  return walk(roots);
}
