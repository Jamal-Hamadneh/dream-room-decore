import { createFileRoute } from "@tanstack/react-router";
import { useMemo, useState } from "react";
import { Search } from "lucide-react";
import { PRODUCTS, CATEGORIES, Category } from "@/data/products";
import { ProductCard } from "@/components/ProductCard";

type ShopSearch = {
  category?: Category | "All";
};

export const Route = createFileRoute("/shop/")({
  head: () => ({
    meta: [
      { title: "Shop — haus" },
      { name: "description", content: "Browse sofas, beds, chairs, tables and more. Filter by category and price." },
      { property: "og:title", content: "Shop — haus" },
      { property: "og:description", content: "Browse our full collection of Scandinavian furniture." },
    ],
  }),
  validateSearch: (search: Record<string, unknown>): ShopSearch => ({
    category: (search.category as Category | "All") ?? "All",
  }),
  component: ShopPage,
});

function ShopPage() {
  const { category: initialCategory } = Route.useSearch();
  const navigate = Route.useNavigate();
  const [query, setQuery] = useState("");
  const [maxPrice, setMaxPrice] = useState(1500);

  const category = initialCategory ?? "All";
  const setCategory = (c: Category | "All") =>
    navigate({ search: { category: c === "All" ? undefined : c } });

  const filtered = useMemo(() => {
    return PRODUCTS.filter((p) => {
      if (category !== "All" && p.category !== category) return false;
      if (p.price > maxPrice) return false;
      if (query && !p.name.toLowerCase().includes(query.toLowerCase())) return false;
      return true;
    });
  }, [category, maxPrice, query]);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <div className="mb-10">
        <h1 className="font-display text-4xl md:text-5xl">The collection</h1>
        <p className="mt-2 text-muted-foreground">
          {filtered.length} {filtered.length === 1 ? "piece" : "pieces"}
        </p>
      </div>

      <div className="grid gap-10 md:grid-cols-[240px_1fr]">
        {/* Filters */}
        <aside className="space-y-8">
          <div>
            <label className="relative block">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <input
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder="Search"
                className="w-full rounded-full border border-border bg-background py-2 pl-10 pr-4 text-sm outline-none transition focus:border-foreground"
              />
            </label>
          </div>

          <div>
            <h3 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
              Category
            </h3>
            <ul className="space-y-1.5">
              {(["All", ...CATEGORIES] as const).map((c) => (
                <li key={c}>
                  <button
                    onClick={() => setCategory(c)}
                    className={`w-full text-left text-sm transition ${
                      category === c ? "font-medium text-foreground" : "text-muted-foreground hover:text-foreground"
                    }`}
                  >
                    {c}
                  </button>
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h3 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
              Max price
            </h3>
            <input
              type="range"
              min={200}
              max={1500}
              step={50}
              value={maxPrice}
              onChange={(e) => setMaxPrice(Number(e.target.value))}
              className="w-full accent-[oklch(0.32_0.04_50)]"
            />
            <p className="mt-2 text-sm text-muted-foreground">Up to ${maxPrice}</p>
          </div>
        </aside>

        {/* Grid */}
        <div>
          {filtered.length === 0 ? (
            <div className="rounded-xl border border-dashed border-border bg-cream/50 p-12 text-center text-muted-foreground">
              No pieces match your filters.
            </div>
          ) : (
            <div className="grid grid-cols-1 gap-x-5 gap-y-10 sm:grid-cols-2 lg:grid-cols-3">
              {filtered.map((p) => (
                <ProductCard key={p.id} product={p} />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
