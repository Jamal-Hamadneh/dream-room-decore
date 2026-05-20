import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect } from "react";
import { Heart, LogOut } from "lucide-react";
import { useApp } from "@/context/AppContext";
import { PRODUCTS } from "@/data/products";
import { ProductCard } from "@/components/ProductCard";

export const Route = createFileRoute("/profile")({
  head: () => ({ meta: [{ title: "Profile — haus" }] }),
  component: ProfilePage,
});

function ProfilePage() {
  const { user, signOut, wishlist } = useApp();
  const navigate = useNavigate();

  useEffect(() => {
    if (!user) navigate({ to: "/login" });
  }, [user, navigate]);

  if (!user) return null;

  const wished = PRODUCTS.filter((p) => wishlist.includes(p.id));

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-xs uppercase tracking-widest text-muted-foreground">Account</p>
          <h1 className="mt-1 font-display text-4xl md:text-5xl">Hi, {user.name}</h1>
          <p className="mt-1 text-sm text-muted-foreground">{user.email}</p>
        </div>
        <button
          onClick={signOut}
          className="inline-flex items-center gap-2 rounded-full border border-border px-5 py-2.5 text-sm transition hover:bg-cream"
        >
          <LogOut className="h-4 w-4" /> Sign out
        </button>
      </div>

      <section className="mt-14">
        <div className="mb-6 flex items-center gap-2">
          <Heart className="h-5 w-5 text-accent" />
          <h2 className="font-display text-2xl">Wishlist</h2>
        </div>
        {wished.length === 0 ? (
          <div className="rounded-xl border border-dashed border-border bg-cream/50 p-12 text-center">
            <p className="text-muted-foreground">Nothing saved yet.</p>
            <Link to="/shop" className="mt-4 inline-block text-sm text-foreground underline-offset-4 hover:underline">
              Browse the collection →
            </Link>
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-x-5 gap-y-10 lg:grid-cols-4">
            {wished.map((p) => <ProductCard key={p.id} product={p} />)}
          </div>
        )}
      </section>
    </div>
  );
}
