import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  ReactNode,
} from "react";
import { toast } from "sonner";
import { Product } from "@/data/products";
import { api, getStoredUser, setUnauthorizedListener, type StoredUser } from "@/lib/api";
import { resolveImage } from "@/lib/catalog";
import type { CartItemSummary } from "@/lib/types";

export type CartItem = {
  product: Product;
  qty: number;
  // Backend bookkeeping (present only for logged-in users):
  itemId?: number;
  variantId?: number | null;
};

type User = { id?: number; name: string; email: string; role: string } | null;

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
  isAdmin: boolean;
  authReady: boolean;
  signIn: (email: string, password: string) => Promise<void>;
  signUp: (firstName: string, lastName: string, email: string, password: string) => Promise<void>;
  signOut: () => void;

  // Exposed so checkout can place a real order.
  cartId: number | null;
};

const AppContext = createContext<AppContextValue | null>(null);

const LS_CART = "drd_cart";
const LS_WISH = "drd_wish";

function toUser(stored: StoredUser | null): User {
  if (!stored) return null;
  const name = `${stored.firstName} ${stored.lastName}`.trim() || stored.email;
  return { id: stored.id, name, email: stored.email, role: stored.role };
}

function cartItemToProduct(item: CartItemSummary): Product {
  return {
    id: String(item.productId),
    name: item.productName,
    category: "",
    price: item.unitPrice,
    image: resolveImage(item.productImageUrl, item.productName, item.productId),
    description: "",
    features: [],
  };
}

export function AppProvider({ children }: { children: ReactNode }) {
  const [cart, setCart] = useState<CartItem[]>([]);
  const [wishlist, setWishlist] = useState<string[]>([]);
  const [user, setUser] = useState<User>(null);
  const [authReady, setAuthReady] = useState(false);
  const [cartId, setCartId] = useState<number | null>(null);

  // productId(string) -> favorite row id, for deletes.
  const favoriteIds = useRef<Record<string, number>>({});

  const isAuthed = !!user?.id;

  // --- backend loaders ----------------------------------------------------

  const loadFavorites = useCallback(async (userId: number) => {
    try {
      const favs = (await api.getFavorites()).filter((f) => f.userId === userId);
      const map: Record<string, number> = {};
      const ids: string[] = [];
      for (const f of favs) {
        const pid = String(f.productId);
        map[pid] = f.id;
        ids.push(pid);
      }
      favoriteIds.current = map;
      setWishlist(ids);
    } catch {
      /* leave wishlist as-is */
    }
  }, []);

  const loadCart = useCallback(async (userId: number) => {
    try {
      const carts = await api.getCarts();
      let cartRow = carts.find((c) => c.userId === userId);
      if (!cartRow) {
        // createCart may 500-after-persist; re-fetch to get the real row.
        cartRow = (await api.createCart(userId)) ?? undefined;
        if (!cartRow) {
          const refreshed = await api.getCarts();
          cartRow = refreshed.find((c) => c.userId === userId);
        }
      }
      if (!cartRow) return;
      setCartId(cartRow.id);
      const lines: CartItem[] = cartRow.items.map((item) => ({
        product: cartItemToProduct(item),
        qty: item.quantity,
        itemId: item.id,
        variantId: item.productVariantId ?? null,
      }));
      setCart(lines);
    } catch {
      /* leave cart as-is */
    }
  }, []);

  const reloadCart = useCallback(async () => {
    if (user?.id) await loadCart(user.id);
  }, [user, loadCart]);

  // --- hydrate session on mount ------------------------------------------

  useEffect(() => {
    const stored = getStoredUser();
    if (stored) {
      setUser(toUser(stored));
    } else {
      // guest: hydrate cart/wishlist from localStorage
      try {
        const c = localStorage.getItem(LS_CART);
        const w = localStorage.getItem(LS_WISH);
        if (c) setCart(JSON.parse(c));
        if (w) setWishlist(JSON.parse(w));
      } catch {
        /* noop */
      }
    }
    setAuthReady(true);
  }, []);

  // When a user becomes authenticated, pull their cart + favorites.
  useEffect(() => {
    if (user?.id) {
      void loadCart(user.id);
      void loadFavorites(user.id);
    }
  }, [user, loadCart, loadFavorites]);

  // If the session is dropped (refresh failed), sign out locally.
  useEffect(() => {
    setUnauthorizedListener(() => {
      setUser(null);
      setCart([]);
      setWishlist([]);
      setCartId(null);
      favoriteIds.current = {};
      toast.error("Your session expired. Please sign in again.");
    });
    return () => setUnauthorizedListener(null);
  }, []);

  // Guest persistence only (logged-in state lives in the backend).
  useEffect(() => {
    if (!isAuthed) localStorage.setItem(LS_CART, JSON.stringify(cart));
  }, [cart, isAuthed]);
  useEffect(() => {
    if (!isAuthed) localStorage.setItem(LS_WISH, JSON.stringify(wishlist));
  }, [wishlist, isAuthed]);

  // --- auth ---------------------------------------------------------------

  const signIn = useCallback(async (email: string, password: string) => {
    const auth = await api.login({ email, password });
    setUser(toUser(auth));
  }, []);

  const signUp = useCallback(
    async (firstName: string, lastName: string, email: string, password: string) => {
      const auth = await api.register({ firstName, lastName, email, password });
      setUser(toUser(auth));
    },
    [],
  );

  const signOut = useCallback(() => {
    void api.logout();
    setUser(null);
    setCart([]);
    setWishlist([]);
    setCartId(null);
    favoriteIds.current = {};
    try {
      localStorage.removeItem(LS_CART);
      localStorage.removeItem(LS_WISH);
    } catch {
      /* noop */
    }
  }, []);

  // --- cart ---------------------------------------------------------------

  const ensureCart = useCallback(async (): Promise<number | null> => {
    if (!user?.id) return null;
    if (cartId) return cartId;
    const carts = await api.getCarts();
    let row = carts.find((c) => c.userId === user.id);
    if (!row) {
      row = (await api.createCart(user.id)) ?? undefined;
      if (!row) {
        const refreshed = await api.getCarts();
        row = refreshed.find((c) => c.userId === user.id);
      }
    }
    if (!row) return null;
    setCartId(row.id);
    return row.id;
  }, [user, cartId]);

  const addToCart = useCallback(
    (product: Product, qty = 1) => {
      // optimistic UI update
      setCart((prev) => {
        const existing = prev.find((i) => i.product.id === product.id);
        if (existing) {
          return prev.map((i) =>
            i.product.id === product.id ? { ...i, qty: i.qty + qty } : i,
          );
        }
        return [...prev, { product, qty }];
      });

      if (!isAuthed) return;

      void (async () => {
        try {
          const cid = await ensureCart();
          if (!cid) return;
          const productId = Number(product.id);
          const existing = cart.find((i) => i.product.id === product.id);
          if (existing?.itemId) {
            await api.updateCartItem(existing.itemId, cid, productId, existing.qty + qty, existing.variantId);
          } else {
            await api.addCartItem(cid, productId, qty, null);
          }
          await reloadCart();
        } catch {
          toast.error("Couldn't update your cart.");
          await reloadCart();
        }
      })();
    },
    [isAuthed, ensureCart, cart, reloadCart],
  );

  const removeFromCart = useCallback(
    (id: string) => {
      const line = cart.find((i) => i.product.id === id);
      setCart((prev) => prev.filter((i) => i.product.id !== id));
      if (!isAuthed || !line?.itemId) return;
      void (async () => {
        try {
          await api.removeCartItem(line.itemId!);
        } catch {
          toast.error("Couldn't remove the item.");
          await reloadCart();
        }
      })();
    },
    [cart, isAuthed, reloadCart],
  );

  const updateQty = useCallback(
    (id: string, qty: number) => {
      const line = cart.find((i) => i.product.id === id);
      setCart((prev) =>
        prev
          .map((i) => (i.product.id === id ? { ...i, qty: Math.max(0, qty) } : i))
          .filter((i) => i.qty > 0),
      );
      if (!isAuthed || !line?.itemId || !cartId) return;
      void (async () => {
        try {
          if (qty <= 0) {
            await api.removeCartItem(line.itemId!);
          } else {
            await api.updateCartItem(line.itemId!, cartId, Number(id), qty, line.variantId);
          }
        } catch {
          toast.error("Couldn't update the quantity.");
          await reloadCart();
        }
      })();
    },
    [cart, isAuthed, cartId, reloadCart],
  );

  const clearCart = useCallback(() => {
    const lines = cart;
    setCart([]);
    if (!isAuthed) return;
    void (async () => {
      try {
        await Promise.all(lines.filter((l) => l.itemId).map((l) => api.removeCartItem(l.itemId!)));
      } catch {
        /* best-effort */
      }
    })();
  }, [cart, isAuthed]);

  // --- wishlist / favorites ----------------------------------------------

  const toggleWishlist = useCallback(
    (id: string) => {
      const currentlyWished = wishlist.includes(id);
      // optimistic
      setWishlist((prev) => (currentlyWished ? prev.filter((x) => x !== id) : [...prev, id]));

      if (!isAuthed || !user?.id) return;

      void (async () => {
        try {
          if (currentlyWished) {
            const favId = favoriteIds.current[id];
            if (favId) {
              await api.removeFavorite(favId);
              delete favoriteIds.current[id];
            }
          } else {
            // addFavorite may 500-after-persist (returns null), so re-fetch
            // favorites to capture the new row id for later deletes.
            await api.addFavorite(user.id!, Number(id));
            await loadFavorites(user.id!);
          }
        } catch {
          toast.error("Couldn't update your wishlist.");
          // rollback
          setWishlist((prev) => (currentlyWished ? [...prev, id] : prev.filter((x) => x !== id)));
        }
      })();
    },
    [wishlist, isAuthed, user, loadFavorites],
  );

  const isWishlisted = useCallback((id: string) => wishlist.includes(id), [wishlist]);

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
      isAdmin: user?.role === "admin",
      authReady,
      signIn,
      signUp,
      signOut,
      cartId,
    }),
    [
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
      authReady,
      signIn,
      signUp,
      signOut,
      cartId,
    ],
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useApp() {
  const ctx = useContext(AppContext);
  if (!ctx) throw new Error("useApp must be used within AppProvider");
  return ctx;
}
