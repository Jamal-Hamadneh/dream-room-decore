import { createFileRoute } from "@tanstack/react-router";
import { useRef, useState } from "react";
import { Sparkles, Upload, Check, Loader2, RefreshCw } from "lucide-react";
import { PRODUCTS } from "@/data/products";
import aiResultImg from "@/assets/ai-result.jpg";

export const Route = createFileRoute("/ai-room")({
  head: () => ({
    meta: [
      { title: "AI Room — haus" },
      { name: "description", content: "Upload your room and design it with AI. See haus furniture in your space before you buy." },
      { property: "og:title", content: "AI Room — haus" },
      { property: "og:description", content: "Design your room with AI." },
    ],
  }),
  component: AIRoomPage,
});

function AIRoomPage() {
  const inputRef = useRef<HTMLInputElement>(null);
  const [roomImage, setRoomImage] = useState<string | null>(null);
  const [selected, setSelected] = useState<string[]>([]);
  const [generating, setGenerating] = useState(false);
  const [result, setResult] = useState<string | null>(null);

  const handleFile = (file: File) => {
    const reader = new FileReader();
    reader.onload = (e) => setRoomImage(e.target?.result as string);
    reader.readAsDataURL(file);
    setResult(null);
  };

  const toggle = (id: string) =>
    setSelected((s) => (s.includes(id) ? s.filter((x) => x !== id) : [...s, id]));

  const generate = () => {
    if (!roomImage || selected.length === 0) return;
    setGenerating(true);
    setResult(null);
    setTimeout(() => {
      setResult(aiResultImg);
      setGenerating(false);
    }, 2400);
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <header className="mb-12 max-w-2xl">
        <p className="mb-3 inline-flex items-center gap-2 rounded-full border border-border bg-cream px-3 py-1 text-xs uppercase tracking-widest text-muted-foreground">
          <Sparkles className="h-3 w-3 text-accent" /> AI Room
        </p>
        <h1 className="font-display text-4xl md:text-5xl text-balance">
          Design your room, before you commit.
        </h1>
        <p className="mt-4 text-muted-foreground">
          Upload a photo of your space, pick a few pieces, and we'll show you how they look together.
        </p>
      </header>

      <div className="grid gap-10 lg:grid-cols-[1.2fr_1fr]">
        {/* Left: room + result */}
        <div className="space-y-6">
          <div>
            <h2 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
              1 · Your room
            </h2>
            <button
              onClick={() => inputRef.current?.click()}
              className="group relative flex aspect-[4/3] w-full items-center justify-center overflow-hidden rounded-2xl border border-dashed border-border bg-cream/50 transition hover:border-foreground"
            >
              {roomImage ? (
                <img src={roomImage} alt="Your room" className="h-full w-full object-cover" />
              ) : (
                <div className="text-center">
                  <Upload className="mx-auto h-8 w-8 text-muted-foreground" />
                  <p className="mt-3 text-sm font-medium">Upload room photo</p>
                  <p className="mt-1 text-xs text-muted-foreground">PNG or JPG, up to 10MB</p>
                </div>
              )}
              {roomImage && (
                <div className="absolute inset-x-0 bottom-0 flex items-center justify-between bg-gradient-to-t from-foreground/70 to-transparent p-4 text-primary-foreground opacity-0 transition group-hover:opacity-100">
                  <span className="text-xs">Click to replace</span>
                  <RefreshCw className="h-4 w-4" />
                </div>
              )}
            </button>
            <input
              ref={inputRef}
              type="file"
              accept="image/*"
              className="hidden"
              onChange={(e) => e.target.files?.[0] && handleFile(e.target.files[0])}
            />
          </div>

          <div>
            <h2 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
              3 · AI-designed result
            </h2>
            <div className="relative flex aspect-[4/3] w-full items-center justify-center overflow-hidden rounded-2xl bg-cream/50">
              {generating ? (
                <div className="text-center">
                  <Loader2 className="mx-auto h-8 w-8 animate-spin text-accent" />
                  <p className="mt-4 font-display text-lg">Designing your room…</p>
                  <p className="mt-1 text-xs text-muted-foreground">Placing furniture, balancing the light</p>
                </div>
              ) : result ? (
                <img src={result} alt="AI generated room design" className="h-full w-full object-cover animate-in fade-in duration-700" />
              ) : (
                <p className="text-sm text-muted-foreground">Your designed room will appear here</p>
              )}
            </div>
          </div>
        </div>

        {/* Right: selection */}
        <div>
          <h2 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
            2 · Pick furniture ({selected.length})
          </h2>
          <div className="grid max-h-[560px] grid-cols-2 gap-3 overflow-y-auto rounded-2xl border border-border p-3 sm:grid-cols-3 lg:grid-cols-2 xl:grid-cols-3">
            {PRODUCTS.map((p) => {
              const on = selected.includes(p.id);
              return (
                <button
                  key={p.id}
                  onClick={() => toggle(p.id)}
                  className={`group relative overflow-hidden rounded-xl border-2 text-left transition ${
                    on ? "border-accent" : "border-transparent hover:border-border"
                  }`}
                >
                  <div className="aspect-square overflow-hidden bg-cream/70">
                    <img src={p.image} alt={p.name} className="h-full w-full object-cover transition group-hover:scale-105" />
                  </div>
                  <div className="p-2">
                    <p className="truncate text-xs font-medium">{p.name}</p>
                    <p className="text-[11px] text-muted-foreground">${p.price}</p>
                  </div>
                  {on && (
                    <div className="absolute right-2 top-2 flex h-6 w-6 items-center justify-center rounded-full bg-accent text-accent-foreground">
                      <Check className="h-3.5 w-3.5" />
                    </div>
                  )}
                </button>
              );
            })}
          </div>

          <button
            onClick={generate}
            disabled={!roomImage || selected.length === 0 || generating}
            className="mt-6 inline-flex w-full items-center justify-center gap-2 rounded-full bg-primary px-6 py-3.5 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <Sparkles className="h-4 w-4" />
            {generating ? "Generating…" : "Generate room design"}
          </button>
          {(!roomImage || selected.length === 0) && (
            <p className="mt-2 text-center text-xs text-muted-foreground">
              {!roomImage ? "Upload a room photo" : "Choose at least one piece"} to start.
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
