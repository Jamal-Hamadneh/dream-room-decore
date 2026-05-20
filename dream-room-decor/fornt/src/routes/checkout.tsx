import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useState } from "react";
import { Check, Lock } from "lucide-react";
import { useApp } from "@/context/AppContext";

export const Route = createFileRoute("/checkout")({
  head: () => ({ meta: [{ title: "Checkout — haus" }] }),
  component: CheckoutPage,
});

function CheckoutPage() {
  const { cart, cartTotal, clearCart } = useApp();
  const navigate = useNavigate();
  const [placing, setPlacing] = useState(false);
  const [done, setDone] = useState(false);

  const shipping = cartTotal > 500 || cartTotal === 0 ? 0 : 49;
  const total = cartTotal + shipping;

  const onSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPlacing(true);
    setTimeout(() => {
      clearCart();
      setDone(true);
    }, 1200);
  };

  if (done) {
    return (
      <div className="mx-auto max-w-2xl px-6 py-24 text-center">
        <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-accent/15">
          <Check className="h-7 w-7 text-accent" />
        </div>
        <h1 className="mt-6 font-display text-4xl">Thank you</h1>
        <p className="mt-3 text-muted-foreground">
          Your order is on its way. We've sent a confirmation to your inbox.
        </p>
        <Link
          to="/"
          className="mt-8 inline-flex items-center justify-center rounded-full bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition hover:opacity-90"
        >
          Back home
        </Link>
      </div>
    );
  }

  if (cart.length === 0) {
    navigate({ to: "/cart" });
    return null;
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
      <h1 className="font-display text-4xl md:text-5xl">Checkout</h1>

      <div className="mt-10 grid gap-10 lg:grid-cols-[1fr_360px]">
        <form onSubmit={onSubmit} className="space-y-10">
          <Section title="Contact & shipping">
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Full name" required />
              <Field label="Phone" type="tel" required />
              <Field label="Email" type="email" required className="sm:col-span-2" />
              <Field label="Address" required className="sm:col-span-2" />
              <Field label="City" required />
              <Field label="Postal code" required />
            </div>
          </Section>

          <Section title="Payment">
            <div className="grid gap-4">
              <Field label="Card number" placeholder="4242 4242 4242 4242" required />
              <div className="grid grid-cols-2 gap-4">
                <Field label="Expiry" placeholder="MM / YY" required />
                <Field label="CVC" placeholder="123" required />
              </div>
              <p className="inline-flex items-center gap-2 text-xs text-muted-foreground">
                <Lock className="h-3 w-3" /> Mock checkout — no real payment is processed.
              </p>
            </div>
          </Section>

          <button
            disabled={placing}
            className="inline-flex w-full items-center justify-center rounded-full bg-primary px-6 py-3.5 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:opacity-60"
          >
            {placing ? "Placing order…" : `Place order — $${total}`}
          </button>
        </form>

        <aside className="h-fit rounded-2xl border border-border bg-cream/50 p-6">
          <h2 className="font-display text-xl">Order summary</h2>
          <ul className="mt-5 space-y-4">
            {cart.map(({ product, qty }) => (
              <li key={product.id} className="flex gap-3">
                <div className="h-14 w-14 shrink-0 overflow-hidden rounded-md bg-cream">
                  <img src={product.image} alt={product.name} className="h-full w-full object-cover" />
                </div>
                <div className="flex flex-1 items-start justify-between gap-2 text-sm">
                  <div>
                    <p className="font-medium">{product.name}</p>
                    <p className="text-xs text-muted-foreground">Qty {qty}</p>
                  </div>
                  <p>${product.price * qty}</p>
                </div>
              </li>
            ))}
          </ul>
          <dl className="mt-6 space-y-2 border-t border-border pt-4 text-sm">
            <div className="flex justify-between"><dt className="text-muted-foreground">Subtotal</dt><dd>${cartTotal}</dd></div>
            <div className="flex justify-between"><dt className="text-muted-foreground">Shipping</dt><dd>{shipping === 0 ? "Free" : `$${shipping}`}</dd></div>
            <div className="flex justify-between border-t border-border pt-2 font-medium"><dt>Total</dt><dd>${total}</dd></div>
          </dl>
        </aside>
      </div>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section>
      <h2 className="mb-5 font-display text-2xl">{title}</h2>
      {children}
    </section>
  );
}

function Field({
  label,
  className = "",
  ...props
}: React.InputHTMLAttributes<HTMLInputElement> & { label: string }) {
  return (
    <label className={`block ${className}`}>
      <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">{label}</span>
      <input
        {...props}
        className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none transition focus:border-foreground"
      />
    </label>
  );
}
