import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";

export function AdminUsers() {
  const { data: users, isLoading } = useQuery({
    queryKey: ["admin", "users"],
    queryFn: async () => (await api.getUsers()).sort((a, b) => a.id - b.id),
  });

  return (
    <div>
      <h2 className="mb-4 font-display text-2xl">Users {users && <span className="text-muted-foreground">({users.length})</span>}</h2>
      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-border">
          <table className="w-full text-sm">
            <thead className="bg-cream/60 text-left text-xs uppercase tracking-wider text-muted-foreground">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Phone</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Joined</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {users?.map((u) => (
                <tr key={u.id}>
                  <td className="px-4 py-3 font-medium">{u.firstName} {u.lastName}</td>
                  <td className="px-4 py-3 text-muted-foreground">{u.email}</td>
                  <td className="px-4 py-3 text-muted-foreground">{u.phone ?? "—"}</td>
                  <td className="px-4 py-3">
                    <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium capitalize ${u.role === "admin" ? "bg-accent/15 text-accent" : "bg-muted text-muted-foreground"}`}>
                      {u.role}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">{new Date(u.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
