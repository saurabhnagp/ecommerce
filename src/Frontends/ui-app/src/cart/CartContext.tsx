import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { fetchCart, type CartDto } from "../api/cart";
import { onAuthChange } from "../auth/notify";
import { getAccessToken } from "../auth/storage";

type CartContextValue = {
  cart: CartDto | null;
  loading: boolean;
  refreshCart: () => Promise<void>;
  itemCount: number;
};

const CartContext = createContext<CartContextValue | null>(null);

export function CartProvider({ children }: { children: ReactNode }) {
  const [cart, setCart] = useState<CartDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [authRev, setAuthRev] = useState(0);

  useEffect(() => onAuthChange(() => setAuthRev((n) => n + 1)), []);

  const refreshCart = useCallback(async () => {
    try {
      const data = await fetchCart();
      setCart(data);
    } catch {
      setCart(null);
    } finally {
      setLoading(false);
    }
  }, []);

  const token = getAccessToken();

  useEffect(() => {
    setLoading(true);
    void refreshCart();
  }, [refreshCart, authRev, token]);

  const itemCount = useMemo(
    () => cart?.items.reduce((n, i) => n + i.quantity, 0) ?? 0,
    [cart]
  );

  const value = useMemo(
    () => ({ cart, loading, refreshCart, itemCount }),
    [cart, loading, refreshCart, itemCount]
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error("useCart must be used within CartProvider");
  return ctx;
}
