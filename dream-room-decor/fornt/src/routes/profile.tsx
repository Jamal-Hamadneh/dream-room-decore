import { createFileRoute, Link } from "@tanstack/react-router";
import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Heart, LogOut, MapPin, Package, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { useApp } from "@/context/AppContext";
import { useRequireAuth } from "@/hooks/useAuthGuard";
import { ProductCard } from "@/components/ProductCard";
import { useProducts } from "@/hooks/useCatalog";
import { api } from "@/lib/api";

export const Route = createFileRoute("/profile")({
  head: () => ({ meta: [{ title: "Profile — Dream Room Decor" }] }),
  component: ProfilePage,
});

function ProfilePage() {
  const { user, signOut, wishlist } = useApp();
  const { ready } = useRequireAuth();
  const { products } = useProducts();

  if (!ready || !user) return null;

  const wished = products.filter((p) => wishlist.includes(p.id));

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

      <Link
        to="/orders"
        className="mt-8 inline-flex items-center gap-2 rounded-full border border-border px-5 py-2.5 text-sm transition hover:bg-cream"
      >
        <Package className="h-4 w-4" /> View my orders
      </Link>

      <AddressesSection userId={user.id!} />

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

const EMPTY_ADDR = { country: "", city: "", street: "", building: "", postalCode: "" };

function AddressesSection({ userId }: { userId: number }) {
  const qc = useQueryClient();
  const [form, setForm] = useState(EMPTY_ADDR);
  const [adding, setAdding] = useState(false);
  const [saving, setSaving] = useState(false);

  const { data: addresses, isLoading } = useQuery({
    queryKey: ["addresses", userId],
    queryFn: async () => (await api.getAddresses()).filter((a) => a.userId === userId),
  });

  const refresh = () => qc.invalidateQueries({ queryKey: ["addresses", userId] });

  const save = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.country.trim() || !form.city.trim() || !form.street.trim()) {
      toast.error("Country, city and street are required.");
      return;
    }
    setSaving(true);
    try {
      await api.createAddress({
        userId,
        country: form.country,
        city: form.city,
        street: form.street,
        building: form.building || null,
        postalCode: form.postalCode || null,
        isDefault: (addresses?.length ?? 0) === 0,
      });
      setForm(EMPTY_ADDR);
      setAdding(false);
      await refresh();
      toast.success("Address added.");
    } catch {
      toast.error("Couldn't add the address.");
    } finally {
      setSaving(false);
    }
  };

  const remove = async (id: number) => {
    try {
      await api.deleteAddress(id);
      await refresh();
    } catch {
      toast.error("Couldn't delete the address.");
    }
  };

  return (
    <section className="mt-14">
      <div className="mb-6 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <MapPin className="h-5 w-5 text-accent" />
          <h2 className="font-display text-2xl">Addresses</h2>
        </div>
        <button
          onClick={() => setAdding((a) => !a)}
          className="inline-flex items-center gap-1.5 rounded-full border border-border px-4 py-2 text-sm transition hover:bg-cream"
        >
          <Plus className="h-4 w-4" /> {adding ? "Cancel" : "Add address"}
        </button>
      </div>

      {adding && (
        <form onSubmit={save} className="mb-6 grid gap-3 rounded-2xl border border-border bg-cream/40 p-5 sm:grid-cols-2">
          <AddrField label="Country" value={form.country} onChange={(v) => setForm((f) => ({ ...f, country: v }))} />
          <AddrField label="City" value={form.city} onChange={(v) => setForm((f) => ({ ...f, city: v }))} />
          <AddrField label="Street" className="sm:col-span-2" value={form.street} onChange={(v) => setForm((f) => ({ ...f, street: v }))} />
          <AddrField label="Building (optional)" value={form.building} onChange={(v) => setForm((f) => ({ ...f, building: v }))} />
          <AddrField label="Postal code (optional)" value={form.postalCode} onChange={(v) => setForm((f) => ({ ...f, postalCode: v }))} />
          <div className="sm:col-span-2">
            <button
              disabled={saving}
              className="rounded-full bg-primary px-6 py-2.5 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:opacity-60"
            >
              {saving ? "Saving…" : "Save address"}
            </button>
          </div>
        </form>
      )}

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading addresses…</p>
      ) : !addresses || addresses.length === 0 ? (
        <div className="rounded-xl border border-dashed border-border bg-cream/50 p-8 text-center text-sm text-muted-foreground">
          No saved addresses.
        </div>
      ) : (
        <ul className="grid gap-3 sm:grid-cols-2">
          {addresses.map((a) => (
            <li key={a.id} className="flex items-start justify-between gap-3 rounded-xl border border-border bg-background p-4">
              <div className="text-sm">
                <p className="font-medium">
                  {a.street}
                  {a.building ? `, ${a.building}` : ""}
                  {a.isDefault && <span className="ml-2 rounded-full bg-accent/15 px-2 py-0.5 text-xs text-accent">Default</span>}
                </p>
                <p className="text-muted-foreground">
                  {a.city}, {a.country}
                  {a.postalCode ? ` · ${a.postalCode}` : ""}
                </p>
              </div>
              <button
                onClick={() => remove(a.id)}
                className="rounded-full p-1.5 text-muted-foreground transition hover:bg-muted hover:text-destructive"
                aria-label="Delete address"
              >
                <Trash2 className="h-4 w-4" />
              </button>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

function AddrField({
  label,
  value,
  onChange,
  className = "",
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  className?: string;
}) {
  return (
    <label className={`block ${className}`}>
      <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">{label}</span>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none transition focus:border-foreground"
      />
    </label>
  );
}
