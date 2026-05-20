import { createContext, useContext, useEffect, useMemo, useState, ReactNode } from "react";
import { Product } from "@/data/products";

type CartItem = { product: Product; qty: number };

type User = { name: string; email: string } | null;

type AppContextValue = {
  cart: CartItem[];
  addToCart: (product: Product, qty?: number) => void;
  removeFromCart: (id: string) => void;
  updateQty: (id: string, qty: number) => void;
  clearCart: () => void;
  cartCount: number;
  cartTotal: number;

  wishlist: string[];
  toggleWishlist: (id: string) => void;
  isWishlisted: (id: string) => boolean;

  user: User;
  signIn: (email: string, name?: string) => void;
  signOut: () => void;
};

const AppContext = createContext<AppContextValue | null>(null);

const LS_CART = "haus_cart";
const LS_WISH = "haus_wish";
const LS_USER = "haus_user";

export function AppProvider({ children }: { children: ReactNode }) {
  const [cart, setCart] = useState<CartItem[]>([]);
  const [wishlist, setWishlist] = useState<string[]>([]);
  const [user, setUser] = useState<User>(null);

  // hydrate
  useEffect(() => {
    try {
      const c = localStorage.getItem(LS_CART);
      const w = localStorage.getItem(LS_WISH);
      const u = localStorage.getItem(LS_USER);
      if (c) setCart(JSON.parse(c));
      if (w) setWishlist(JSON.parse(w));
      if (u) setUser(JSON.parse(u));
    } catch {
      /* noop */
    }
  }, []);

  useEffect(() => {
    localStorage.setItem(LS_CART, JSON.stringify(cart));
  }, [cart]);
  useEffect(() => {
    localStorage.setItem(LS_WISH, JSON.stringify(wishlist));
  }, [wishlist]);
  useEffect(() => {
    if (user) localStorage.setItem(LS_USER, JSON.stringify(user));
    else localStorage.removeItem(LS_USER);
  }, [user]);

  const addToCart = (product: Product, qty = 1) => {
    setCart((prev) => {
      const existing = prev.find((i) => i.product.id === product.id);
      if (existing) {
        return prev.map((i) =>
          i.product.id === product.id ? { ...i, qty: i.qty + qty } : i,
        );
      }
      return [...prev, { product, qty }];
    });
  };

  const removeFromCart = (id: string) =>
    setCart((prev) => prev.filter((i) => i.product.id !== id));

  const updateQty = (id: string, qty: number) =>
    setCart((prev) =>
      prev
        .map((i) => (i.product.id === id ? { ...i, qty: Math.max(0, qty) } : i))
        .filter((i) => i.qty > 0),
    );

  const clearCart = () => setCart([]);

  const toggleWishlist = (id: string) =>
    setWishlist((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));

  const isWishlisted = (id: string) => wishlist.includes(id);

  const signIn = (email: string, name?: string) =>
    setUser({ email, name: name ?? email.split("@")[0] });
  const signOut = () => setUser(null);

  const cartCount = cart.reduce((s, i) => s + i.qty, 0);
  const cartTotal = cart.reduce((s, i) => s + i.qty * i.product.price, 0);

  const value = useMemo(
    () => ({
      cart,
      addToCart,
      removeFromCart,
      updateQty,
      clearCart,
      cartCount,
      cartTotal,
      wishlist,
      toggleWishlist,
      isWishlisted,
      user,
      signIn,
      signOut,
    }),
    [cart, wishlist, user, cartCount, cartTotal],
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useApp() {
  const ctx = useContext(AppContext);
  if (!ctx) throw new Error("useApp must be used within AppProvider");
  return ctx;
}
