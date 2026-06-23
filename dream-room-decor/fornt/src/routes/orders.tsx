import { createFileRoute, Link } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Package, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { useApp } from "@/context/AppContext";
import { useRequireAuth } from "@/hooks/useAuthGuard";
import { api } from "@/lib/api";
import { resolveImage } from "@/lib/catalog";
import type { OrderResponse } from "@/lib/types";

export const Route = createFileRoute("/orders")({
  head: () => ({ meta: [{ title: "My orders — Dream Room Decor" }] }),
  component: OrdersPage,
});

const STATUS_TINT: Record<string, string> = {
  pending: "bg-amber-100 text-amber-800",
  processing: "bg-blue-100 text-blue-800",
  shipped: "bg-indigo-100 text-indigo-800",
  delivered: "bg-emerald-100 text-emerald-800",
  cancelled: "bg-rose-100 text-rose-800",
};

// Statuses a customer is allowed to set on their own order.
const STATUS_OPTIONS = ["pending", "processing", "shipped", "delivered", "cancelled"] as const;

function OrdersPage() {
  const { user } = useApp();
  const { ready } = useRequireAuth();
  const qc = useQueryClient();

  const ordersKey = ["orders", "mine", user?.id];

  const { data: orders, isLoading } = useQuery({
    queryKey: ordersKey,
    queryFn: async () => {
      const all = await api.getOrders();
      return all
        .filter((o) => o.userId === user!.id)
        .sort((a, b) => b.id - a.id);
    },
    enabled: ready && !!user?.id,
  });

  const statusMutation = useMutation({
    mutationFn: ({ order, status }: { order: OrderResponse; status: string }) =>
      api.updateOrder(order.id, {
        userId: order.userId,
        addressId: order.addressId,
        totalPrice: order.totalPrice,
        status,
        paymentStatus: order.paymentStatus,
        stripePaymentIntentId: order.stripePaymentIntentId ?? null,
      }),
    // Optimistically update the cached order so the badge changes instantly.
    onMutate: async ({ order, status }) => {
      await qc.cancelQueries({ queryKey: ordersKey });
      const previous = qc.getQueryData<OrderResponse[]>(ordersKey);
      qc.setQueryData<OrderResponse[]>(ordersKey, (old) =>
        old?.map((o) => (o.id === order.id ? { ...o, status } : o)),
      );
      return { previous };
    },
    onError: (_e, _vars, ctx) => {
      if (ctx?.previous) qc.setQueryData(ordersKey, ctx.previous);
      toast.error("Couldn't update the order status. Please try again.");
    },
    onSuccess: () => {
      toast.success("Order status updated.");
    },
    onSettled: () => {
      qc.invalidateQueries({ queryKey: ordersKey });
    },
  });

  if (!ready) return null;

  return (
    <div className="mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8">
      <div className="mb-8">
        <p className="text-xs uppercase tracking-widest text-muted-foreground">Account</p>
        <h1 className="mt-1 font-display text-4xl md:text-5xl">My orders</h1>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center py-16">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      ) : !orders || orders.length === 0 ? (
        <div className="rounded-xl border border-dashed border-border bg-cream/50 p-12 text-center">
          <Package className="mx-auto h-9 w-9 text-muted-foreground" />
          <p className="mt-4 text-muted-foreground">You haven't placed any orders yet.</p>
          <Link to="/shop" className="mt-4 inline-block text-sm text-foreground underline-offset-4 hover:underline">
            Start shopping →
          </Link>
        </div>
      ) : (
        <ul className="space-y-5">
          {orders.map((o) => (
            <li key={o.id} className="rounded-2xl border border-border bg-background p-5">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="font-display text-lg">Order #{o.id}</p>
                  <p className="text-xs text-muted-foreground">
                    {new Date(o.createdAt).toLocaleDateString()} · {o.items?.length ?? 0} item
                    {(o.items?.length ?? 0) === 1 ? "" : "s"}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <div className="relative inline-flex items-center">
                    <select
                      value={o.status}
                      disabled={statusMutation.isPending}
                      onChange={(e) => statusMutation.mutate({ order: o, status: e.target.value })}
                      aria-label={`Update status for order ${o.id}`}
                      className={`appearance-none cursor-pointer rounded-full px-3 py-1 pr-7 text-xs font-medium capitalize outline-none transition focus:ring-2 focus:ring-foreground/20 disabled:opacity-60 ${STATUS_TINT[o.status] ?? "bg-muted text-foreground"}`}
                    >
                      {STATUS_OPTIONS.map((s) => (
                        <option key={s} value={s} className="bg-background capitalize text-foreground">
                          {s}
                        </option>
                      ))}
                    </select>
                    {statusMutation.isPending && statusMutation.variables?.order.id === o.id ? (
                      <Loader2 className="pointer-events-none absolute right-2 h-3 w-3 animate-spin opacity-70" />
                    ) : (
                      <svg className="pointer-events-none absolute right-2 h-3 w-3 opacity-60" viewBox="0 0 12 12" fill="none">
                        <path d="M3 4.5 6 7.5 9 4.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                      </svg>
                    )}
                  </div>
                  <span className="rounded-full bg-muted px-3 py-1 text-xs font-medium capitalize text-muted-foreground">
                    {o.paymentStatus}
                  </span>
                </div>
              </div>

              {o.items && o.items.length > 0 && (
                <ul className="mt-4 flex flex-wrap gap-3 border-t border-border pt-4">
                  {o.items.map((it) => (
                    <li key={it.id} className="flex items-center gap-2">
                      <div className="h-12 w-12 overflow-hidden rounded-md bg-cream">
                        <img
                          src={resolveImage(it.productImageUrl, it.productName, it.productId)}
                          alt={it.productName}
                          className="h-full w-full object-cover"
                        />
                      </div>
                      <div className="text-sm">
                        <p className="font-medium leading-tight">{it.productName}</p>
                        <p className="text-xs text-muted-foreground">
                          {it.quantity} × ${it.price}
                        </p>
                      </div>
                    </li>
                  ))}
                </ul>
              )}

              <div className="mt-4 flex justify-end border-t border-border pt-3 text-sm">
                <span className="text-muted-foreground">Total&nbsp;</span>
                <span className="font-medium">${o.totalPrice}</span>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
