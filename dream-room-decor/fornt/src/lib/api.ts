// Typed API client for the ASP.NET Core backend.
//
// Requests go to /api/* which the Vite dev server proxies to the backend
// (see vite.config.ts) — this keeps the browser same-origin so the backend's
// missing CORS config is never an issue.
//
// The client stores the JWT + refresh token in localStorage, attaches the
// Bearer header automatically, and transparently refreshes an expired token
// once on a 401 before retrying the original request.

import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  ProductResponse,
  CategoryResponse,
  FavoriteResponse,
  CartResponse,
  CartItemResponse,
  AddressResponse,
  OrderResponse,
  UserResponse,
  ReviewResponse,
  ProductInput,
  UploadAndCreateDesignResponse,
  PlacementResponse,
  GenerateRealisticDesignResponse,
  ChatMessageResponse,
  ConversationSummaryResponse,
  ConversationDetailResponse,
  StripeConfigResponse,
  CreatePaymentIntentResponse,
  SyncPaymentIntentResponse,
} from "./types";

const BASE = "/api";

const TOKEN_KEY = "drd_token";
const REFRESH_KEY = "drd_refresh";
const USER_KEY = "drd_auth_user";

export type StoredUser = {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
};

const isBrowser = typeof window !== "undefined";

// --- token / user storage -------------------------------------------------

export function getToken(): string | null {
  return isBrowser ? localStorage.getItem(TOKEN_KEY) : null;
}

function getRefreshToken(): string | null {
  return isBrowser ? localStorage.getItem(REFRESH_KEY) : null;
}

export function getStoredUser(): StoredUser | null {
  if (!isBrowser) return null;
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as StoredUser;
  } catch {
    return null;
  }
}

function persistAuth(auth: AuthResponse) {
  if (!isBrowser) return;
  localStorage.setItem(TOKEN_KEY, auth.token);
  localStorage.setItem(REFRESH_KEY, auth.refreshToken);
  const user: StoredUser = {
    id: auth.id,
    firstName: auth.firstName,
    lastName: auth.lastName,
    email: auth.email,
    role: auth.role,
  };
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

function clearAuth() {
  if (!isBrowser) return;
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_KEY);
  localStorage.removeItem(USER_KEY);
}

// Lets AppContext react when the session is dropped (e.g. refresh failed).
let onUnauthorized: (() => void) | null = null;
export function setUnauthorizedListener(fn: (() => void) | null) {
  onUnauthorized = fn;
}

// --- errors ---------------------------------------------------------------

export class ApiError extends Error {
  status: number;
  errors?: Record<string, string[]>;
  constructor(status: number, message: string, errors?: Record<string, string[]>) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
  }
}

async function parseError(res: Response): Promise<ApiError> {
  let title = res.statusText || "Request failed";
  let errors: Record<string, string[]> | undefined;
  try {
    const body = await res.json();
    if (body?.title) title = body.title;
    if (body?.errors) {
      errors = body.errors;
      // Surface the first validation message so forms can show something useful.
      const first = Object.values(errors ?? {})[0]?.[0];
      if (first) title = first;
    }
  } catch {
    /* non-JSON body */
  }
  return new ApiError(res.status, title, errors);
}

// --- core request with one-shot refresh-on-401 ----------------------------

type RequestOptions = {
  method?: string;
  body?: unknown;
  auth?: boolean; // attach bearer token (default true)
  absolute?: boolean; // use `path` as-is instead of prefixing /api (for /AiRoom)
  _retried?: boolean;
};

async function rawRequest(path: string, opts: RequestOptions): Promise<Response> {
  const headers: Record<string, string> = {};
  if (opts.body !== undefined) headers["Content-Type"] = "application/json";
  if (opts.auth !== false) {
    const token = getToken();
    if (token) headers["Authorization"] = `Bearer ${token}`;
  }
  const url = opts.absolute ? path : `${BASE}${path}`;
  return fetch(url, {
    method: opts.method ?? "GET",
    headers,
    body: opts.body !== undefined ? JSON.stringify(opts.body) : undefined,
  });
}

// Multipart upload (FormData). Mirrors request()'s one-shot refresh-on-401 but
// lets the browser set the multipart boundary itself (no Content-Type header).
async function uploadForm<T>(path: string, form: FormData, opts: { absolute?: boolean; _retried?: boolean } = {}): Promise<T> {
  const headers: Record<string, string> = {};
  const token = getToken();
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const url = opts.absolute ? path : `${BASE}${path}`;
  const res = await fetch(url, { method: "POST", headers, body: form });

  if (res.status === 401 && !opts._retried && getRefreshToken()) {
    const refreshed = await tryRefresh();
    if (refreshed) return uploadForm<T>(path, form, { ...opts, _retried: true });
    clearAuth();
    onUnauthorized?.();
    throw await parseError(res);
  }
  if (!res.ok) throw await parseError(res);
  if (res.status === 204) return undefined as T;
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

async function tryRefresh(): Promise<boolean> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;
  try {
    const res = await rawRequest("/auth/refresh", {
      method: "POST",
      body: { refreshToken },
      auth: false,
    });
    if (!res.ok) return false;
    const auth = (await res.json()) as AuthResponse;
    persistAuth(auth);
    return true;
  } catch {
    return false;
  }
}

// The backend has a known bug: on create/update it maps the freshly-saved
// entity to a response DTO while its navigation properties are still null,
// throwing a NullReferenceException -> HTTP 500 *after the row is committed*.
// For those writes the data is actually persisted, so we swallow the 500 and
// let the caller reconcile by re-fetching. Genuine domain errors come back as
// 4xx (proper exceptions), so they still surface normally.
async function tolerant<T>(p: Promise<T>): Promise<T | null> {
  try {
    return await p;
  } catch (e) {
    if (e instanceof ApiError && e.status === 500) return null;
    throw e;
  }
}

async function request<T>(path: string, opts: RequestOptions = {}): Promise<T> {
  const res = await rawRequest(path, opts);

  if (res.status === 401 && opts.auth !== false && !opts._retried && getRefreshToken()) {
    const refreshed = await tryRefresh();
    if (refreshed) {
      return request<T>(path, { ...opts, _retried: true });
    }
    clearAuth();
    onUnauthorized?.();
    throw await parseError(res);
  }

  if (!res.ok) throw await parseError(res);

  if (res.status === 204) return undefined as T;
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

// --- public API -----------------------------------------------------------

export const api = {
  hasSession: () => !!getToken(),
  getStoredUser,

  async login(payload: LoginRequest): Promise<AuthResponse> {
    const auth = await request<AuthResponse>("/auth/login", {
      method: "POST",
      body: payload,
      auth: false,
    });
    persistAuth(auth);
    return auth;
  },

  async register(payload: RegisterRequest): Promise<AuthResponse> {
    const auth = await request<AuthResponse>("/auth/register", {
      method: "POST",
      body: { role: "customer", ...payload },
      auth: false,
    });
    persistAuth(auth);
    return auth;
  },

  async logout(): Promise<void> {
    const refreshToken = getRefreshToken();
    try {
      if (refreshToken) {
        await request<void>("/auth/logout", {
          method: "POST",
          body: { refreshToken },
        });
      }
    } catch {
      /* best-effort — clear locally regardless */
    } finally {
      clearAuth();
    }
  },

  // Catalog
  getProducts: () => request<ProductResponse[]>("/products"),
  getProduct: (id: number) => request<ProductResponse>(`/products/${id}`),
  getCategories: () => request<CategoryResponse[]>("/categories"),

  // Catalog admin (create/update tolerate the 500-after-persist bug)
  createProduct: (input: ProductInput) =>
    tolerant(request<ProductResponse>("/products", { method: "POST", body: input })),
  updateProduct: (id: number, input: ProductInput) =>
    tolerant(request<ProductResponse>(`/products/${id}`, { method: "PUT", body: input })),
  deleteProduct: (id: number) => request<void>(`/products/${id}`, { method: "DELETE" }),
  createCategory: (name: string) =>
    tolerant(request<CategoryResponse>("/categories", { method: "POST", body: { name } })),
  deleteCategory: (id: number) => request<void>(`/categories/${id}`, { method: "DELETE" }),

  // Users (admin)
  getUsers: () => request<UserResponse[]>("/users"),

  // Reviews
  getReviews: () => request<ReviewResponse[]>("/reviews"),
  createReview: (userId: number, productId: number, rating: number, comment?: string) =>
    tolerant(
      request<ReviewResponse>("/reviews", {
        method: "POST",
        body: { userId, productId, rating, comment: comment ?? null },
      }),
    ),
  deleteReview: (id: number) => request<void>(`/reviews/${id}`, { method: "DELETE" }),

  // Addresses
  getAddresses: () => request<AddressResponse[]>("/addresses"),
  deleteAddress: (id: number) => request<void>(`/addresses/${id}`, { method: "DELETE" }),

  // Favorites (add tolerates the backend's 500-after-persist bug)
  getFavorites: () => request<FavoriteResponse[]>("/favorites"),
  addFavorite: (userId: number, productId: number) =>
    tolerant(request<FavoriteResponse>("/favorites", { method: "POST", body: { userId, productId } })),
  removeFavorite: (id: number) => request<void>(`/favorites/${id}`, { method: "DELETE" }),

  // Cart (writes tolerate the 500-after-persist bug; caller re-fetches)
  getCarts: () => request<CartResponse[]>("/cart"),
  getCart: (id: number) => request<CartResponse>(`/cart/${id}`),
  createCart: (userId: number) =>
    tolerant(request<CartResponse>("/cart", { method: "POST", body: { userId } })),
  addCartItem: (cartId: number, productId: number, quantity: number, productVariantId?: number | null) =>
    tolerant(
      request<CartItemResponse>("/cart-items", {
        method: "POST",
        body: { cartId, productId, quantity, productVariantId: productVariantId ?? null },
      }),
    ),
  updateCartItem: (id: number, cartId: number, productId: number, quantity: number, productVariantId?: number | null) =>
    tolerant(
      request<CartItemResponse>(`/cart-items/${id}`, {
        method: "PUT",
        body: { cartId, productId, quantity, productVariantId: productVariantId ?? null },
      }),
    ),
  removeCartItem: (id: number) => request<void>(`/cart-items/${id}`, { method: "DELETE" }),

  // Addresses + Orders
  createAddress: (payload: {
    userId: number;
    country: string;
    city: string;
    street: string;
    building?: string | null;
    postalCode?: string | null;
    isDefault?: boolean;
  }) => request<AddressResponse>("/addresses", { method: "POST", body: payload }),
  getOrders: () => request<OrderResponse[]>("/orders"),
  getOrder: (id: number) => request<OrderResponse>(`/orders/${id}`),
  updateOrder: (
    id: number,
    payload: {
      userId: number;
      addressId: number;
      totalPrice: number;
      status: string;
      paymentStatus: string;
      stripePaymentIntentId?: string | null;
    },
  ) => tolerant(request<OrderResponse>(`/orders/${id}`, { method: "PUT", body: payload })),
  createOrder: (payload: {
    userId: number;
    addressId: number;
    totalPrice: number;
    status?: string;
    paymentStatus?: string;
  }) =>
    tolerant(
      request<OrderResponse>("/orders", {
        method: "POST",
        body: { status: "pending", paymentStatus: "pending", ...payload },
      }),
    ),
  createOrderItem: (payload: {
    orderId: number;
    productId: number;
    productVariantId?: number | null;
    quantity: number;
    price: number;
  }) => tolerant(request<unknown>("/order-items", { method: "POST", body: payload })),

  // Product variants (admin)
  createProductVariant: (payload: {
    productId: number;
    color?: string | null;
    size?: string | null;
    material?: string | null;
    sku: string;
    price: number;
    stockQuantity: number;
  }) => tolerant(request<unknown>("/product-variants", { method: "POST", body: payload })),
  deleteProductVariant: (id: number) => request<void>(`/product-variants/${id}`, { method: "DELETE" }),

  // Product images (admin)
  createProductImage: (payload: {
    productId: number;
    imageUrl: string;
    isMain: boolean;
  }) => tolerant(request<unknown>("/product-images", { method: "POST", body: payload })),
  deleteProductImage: (id: number) => request<void>(`/product-images/${id}`, { method: "DELETE" }),

  // ── AI Room (routed at /AiRoom, not under /api) ──────────────────────────────
  aiRoom: {
    uploadAndCreateDesign: (
      file: File,
      meta: { roomType?: string; height?: number; width?: number; depth?: number } = {},
    ) => {
      const form = new FormData();
      form.append("RoomImage", file);
      if (meta.roomType) form.append("RoomType", meta.roomType);
      if (meta.height != null) form.append("Height", String(meta.height));
      if (meta.width != null) form.append("Width", String(meta.width));
      if (meta.depth != null) form.append("Depth", String(meta.depth));
      return uploadForm<UploadAndCreateDesignResponse>("/AiRoom/UploadAndCreateDesign", form, { absolute: true });
    },
    savePlacement: (payload: {
      roomDesignId: number;
      productId: number;
      positionX: number;
      positionY: number;
      rotation: number;
      scale: number;
      zIndex: number;
    }) => request<PlacementResponse>("/AiRoom/SavePlacement", { method: "POST", body: payload, absolute: true }),
    updatePlacement: (payload: {
      placementId: number;
      positionX: number;
      positionY: number;
      rotation: number;
      scale: number;
      zIndex: number;
    }) => request<PlacementResponse>("/AiRoom/UpdatePlacement", { method: "POST", body: payload, absolute: true }),
    switchProduct: (payload: { roomDesignId: number; oldProductId: number; newProductId: number }) =>
      request<PlacementResponse>("/AiRoom/SwitchProduct", { method: "POST", body: payload, absolute: true }),
    generateRealisticDesign: (roomDesignId: number) =>
      request<GenerateRealisticDesignResponse>("/AiRoom/GenerateRealisticDesign", {
        method: "POST",
        body: { roomDesignId },
        absolute: true,
      }),
  },

  // ── Stripe payments ──────────────────────────────────────────────────────────
  stripe: {
    getConfig: () => request<StripeConfigResponse>("/stripe/config", { auth: false }),
    createPaymentIntent: (orderId: number) =>
      request<CreatePaymentIntentResponse>("/stripe/payment-intents", { method: "POST", body: { orderId } }),
    syncPaymentIntent: (paymentIntentId: string) =>
      request<SyncPaymentIntentResponse>("/stripe/sync-payment-intent", { method: "POST", body: { paymentIntentId } }),
  },

  // ── Chat assistant ───────────────────────────────────────────────────────────
  chat: {
    getConversations: () => request<ConversationSummaryResponse[]>("/chat/conversations"),
    getConversation: (id: number) => request<ConversationDetailResponse>(`/chat/conversations/${id}`),
    createConversation: (title?: string) =>
      request<ConversationSummaryResponse>("/chat/conversations", { method: "POST", body: { title: title ?? null } }),
    deleteConversation: (id: number) => request<void>(`/chat/conversations/${id}`, { method: "DELETE" }),
    sendMessage: (message: string, conversationId?: number) =>
      request<ChatMessageResponse>("/chat/message", {
        method: "POST",
        body: { message, conversationId: conversationId ?? null },
      }),
  },
};
