import { Link, useLocation } from "@tanstack/react-router";
import { ShoppingBag, User, Sparkles, Menu, X, LayoutDashboard } from "lucide-react";
import { useState } from "react";
import { useApp } from "@/context/AppContext";

const baseLinks = [
  { to: "/" as const, label: "Home" },
  { to: "/shop" as const, label: "Shop" },
  { to: "/ai-room" as const, label: "AI Room" },
];

export function Navbar() {
  const { cartCount, user, isAdmin } = useApp();
  const [open, setOpen] = useState(false);
  const location = useLocation();

  const links = [
    ...baseLinks,
    ...(user ? [{ to: "/orders" as const, label: "Orders" }] : []),
    ...(isAdmin ? [{ to: "/admin" as const, label: "Admin" }] : []),
  ];

  return (
    <header className="sticky top-0 z-40 border-b border-border/60 bg-background/80 backdrop-blur-md">
      <div className="mx-auto flex h-16 w-full max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <Link to="/" className="flex items-center gap-2">
          <span className="font-display text-2xl font-semibold tracking-tight">
            Dream Room Decor<span className="text-accent">.</span>
          </span>
        </Link>

        <nav className="hidden items-center gap-8 md:flex">
          {links.map((l) => {
            const active = location.pathname === l.to || (l.to !== "/" && location.pathname.startsWith(l.to));
            return (
              <Link
                key={l.to}
                to={l.to}
                className={`text-sm transition-colors hover:text-foreground ${
                  active ? "text-foreground font-medium" : "text-muted-foreground"
                }`}
              >
                {l.label === "AI Room" ? (
                  <span className="inline-flex items-center gap-1.5">
                    <Sparkles className="h-3.5 w-3.5 text-accent" />
                    {l.label}
                  </span>
                ) : l.label === "Admin" ? (
                  <span className="inline-flex items-center gap-1.5">
                    <LayoutDashboard className="h-3.5 w-3.5 text-accent" />
                    {l.label}
                  </span>
                ) : (
                  l.label
                )}
              </Link>
            );
          })}
        </nav>

        <div className="flex items-center gap-1">
          <Link
            to={user ? "/profile" : "/login"}
            className="rounded-full p-2 text-muted-foreground transition hover:bg-muted hover:text-foreground"
            aria-label="Profile"
          >
            <User className="h-5 w-5" />
          </Link>
          <Link
            to="/cart"
            className="relative rounded-full p-2 text-muted-foreground transition hover:bg-muted hover:text-foreground"
            aria-label="Cart"
          >
            <ShoppingBag className="h-5 w-5" />
            {cartCount > 0 && (
              <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-accent px-1 text-[10px] font-medium text-accent-foreground">
                {cartCount}
              </span>
            )}
          </Link>
          <button
            onClick={() => setOpen((o) => !o)}
            className="rounded-full p-2 text-muted-foreground transition hover:bg-muted md:hidden"
            aria-label="Menu"
          >
            {open ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
        </div>
      </div>

      {open && (
        <div className="border-t border-border/60 bg-background md:hidden">
          <nav className="mx-auto flex max-w-7xl flex-col px-4 py-3 sm:px-6">
            {links.map((l) => (
              <Link
                key={l.to}
                to={l.to}
                onClick={() => setOpen(false)}
                className="py-2 text-sm text-foreground"
              >
                {l.label}
              </Link>
            ))}
          </nav>
        </div>
      )}
    </header>
  );
}
