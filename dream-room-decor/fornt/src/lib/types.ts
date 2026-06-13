// Types mirroring the ASP.NET Core backend contracts (WebApplication1.Contracts).
// Kept in sync by hand — see the backend's Contracts/Requests and Contracts/Responses.

export type AuthResponse = {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  token: string;
  refreshToken: string;
  expiresAt: string;
  refreshTokenExpiresAt: string;
};

export type RegisterRequest = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone?: string | null;
  profileImage?: string | null;
  role?: "customer" | "admin";
};

export type LoginRequest = {
  email: string;
  password: string;
};

export type CategorySummary = {
  id: number;
  name: string;
};

export type ProductImageSummary = {
  id: number;
  imageUrl: string;
  isMain: boolean;
};

export type ProductVariantSummary = {
  id: number;
  color?: string | null;
  size?: string | null;
  material?: string | null;
  sku: string;
  price: number;
  stockQuantity: number;
};

export type ProductResponse = {
  id: number;
  categoryId: number;
  name: string;
  description: string;
  price: number;
  discountPrice?: number | null;
  stockQuantity: number;
  material?: string | null;
  color?: string | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  isActive: boolean;
  isFeatured: boolean;
  averageRating: number;
  reviewsCount: number;
  createdAt: string;
  updatedAt?: string | null;
  category?: CategorySummary | null;
  mainImageUrl?: string | null;
  images: ProductImageSummary[];
  variants: ProductVariantSummary[];
};

export type ProductSummary = {
  id: number;
  name: string;
  price: number;
  discountPrice?: number | null;
  mainImageUrl?: string | null;
  isActive: boolean;
};

export type CategoryResponse = {
  id: number;
  name: string;
  createdAt: string;
  productsCount: number;
  products: ProductSummary[];
};

export type FavoriteResponse = {
  id: number;
  userId: number;
  productId: number;
  createdAt: string;
  product?: ProductSummary | null;
};

export type CartItemSummary = {
  id: number;
  productId: number;
  productVariantId?: number | null;
  productName: string;
  productImageUrl?: string | null;
  variantSku?: string | null;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
};

export type CartResponse = {
  id: number;
  userId: number;
  createdAt: string;
  updatedAt?: string | null;
  items: CartItemSummary[];
  itemsCount: number;
  totalPrice: number;
};

export type CartItemResponse = {
  id: number;
  cartId: number;
  productId: number;
  productVariantId?: number | null;
  quantity: number;
  createdAt: string;
};

export type AddressResponse = {
  id: number;
  userId: number;
  country: string;
  city: string;
  street: string;
  building?: string | null;
  postalCode?: string | null;
  isDefault: boolean;
  createdAt: string;
};

export type OrderItemSummary = {
  id: number;
  productId: number;
  productVariantId?: number | null;
  productName: string;
  productImageUrl?: string | null;
  quantity: number;
  price: number;
};

export type UserSummary = {
  id: number;
  fullName: string;
  email: string;
};

export type AddressSummary = {
  id: number;
  country: string;
  city: string;
  street: string;
  building?: string | null;
  isDefault: boolean;
};

export type OrderResponse = {
  id: number;
  userId: number;
  addressId: number;
  totalPrice: number;
  status: string;
  paymentStatus: string;
  stripePaymentIntentId?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  user?: UserSummary | null;
  address?: AddressSummary | null;
  items: OrderItemSummary[];
};

export type UserResponse = {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string | null;
  profileImage?: string | null;
  role: string;
  createdAt: string;
  updatedAt?: string | null;
};

export type ReviewResponse = {
  id: number;
  userId: number;
  productId: number;
  rating: number;
  comment?: string | null;
  createdAt: string;
  user?: UserSummary | null;
};

// ─── AI Room ──────────────────────────────────────────────────────────────────
// The backend composites furniture onto an 800×600 canvas; PositionX/Y are the
// item's CENTER in that pixel space, Scale multiplies a 220px base width, and
// Rotation is in degrees.

export type AiRoomProduct = {
  productId: number;
  name: string;
  description: string;
  material?: string | null;
  color?: string | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  quantity: number;
  imageUrl?: string | null;
};

export type UploadAndCreateDesignResponse = {
  roomUploadId: number;
  roomDesignId: number;
  imageUrl: string;
  cartProducts: AiRoomProduct[];
};

export type PlacementResponse = {
  id: number;
  roomDesignId: number;
  productId: number;
  positionX: number;
  positionY: number;
  rotation: number;
  scale: number;
  zIndex: number;
};

export type GenerateRealisticDesignResponse = {
  generatedRoomImageId: number;
  generatedImageUrl: string;
  aiAnalysisJson: string;
};

// ─── Chat assistant ─────────────────────────────────────────────────────────

export type RecommendedProduct = {
  id: number;
  name: string;
  price: number;
  imageUrl?: string | null;
  category: string;
};

export type ChatMessageResponse = {
  conversationId: number;
  message: string;
  recommendedProducts: RecommendedProduct[];
  createdAt: string;
};

export type ChatMessageDto = {
  id: number;
  role: string; // "user" | "assistant"
  content: string;
  recommendedProducts: RecommendedProduct[];
  createdAt: string;
};

export type ConversationSummaryResponse = {
  id: number;
  title: string;
  lastMessagePreview?: string | null;
  createdAt: string;
  updatedAt?: string | null;
};

export type ConversationDetailResponse = {
  id: number;
  title: string;
  createdAt: string;
  updatedAt?: string | null;
  messages: ChatMessageDto[];
};

// ─── Stripe / Payments ──────────────────────────────────────────────────────

export type StripeConfigResponse = {
  publishableKey: string;
  currency: string;
  isConfigured: boolean;
};

export type CreatePaymentIntentResponse = {
  orderId: number;
  paymentId: number;
  paymentIntentId: string;
  clientSecret: string;
  amount: number;
  currency: string;
  status: string;
  publishableKey: string;
};

export type SyncPaymentIntentResponse = {
  orderId: number;
  paymentId: number;
  paymentIntentId: string;
  paymentStatus: string;
  orderStatus: string;
  stripeStatus: string;
};

// Form input for create/update of a product (mirrors ProductRequest).
export type ProductInput = {
  categoryId: number;
  name: string;
  description: string;
  price: number;
  discountPrice?: number | null;
  stockQuantity: number;
  material?: string | null;
  color?: string | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  isActive: boolean;
  isFeatured: boolean;
  averageRating: number;
  reviewsCount: number;
};
