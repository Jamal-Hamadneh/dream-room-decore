import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { api } from "@/lib/api";

export function AdminCategories() {
  const qc = useQueryClient();
  const [name, setName] = useState("");
  const [saving, setSaving] = useState(false);

  const { data: categories, isLoading } = useQuery({
    queryKey: ["admin", "categories"],
    queryFn: api.getCategories,
  });

  const refresh = () => qc.invalidateQueries({ queryKey: ["admin", "categories"] });

  const add = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    setSaving(true);
    try {
      await api.createCategory(name.trim());
      setName("");
      await refresh();
      toast.success("Category added.");
    } catch {
      toast.error("Couldn't add category.");
    } finally {
      setSaving(false);
    }
  };

  const remove = async (id: number) => {
    if (!confirm("Delete this category? Products in it may block deletion.")) return;
    try {
      await api.deleteCategory(id);
      await refresh();
      toast.success("Category deleted.");
    } catch {
      toast.error("Delete failed (category may have products).");
    }
  };

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h2 className="font-display text-2xl">Categories {categories && <span className="text-muted-foreground">({categories.length})</span>}</h2>
      </div>

      <form onSubmit={add} className="mb-6 flex max-w-md gap-2">
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="New category name"
          className="flex-1 rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none focus:border-foreground"
        />
        <button
          disabled={saving}
          className="inline-flex items-center gap-1.5 rounded-full bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:opacity-60"
        >
          <Plus className="h-4 w-4" /> Add
        </button>
      </form>

      {isLoading ? (
        <div className="flex items-center justify-center py-16">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      ) : (
        <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {categories?.map((c) => (
            <li key={c.id} className="flex items-center justify-between rounded-xl border border-border bg-background p-4">
              <div>
                <p className="font-medium">{c.name}</p>
                <p className="text-xs text-muted-foreground">{c.productsCount} product{c.productsCount === 1 ? "" : "s"}</p>
              </div>
              <button onClick={() => remove(c.id)} className="rounded-full p-1.5 text-muted-foreground hover:bg-muted hover:text-destructive" aria-label="Delete">
                <Trash2 className="h-4 w-4" />
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
