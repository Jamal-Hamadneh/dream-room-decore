import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowRight, Sparkles, Truck, Leaf, Award } from "lucide-react";
import heroImg from "@/assets/hero-living-room.jpg";
import { ProductCard } from "@/components/ProductCard";
import { useProducts, useCategories } from "@/hooks/useCatalog";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "Dream Room Decor — Furniture for every home" },
      { name: "description", content: "Warm, considered furniture for everyday living. Shop sofas, beds, tables and design your room with AI." },
      { property: "og:title", content: "Dream Room Decor — Furniture for every home" },
      { property: "og:description", content: "Warm, considered furniture for everyday living." },
    ],
  }),
  component: HomePage,
});

const TINT_CYCLE = ["bg-cream", "bg-sand", "bg-muted"];
const categoryTints: Record<string, string> = {
  Sofas: "bg-cream",
  Beds: "bg-sand",
  Chairs: "bg-muted",
  Tables: "bg-cream",
  Wardrobes: "bg-sand",
  Lighting: "bg-muted",
  Storage: "bg-cream",
};
function categoryTint(name: string, idx: number) {
  return categoryTints[name] ?? TINT_CYCLE[idx % TINT_CYCLE.length];
}

function HomePage() {
  const { products } = useProducts();
  const { categories } = useCategories();
  const featured = products.slice(0, 4);

  return (
    <div>
      {/* Hero */}
      <section className="relative">
        <div className="mx-auto grid max-w-7xl gap-10 px-4 pb-16 pt-10 sm:px-6 md:grid-cols-12 md:pt-16 lg:px-8">
          <div className="md:col-span-5 md:pt-12">
            <p className="mb-4 inline-flex items-center gap-2 rounded-full border border-border bg-cream px-3 py-1 text-xs uppercase tracking-widest text-muted-foreground">
              <Sparkles className="h-3 w-3 text-accent" /> Spring collection
            </p>
            <h1 className="font-display text-5xl leading-[1.05] text-balance md:text-6xl lg:text-7xl">
              A home, slowly made.
            </h1>
            <p className="mt-6 max-w-md text-lg text-muted-foreground text-pretty">
              Honest oak, washed linen, and quiet shapes. Furniture you can live with for years —
              and now, design with AI before you buy.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Link
                to="/shop"
                className="inline-flex items-center gap-2 rounded-full bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition hover:opacity-90"
              >
                Shop the collection <ArrowRight className="h-4 w-4" />
              </Link>
              <Link
                to="/ai-room"
                className="inline-flex items-center gap-2 rounded-full border border-border bg-background px-6 py-3 text-sm font-medium transition hover:bg-cream"
              >
                <Sparkles className="h-4 w-4 text-accent" /> Try AI Room
              </Link>
            </div>
          </div>
          <div className="md:col-span-7">
            <div className="relative overflow-hidden rounded-2xl shadow-[var(--shadow-warm)]">
              <img
                src={heroImg}
                alt="Warm Scandinavian living room"
                width={1600}
                height={1100}
                className="h-full w-full object-cover"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Categories */}
      <section className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="mb-8 flex items-end justify-between">
          <h2 className="font-display text-3xl md:text-4xl">Shop by room</h2>
          <Link to="/shop" className="text-sm text-muted-foreground hover:text-foreground">
            View all →
          </Link>
        </div>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-7">
          {categories.map((c, i) => (
            <Link
              key={c}
              to="/shop"
              search={{ category: c }}
              className={`group flex aspect-square flex-col items-center justify-center rounded-xl border border-border/40 ${categoryTint(c, i)} p-4 text-center transition hover:-translate-y-0.5 hover:shadow-[var(--shadow-soft)]`}
            >
              <span className="font-display text-lg group-hover:text-accent">{c}</span>
            </Link>
          ))}
        </div>
      </section>

      {/* Featured */}
      <section className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="mb-8 flex items-end justify-between">
          <div>
            <p className="text-xs uppercase tracking-widest text-muted-foreground">New & noteworthy</p>
            <h2 className="mt-1 font-display text-3xl md:text-4xl">Featured pieces</h2>
          </div>
          <Link to="/shop" className="text-sm text-muted-foreground hover:text-foreground">
            All products →
          </Link>
        </div>
        <div className="grid grid-cols-2 gap-x-5 gap-y-10 lg:grid-cols-4">
          {featured.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
      </section>

      {/* AI banner */}
      <section className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="overflow-hidden rounded-2xl bg-primary text-primary-foreground">
          <div className="grid items-center gap-8 p-8 md:grid-cols-2 md:p-14">
            <div>
              <p className="mb-3 inline-flex items-center gap-2 text-xs uppercase tracking-widest text-primary-foreground/70">
                <Sparkles className="h-3 w-3" /> AI Room
              </p>
              <h2 className="font-display text-3xl md:text-4xl">
                See it in your space, before you buy.
              </h2>
              <p className="mt-4 max-w-md text-primary-foreground/80">
                Upload a photo of your room, pick a few pieces, and we'll show you how they look together.
              </p>
              <Link
                to="/ai-room"
                className="mt-6 inline-flex items-center gap-2 rounded-full bg-background px-6 py-3 text-sm font-medium text-foreground transition hover:bg-cream"
              >
                Try it now <ArrowRight className="h-4 w-4" />
              </Link>
            </div>
            <div className="relative aspect-[4/3] overflow-hidden rounded-xl">
              <img
                src={heroImg}
                alt="AI room preview"
                loading="lazy"
                width={1280}
                height={896}
                className="h-full w-full object-cover"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Promises */}
      <section className="mx-auto grid max-w-7xl gap-6 px-4 py-12 sm:grid-cols-3 sm:px-6 lg:px-8">
        {[
          { icon: Truck, title: "Free delivery", text: "On orders over $500" },
          { icon: Leaf, title: "FSC certified", text: "Responsibly sourced wood" },
          { icon: Award, title: "10-year warranty", text: "Built to outlast trends" },
        ].map(({ icon: Icon, title, text }) => (
          <div key={title} className="rounded-xl border border-border/60 bg-cream/50 p-6">
            <Icon className="h-6 w-6 text-accent" />
            <h3 className="mt-4 font-display text-lg">{title}</h3>
            <p className="text-sm text-muted-foreground">{text}</p>
          </div>
        ))}
      </section>
    </div>
  );
}
