import { useQuery } from "@tanstack/react-query";
import {
  TrendingUp, DollarSign, ShoppingBag, Users, Package,
  ArrowUpRight, Loader2,
} from "lucide-react";
import { api } from "@/lib/api";

// ── Fallback colors for category bars ────────────────────────────────────────
const CATEGORY_COLORS = ["bg-accent", "bg-emerald-500", "bg-blue-500", "bg-violet-500", "bg-amber-500"];

const STATUS_TINT: Record<string, string> = {
  pending: "bg-amber-100 text-amber-700",
  processing: "bg-blue-100 text-blue-700",
  shipped: "bg-indigo-100 text-indigo-700",
  delivered: "bg-emerald-100 text-emerald-700",
  cancelled: "bg-rose-100 text-rose-700",
};

function fmt(n: number) {
  if (n >= 1_000_000) return `$${(n / 1_000_000).toFixed(1)}M`;
  if (n >= 1_000) return `$${(n / 1_000).toFixed(1)}k`;
  return `$${n.toFixed(0)}`;
}

// ── Component ────────────────────────────────────────────────────────────────
export function AdminDashboard() {
  const { data: summary, isLoading: loadingSummary } = useQuery({
    queryKey: ["admin", "stats", "summary"],
    queryFn: api.stats.getSummary,
  });

  const { data: revenue, isLoading: loadingRevenue } = useQuery({
    queryKey: ["admin", "stats", "revenue"],
    queryFn: () => api.stats.getRevenue(),
  });

  const { data: categories, isLoading: loadingCategories } = useQuery({
    queryKey: ["admin", "stats", "categories"],
    queryFn: api.stats.getCategories,
  });

  const { data: recentOrders, isLoading: loadingOrders } = useQuery({
    queryKey: ["admin", "orders"],
    queryFn: async () => (await api.getOrders()).sort((a, b) => b.id - a.id).slice(0, 5),
  });

  const revenueMax = Math.max(...(revenue?.months.map((m) => m.revenue) ?? [1]));

  const stats = summary
    ? [
        { label: "Total Revenue", value: fmt(summary.totalRevenue), icon: DollarSign, tint: "text-emerald-600", bg: "bg-emerald-50" },
        { label: "Orders", value: summary.orderCount.toLocaleString(), icon: ShoppingBag, tint: "text-blue-600", bg: "bg-blue-50" },
        { label: "Customers", value: summary.customerCount.toLocaleString(), icon: Users, tint: "text-violet-600", bg: "bg-violet-50" },
        { label: "Avg. Order", value: fmt(summary.averageOrderValue), icon: Package, tint: "text-amber-600", bg: "bg-amber-50" },
      ]
    : [];

  return (
    <div className="space-y-6">
      {/* Stat cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {loadingSummary
          ? Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="flex h-32 items-center justify-center rounded-2xl border border-border bg-background">
                <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
              </div>
            ))
          : stats.map((s) => {
              const Icon = s.icon;
              return (
                <div
                  key={s.label}
                  className="rounded-2xl border border-border bg-background p-5 transition hover:shadow-[var(--shadow-warm)]"
                >
                  <div className="flex items-start justify-between">
                    <div className={`flex h-10 w-10 items-center justify-center rounded-xl ${s.bg}`}>
                      <Icon className={`h-5 w-5 ${s.tint}`} />
                    </div>
                    <span className="inline-flex items-center gap-0.5 rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700">
                      <TrendingUp className="h-3 w-3" /> live
                    </span>
                  </div>
                  <p className="mt-4 font-display text-3xl">{s.value}</p>
                  <p className="mt-1 text-sm text-muted-foreground">{s.label}</p>
                </div>
              );
            })}
      </div>

      {/* Charts row */}
      <div className="grid gap-6 lg:grid-cols-[1.6fr_1fr]">
        {/* Monthly revenue bar chart */}
        <div className="rounded-2xl border border-border bg-background p-6">
          <div className="mb-6 flex items-center justify-between">
            <div>
              <h3 className="font-display text-xl">Revenue overview</h3>
              <p className="text-sm text-muted-foreground">
                Monthly sales — {revenue?.year ?? new Date().getFullYear()}
              </p>
            </div>
          </div>
          {loadingRevenue ? (
            <div className="flex h-52 items-center justify-center">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : (
            <div className="flex h-52 items-end justify-between gap-2">
              {revenue?.months.map((m) => (
                <div key={m.month} className="group flex flex-1 flex-col items-center gap-2">
                  <div className="relative flex w-full flex-1 items-end">
                    <div
                      className="w-full rounded-t-md bg-accent/80 transition-all duration-300 group-hover:bg-accent"
                      style={{ height: `${revenueMax === 0 ? 4 : Math.max(4, (m.revenue / revenueMax) * 100)}%` }}
                    >
                      <span className="absolute -top-5 left-1/2 -translate-x-1/2 whitespace-nowrap text-[10px] font-medium text-foreground opacity-0 transition group-hover:opacity-100">
                        {fmt(m.revenue)}
                      </span>
                    </div>
                  </div>
                  <span className="text-[10px] text-muted-foreground">{m.monthName.slice(0, 3)}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Category breakdown */}
        <div className="rounded-2xl border border-border bg-background p-6">
          <h3 className="font-display text-xl">Sales by category</h3>
          <p className="text-sm text-muted-foreground">Share of paid orders</p>
          {loadingCategories ? (
            <div className="flex h-40 items-center justify-center">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : !categories || categories.length === 0 ? (
            <p className="mt-6 text-sm text-muted-foreground">No sales data yet.</p>
          ) : (
            <div className="mt-6 space-y-4">
              {categories.map((c, i) => (
                <div key={c.categoryId}>
                  <div className="mb-1.5 flex items-center justify-between text-sm">
                    <span className="font-medium">{c.categoryName}</span>
                    <span className="text-muted-foreground">{c.percentage}%</span>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-muted">
                    <div
                      className={`h-full rounded-full ${CATEGORY_COLORS[i % CATEGORY_COLORS.length]}`}
                      style={{ width: `${c.percentage}%` }}
                    />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Recent orders */}
      <div className="rounded-2xl border border-border bg-background p-6">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-display text-xl">Recent orders</h3>
          <button className="inline-flex items-center gap-1 text-sm text-accent hover:underline">
            View all <ArrowUpRight className="h-3.5 w-3.5" />
          </button>
        </div>
        {loadingOrders ? (
          <div className="flex items-center justify-center py-10">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        ) : !recentOrders || recentOrders.length === 0 ? (
          <p className="text-sm text-muted-foreground">No orders yet.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="text-left text-xs uppercase tracking-wider text-muted-foreground">
                <tr className="border-b border-border">
                  <th className="pb-2 font-medium">Order</th>
                  <th className="pb-2 font-medium">Customer</th>
                  <th className="pb-2 font-medium">Amount</th>
                  <th className="pb-2 font-medium">Status</th>
                  <th className="pb-2 font-medium">Payment</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {recentOrders.map((o) => (
                  <tr key={o.id} className="transition hover:bg-cream/40">
                    <td className="py-3 font-medium">#{o.id}</td>
                    <td className="py-3 text-muted-foreground">
                      {o.user?.fullName ?? "—"}
                    </td>
                    <td className="py-3 font-medium">${o.totalPrice.toFixed(2)}</td>
                    <td className="py-3">
                      <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium capitalize ${STATUS_TINT[o.status] ?? "bg-muted text-muted-foreground"}`}>
                        {o.status}
                      </span>
                    </td>
                    <td className="py-3">
                      <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium capitalize ${o.paymentStatus === "paid" ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"}`}>
                        {o.paymentStatus}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
