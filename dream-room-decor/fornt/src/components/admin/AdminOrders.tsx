import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { api } from "@/lib/api";
import { resolveImage } from "@/lib/catalog";
import type { OrderResponse } from "@/lib/types";

const STATUSES         = ["pending", "processing", "shipped", "delivered", "cancelled"];
const PAYMENT_STATUSES = ["pending", "paid", "failed"];

const STATUS_TINT: Record<string, string> = {
  pending:    "bg-amber-100 text-amber-800",
  processing: "bg-blue-100 text-blue-800",
  shipped:    "bg-indigo-100 text-indigo-800",
  delivered:  "bg-emerald-100 text-emerald-800",
  cancelled:  "bg-rose-100 text-rose-800",
};

export function AdminOrders() {
  const qc = useQueryClient();
  const { data: orders, isLoading } = useQuery({
    queryKey: ["admin", "orders"],
    queryFn: async () => (await api.getOrders()).sort((a, b) => b.id - a.id),
  });

  const refresh = () => qc.invalidateQueries({ queryKey: ["admin", "orders"] });

  const update = async (
    o: OrderResponse,
    patch: Partial<Pick<OrderResponse, "status" | "paymentStatus">>,
  ) => {
    try {
      await api.updateOrder(o.id, {
        userId: o.userId,
        addressId: o.addressId,
        totalPrice: o.totalPrice,
        status: patch.status ?? o.status,
        paymentStatus: patch.paymentStatus ?? o.paymentStatus,
      });
      await refresh();
      toast.success(`Order #${o.id} updated.`);
    } catch {
      toast.error("Update failed.");
    }
  };

  return (
    <div>
      <h2 className="mb-4 font-display text-2xl">
        Orders{orders && <span className="ml-1 text-muted-foreground">({orders.length})</span>}
      </h2>

      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : !orders || orders.length === 0 ? (
        <p className="text-muted-foreground">No orders yet.</p>
      ) : (
        <div className="space-y-4">
          {orders.map((o) => (
            <div key={o.id} className="rounded-2xl border border-border bg-background p-5">
              {/* Order header */}
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="font-display text-lg">Order #{o.id}</p>
                  <p className="text-xs text-muted-foreground">
                    {new Date(o.createdAt).toLocaleDateString()} ·{" "}
                    {o.user?.fullName ?? `User #${o.userId}`} · ${o.totalPrice}
                  </p>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  {/* Status */}
                  <div className="flex items-center gap-1.5">
                    <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium capitalize ${STATUS_TINT[o.status] ?? "bg-muted text-foreground"}`}>
                      {o.status}
                    </span>
                    <select
                      value={o.status}
                      onChange={(e) => update(o, { status: e.target.value })}
                      className="rounded-lg border border-border bg-background px-2 py-1 text-xs capitalize outline-none focus:border-foreground"
                    >
                      {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                  {/* Payment */}
                  <div className="flex items-center gap-1.5">
                    <span className="rounded-full bg-muted px-2.5 py-0.5 text-xs font-medium capitalize text-muted-foreground">
                      {o.paymentStatus}
                    </span>
                    <select
                      value={o.paymentStatus}
                      onChange={(e) => update(o, { paymentStatus: e.target.value })}
                      className="rounded-lg border border-border bg-background px-2 py-1 text-xs capitalize outline-none focus:border-foreground"
                    >
                      {PAYMENT_STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                </div>
              </div>

              {/* Order items with images */}
              {o.items && o.items.length > 0 && (
                <ul className="mt-4 flex flex-wrap gap-3 border-t border-border pt-4">
                  {o.items.map((it) => (
                    <li key={it.id} className="flex items-center gap-2.5">
                      <div className="h-14 w-14 flex-shrink-0 overflow-hidden rounded-xl border border-border bg-cream">
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
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
