// Maps backend ProductResponse / CategoryResponse into the UI's Product shape.
//
// The seeded backend uses placeholder image URLs (example.com/...), so we map
// each product to a bundled local image by keyword, keeping the UI looking real.

import sofa from "@/assets/product-sofa.jpg";
import bed from "@/assets/product-bed.jpg";
import chair from "@/assets/product-chair.jpg";
import table from "@/assets/product-table.jpg";
import wardrobe from "@/assets/product-wardrobe.jpg";
import lamp from "@/assets/product-lamp.jpg";
import shelf from "@/assets/product-shelf.jpg";
import type { Product } from "@/data/products";
import type { ProductResponse } from "./types";

const FALLBACKS = [sofa, table, bed, lamp, chair, wardrobe, shelf];

const KEYWORD_IMAGE: Array<[RegExp, string]> = [
  [/sofa|couch|sectional|armchair/i, sofa],
  [/bed|mattress/i, bed],
  [/chair|stool|seat/i, chair],
  [/table|desk/i, table],
  [/wardrobe|closet|cabinet|dresser/i, wardrobe],
  [/lamp|light|lighting/i, lamp],
  [/shelf|bookcase|storage|rack/i, shelf],
];

// Resolve a usable image from a (possibly placeholder) URL + a name to match on.
export function resolveImage(url: string | null | undefined, name: string, seed = 0): string {
  if (url && !/example\.com/i.test(url)) return url;
  for (const [re, img] of KEYWORD_IMAGE) {
    if (re.test(name)) return img;
  }
  return FALLBACKS[Math.abs(seed) % FALLBACKS.length];
}

function pickImage(p: ProductResponse): string {
  const url = p.mainImageUrl ?? p.images.find((i) => i.isMain)?.imageUrl ?? p.images[0]?.imageUrl;
  return resolveImage(url, `${p.name} ${p.category?.name ?? ""}`, p.id);
}

export function mapProduct(p: ProductResponse): Product {
  const effectivePrice = p.discountPrice ?? p.price;
  const features = [
    p.material ? `Material: ${p.material}` : null,
    p.color ? `Color: ${p.color}` : null,
    p.width && p.height ? `${p.width} × ${p.height} cm` : null,
    p.stockQuantity > 0 ? "In stock" : "Out of stock",
  ].filter((f): f is string => !!f);

  return {
    id: String(p.id),
    name: p.name,
    category: p.category?.name ?? "Furniture",
    price: effectivePrice,
    image: pickImage(p),
    description: p.description,
    features,
    stock: p.stockQuantity,
    categoryId: p.categoryId,
  };
}

export function mapProducts(products: ProductResponse[]): Product[] {
  return products.map(mapProduct);
}
