// Route guards. They wait for auth hydration (authReady) before deciding, so
// a logged-in user isn't bounced on a refresh.

import { useEffect } from "react";
import { useNavigate } from "@tanstack/react-router";
import { useApp } from "@/context/AppContext";

export function useRequireAuth() {
  const { user, authReady } = useApp();
  const navigate = useNavigate();
  useEffect(() => {
    if (authReady && !user) navigate({ to: "/login" });
  }, [authReady, user, navigate]);
  return { ready: authReady && !!user };
}

export function useRequireAdmin() {
  const { user, isAdmin, authReady } = useApp();
  const navigate = useNavigate();
  useEffect(() => {
    if (!authReady) return;
    if (!user) navigate({ to: "/login" });
    else if (!isAdmin) navigate({ to: "/" });
  }, [authReady, user, isAdmin, navigate]);
  return { ready: authReady && isAdmin };
}
