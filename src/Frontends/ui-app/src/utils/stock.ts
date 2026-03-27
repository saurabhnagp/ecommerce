/** Sellable when inventory is not tracked or quantity &gt; 0. */
export function productInStock(p: {
  trackInventory?: boolean;
  quantity?: number;
}): boolean {
  if (p.trackInventory === false) return true;
  if (p.trackInventory === undefined && (p.quantity === undefined || p.quantity === null)) return true;
  return (p.quantity ?? 0) > 0;
}

/** True when tracked and 0 &lt; qty &lt;= threshold (for optional “low stock” styling). */
export function productIsLowStock(p: {
  trackInventory?: boolean;
  quantity?: number;
  lowStockThreshold?: number;
}): boolean {
  if (!p.trackInventory) return false;
  const q = p.quantity ?? 0;
  const t = p.lowStockThreshold ?? 5;
  return q > 0 && q <= t;
}
