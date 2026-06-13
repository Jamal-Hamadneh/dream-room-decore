import { createFileRoute, Link, useRouter } from "@tanstack/react-router";
import { useState } from "react";
import { ArrowLeft, Heart, Minus, Plus, ShoppingBag, Check } from "lucide-react";
import { findProduct } from "@/data/products";
import { useApp } from "@/context/AppContext";
import { ProductCard } from "@/components/ProductCard";
import { useProduct, useProducts } from "@/hooks/useCatalog";
import { ProductReviews } from "@/components/ProductReviews";

export const Route = createFileRoute("/shop/$productId")({
  head: ({ params }) => {
    const product = findProduct(params.productId);
    return {
      meta: [
        { title: product ? `${product.name} — Dream Room Decor` : "Product — Dream Room Decor" },
        { name: "description", content: product?.description ?? "Furniture detail" },
        { property: "og:title", content: product?.name ?? "Dream Room Decor" },
        { property: "og:description", content: product?.description ?? "" },
        ...(product ? [{ property: "og:image", content: product.image }] : []),
      ],
    };
  },
  notFoundComponent: () => (
    <div className="mx-auto max-w-3xl px-6 py-24 text-center">
      <h1 className="font-display text-3xl">Product not found</h1>
      <Link to="/shop" className="mt-6 inline-block text-accent">← Back to shop</Link>
    </div>
  ),
  component: ProductPage,
});

function ProductPage() {
  const { productId } = Route.useParams();
  const router = useRouter();
  const { product, isLoading } = useProduct(productId);
  const { products } = useProducts();
  const { addToCart, toggleWishlist, isWishlisted } = useApp();
  const [qty, setQty] = useState(1);
  const [added, setAdded] = useState(false);

  if (!product) {
    return (
      <div className="mx-auto max-w-3xl px-6 py-24 text-center">
        <h1 className="font-display text-3xl">{isLoading ? "Loading…" : "Product not found"}</h1>
      </div>
    );
  }

  const related = products.filter((p) => p.category === product.category && p.id !== product.id).slice(0, 4);

  const handleAdd = () => {
    addToCart(product, qty);
    setAdded(true);
    setTimeout(() => setAdded(false), 1800);
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <button
        onClick={() => router.history.back()}
        className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="h-4 w-4" /> Back
      </button>

      <div className="grid gap-10 md:grid-cols-2">
        <div className="space-y-3">
          <div className="overflow-hidden rounded-2xl bg-cream/70">
            <img
              src={product.image}
              alt={product.name}
              width={1024}
              height={1024}
              className="h-full w-full object-cover"
            />
          </div>
          <div className="grid grid-cols-3 gap-3">
            {[product.image, product.image, product.image].map((src, i) => (
              <div key={i} className="aspect-square overflow-hidden rounded-lg bg-cream/70">
                <img src={src} alt="" loading="lazy" className="h-full w-full object-cover opacity-90" />
              </div>
            ))}
          </div>
        </div>

        <div className="md:pt-4">
          <p className="text-xs uppercase tracking-widest text-muted-foreground">{product.category}</p>
          <h1 className="mt-2 font-display text-4xl md:text-5xl">{product.name}</h1>
          <p className="mt-4 text-2xl">${product.price}</p>

          <p className="mt-6 max-w-md text-muted-foreground">{product.description}</p>

          <ul className="mt-6 space-y-2">
            {product.features.map((f) => (
              <li key={f} className="flex items-center gap-2 text-sm">
                <Check className="h-4 w-4 text-accent" /> {f}
              </li>
            ))}
          </ul>

          <div className="mt-8 flex items-center gap-3">
            <div className="flex items-center rounded-full border border-border">
              <button
                onClick={() => setQty((q) => Math.max(1, q - 1))}
                className="p-3 text-muted-foreground hover:text-foreground"
                aria-label="Decrease"
              >
                <Minus className="h-4 w-4" />
              </button>
              <span className="w-8 text-center text-sm">{qty}</span>
              <button
                onClick={() => setQty((q) => q + 1)}
                className="p-3 text-muted-foreground hover:text-foreground"
                aria-label="Increase"
              >
                <Plus className="h-4 w-4" />
              </button>
            </div>
            <button
              onClick={handleAdd}
              className="inline-flex flex-1 items-center justify-center gap-2 rounded-full bg-primary px-6 py-3.5 text-sm font-medium text-primary-foreground transition hover:opacity-90"
            >
              {added ? (<><Check className="h-4 w-4" /> Added</>) : (<><ShoppingBag className="h-4 w-4" /> Add to cart</>)}
            </button>
            <button
              onClick={() => toggleWishlist(product.id)}
              className="rounded-full border border-border p-3.5 transition hover:bg-cream"
              aria-label="Wishlist"
            >
              <Heart className={`h-4 w-4 ${isWishlisted(product.id) ? "fill-accent text-accent" : ""}`} />
            </button>
          </div>

          <p className="mt-6 text-xs text-muted-foreground">
            Free delivery on orders over $500. Ships in 1–2 weeks.
          </p>
        </div>
      </div>

      <ProductReviews productId={product.id} />

      {related.length > 0 && (
        <section className="mt-24">
          <h2 className="mb-8 font-display text-3xl">You may also like</h2>
          <div className="grid grid-cols-2 gap-x-5 gap-y-10 lg:grid-cols-4">
            {related.map((p) => <ProductCard key={p.id} product={p} />)}
          </div>
        </section>
      )}
    </div>
  );
}
