import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { Check, Lock, CreditCard, ArrowLeft } from "lucide-react";
import { loadStripe } from "@stripe/stripe-js";
import {
  Elements,
  CardElement,
  useStripe,
  useElements,
} from "@stripe/react-stripe-js";
import { toast } from "sonner";
import { useApp } from "@/context/AppContext";
import { api } from "@/lib/api";

export const Route = createFileRoute("/checkout")({
  head: () => ({ meta: [{ title: "Checkout — Dream Room Decor" }] }),
  component: CheckoutPage,
});

// ── Types ──────────────────────────────────────────────────────────────────────
type Shipping = {
  fullName: string;
  phone: string;
  email: string;
  country: string;
  city: string;
  street: string;
  postalCode: string;
};

const EMPTY: Shipping = {
  fullName: "", phone: "", email: "",
  country: "", city: "", street: "", postalCode: "",
};

// ── Card element style ────────────────────────────────────────────────────────
const CARD_STYLE = {
  style: {
    base: {
      fontSize: "14px",
      color: "#1c1917",
      fontFamily: "Calibri, sans-serif",
      "::placeholder": { color: "#a8a29e" },
    },
    invalid: { color: "#dc2626" },
  },
};

// ── Main page ─────────────────────────────────────────────────────────────────
function CheckoutPage() {
  const { cart, cartTotal, user, clearCart } = useApp();
  const navigate = useNavigate();
  const [stripePromise, setStripePromise] = useState<ReturnType<typeof loadStripe> | null>(null);
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [orderId, setOrderId] = useState<number | null>(null);
  const [paymentIntentId, setPaymentIntentId] = useState<string | null>(null);
  const [shipping, setShipping] = useState<Shipping>(EMPTY);
  const [step, setStep] = useState<"shipping" | "payment" | "done">("shipping");
  const [busy, setBusy] = useState(false);
  const [stripeReady, setStripeReady] = useState(false);

  const shippingCost = cartTotal > 500 || cartTotal === 0 ? 0 : 49;
  const total = cartTotal + shippingCost;

  // Load Stripe config once
  useEffect(() => {
    api.stripe.getConfig().then((cfg) => {
      if (cfg.isConfigured) {
        setStripePromise(loadStripe(cfg.publishableKey));
        setStripeReady(true);
      }
    }).catch(() => {});
  }, []);

  if (cart.length === 0 && step !== "done") {
    navigate({ to: "/cart" });
    return null;
  }

  if (step === "done") {
    return (
      <div className="mx-auto max-w-2xl px-6 py-24 text-center">
        <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-accent/15">
          <Check className="h-8 w-8 text-accent" />
        </div>
        <h1 className="mt-6 font-display text-4xl">Order placed!</h1>
        <p className="mt-3 text-muted-foreground">
          Your payment was successful. We'll send a confirmation to your inbox.
        </p>
        <div className="mt-8 flex justify-center gap-3">
          <Link
            to="/orders"
            className="inline-flex items-center justify-center rounded-full bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition hover:opacity-90"
          >
            View my orders
          </Link>
          <Link
            to="/"
            className="inline-flex items-center justify-center rounded-full border border-border px-6 py-3 text-sm font-medium transition hover:bg-cream"
          >
            Back home
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
      <h1 className="font-display text-4xl md:text-5xl">Checkout</h1>

      {/* Step indicator */}
      <div className="mt-6 flex items-center gap-3 text-sm">
        <StepDot n={1} active={step === "shipping"} done={step === "payment"} label="Shipping" />
        <div className="h-px w-8 bg-border" />
        <StepDot n={2} active={step === "payment"} done={false} label="Payment" />
      </div>

      <div className="mt-8 grid gap-10 lg:grid-cols-[1fr_360px]">
        {/* Left column */}
        <div>
          {step === "shipping" ? (
            <ShippingForm
              shipping={shipping}
              setShipping={setShipping}
              total={total}
              user={user}
              cart={cart}
              setBusy={setBusy}
              busy={busy}
              onSuccess={(oid, cs, piid) => {
                setOrderId(oid);
                setClientSecret(cs);
                setPaymentIntentId(piid);
                setStep("payment");
              }}
            />
          ) : stripeReady && stripePromise && clientSecret ? (
            <Elements stripe={stripePromise}>
              <PaymentForm
                clientSecret={clientSecret}
                paymentIntentId={paymentIntentId!}
                total={total}
                onBack={() => setStep("shipping")}
                onSuccess={() => {
                  clearCart?.();
                  setStep("done");
                }}
              />
            </Elements>
          ) : (
            <div className="py-12 text-center text-muted-foreground text-sm">
              Loading payment form…
            </div>
          )}
        </div>

        {/* Order summary */}
        <OrderSummary cart={cart} cartTotal={cartTotal} shippingCost={shippingCost} total={total} />
      </div>
    </div>
  );
}

// ── Step 1: Shipping ──────────────────────────────────────────────────────────
function ShippingForm({
  shipping, setShipping, total, user, cart, busy, setBusy, onSuccess,
}: {
  shipping: Shipping;
  setShipping: React.Dispatch<React.SetStateAction<Shipping>>;
  total: number;
  user: ReturnType<typeof useApp>["user"];
  cart: ReturnType<typeof useApp>["cart"];
  busy: boolean;
  setBusy: (v: boolean) => void;
  onSuccess: (orderId: number, clientSecret: string, paymentIntentId: string) => void;
}) {
  const set = (k: keyof Shipping) => (v: string) =>
    setShipping((s) => ({ ...s, [k]: v }));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user?.id) { toast.error("Please sign in to checkout."); return; }
    setBusy(true);
    try {
      // 1. Save address
      const address = await api.createAddress({
        userId: user.id,
        country: shipping.country,
        city: shipping.city,
        street: shipping.street,
        postalCode: shipping.postalCode || null,
        isDefault: false,
      });

      // 2. Create order
      let order = await api.createOrder({
        userId: user.id,
        addressId: address.id,
        totalPrice: total,
        status: "pending",
        paymentStatus: "pending",
      });
      if (!order) {
        const all = await api.getOrders();
        order = all.filter((o) => o.userId === user.id && o.addressId === address.id)
          .sort((a, b) => b.id - a.id)[0] ?? null;
      }
      if (!order) throw new Error("Order could not be created.");

      // 3. Create order items
      await Promise.all(
        cart.map((line) =>
          api.createOrderItem({
            orderId: order!.id,
            productId: Number(line.product.id),
            productVariantId: line.variantId ?? null,
            quantity: line.qty,
            price: line.product.price,
          }),
        ),
      );

      // 4. Create Stripe payment intent
      const pi = await api.stripe.createPaymentIntent(order.id);
      onSuccess(order.id, pi.clientSecret, pi.paymentIntentId);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Could not prepare order.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <form onSubmit={submit} className="space-y-8">
      <section>
        <h2 className="mb-5 font-display text-2xl">Contact & shipping</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field label="Full name" required value={shipping.fullName} onChange={set("fullName")} />
          <Field label="Phone" type="tel" required value={shipping.phone} onChange={set("phone")} />
          <Field label="Email" type="email" required className="sm:col-span-2" value={shipping.email} onChange={set("email")} />
          <Field label="Street address" required className="sm:col-span-2" value={shipping.street} onChange={set("street")} />
          <Field label="City" required value={shipping.city} onChange={set("city")} />
          <Field label="Country" required value={shipping.country} onChange={set("country")} />
          <Field label="Postal code" value={shipping.postalCode} onChange={set("postalCode")} />
        </div>
      </section>

      <button
        disabled={busy}
        className="inline-flex w-full items-center justify-center gap-2 rounded-full bg-primary px-6 py-3.5 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:opacity-60"
      >
        <CreditCard className="h-4 w-4" />
        {busy ? "Preparing…" : "Continue to payment"}
      </button>
    </form>
  );
}

// ── Step 2: Stripe payment ────────────────────────────────────────────────────
function PaymentForm({
  clientSecret, paymentIntentId, total, onBack, onSuccess,
}: {
  clientSecret: string;
  paymentIntentId: string;
  total: number;
  onBack: () => void;
  onSuccess: () => void;
}) {
  const stripe = useStripe();
  const elements = useElements();
  const [busy, setBusy] = useState(false);
  const [cardError, setCardError] = useState<string | null>(null);

  const pay = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!stripe || !elements) return;
    const card = elements.getElement(CardElement);
    if (!card) return;

    setBusy(true);
    setCardError(null);

    const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
      payment_method: { card },
    });

    if (error) {
      setCardError(error.message ?? "Payment failed.");
      setBusy(false);
      return;
    }

    if (paymentIntent?.status === "succeeded" || paymentIntent?.status === "processing") {
      // Fire-and-forget — don't block the success screen on a backend sync call
      api.stripe.syncPaymentIntent(paymentIntentId).catch(() => {});
      onSuccess();
    } else {
      setCardError("Payment did not complete. Please try again.");
      setBusy(false);
    }
  };

  return (
    <form onSubmit={pay} className="space-y-8">
      <section>
        <div className="mb-5 flex items-center gap-3">
          <button
            type="button"
            onClick={onBack}
            className="rounded-full p-1.5 hover:bg-muted transition"
          >
            <ArrowLeft className="h-4 w-4" />
          </button>
          <h2 className="font-display text-2xl">Payment</h2>
        </div>

        <div className="rounded-xl border border-border bg-background p-4">
          <CardElement options={CARD_STYLE} />
        </div>

        {cardError && (
          <p className="mt-2 text-sm text-destructive">{cardError}</p>
        )}

        <p className="mt-3 flex items-center gap-1.5 text-xs text-muted-foreground">
          <Lock className="h-3 w-3" />
          Secured by Stripe — your card details are never stored on our servers.
        </p>

        <div className="mt-4 rounded-xl border border-border bg-cream/40 p-3 text-xs text-muted-foreground">
          <span className="font-medium text-foreground">Test card:</span>{" "}
          4242 4242 4242 4242 · any future date · any CVC
        </div>
      </section>

      <button
        disabled={busy || !stripe}
        className="inline-flex w-full items-center justify-center gap-2 rounded-full bg-primary px-6 py-3.5 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:opacity-60"
      >
        <Lock className="h-4 w-4" />
        {busy ? "Processing…" : `Pay $${total}`}
      </button>
    </form>
  );
}

// ── Shared components ─────────────────────────────────────────────────────────
function StepDot({ n, active, done, label }: { n: number; active: boolean; done: boolean; label: string }) {
  return (
    <div className="flex items-center gap-2">
      <div className={`flex h-6 w-6 items-center justify-center rounded-full text-xs font-bold
        ${done ? "bg-accent text-white" : active ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground"}`}
      >
        {done ? <Check className="h-3.5 w-3.5" /> : n}
      </div>
      <span className={`text-sm ${active ? "font-medium text-foreground" : "text-muted-foreground"}`}>{label}</span>
    </div>
  );
}

function OrderSummary({
  cart, cartTotal, shippingCost, total,
}: {
  cart: ReturnType<typeof useApp>["cart"];
  cartTotal: number;
  shippingCost: number;
  total: number;
}) {
  return (
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
        <div className="flex justify-between"><dt className="text-muted-foreground">Shipping</dt><dd>{shippingCost === 0 ? "Free" : `$${shippingCost}`}</dd></div>
        <div className="flex justify-between border-t border-border pt-2 font-medium"><dt>Total</dt><dd>${total}</dd></div>
      </dl>
    </aside>
  );
}

function Field({
  label, className = "", value, onChange, ...props
}: Omit<React.InputHTMLAttributes<HTMLInputElement>, "onChange"> & {
  label: string;
  onChange?: (v: string) => void;
}) {
  return (
    <label className={`block ${className}`}>
      <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">{label}</span>
      <input
        {...props}
        value={value}
        onChange={onChange ? (e) => onChange(e.target.value) : undefined}
        className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none transition focus:border-foreground"
      />
    </label>
  );
}
