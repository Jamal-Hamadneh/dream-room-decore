// Catalog hooks. When the user is authenticated we load real products and
// categories from the backend; when logged out we fall back to the bundled
// demo catalog so the public pages still render (the backend requires a JWT
// for every read endpoint).

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { mapProducts } from "@/lib/catalog";
import { PRODUCTS, CATEGORIES, type Product } from "@/data/products";
import { useApp } from "@/context/AppContext";

export function useProducts() {
  const { user } = useApp();
  const enabled = !!user;

  const query = useQuery({
    queryKey: ["products"],
    queryFn: async () => mapProducts(await api.getProducts()),
    enabled,
  });

  const products: Product[] = enabled && query.data ? query.data : PRODUCTS;
  const fromBackend = enabled && !!query.data;

  return {
    products,
    fromBackend,
    isLoading: enabled && query.isLoading,
    error: query.error,
  };
}

export function useProduct(id: string) {
  const { products, isLoading, fromBackend } = useProducts();
  const product = products.find((p) => p.id === id);
  return { product, isLoading, fromBackend };
}

export function useCategories() {
  const { user } = useApp();
  const enabled = !!user;

  const query = useQuery({
    queryKey: ["categories"],
    queryFn: async () => (await api.getCategories()).map((c) => c.name),
    enabled,
  });

  const categories: string[] = enabled && query.data ? query.data : CATEGORIES;
  return { categories, isLoading: enabled && query.isLoading };
}
