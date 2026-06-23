import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Star } from "lucide-react";
import { toast } from "sonner";
import { useApp } from "@/context/AppContext";
import { api } from "@/lib/api";

function Stars({ value, onSelect }: { value: number; onSelect?: (n: number) => void }) {
  return (
    <div className="flex items-center gap-0.5">
      {[1, 2, 3, 4, 5].map((n) => (
        <button
          key={n}
          type={onSelect ? "button" : undefined}
          disabled={!onSelect}
          onClick={onSelect ? () => onSelect(n) : undefined}
          className={onSelect ? "transition hover:scale-110" : "cursor-default"}
          aria-label={`${n} star${n === 1 ? "" : "s"}`}
        >
          <Star className={`h-4 w-4 ${n <= value ? "fill-accent text-accent" : "text-muted-foreground"}`} />
        </button>
      ))}
    </div>
  );
}

export function ProductReviews({ productId }: { productId: string }) {
  const { user } = useApp();
  const qc = useQueryClient();
  const numericId = Number(productId);
  const canReview = !!user?.id && Number.isFinite(numericId) && String(numericId) === productId;

  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [saving, setSaving] = useState(false);

  const { data: reviews, isLoading } = useQuery({
    queryKey: ["reviews", productId],
    queryFn: async () => (await api.getReviews()).filter((r) => r.productId === numericId),
    enabled: !!user?.id && Number.isFinite(numericId),
  });

  if (!user?.id) {
    // Reviews live in the backend, which requires a session.
    return null;
  }

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.createReview(user.id!, numericId, rating, comment || undefined);
      setComment("");
      setRating(5);
      await qc.invalidateQueries({ queryKey: ["reviews", productId] });
      toast.success("Thanks for your review!");
    } catch {
      toast.error("Couldn't submit your review.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="mt-24">
      <h2 className="mb-8 font-display text-3xl">Reviews</h2>

      {canReview && (
        <form onSubmit={submit} className="mb-10 max-w-xl rounded-2xl border border-border bg-cream/40 p-5">
          <span className="mb-2 block text-xs uppercase tracking-widest text-muted-foreground">Your rating</span>
          <Stars value={rating} onSelect={setRating} />
          <textarea
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder="Share your thoughts (optional)…"
            rows={3}
            className="mt-4 w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none transition focus:border-foreground"
          />
          <button
            disabled={saving}
            className="mt-3 rounded-full bg-primary px-6 py-2.5 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:opacity-60"
          >
            {saving ? "Submitting…" : "Submit review"}
          </button>
        </form>
      )}

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading reviews…</p>
      ) : !reviews || reviews.length === 0 ? (
        <p className="text-sm text-muted-foreground">No reviews yet. Be the first to review this piece.</p>
      ) : (
        <ul className="max-w-2xl space-y-5">
          {reviews.map((r) => (
            <li key={r.id} className="rounded-xl border border-border bg-background p-4">
              <div className="flex items-center justify-between">
                <Stars value={r.rating} />
                <span className="text-xs text-muted-foreground">{new Date(r.createdAt).toLocaleDateString()}</span>
              </div>
              {r.comment && <p className="mt-2 text-sm">{r.comment}</p>}
              <p className="mt-2 text-xs text-muted-foreground">{r.user?.fullName ?? "Customer"}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
