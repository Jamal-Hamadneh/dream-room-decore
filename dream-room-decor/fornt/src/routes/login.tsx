import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useState } from "react";
import { useApp } from "@/context/AppContext";

export const Route = createFileRoute("/login")({
  head: () => ({ meta: [{ title: "Sign in — haus" }] }),
  component: LoginPage,
});

function LoginPage() {
  const { signIn } = useApp();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState("");

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.includes("@") || password.length < 6) {
      setErr("Enter a valid email and a password of 6+ characters.");
      return;
    }
    signIn(email);
    navigate({ to: "/profile" });
  };

  return (
    <div className="mx-auto flex min-h-[70vh] max-w-md flex-col justify-center px-4 py-12 sm:px-6">
      <h1 className="font-display text-4xl">Welcome back</h1>
      <p className="mt-2 text-sm text-muted-foreground">Sign in to your haus account.</p>

      <form onSubmit={submit} className="mt-8 space-y-4">
        <label className="block">
          <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">Email</span>
          <input
            type="email" value={email} onChange={(e) => setEmail(e.target.value)}
            className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none transition focus:border-foreground"
            required
          />
        </label>
        <label className="block">
          <span className="mb-1.5 block text-xs uppercase tracking-widest text-muted-foreground">Password</span>
          <input
            type="password" value={password} onChange={(e) => setPassword(e.target.value)}
            className="w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm outline-none transition focus:border-foreground"
            required
          />
        </label>
        {err && <p className="text-sm text-destructive">{err}</p>}
        <button className="w-full rounded-full bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition hover:opacity-90">
          Sign in
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-muted-foreground">
        New to haus? <Link to="/signup" className="text-foreground underline-offset-4 hover:underline">Create an account</Link>
      </p>
    </div>
  );
}
