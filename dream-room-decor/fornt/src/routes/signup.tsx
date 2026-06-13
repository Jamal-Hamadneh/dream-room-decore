import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { useApp } from "@/context/AppContext";

export const Route = createFileRoute("/signup")({
  head: () => ({ meta: [{ title: "Create account — Dream Room Decor" }] }),
  component: SignupPage,
});

function SignupPage() {
  const { signUp, user, authReady } = useApp();
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);

  // Already signed in? Skip the form and go home.
  useEffect(() => {
    if (authReady && user) navigate({ to: "/" });
  }, [authReady, user, navigate]);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErr("");
    if (!name.trim()) return setErr("Please enter your name.");
    if (!email.includes("@")) return setErr("Enter a valid email.");
    if (password.length < 8) return setErr("Password must be at least 8 characters.");

    const trimmed = name.trim();
    const spaceIdx = trimmed.indexOf(" ");
    const firstName = spaceIdx === -1 ? trimmed : trimmed.slice(0, spaceIdx);
    const lastName = spaceIdx === -1 ? trimmed : trimmed.slice(spaceIdx + 1);

    setLoading(true);
    try {
      await signUp(firstName, lastName, email, password);
      navigate({ to: "/" });
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Could not create your account.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="mx-auto flex min-h-[70vh] max-w-md flex-col justify-center px-4 py-12 sm:px-6">
      <h1 className="font-display text-4xl">Create your account</h1>
      <p className="mt-2 text-sm text-muted-foreground">Save your wishlist and orders.</p>

      <form onSubmit={submit} className="mt-8 space-y-4">
        <Field label="Full name" value={name} onChange={(v) => setName(v)} />
        <Field label="Email" type="email" value={email} onChange={(v) => setEmail(v)} />
        <Field label="Password" type="password" value={password} onChange={(v) => setPassword(v)} />
        {err && <p className="text-sm text-destructive">{err}</p>}
        <button
          disabled={loading}
          className="w-full rounded-full bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition hover:opacity-90 disabled:opacity-60"
        >
          {loading ? "Creating account…" : "Create account"}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-muted-foreground">
        Already have an account? <Link to="/login" className="text-foreground underline-offset-4 hover:underline">Sign in</Link>
      </p>
    </div>
  );
}

function Field({
  label, type = "text", value, onChange,
}: { label: string; type?: string; value: string; onChange: (v: string) => void }) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">{label}</span>
      <input
        type={type} value={value} onChange={(e) => onChange(e.target.value)}
        className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none transition focus:border-foreground"
        required
      />
    </label>
  );
}
