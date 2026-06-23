import { Link } from "@tanstack/react-router";
import { Heart } from "lucide-react";
import { Product } from "@/data/products";
import { useApp } from "@/context/AppContext";

export function ProductCard({ product }: { product: Product }) {
  const { toggleWishlist, isWishlisted } = useApp();
  const wished = isWishlisted(product.id);

  return (
    <Link
      to="/shop/$productId"
      params={{ productId: product.id }}
      className="group block"
    >
      <div className="relative overflow-hidden rounded-xl bg-cream/70 aspect-square">
        <img
          src={product.image}
          alt={product.name}
          loading="lazy"
          width={1024}
          height={1024}
          className="h-full w-full object-cover transition-transform duration-700 group-hover:scale-105"
        />
        <button
          onClick={(e) => {
            e.preventDefault();
            toggleWishlist(product.id);
          }}
          className="absolute right-3 top-3 rounded-full bg-background/90 p-2 backdrop-blur transition hover:bg-background"
          aria-label="Toggle wishlist"
        >
          <Heart
            className={`h-4 w-4 ${wished ? "fill-accent text-accent" : "text-foreground"}`}
          />
        </button>
      </div>
      <div className="mt-3 flex items-start justify-between gap-3">
        <div>
          <p className="text-xs uppercase tracking-widest text-muted-foreground">
            {product.category}
          </p>
          <h3 className="mt-0.5 font-display text-lg leading-tight">{product.name}</h3>
        </div>
        <p className="whitespace-nowrap font-medium">${product.price}</p>
      </div>
    </Link>
  );
}
