import { createFileRoute, Link } from "@tanstack/react-router";
import { useCallback, useRef, useState } from "react";
import {
  Sparkles, Upload, Loader2, RefreshCw, Plus, RotateCw,
  Maximize2, ArrowUpToLine, Trash2, ImageIcon, AlertCircle, ShoppingBag,
} from "lucide-react";
import { toast } from "sonner";
import { api, ApiError } from "@/lib/api";
import { resolveImage } from "@/lib/catalog";
import { useRequireAuth } from "@/hooks/useAuthGuard";
import type { AiRoomProduct, UploadAndCreateDesignResponse } from "@/lib/types";

// The backend composites on an 800×600 canvas; PositionX/Y are item centers in
// that pixel space and Scale multiplies a 220px base width.
const CANVAS_W = 800;
const CANVAS_H = 600;
const BASE_SIZE = 220;

export const Route = createFileRoute("/ai-room")({
  head: () => ({
    meta: [
      { title: "AI Room — Dream Room Decor" },
      { name: "description", content: "Upload your room and design it with AI. See Dream Room Decor furniture in your space before you buy." },
      { property: "og:title", content: "AI Room — Dream Room Decor" },
      { property: "og:description", content: "Design your room with AI." },
    ],
  }),
  component: AIRoomPage,
});

type Placement = {
  productId: number;
  positionX: number;
  positionY: number;
  rotation: number;
  scale: number;
  zIndex: number;
};

type AiAnalysis = {
  palette?: string[];
  recommendations?: string[];
  summary?: string;
  [k: string]: unknown;
};

function AIRoomPage() {
  const { ready } = useRequireAuth();

  const fileRef = useRef<HTMLInputElement>(null);
  const canvasRef = useRef<HTMLDivElement>(null);

  const [uploading, setUploading] = useState(false);
  const [design, setDesign] = useState<UploadAndCreateDesignResponse | null>(null);
  const [roomType, setRoomType] = useState("");

  const [placements, setPlacements] = useState<Placement[]>([]);
  const [selected, setSelected] = useState<number | null>(null);
  const [zCounter, setZCounter] = useState(1);

  const [generating, setGenerating] = useState(false);
  const [result, setResult] = useState<{ url: string; analysis: AiAnalysis | string } | null>(null);

  if (!ready) return null;

  // ── Upload ────────────────────────────────────────────────────────────────
  const onUpload = async (file: File) => {
    setUploading(true);
    setResult(null);
    try {
      const res = await api.aiRoom.uploadAndCreateDesign(file, { roomType: roomType || undefined });
      setDesign(res);
      setPlacements([]);
      setSelected(null);
      setZCounter(1);
      if (res.cartProducts.length === 0) {
        toast.message("Your room is uploaded, but your cart is empty — add furniture to place it here.");
      }
    } catch (e) {
      if (e instanceof ApiError && (e.status === 400 || e.status === 422)) {
        toast.error(e.message || "Add furniture to your cart before designing a room.");
      } else if (e instanceof ApiError && e.status === 500) {
        toast.error("Image upload failed on the server (Cloudinary keys may be missing).");
      } else {
        toast.error("Could not upload your room. Please try again.");
      }
    } finally {
      setUploading(false);
    }
  };

  // ── Placement helpers ───────────────────────────────────────────────────────
  const persist = useCallback(async (p: Placement, roomDesignId: number) => {
    try {
      await api.aiRoom.savePlacement({ roomDesignId, ...p });
    } catch {
      toast.error("Couldn't save that change.");
    }
  }, []);

  const addToCanvas = async (product: AiRoomProduct) => {
    if (!design) return;
    if (placements.some((p) => p.productId === product.productId)) {
      setSelected(product.productId);
      return;
    }
    const z = zCounter;
    setZCounter((c) => c + 1);
    const p: Placement = {
      productId: product.productId,
      positionX: CANVAS_W / 2,
      positionY: CANVAS_H / 2,
      rotation: 0,
      scale: 1,
      zIndex: z,
    };
    setPlacements((list) => [...list, p]);
    setSelected(product.productId);
    await persist(p, design.roomDesignId);
  };

  const updatePlacement = (productId: number, patch: Partial<Placement>, save = false) => {
    setPlacements((list) => {
      const next = list.map((p) => (p.productId === productId ? { ...p, ...patch } : p));
      if (save && design) {
        const updated = next.find((p) => p.productId === productId);
        if (updated) void persist(updated, design.roomDesignId);
      }
      return next;
    });
  };

  const bringToFront = (productId: number) => {
    const z = zCounter;
    setZCounter((c) => c + 1);
    updatePlacement(productId, { zIndex: z }, true);
  };

  const removeFromCanvas = (productId: number) => {
    // No delete endpoint exists; push the item far off-canvas so the server
    // composite no longer shows it, then drop it from the local canvas.
    if (design) {
      const off: Placement = {
        productId, positionX: -9999, positionY: -9999, rotation: 0, scale: 1, zIndex: 0,
      };
      void persist(off, design.roomDesignId);
    }
    setPlacements((list) => list.filter((p) => p.productId !== productId));
    setSelected((s) => (s === productId ? null : s));
  };

  // ── Dragging ────────────────────────────────────────────────────────────────
  const onPointerDown = (e: React.PointerEvent, productId: number) => {
    e.preventDefault();
    setSelected(productId);
    const canvas = canvasRef.current;
    if (!canvas) return;
    (e.target as HTMLElement).setPointerCapture(e.pointerId);

    const move = (ev: PointerEvent) => {
      const rect = canvas.getBoundingClientRect();
      const x = ((ev.clientX - rect.left) / rect.width) * CANVAS_W;
      const y = ((ev.clientY - rect.top) / rect.height) * CANVAS_H;
      updatePlacement(productId, {
        positionX: Math.max(0, Math.min(CANVAS_W, x)),
        positionY: Math.max(0, Math.min(CANVAS_H, y)),
      });
    };
    const up = () => {
      window.removeEventListener("pointermove", move);
      window.removeEventListener("pointerup", up);
      setPlacements((list) => {
        const moved = list.find((p) => p.productId === productId);
        if (moved && design) void persist(moved, design.roomDesignId);
        return list;
      });
    };
    window.addEventListener("pointermove", move);
    window.addEventListener("pointerup", up);
  };

  // ── Generate ──────────────────────────────────────────────────────────────
  const generate = async () => {
    if (!design) return;
    if (placements.length === 0) {
      toast.error("Place at least one piece of furniture first.");
      return;
    }
    setGenerating(true);
    setResult(null);
    try {
      const res = await api.aiRoom.generateRealisticDesign(design.roomDesignId);
      let analysis: AiAnalysis | string = res.aiAnalysisJson;
      try {
        analysis = JSON.parse(res.aiAnalysisJson) as AiAnalysis;
      } catch { /* keep raw string */ }
      setResult({ url: res.generatedImageUrl, analysis });
      toast.success("Your AI room design is ready.");
    } catch (e) {
      if (e instanceof ApiError && e.status === 500) {
        toast.error("The AI image service failed (OpenAI keys may be missing on the backend).");
      } else {
        toast.error("Generation failed. Please try again.");
      }
    } finally {
      setGenerating(false);
    }
  };

  const productById = (id: number) => design?.cartProducts.find((p) => p.productId === id);
  const selectedPlacement = placements.find((p) => p.productId === selected) ?? null;

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <header className="mb-10 max-w-2xl">
        <p className="mb-3 inline-flex items-center gap-2 rounded-full border border-border bg-cream px-3 py-1 text-xs uppercase tracking-widest text-muted-foreground">
          <Sparkles className="h-3 w-3 text-accent" /> AI Room
        </p>
        <h1 className="font-display text-4xl md:text-5xl text-balance">
          Design your room, before you commit.
        </h1>
        <p className="mt-4 text-muted-foreground">
          Upload a photo of your space, arrange the furniture from your cart, and let AI render a realistic preview.
        </p>
      </header>

      {!design ? (
        // ── STEP 1: upload ──
        <div className="mx-auto max-w-xl">
          <label className="mb-4 block">
            <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">Room type (optional)</span>
            <input
              value={roomType}
              onChange={(e) => setRoomType(e.target.value)}
              placeholder="e.g. Living room, Bedroom"
              className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none focus:border-foreground"
            />
          </label>
          <button
            onClick={() => fileRef.current?.click()}
            disabled={uploading}
            className="group relative flex aspect-[4/3] w-full items-center justify-center overflow-hidden rounded-2xl border-2 border-dashed border-border bg-cream/50 transition hover:border-foreground disabled:opacity-60"
          >
            {uploading ? (
              <div className="text-center">
                <Loader2 className="mx-auto h-8 w-8 animate-spin text-accent" />
                <p className="mt-3 text-sm font-medium">Uploading your room…</p>
              </div>
            ) : (
              <div className="text-center">
                <Upload className="mx-auto h-8 w-8 text-muted-foreground" />
                <p className="mt-3 text-sm font-medium">Upload room photo</p>
                <p className="mt-1 text-xs text-muted-foreground">PNG or JPG, up to 10MB</p>
              </div>
            )}
          </button>
          <input
            ref={fileRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={(e) => { const f = e.target.files?.[0]; if (f) onUpload(f); e.target.value = ""; }}
          />
          <div className="mt-4 flex items-start gap-2 rounded-xl border border-border bg-cream/40 p-3 text-xs text-muted-foreground">
            <ShoppingBag className="mt-0.5 h-4 w-4 flex-shrink-0 text-accent" />
            <p>Add furniture to your <Link to="/cart" className="text-foreground underline underline-offset-2">cart</Link> first — the designer arranges the pieces you're shopping for.</p>
          </div>
        </div>
      ) : (
        // ── STEP 2: design ──
        <div className="grid gap-8 lg:grid-cols-[1fr_300px]">
          {/* Canvas + result */}
          <div className="space-y-6">
            <div>
              <div className="mb-3 flex items-center justify-between">
                <h2 className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">Arrange your room</h2>
                <button
                  onClick={() => { setDesign(null); setResult(null); }}
                  className="inline-flex items-center gap-1.5 text-xs text-muted-foreground hover:text-foreground"
                >
                  <RefreshCw className="h-3.5 w-3.5" /> New photo
                </button>
              </div>
              <div
                ref={canvasRef}
                className="relative aspect-[4/3] w-full select-none overflow-hidden rounded-2xl border border-border bg-cream/50"
                onPointerDown={(e) => { if (e.target === e.currentTarget || (e.target as HTMLElement).tagName === "IMG" && (e.target as HTMLImageElement).alt === "Your room") setSelected(null); }}
              >
                <img src={design.imageUrl} alt="Your room" className="pointer-events-none absolute inset-0 h-full w-full object-cover" />
                {placements.map((p) => {
                  const product = productById(p.productId);
                  if (!product) return null;
                  const widthPct = ((BASE_SIZE * p.scale) / CANVAS_W) * 100;
                  return (
                    <div
                      key={p.productId}
                      onPointerDown={(e) => onPointerDown(e, p.productId)}
                      style={{
                        left: `${(p.positionX / CANVAS_W) * 100}%`,
                        top: `${(p.positionY / CANVAS_H) * 100}%`,
                        width: `${widthPct}%`,
                        transform: `translate(-50%, -50%) rotate(${p.rotation}deg)`,
                        zIndex: p.zIndex,
                      }}
                      className={`absolute cursor-grab touch-none active:cursor-grabbing ${
                        selected === p.productId ? "outline outline-2 outline-accent" : ""
                      }`}
                    >
                      <img
                        src={resolveImage(product.imageUrl, product.name, product.productId)}
                        alt={product.name}
                        draggable={false}
                        className="pointer-events-none h-auto w-full drop-shadow-lg"
                      />
                    </div>
                  );
                })}

                {placements.length === 0 && (
                  <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
                    <p className="rounded-full bg-foreground/70 px-4 py-2 text-sm text-primary-foreground">
                      Click a product on the right to place it →
                    </p>
                  </div>
                )}
              </div>

              {/* Selected item toolbar */}
              {selectedPlacement && (
                <div className="mt-3 rounded-xl border border-border bg-background p-3">
                  <div className="mb-2 flex items-center justify-between">
                    <span className="text-sm font-medium">{productById(selectedPlacement.productId)?.name}</span>
                    <div className="flex items-center gap-1">
                      <button onClick={() => bringToFront(selectedPlacement.productId)} title="Bring to front"
                        className="rounded-full p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground">
                        <ArrowUpToLine className="h-4 w-4" />
                      </button>
                      <button onClick={() => removeFromCanvas(selectedPlacement.productId)} title="Remove"
                        className="rounded-full p-1.5 text-muted-foreground hover:bg-muted hover:text-destructive">
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                  <label className="mb-2 flex items-center gap-2 text-xs text-muted-foreground">
                    <Maximize2 className="h-3.5 w-3.5" />
                    <input type="range" min={0.3} max={3} step={0.05} value={selectedPlacement.scale}
                      onChange={(e) => updatePlacement(selectedPlacement.productId, { scale: Number(e.target.value) })}
                      onPointerUp={() => updatePlacement(selectedPlacement.productId, {}, true)}
                      className="flex-1 accent-[oklch(0.32_0.04_50)]" />
                  </label>
                  <label className="flex items-center gap-2 text-xs text-muted-foreground">
                    <RotateCw className="h-3.5 w-3.5" />
                    <input type="range" min={-180} max={180} step={1} value={selectedPlacement.rotation}
                      onChange={(e) => updatePlacement(selectedPlacement.productId, { rotation: Number(e.target.value) })}
                      onPointerUp={() => updatePlacement(selectedPlacement.productId, {}, true)}
                      className="flex-1 accent-[oklch(0.32_0.04_50)]" />
                  </label>
                </div>
              )}
            </div>

            {/* Result */}
            {(generating || result) && (
              <div>
                <h2 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">AI-designed result</h2>
                <div className="relative flex aspect-[4/3] w-full items-center justify-center overflow-hidden rounded-2xl bg-cream/50">
                  {generating ? (
                    <div className="text-center">
                      <Loader2 className="mx-auto h-8 w-8 animate-spin text-accent" />
                      <p className="mt-4 font-display text-lg">Rendering your room…</p>
                      <p className="mt-1 text-xs text-muted-foreground">This can take up to a minute</p>
                    </div>
                  ) : result ? (
                    <img src={result.url} alt="AI generated room design" className="h-full w-full object-cover" />
                  ) : null}
                </div>
                {result && <AnalysisPanel analysis={result.analysis} />}
              </div>
            )}
          </div>

          {/* Furniture palette */}
          <div>
            <h2 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
              Furniture in your cart ({design.cartProducts.length})
            </h2>
            {design.cartProducts.length === 0 ? (
              <div className="rounded-xl border border-dashed border-border bg-cream/40 p-6 text-center">
                <AlertCircle className="mx-auto h-6 w-6 text-muted-foreground" />
                <p className="mt-2 text-sm text-muted-foreground">Your cart is empty.</p>
                <Link to="/shop" className="mt-2 inline-block text-sm text-foreground underline underline-offset-2">Browse the shop →</Link>
              </div>
            ) : (
              <div className="grid max-h-[420px] grid-cols-2 gap-3 overflow-y-auto rounded-2xl border border-border p-3">
                {design.cartProducts.map((p) => {
                  const placed = placements.some((pl) => pl.productId === p.productId);
                  return (
                    <button
                      key={p.productId}
                      onClick={() => addToCanvas(p)}
                      className={`group relative overflow-hidden rounded-xl border-2 text-left transition ${
                        placed ? "border-accent" : "border-transparent hover:border-border"
                      }`}
                    >
                      <div className="aspect-square overflow-hidden bg-cream/70">
                        <img src={resolveImage(p.imageUrl, p.name, p.productId)} alt={p.name}
                          className="h-full w-full object-cover transition group-hover:scale-105" />
                      </div>
                      <div className="p-2">
                        <p className="truncate text-xs font-medium">{p.name}</p>
                        <p className="text-[11px] text-muted-foreground">Qty {p.quantity}</p>
                      </div>
                      <div className={`absolute right-1.5 top-1.5 flex h-6 w-6 items-center justify-center rounded-full ${placed ? "bg-accent text-accent-foreground" : "bg-foreground/60 text-primary-foreground opacity-0 group-hover:opacity-100"}`}>
                        <Plus className="h-3.5 w-3.5" />
                      </div>
                    </button>
                  );
                })}
              </div>
            )}

            <button
              onClick={generate}
              disabled={placements.length === 0 || generating}
              className="mt-6 inline-flex w-full items-center justify-center gap-2 rounded-full bg-primary px-6 py-3.5 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {generating ? <Loader2 className="h-4 w-4 animate-spin" /> : <Sparkles className="h-4 w-4" />}
              {generating ? "Generating…" : "Generate room design"}
            </button>
            {placements.length === 0 && (
              <p className="mt-2 text-center text-xs text-muted-foreground">Place at least one piece to start.</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

// Renders the AI analysis — either a structured palette/recommendations object
// or, if parsing failed, the raw text.
function AnalysisPanel({ analysis }: { analysis: AiAnalysis | string }) {
  if (typeof analysis === "string") {
    if (!analysis.trim()) return null;
    return (
      <div className="mt-3 rounded-xl border border-border bg-cream/40 p-4">
        <p className="flex items-center gap-2 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
          <ImageIcon className="h-3.5 w-3.5" /> AI notes
        </p>
        <p className="mt-2 whitespace-pre-wrap text-sm text-foreground">{analysis}</p>
      </div>
    );
  }

  const palette = Array.isArray(analysis.palette) ? analysis.palette : [];
  const recs = Array.isArray(analysis.recommendations) ? analysis.recommendations : [];
  const summary = typeof analysis.summary === "string" ? analysis.summary : "";

  if (!palette.length && !recs.length && !summary) return null;

  return (
    <div className="mt-3 space-y-3 rounded-xl border border-border bg-cream/40 p-4">
      {summary && <p className="text-sm text-foreground">{summary}</p>}
      {palette.length > 0 && (
        <div>
          <p className="mb-1.5 text-xs font-semibold uppercase tracking-widest text-muted-foreground">Color palette</p>
          <div className="flex flex-wrap gap-2">
            {palette.map((c, i) => (
              <span key={i} className="flex items-center gap-1.5 rounded-full border border-border bg-background px-2 py-1 text-xs">
                <span className="h-3.5 w-3.5 rounded-full border border-border" style={{ background: c }} />
                {c}
              </span>
            ))}
          </div>
        </div>
      )}
      {recs.length > 0 && (
        <div>
          <p className="mb-1.5 text-xs font-semibold uppercase tracking-widest text-muted-foreground">Recommendations</p>
          <ul className="list-inside list-disc space-y-1 text-sm text-foreground">
            {recs.map((r, i) => <li key={i}>{r}</li>)}
          </ul>
        </div>
      )}
    </div>
  );
}
