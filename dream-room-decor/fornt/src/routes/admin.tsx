import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { Boxes, LayoutDashboard, ShoppingCart, Tags, Users } from "lucide-react";
import { useRequireAdmin } from "@/hooks/useAuthGuard";
import { AdminProducts } from "@/components/admin/AdminProducts";
import { AdminCategories } from "@/components/admin/AdminCategories";
import { AdminOrders } from "@/components/admin/AdminOrders";
import { AdminUsers } from "@/components/admin/AdminUsers";

export const Route = createFileRoute("/admin")({
  head: () => ({ meta: [{ title: "Admin — Dream Room Decor" }] }),
  component: AdminPage,
});

const TABS = [
  { id: "products", label: "Products", icon: Boxes, render: () => <AdminProducts /> },
  { id: "categories", label: "Categories", icon: Tags, render: () => <AdminCategories /> },
  { id: "orders", label: "Orders", icon: ShoppingCart, render: () => <AdminOrders /> },
  { id: "users", label: "Users", icon: Users, render: () => <AdminUsers /> },
] as const;

function AdminPage() {
  const { ready } = useRequireAdmin();
  const [tab, setTab] = useState<(typeof TABS)[number]["id"]>("products");

  if (!ready) return null;

  const active = TABS.find((t) => t.id === tab)!;

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <div className="mb-8 flex items-center gap-2">
        <LayoutDashboard className="h-5 w-5 text-accent" />
        <h1 className="font-display text-4xl md:text-5xl">Admin</h1>
      </div>

      <div className="mb-8 flex flex-wrap gap-2 border-b border-border">
        {TABS.map((t) => {
          const Icon = t.icon;
          const isActive = t.id === tab;
          return (
            <button
              key={t.id}
              onClick={() => setTab(t.id)}
              className={`inline-flex items-center gap-2 border-b-2 px-4 py-2.5 text-sm transition ${
                isActive
                  ? "border-foreground font-medium text-foreground"
                  : "border-transparent text-muted-foreground hover:text-foreground"
              }`}
            >
              <Icon className="h-4 w-4" /> {t.label}
            </button>
          );
        })}
      </div>

      {active.render()}
    </div>
  );
}
