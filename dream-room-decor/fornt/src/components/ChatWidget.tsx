import { useEffect, useRef, useState } from "react";
import { Link } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  MessageCircle, X, Send, Plus, ArrowLeft, Trash2, Loader2, Sparkles, Bot,
} from "lucide-react";
import { toast } from "sonner";
import { api, ApiError } from "@/lib/api";
import { resolveImage } from "@/lib/catalog";
import { useApp } from "@/context/AppContext";
import type { ChatMessageDto, RecommendedProduct } from "@/lib/types";

type View = "list" | "chat";

export function ChatWidget() {
  const { user, authReady } = useApp();
  const qc = useQueryClient();

  const [open, setOpen] = useState(false);
  const [view, setView] = useState<View>("list");
  const [activeId, setActiveId] = useState<number | null>(null);
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [input, setInput] = useState("");
  const [loadingConv, setLoadingConv] = useState(false);

  const scrollRef = useRef<HTMLDivElement>(null);

  // Conversation list — only fetched while the panel is open and signed in.
  const { data: conversations, isLoading: loadingList } = useQuery({
    queryKey: ["chat", "conversations"],
    queryFn: api.chat.getConversations,
    enabled: open && !!user && view === "list",
  });

  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: "smooth" });
  }, [messages, loadingConv]);

  const sendMutation = useMutation({
    mutationFn: (vars: { text: string; conversationId: number | null }) =>
      api.chat.sendMessage(vars.text, vars.conversationId ?? undefined),
    onSuccess: (res) => {
      setActiveId(res.conversationId);
      setMessages((m) => [
        ...m,
        {
          id: Date.now(),
          role: "assistant",
          content: res.message,
          recommendedProducts: res.recommendedProducts,
          createdAt: res.createdAt,
        },
      ]);
      qc.invalidateQueries({ queryKey: ["chat", "conversations"] });
    },
    onError: (e) => {
      // Roll back the optimistic user bubble.
      setMessages((m) => m.slice(0, -1));
      if (e instanceof ApiError && e.status === 429) {
        toast.error("You're sending messages too quickly — give it a moment and try again.");
      } else {
        toast.error("The assistant couldn't reply. Please try again.");
      }
    },
  });

  const openConversation = async (id: number) => {
    setLoadingConv(true);
    setView("chat");
    setActiveId(id);
    setMessages([]);
    try {
      const detail = await api.chat.getConversation(id);
      setMessages(detail.messages);
    } catch {
      toast.error("Couldn't open that conversation.");
      setView("list");
    } finally {
      setLoadingConv(false);
    }
  };

  const newChat = () => {
    setActiveId(null);
    setMessages([]);
    setView("chat");
  };

  const backToList = () => {
    setView("list");
    setActiveId(null);
    setMessages([]);
    qc.invalidateQueries({ queryKey: ["chat", "conversations"] });
  };

  const removeConversation = async (id: number, e: React.MouseEvent) => {
    e.stopPropagation();
    try {
      await api.chat.deleteConversation(id);
      qc.invalidateQueries({ queryKey: ["chat", "conversations"] });
      if (activeId === id) backToList();
    } catch {
      toast.error("Couldn't delete that conversation.");
    }
  };

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    const text = input.trim();
    if (!text || sendMutation.isPending) return;
    setMessages((m) => [
      ...m,
      { id: Date.now(), role: "user", content: text, recommendedProducts: [], createdAt: new Date().toISOString() },
    ]);
    setInput("");
    sendMutation.mutate({ text, conversationId: activeId });
  };

  if (!authReady) return null;

  return (
    <>
      {/* Launcher */}
      {!open && (
        <button
          onClick={() => setOpen(true)}
          aria-label="Open chat assistant"
          className="fixed bottom-5 right-5 z-50 flex h-14 w-14 items-center justify-center rounded-full bg-primary text-primary-foreground shadow-[var(--shadow-warm)] transition hover:scale-105"
        >
          <MessageCircle className="h-6 w-6" />
        </button>
      )}

      {/* Panel */}
      {open && (
        <div className="fixed bottom-5 right-5 z-50 flex h-[min(600px,80vh)] w-[min(380px,calc(100vw-2.5rem))] flex-col overflow-hidden rounded-2xl border border-border bg-background shadow-[var(--shadow-warm)]">
          {/* Header */}
          <div className="flex items-center justify-between border-b border-border bg-cream/60 px-4 py-3">
            <div className="flex items-center gap-2">
              {view === "chat" && user && (
                <button onClick={backToList} className="rounded-full p-1 hover:bg-muted" aria-label="Back">
                  <ArrowLeft className="h-4 w-4" />
                </button>
              )}
              <Sparkles className="h-4 w-4 text-accent" />
              <span className="font-display text-base">Design Assistant</span>
            </div>
            <button onClick={() => setOpen(false)} className="rounded-full p-1 hover:bg-muted" aria-label="Close chat">
              <X className="h-4 w-4" />
            </button>
          </div>

          {/* Body */}
          {!user ? (
            <SignedOut />
          ) : view === "list" ? (
            <ConversationList
              conversations={conversations}
              loading={loadingList}
              onOpen={openConversation}
              onDelete={removeConversation}
              onNew={newChat}
            />
          ) : (
            <ChatView
              messages={messages}
              loading={loadingConv}
              sending={sendMutation.isPending}
              input={input}
              setInput={setInput}
              onSubmit={submit}
              scrollRef={scrollRef}
            />
          )}
        </div>
      )}
    </>
  );
}

function SignedOut() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-3 px-6 text-center">
      <Bot className="h-10 w-10 text-muted-foreground" />
      <p className="text-sm text-muted-foreground">Sign in to chat with our design assistant and get product recommendations.</p>
      <Link to="/login" className="rounded-full bg-primary px-5 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90">
        Sign in
      </Link>
    </div>
  );
}

function ConversationList({
  conversations, loading, onOpen, onDelete, onNew,
}: {
  conversations: import("@/lib/types").ConversationSummaryResponse[] | undefined;
  loading: boolean;
  onOpen: (id: number) => void;
  onDelete: (id: number, e: React.MouseEvent) => void;
  onNew: () => void;
}) {
  return (
    <div className="flex flex-1 flex-col overflow-hidden">
      <div className="p-3">
        <button
          onClick={onNew}
          className="inline-flex w-full items-center justify-center gap-2 rounded-full bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition hover:opacity-90"
        >
          <Plus className="h-4 w-4" /> New conversation
        </button>
      </div>
      <div className="flex-1 overflow-y-auto px-3 pb-3">
        {loading ? (
          <div className="flex justify-center py-8"><Loader2 className="h-5 w-5 animate-spin text-muted-foreground" /></div>
        ) : !conversations || conversations.length === 0 ? (
          <p className="px-2 py-8 text-center text-sm text-muted-foreground">No conversations yet. Start a new one above.</p>
        ) : (
          <ul className="space-y-1">
            {conversations.map((c) => (
              <li key={c.id}>
                <button
                  onClick={() => onOpen(c.id)}
                  className="group flex w-full items-center gap-2 rounded-xl px-3 py-2.5 text-left transition hover:bg-cream"
                >
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium">{c.title}</p>
                    {c.lastMessagePreview && <p className="truncate text-xs text-muted-foreground">{c.lastMessagePreview}</p>}
                  </div>
                  <span
                    onClick={(e) => onDelete(c.id, e)}
                    className="rounded-full p-1 text-muted-foreground opacity-0 transition hover:bg-muted hover:text-destructive group-hover:opacity-100"
                    aria-label="Delete conversation"
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </span>
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}

function ChatView({
  messages, loading, sending, input, setInput, onSubmit, scrollRef,
}: {
  messages: ChatMessageDto[];
  loading: boolean;
  sending: boolean;
  input: string;
  setInput: (v: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  scrollRef: React.RefObject<HTMLDivElement | null>;
}) {
  return (
    <div className="flex flex-1 flex-col overflow-hidden">
      <div ref={scrollRef} className="flex-1 space-y-3 overflow-y-auto p-3">
        {loading ? (
          <div className="flex justify-center py-8"><Loader2 className="h-5 w-5 animate-spin text-muted-foreground" /></div>
        ) : messages.length === 0 ? (
          <div className="flex h-full flex-col items-center justify-center gap-2 text-center">
            <Sparkles className="h-8 w-8 text-accent" />
            <p className="text-sm text-muted-foreground">Ask me to recommend furniture by room, style, or budget.</p>
          </div>
        ) : (
          messages.map((m) => <MessageBubble key={m.id} message={m} />)
        )}
        {sending && (
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <Loader2 className="h-3.5 w-3.5 animate-spin" /> Assistant is typing…
          </div>
        )}
      </div>

      <form onSubmit={onSubmit} className="flex items-center gap-2 border-t border-border p-3">
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Ask about furniture…"
          className="flex-1 rounded-full border border-border bg-background px-4 py-2 text-sm outline-none focus:border-foreground"
        />
        <button
          type="submit"
          disabled={!input.trim() || sending}
          className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-primary text-primary-foreground transition hover:opacity-90 disabled:opacity-50"
          aria-label="Send"
        >
          <Send className="h-4 w-4" />
        </button>
      </form>
    </div>
  );
}

function MessageBubble({ message }: { message: ChatMessageDto }) {
  const isUser = message.role === "user";
  return (
    <div className={`flex flex-col gap-2 ${isUser ? "items-end" : "items-start"}`}>
      <div
        className={`max-w-[85%] whitespace-pre-wrap rounded-2xl px-3.5 py-2 text-sm ${
          isUser ? "rounded-br-sm bg-primary text-primary-foreground" : "rounded-bl-sm bg-cream text-foreground"
        }`}
      >
        {message.content}
      </div>
      {!isUser && message.recommendedProducts.length > 0 && (
        <div className="flex w-full flex-col gap-2">
          {message.recommendedProducts.map((p) => <ProductCard key={p.id} product={p} />)}
        </div>
      )}
    </div>
  );
}

function ProductCard({ product }: { product: RecommendedProduct }) {
  return (
    <Link
      to="/shop/$productId"
      params={{ productId: String(product.id) }}
      className="flex items-center gap-3 rounded-xl border border-border bg-background p-2 transition hover:border-foreground"
    >
      <div className="h-12 w-12 flex-shrink-0 overflow-hidden rounded-lg bg-cream">
        <img src={resolveImage(product.imageUrl, product.name, product.id)} alt={product.name} className="h-full w-full object-cover" />
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium">{product.name}</p>
        <p className="text-xs text-muted-foreground">{product.category}</p>
      </div>
      <span className="text-sm font-medium">${product.price}</span>
    </Link>
  );
}
