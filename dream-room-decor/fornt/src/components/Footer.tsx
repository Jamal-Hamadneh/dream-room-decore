export function Footer() {
  return (
    <footer className="mt-24 border-t border-border/60 bg-cream/40">
      <div className="mx-auto grid max-w-7xl gap-10 px-4 py-14 sm:px-6 md:grid-cols-4 lg:px-8">
        <div>
          <div className="font-display text-2xl font-semibold">
            haus<span className="text-accent">.</span>
          </div>
          <p className="mt-3 max-w-xs text-sm text-muted-foreground">
            Honest furniture, made to live with for years. Designed in Copenhagen.
          </p>
        </div>
        <div>
          <h4 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">Shop</h4>
          <ul className="space-y-2 text-sm">
            <li>Sofas</li><li>Beds</li><li>Chairs</li><li>Tables</li>
          </ul>
        </div>
        <div>
          <h4 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">Help</h4>
          <ul className="space-y-2 text-sm">
            <li>Shipping</li><li>Returns</li><li>Care guide</li><li>Contact</li>
          </ul>
        </div>
        <div>
          <h4 className="mb-3 text-xs font-semibold uppercase tracking-widest text-muted-foreground">Studio</h4>
          <ul className="space-y-2 text-sm">
            <li>About</li><li>Sustainability</li><li>Press</li>
          </ul>
        </div>
      </div>
      <div className="border-t border-border/60 px-6 py-5 text-center text-xs text-muted-foreground">
        © {new Date().getFullYear()} haus studio. All rights reserved.
      </div>
    </footer>
  );
}
