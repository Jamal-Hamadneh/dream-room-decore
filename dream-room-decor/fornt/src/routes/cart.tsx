import { createFileRoute, Link } from "@tanstack/react-router";
import { Minus, Plus, X, ShoppingBag } from "lucide-react";
import { useApp } from "@/context/AppContext";

export const Route = createFileRoute("/cart")({
  head: () => ({
    meta: [
      { title: "Cart — haus" },
      { name: "description", content: "Review your cart and checkout." },
    ],
  }),
  component: CartPage,
});

function CartPage() {
  const { cart, updateQty, removeFromCart, cartTotal } = useApp();
  const shipping = cartTotal > 500 || cartTotal === 0 ? 0 : 49;

  if (cart.length === 0) {
    return (
      <div className="mx-auto max-w-3xl px-6 py-24 text-center">
        <ShoppingBag className="mx-auto h-10 w-10 text-muted-foreground" />
        <h1 className="mt-6 font-display text-4xl">Your cart is empty</h1>
        <p className="mt-3 text-muted-foreground">Find something you'll love for years.</p>
        <Link
          to="/shop"
          className="mt-8 inline-flex items-center justify-center rounded-full bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition hover:opacity-90"
        >
          Shop the collection
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
      <h1 className="font-display text-4xl md:text-5xl">Your cart</h1>

      <div className="mt-10 grid gap-10 lg:grid-cols-[1fr_360px]">
        <ul className="divide-y divide-border">
          {cart.map(({ product, qty }) => (
            <li key={product.id} className="flex gap-4 py-6">
              <Link
                to="/shop/$productId"
                params={{ productId: product.id }}
                className="block aspect-square w-24 shrink-0 overflow-hidden rounded-lg bg-cream/70 sm:w-32"
              >
                <img src={product.image} alt={product.name} className="h-full w-full object-cover" />
              </Link>
              <div className="flex flex-1 flex-col justify-between">
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <p className="text-xs uppercase tracking-widest text-muted-foreground">{product.category}</p>
                    <h3 className="mt-1 font-display text-lg">{product.name}</h3>
                  </div>
                  <button
                    onClick={() => removeFromCart(product.id)}
                    className="rounded-full p-1 text-muted-foreground transition hover:bg-muted hover:text-foreground"
                    aria-label="Remove"
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center rounded-full border border-border">
                    <button onClick={() => updateQty(product.id, qty - 1)} className="p-2 text-muted-foreground hover:text-foreground">
                      <Minus className="h-3.5 w-3.5" />
                    </button>
                    <span className="w-8 text-center text-sm">{qty}</span>
                    <button onClick={() => updateQty(product.id, qty + 1)} className="p-2 text-muted-foreground hover:text-foreground">
                      <Plus className="h-3.5 w-3.5" />
                    </button>
                  </div>
                  <p className="font-medium">${product.price * qty}</p>
                </div>
              </div>
            </li>
          ))}
        </ul>

        <aside className="h-fit rounded-2xl border border-border bg-cream/50 p-6">
          <h2 className="font-display text-xl">Summary</h2>
          <dl className="mt-6 space-y-3 text-sm">
            <div className="flex justify-between"><dt className="text-muted-foreground">Subtotal</dt><dd>${cartTotal}</dd></div>
            <div className="flex justify-between"><dt className="text-muted-foreground">Shipping</dt><dd>{shipping === 0 ? "Free" : `$${shipping}`}</dd></div>
            <div className="border-t border-border pt-3 flex justify-between text-base font-medium"><dt>Total</dt><dd>${cartTotal + shipping}</dd></div>
          </dl>
          <Link
            to="/checkout"
            className="mt-6 inline-flex w-full items-center justify-center rounded-full bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition hover:opacity-90"
          >
            Checkout
          </Link>
          <Link to="/shop" className="mt-3 block text-center text-xs text-muted-foreground hover:text-foreground">
            Continue shopping
          </Link>
        </aside>
      </div>
    </div>
  );
}
