import { useRef, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ImagePlus, Loader2, Palette, Pencil, Plus, Trash2, Upload, X } from "lucide-react";
import { toast } from "sonner";
import { api } from "@/lib/api";
import type { ProductInput, ProductResponse } from "@/lib/types";

// ─── Blank forms ──────────────────────────────────────────────────────────────

const BLANK: ProductInput = {
  categoryId: 0, name: "", description: "", price: 0, discountPrice: null,
  stockQuantity: 0, material: "", color: "", height: null, width: null,
  depth: null, isActive: true, isFeatured: false, averageRating: 0, reviewsCount: 0,
};

const BLANK_VARIANT = { color: "", size: "", material: "", sku: "", price: 0, stockQuantity: 0 };
const BLANK_IMAGE   = { imageUrl: "", isMain: false };

function toInput(p: ProductResponse): ProductInput {
  return {
    categoryId: p.categoryId, name: p.name, description: p.description,
    price: p.price, discountPrice: p.discountPrice ?? null,
    stockQuantity: p.stockQuantity, material: p.material ?? "",
    color: p.color ?? "", height: p.height ?? null, width: p.width ?? null,
    depth: p.depth ?? null, isActive: p.isActive, isFeatured: p.isFeatured,
    averageRating: p.averageRating, reviewsCount: p.reviewsCount,
  };
}

// ─── Drag-and-drop image picker ───────────────────────────────────────────────

function ImageDropzone({ value, onChange }: { value: string; onChange: (url: string) => void }) {
  const [dragging, setDragging] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const handleFile = (file: File) => {
    if (!file.type.startsWith("image/")) { toast.error("Please select an image file."); return; }
    const reader = new FileReader();
    reader.onload = (e) => onChange(e.target?.result as string);
    reader.readAsDataURL(file);
  };

  const onDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragging(false);
    const file = e.dataTransfer.files[0];
    if (file) handleFile(file);
  };

  return (
    <div>
      {value ? (
        <div className="relative inline-block">
          <img src={value} alt="Preview" className="h-32 w-32 rounded-xl border border-border object-cover" />
          <button
            type="button"
            onClick={() => onChange("")}
            className="absolute -right-2 -top-2 rounded-full bg-destructive p-1 text-white"
            aria-label="Remove image"
          >
            <X className="h-3 w-3" />
          </button>
          <button
            type="button"
            onClick={() => inputRef.current?.click()}
            className="absolute -bottom-2 -right-2 rounded-full bg-primary p-1 text-primary-foreground"
            aria-label="Change image"
          >
            <Upload className="h-3 w-3" />
          </button>
        </div>
      ) : (
        <div
          onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
          onDragLeave={() => setDragging(false)}
          onDrop={onDrop}
          onClick={() => inputRef.current?.click()}
          className={`flex cursor-pointer flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed p-8 transition select-none ${
            dragging
              ? "border-accent bg-accent/5"
              : "border-border hover:border-foreground/30 hover:bg-muted/30"
          }`}
        >
          <Upload className="h-6 w-6 text-muted-foreground" />
          <p className="text-center text-sm text-muted-foreground">
            Drag &amp; drop an image here, or{" "}
            <span className="text-foreground underline underline-offset-2">click to browse</span>
          </p>
          <p className="text-xs text-muted-foreground">PNG · JPG · WEBP</p>
        </div>
      )}
      <input ref={inputRef} type="file" accept="image/*" className="hidden" onChange={(e) => {
        const file = e.target.files?.[0];
        if (file) handleFile(file);
        e.target.value = "";
      }} />
    </div>
  );
}

// ─── Main component ───────────────────────────────────────────────────────────

export function AdminProducts() {
  const qc = useQueryClient();

  // Product form
  const [editingId, setEditingId]   = useState<number | null>(null);
  const [form, setForm]             = useState<ProductInput | null>(null);
  const [saving, setSaving]         = useState(false);

  // Variant sub-form
  const [variantForm, setVariantForm]     = useState(BLANK_VARIANT);
  const [showVariantForm, setShowVariantForm] = useState(false);
  const [variantSaving, setVariantSaving] = useState(false);

  // Image sub-form
  const [imageForm, setImageForm]     = useState(BLANK_IMAGE);
  const [showImageForm, setShowImageForm] = useState(false);
  const [imageSaving, setImageSaving] = useState(false);

  const { data: products, isLoading } = useQuery({
    queryKey: ["admin", "products"],
    queryFn: api.getProducts,
  });
  const { data: categories } = useQuery({
    queryKey: ["admin", "categories"],
    queryFn: api.getCategories,
  });

  const refresh = () => qc.invalidateQueries({ queryKey: ["admin", "products"] });

  const editingProduct = editingId ? (products?.find((p) => p.id === editingId) ?? null) : null;

  // ── Product CRUD ─────────────────────────────────────────────────────────────

  const startCreate = () => {
    setEditingId(null);
    setForm({ ...BLANK, categoryId: categories?.[0]?.id ?? 0 });
    setShowVariantForm(false);
    setShowImageForm(false);
  };

  const startEdit = (p: ProductResponse) => {
    setEditingId(p.id);
    setForm(toInput(p));
    setShowVariantForm(false);
    setShowImageForm(false);
    setVariantForm(BLANK_VARIANT);
    setImageForm(BLANK_IMAGE);
  };

  const close = () => {
    setForm(null); setEditingId(null);
    setShowVariantForm(false); setShowImageForm(false);
  };

  const save = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form) return;
    if (!form.name.trim() || !form.description.trim() || form.categoryId <= 0) {
      toast.error("Name, description and category are required.");
      return;
    }
    setSaving(true);
    try {
      if (editingId) {
        await api.updateProduct(editingId, form);
        toast.success("Product updated.");
      } else {
        await api.createProduct(form);
        toast.success("Product created. Open it to add variants and images.");
      }
      await refresh();
      close();
    } catch {
      toast.error("Save failed.");
    } finally {
      setSaving(false);
    }
  };

  const remove = async (id: number) => {
    if (!confirm("Delete this product?")) return;
    try {
      await api.deleteProduct(id);
      await refresh();
      toast.success("Product deleted.");
    } catch {
      toast.error("Delete failed (it may be referenced by an order).");
    }
  };

  const catName = (id: number) => categories?.find((c) => c.id === id)?.name ?? `#${id}`;

  // ── Variant CRUD ─────────────────────────────────────────────────────────────

  const addVariant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingId) return;
    if (!variantForm.sku.trim()) { toast.error("SKU is required."); return; }
    if (variantForm.price <= 0)  { toast.error("Price must be > 0."); return; }
    setVariantSaving(true);
    try {
      await api.createProductVariant({
        productId: editingId,
        color: variantForm.color || null,
        size: variantForm.size || null,
        material: variantForm.material || null,
        sku: variantForm.sku,
        price: variantForm.price,
        stockQuantity: variantForm.stockQuantity,
      });
      await refresh();
      setVariantForm(BLANK_VARIANT);
      setShowVariantForm(false);
      toast.success("Variant added.");
    } catch {
      toast.error("Failed to add variant.");
    } finally {
      setVariantSaving(false);
    }
  };

  const removeVariant = async (id: number) => {
    try {
      await api.deleteProductVariant(id);
      await refresh();
      toast.success("Variant removed.");
    } catch {
      toast.error("Failed to remove variant.");
    }
  };

  // ── Image CRUD ───────────────────────────────────────────────────────────────

  const addImage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingId) return;
    if (!imageForm.imageUrl.trim()) { toast.error("Please select an image."); return; }
    setImageSaving(true);
    try {
      await api.createProductImage({
        productId: editingId,
        imageUrl: imageForm.imageUrl,
        isMain: imageForm.isMain,
      });
      await refresh();
      setImageForm(BLANK_IMAGE);
      setShowImageForm(false);
      toast.success("Image added.");
    } catch {
      toast.error("Failed to add image.");
    } finally {
      setImageSaving(false);
    }
  };

  const removeImage = async (id: number) => {
    try {
      await api.deleteProductImage(id);
      await refresh();
      toast.success("Image removed.");
    } catch {
      toast.error("Failed to remove image.");
    }
  };

  // ── Render ───────────────────────────────────────────────────────────────────

  return (
    <div>
      {/* Header */}
      <div className="mb-4 flex items-center justify-between">
        <h2 className="font-display text-2xl">
          Products{products && <span className="ml-1 text-muted-foreground">({products.length})</span>}
        </h2>
        <button
          onClick={startCreate}
          className="inline-flex items-center gap-1.5 rounded-full bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90"
        >
          <Plus className="h-4 w-4" /> New product
        </button>
      </div>

      {/* ── Editor panel ── */}
      {form && (
        <div className="mb-6 rounded-2xl border border-border bg-cream/40 p-5">
          <div className="mb-4 flex items-center justify-between">
            <h3 className="font-display text-lg">{editingId ? "Edit product" : "New product"}</h3>
            <button type="button" onClick={close} className="rounded-full p-1 hover:bg-muted" aria-label="Close">
              <X className="h-4 w-4" />
            </button>
          </div>

          {/* Basic fields form */}
          <form onSubmit={save}>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field label="Name" className="sm:col-span-2" value={form.name} onChange={(v) => setForm({ ...form, name: v })} />
              <label className="block sm:col-span-2">
                <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">Description</span>
                <textarea
                  value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  rows={2}
                  className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none focus:border-foreground"
                />
              </label>
              <label className="block">
                <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">Category</span>
                <select
                  value={form.categoryId}
                  onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })}
                  className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none focus:border-foreground"
                >
                  <option value={0} disabled>Select…</option>
                  {categories?.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </label>
              <NumField label="Price"          value={form.price}           onChange={(v) => setForm({ ...form, price: v === "" ? 0 : v })} />
              <NumField label="Discount price" value={form.discountPrice ?? ""} onChange={(v) => setForm({ ...form, discountPrice: v === "" ? null : v })} allowEmpty />
              <NumField label="Stock"          value={form.stockQuantity}   onChange={(v) => setForm({ ...form, stockQuantity: v === "" ? 0 : v })} />
              <Field    label="Material"       value={form.material ?? ""}  onChange={(v) => setForm({ ...form, material: v })} />
              <div className="flex items-center gap-6 sm:col-span-2">
                <Toggle label="Active"   checked={form.isActive}   onChange={(v) => setForm({ ...form, isActive: v })} />
                <Toggle label="Featured" checked={form.isFeatured} onChange={(v) => setForm({ ...form, isFeatured: v })} />
              </div>
            </div>
            <button
              disabled={saving}
              className="mt-4 rounded-full bg-primary px-6 py-2.5 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:opacity-60"
            >
              {saving ? "Saving…" : editingId ? "Save changes" : "Create product"}
            </button>
            {!editingId && (
              <p className="mt-2 text-xs text-muted-foreground">Save first, then open the product to add images and color variants.</p>
            )}
          </form>

          {/* ── Variants section (edit only) ── */}
          {editingId && (
            <div className="mt-6 border-t border-border pt-5">
              <div className="mb-3 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Palette className="h-4 w-4 text-accent" />
                  <h4 className="text-sm font-medium">Colors / Variants</h4>
                  {!!editingProduct?.variants.length && (
                    <span className="rounded-full bg-muted px-2 py-0.5 text-xs text-muted-foreground">
                      {editingProduct.variants.length}
                    </span>
                  )}
                </div>
                {!showVariantForm && (
                  <button
                    type="button"
                    onClick={() => setShowVariantForm(true)}
                    className="inline-flex items-center gap-1 rounded-full border border-border px-3 py-1 text-xs hover:bg-muted"
                  >
                    <Plus className="h-3 w-3" /> Add variant
                  </button>
                )}
              </div>

              {editingProduct?.variants && editingProduct.variants.length > 0 ? (
                <div className="mb-3 space-y-1">
                  {editingProduct.variants.map((v) => (
                    <div key={v.id} className="flex items-center justify-between rounded-lg border border-border bg-background px-3 py-2 text-sm">
                      <div className="flex flex-wrap items-center gap-3">
                        {v.color && (
                          <span className="flex items-center gap-1.5">
                            <span className="inline-block h-3.5 w-3.5 rounded-full border border-border" style={{ background: v.color.toLowerCase() }} />
                            {v.color}
                          </span>
                        )}
                        {v.size && <span className="text-muted-foreground">Size: {v.size}</span>}
                        <span className="text-muted-foreground">SKU: {v.sku}</span>
                        <span>${v.price}</span>
                        <span className="text-muted-foreground">Stock: {v.stockQuantity}</span>
                      </div>
                      <button type="button" onClick={() => removeVariant(v.id)}
                        className="ml-2 rounded-full p-1 text-muted-foreground hover:bg-muted hover:text-destructive" aria-label="Remove">
                        <Trash2 className="h-3.5 w-3.5" />
                      </button>
                    </div>
                  ))}
                </div>
              ) : !showVariantForm ? (
                <p className="mb-3 text-xs text-muted-foreground">No variants yet. Add one to offer different colors or sizes.</p>
              ) : null}

              {showVariantForm && (
                <form onSubmit={addVariant} className="rounded-xl border border-border bg-background p-4">
                  <div className="grid gap-3 sm:grid-cols-3">
                    <Field label="Color"    value={variantForm.color}    onChange={(v) => setVariantForm({ ...variantForm, color: v })}    placeholder="e.g. Midnight Blue" />
                    <Field label="Size"     value={variantForm.size}     onChange={(v) => setVariantForm({ ...variantForm, size: v })}     placeholder="e.g. Large" />
                    <Field label="Material" value={variantForm.material} onChange={(v) => setVariantForm({ ...variantForm, material: v })} placeholder="e.g. Oak" />
                    <Field label="SKU *"    value={variantForm.sku}      onChange={(v) => setVariantForm({ ...variantForm, sku: v })}      placeholder="e.g. CHAIR-BLUE-LG" />
                    <NumField label="Price *" value={variantForm.price}         onChange={(v) => setVariantForm({ ...variantForm, price: v === "" ? 0 : v })} />
                    <NumField label="Stock"   value={variantForm.stockQuantity} onChange={(v) => setVariantForm({ ...variantForm, stockQuantity: v === "" ? 0 : v })} />
                  </div>
                  <div className="mt-3 flex gap-2">
                    <button disabled={variantSaving}
                      className="rounded-full bg-primary px-4 py-1.5 text-xs font-medium text-primary-foreground disabled:opacity-60">
                      {variantSaving ? "Adding…" : "Add variant"}
                    </button>
                    <button type="button" onClick={() => { setShowVariantForm(false); setVariantForm(BLANK_VARIANT); }}
                      className="rounded-full border border-border px-4 py-1.5 text-xs hover:bg-muted">
                      Cancel
                    </button>
                  </div>
                </form>
              )}
            </div>
          )}

          {/* ── Images section (edit only) ── */}
          {editingId && (
            <div className="mt-6 border-t border-border pt-5">
              <div className="mb-3 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <ImagePlus className="h-4 w-4 text-accent" />
                  <h4 className="text-sm font-medium">Product Images</h4>
                  {!!editingProduct?.images.length && (
                    <span className="rounded-full bg-muted px-2 py-0.5 text-xs text-muted-foreground">
                      {editingProduct.images.length}
                    </span>
                  )}
                </div>
                {!showImageForm && (
                  <button type="button" onClick={() => setShowImageForm(true)}
                    className="inline-flex items-center gap-1 rounded-full border border-border px-3 py-1 text-xs hover:bg-muted">
                    <Plus className="h-3 w-3" /> Add image
                  </button>
                )}
              </div>

              {/* Existing thumbnails */}
              {editingProduct?.images && editingProduct.images.length > 0 && (
                <div className="mb-3 flex flex-wrap gap-3">
                  {editingProduct.images.map((img) => (
                    <div key={img.id} className="group relative">
                      <img src={img.imageUrl} alt="Product"
                        className="h-24 w-24 rounded-xl border border-border object-cover"
                        onError={(e) => { (e.target as HTMLImageElement).src = "https://placehold.co/96x96?text=?"; }} />
                      {img.isMain && (
                        <span className="absolute bottom-1 left-0 right-0 text-center text-[9px] font-bold uppercase tracking-widest text-white drop-shadow">
                          Main
                        </span>
                      )}
                      <button type="button" onClick={() => removeImage(img.id)}
                        className="absolute -right-2 -top-2 hidden rounded-full bg-destructive p-1 text-white group-hover:flex"
                        aria-label="Remove image">
                        <X className="h-3 w-3" />
                      </button>
                    </div>
                  ))}
                </div>
              )}

              {!showImageForm && !editingProduct?.images.length && (
                <p className="mb-3 text-xs text-muted-foreground">No images yet. Drop one below to get started.</p>
              )}

              {/* Add image form */}
              {showImageForm && (
                <form onSubmit={addImage} className="rounded-xl border border-border bg-background p-4">
                  <ImageDropzone
                    value={imageForm.imageUrl}
                    onChange={(url) => setImageForm({ ...imageForm, imageUrl: url })}
                  />
                  <div className="mt-3 flex items-center gap-4">
                    <Toggle
                      label="Set as main image"
                      checked={imageForm.isMain}
                      onChange={(v) => setImageForm({ ...imageForm, isMain: v })}
                    />
                  </div>
                  <div className="mt-3 flex gap-2">
                    <button disabled={imageSaving}
                      className="rounded-full bg-primary px-4 py-1.5 text-xs font-medium text-primary-foreground disabled:opacity-60">
                      {imageSaving ? "Uploading…" : "Add image"}
                    </button>
                    <button type="button" onClick={() => { setShowImageForm(false); setImageForm(BLANK_IMAGE); }}
                      className="rounded-full border border-border px-4 py-1.5 text-xs hover:bg-muted">
                      Cancel
                    </button>
                  </div>
                </form>
              )}
            </div>
          )}
        </div>
      )}

      {/* ── Products table ── */}
      {isLoading ? (
        <div className="flex items-center justify-center py-16">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-border">
          <table className="w-full text-sm">
            <thead className="bg-cream/60 text-left text-xs uppercase tracking-wider text-muted-foreground">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Category</th>
                <th className="px-4 py-3">Price</th>
                <th className="px-4 py-3">Stock</th>
                <th className="px-4 py-3">Variants</th>
                <th className="px-4 py-3">Images</th>
                <th className="px-4 py-3">Active</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {products?.map((p) => (
                <tr key={p.id}>
                  <td className="px-4 py-3 font-medium">{p.name}</td>
                  <td className="px-4 py-3 text-muted-foreground">{catName(p.categoryId)}</td>
                  <td className="px-4 py-3">
                    ${p.price}
                    {p.discountPrice ? <span className="ml-1 text-xs text-accent">→ ${p.discountPrice}</span> : null}
                  </td>
                  <td className="px-4 py-3">{p.stockQuantity}</td>
                  <td className="px-4 py-3">
                    {p.variants.length > 0 ? (
                      <div className="flex flex-wrap gap-1">
                        {p.variants.map((v) => (
                          <span key={v.id} title={`${v.color ?? ""}${v.size ? " / " + v.size : ""} — $${v.price}`}
                            className="inline-flex items-center gap-1 rounded-full border border-border px-2 py-0.5 text-xs">
                            {v.color && (
                              <span className="h-2.5 w-2.5 rounded-full border border-border/50"
                                style={{ background: v.color.toLowerCase() }} />
                            )}
                            {v.color ?? v.sku}
                          </span>
                        ))}
                      </div>
                    ) : <span className="text-xs text-muted-foreground">—</span>}
                  </td>
                  <td className="px-4 py-3">
                    {p.images.length > 0 ? (
                      <div className="flex gap-1">
                        {p.images.slice(0, 3).map((img) => (
                          <img key={img.id} src={img.imageUrl} alt=""
                            className="h-8 w-8 rounded border border-border object-cover"
                            onError={(e) => { (e.target as HTMLImageElement).style.display = "none"; }} />
                        ))}
                        {p.images.length > 3 && (
                          <span className="flex h-8 w-8 items-center justify-center rounded border border-border text-xs text-muted-foreground">
                            +{p.images.length - 3}
                          </span>
                        )}
                      </div>
                    ) : <span className="text-xs text-muted-foreground">—</span>}
                  </td>
                  <td className="px-4 py-3">{p.isActive ? "Yes" : "No"}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1">
                      <button onClick={() => startEdit(p)}
                        className="rounded-full p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground" aria-label="Edit">
                        <Pencil className="h-4 w-4" />
                      </button>
                      <button onClick={() => remove(p.id)}
                        className="rounded-full p-1.5 text-muted-foreground hover:bg-muted hover:text-destructive" aria-label="Delete">
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// ─── Shared field helpers ─────────────────────────────────────────────────────

function Field({ label, value, onChange, className = "", placeholder }: {
  label: string; value: string; onChange: (v: string) => void; className?: string; placeholder?: string;
}) {
  return (
    <label className={`block ${className}`}>
      <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">{label}</span>
      <input value={value} onChange={(e) => onChange(e.target.value)} placeholder={placeholder}
        className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none focus:border-foreground" />
    </label>
  );
}

function NumField({ label, value, onChange, allowEmpty = false }: {
  label: string; value: number | string; onChange: (v: number | "") => void; allowEmpty?: boolean;
}) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">{label}</span>
      <input type="number" value={value} min={0} step="0.01"
        onChange={(e) => {
          const raw = e.target.value;
          if (raw === "" && allowEmpty) return onChange("");
          onChange(Number(raw));
        }}
        className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none focus:border-foreground" />
    </label>
  );
}

function Toggle({ label, checked, onChange }: { label: string; checked: boolean; onChange: (v: boolean) => void }) {
  return (
    <label className="inline-flex items-center gap-2 text-sm">
      <input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)}
        className="h-4 w-4 accent-[oklch(0.32_0.04_50)]" />
      {label}
    </label>
  );
}
