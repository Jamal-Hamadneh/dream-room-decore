# Dream Room Decore API Docs

Base URL for local development:

```text
http://localhost:5039
```

Most endpoints require JWT:

```http
Authorization: Bearer YOUR_ACCESS_TOKEN
```

## Frontend Quickstart

Use this document as the frontend integration contract. All JSON examples use camelCase because that is what the API returns.

Local API constant:

```ts
export const API_BASE_URL = "http://localhost:5039";
```

Recommended token storage during development:

```ts
localStorage.setItem("accessToken", loginResponse.token);
localStorage.setItem("refreshToken", loginResponse.refreshToken);
```

For production, prefer secure httpOnly cookies if the backend is later changed to support them.

### JSON Request Helper

```ts
async function apiJson<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem("accessToken");

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw data ?? { title: "Request failed", status: response.status };
  }

  return data as T;
}
```

### Multipart Request Helper

Do not set `Content-Type` manually for `FormData`; the browser adds the boundary.

```ts
async function apiForm<T>(path: string, formData: FormData): Promise<T> {
  const token = localStorage.getItem("accessToken");

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: "POST",
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: formData,
  });

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw data ?? { title: "Request failed", status: response.status };
  }

  return data as T;
}
```

### Error Response Shapes

Normal API errors:

```json
{
  "title": "Product must be in your cart before using it in room design.",
  "status": 400
}
```

Validation errors:

```json
{
  "title": "Validation failed.",
  "status": 400,
  "errors": {
    "email": ["Email is required."]
  }
}
```

Database/relationship errors:

```json
{
  "title": "Database operation failed.",
  "status": 400,
  "detail": "Check related ids, unique fields, and required values."
}
```

Common statuses the frontend should handle:

| Status | Meaning | Frontend Action |
| --- | --- | --- |
| `400` | Invalid request or business rule failed | Show `title` to user |
| `401` | Missing/expired access token | Try refresh token, then redirect to login |
| `403` | Authenticated but not allowed | Show access denied |
| `404` | Resource not found | Show empty/not-found state |
| `409` | Conflict, duplicate, or invalid state | Show `title` to user |
| `500` | Unexpected backend error | Show generic error and check backend logs |

### Refresh Token Flow

When a protected request returns `401`, call refresh once:

```ts
type AuthResponse = {
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

async function refreshAccessToken(): Promise<AuthResponse> {
  const refreshToken = localStorage.getItem("refreshToken");

  const data = await apiJson<AuthResponse>("/api/auth/refresh", {
    method: "POST",
    body: JSON.stringify({ refreshToken }),
  });

  localStorage.setItem("accessToken", data.token);
  localStorage.setItem("refreshToken", data.refreshToken);
  return data;
}
```

## Frontend Endpoint Map

| Feature | Method | Endpoint | Auth | Content Type |
| --- | --- | --- | --- | --- |
| Login | `POST` | `/api/auth/login` | No | JSON |
| Register | `POST` | `/api/auth/register` | No | JSON |
| Refresh token | `POST` | `/api/auth/refresh` | No | JSON |
| Logout | `POST` | `/api/auth/logout` | Yes | JSON |
| Categories | `GET` | `/api/categories` | Yes | JSON |
| Products | `GET` | `/api/products` | Yes | JSON |
| Product images | `GET` | `/api/product-images` | Yes | JSON |
| Product variants | `GET` | `/api/product-variants` | Yes | JSON |
| Cart | `GET` | `/api/cart` | Yes | JSON |
| Cart items | `GET` | `/api/cart-items` | Yes | JSON |
| Favorites | `GET` | `/api/favorites` | Yes | JSON |
| Orders | `GET` | `/api/orders` | Yes | JSON |
| Reviews | `GET` | `/api/reviews` | Yes | JSON |
| Chatbot config | `GET` | `/api/chatbot/config` | No | JSON |
| Chatbot context | `GET` | `/api/chatbot/context` | Yes | JSON |
| Upload room | `POST` | `/AiRoom/UploadAndCreateDesign` | Yes | FormData |
| Save placement | `POST` | `/AiRoom/SavePlacement` | Yes | JSON |
| Update placement | `POST` | `/AiRoom/UpdatePlacement` | Yes | JSON |
| Switch product | `POST` | `/AiRoom/SwitchProduct` | Yes | JSON |
| Generate room image | `POST` | `/AiRoom/GenerateRealisticDesign` | Yes | JSON |

## Main Frontend DTOs

### Product Card DTO

Use this for product listing/grid cards:

```ts
type ProductResponse = {
  id: number;
  categoryId: number;
  name: string;
  description: string;
  price: number;
  discountPrice: number | null;
  stockQuantity: number;
  material: string | null;
  color: string | null;
  height: number | null;
  width: number | null;
  depth: number | null;
  isActive: boolean;
  isFeatured: boolean;
  averageRating: number;
  reviewsCount: number;
  createdAt: string;
  updatedAt: string | null;
  category: { id: number; name: string };
  mainImageUrl: string | null;
  images: { id: number; imageUrl: string; isMain: boolean }[];
  variants: ProductVariantSummary[];
};

type ProductVariantSummary = {
  id: number;
  color: string | null;
  size: string | null;
  material: string | null;
  sku: string;
  price: number;
  stockQuantity: number;
};
```

### Cart Item DTO

Use this for cart page and for AI room design product tray:

```ts
type CartItemResponse = {
  id: number;
  cartId: number;
  productId: number;
  productVariantId: number | null;
  quantity: number;
  createdAt: string;
  product: {
    id: number;
    name: string;
    price: number;
    discountPrice: number | null;
    mainImageUrl: string | null;
    isActive: boolean;
  };
  variant: ProductVariantSummary | null;
  unitPrice: number;
  totalPrice: number;
};
```

### AI Room DTOs

```ts
type AiRoomProductResponse = {
  productId: number;
  name: string;
  description: string;
  material: string | null;
  color: string | null;
  height: number | null;
  width: number | null;
  depth: number | null;
  quantity: number;
  imageUrl: string | null;
};

type UploadAndCreateDesignResponse = {
  roomUploadId: number;
  roomDesignId: number;
  imageUrl: string;
  cartProducts: AiRoomProductResponse[];
};

type PlacementResponse = {
  id: number;
  roomDesignId: number;
  productId: number;
  positionX: number;
  positionY: number;
  rotation: number;
  scale: number;
  zIndex: number;
};

type GenerateRealisticDesignResponse = {
  generatedRoomImageId: number;
  generatedImageUrl: string;
  aiAnalysisJson: string;
};
```

## Frontend Page Flows

### Login Page

Call:

```ts
const login = await apiJson<AuthResponse>("/api/auth/login", {
  method: "POST",
  body: JSON.stringify({ email, password }),
});

localStorage.setItem("accessToken", login.token);
localStorage.setItem("refreshToken", login.refreshToken);
```

Redirect after login based on `login.role`.

### Products Page

Call:

```ts
const products = await apiJson<ProductResponse[]>("/api/products");
```

Recommended card fields:

```ts
product.name
product.mainImageUrl
product.discountPrice ?? product.price
product.averageRating
product.reviewsCount
product.isFeatured
```

### Product Details Page

Call:

```ts
const product = await apiJson<ProductResponse>(`/api/products/${productId}`);
```

Use `product.images` for gallery and `product.variants` for color/size choices.

### Cart Page

Call:

```ts
const carts = await apiJson<CartResponse[]>("/api/cart");
const myCart = carts.find((cart) => cart.userId === loginUserId);
const cartItems = await apiJson<CartItemResponse[]>("/api/cart-items");
```

Cart DTO:

```ts
type CartResponse = {
  id: number;
  userId: number;
  createdAt: string;
  updatedAt: string | null;
  user: {
    id: number;
    fullName: string;
    email: string;
  };
  items: {
    id: number;
    productId: number;
    productVariantId: number | null;
    productName: string;
    productImageUrl: string | null;
    variantSku: string | null;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
  }[];
  itemsCount: number;
  totalPrice: number;
};
```

Create cart item:

```ts
await apiJson<CartItemResponse>("/api/cart-items", {
  method: "POST",
  body: JSON.stringify({
    cartId: myCart.id,
    productId,
    productVariantId: selectedVariantId ?? null,
    quantity,
  }),
});
```

Update cart item quantity:

```ts
await apiJson<CartItemResponse>(`/api/cart-items/${cartItemId}`, {
  method: "PUT",
  body: JSON.stringify({
    cartId,
    productId,
    productVariantId,
    quantity,
  }),
});
```

Delete cart item:

```ts
await fetch(`${API_BASE_URL}/api/cart-items/${cartItemId}`, {
  method: "DELETE",
  headers: { Authorization: `Bearer ${localStorage.getItem("accessToken")}` },
});
```

### AI Room Design Page

The user must have at least one product in cart before this page can create a room design.

Step 1, fetch cart items for the furniture tray:

```ts
const cartItems = await apiJson<CartItemResponse[]>("/api/cart-items");
```

Step 2, upload room image:

```ts
const formData = new FormData();
formData.append("RoomImage", file);
formData.append("RoomType", "living_room");
formData.append("Height", "3");
formData.append("Width", "5");
formData.append("Depth", "4");

const design = await apiForm<UploadAndCreateDesignResponse>(
  "/AiRoom/UploadAndCreateDesign",
  formData
);
```

Step 3, render uploaded room:

```tsx
<img src={design.imageUrl} alt="Uploaded room" />
```

Step 4, render cart products as draggable furniture:

```ts
design.cartProducts.forEach((product) => {
  product.productId;
  product.name;
  product.imageUrl;
});
```

Step 5, save a furniture placement after drag/drop:

```ts
const placement = await apiJson<PlacementResponse>("/AiRoom/SavePlacement", {
  method: "POST",
  body: JSON.stringify({
    roomDesignId: design.roomDesignId,
    productId,
    positionX,
    positionY,
    rotation,
    scale,
    zIndex,
  }),
});
```

Step 6, update placement after moving/resizing:

```ts
await apiJson<PlacementResponse>("/AiRoom/UpdatePlacement", {
  method: "POST",
  body: JSON.stringify({
    placementId: placement.id,
    positionX,
    positionY,
    rotation,
    scale,
    zIndex,
  }),
});
```

Step 7, switch one cart product to another cart product:

```ts
await apiJson<PlacementResponse>("/AiRoom/SwitchProduct", {
  method: "POST",
  body: JSON.stringify({
    roomDesignId: design.roomDesignId,
    oldProductId,
    newProductId,
  }),
});
```

Step 8, generate final image:

```ts
const generated = await apiJson<GenerateRealisticDesignResponse>(
  "/AiRoom/GenerateRealisticDesign",
  {
    method: "POST",
    body: JSON.stringify({ roomDesignId: design.roomDesignId }),
  }
);
```

Step 9, render generated image:

```tsx
<img src={generated.generatedImageUrl} alt="Generated room design" />
```

If `JSON.parse(generated.aiAnalysisJson).mode === "mock"`, OpenAI quota is unavailable and the backend used fallback mode.

### Chatbot Widget

Call public config:

```ts
const config = await apiJson<{
  propertyId: string;
  widgetId: string;
  embedUrl: string;
  isConfigured: boolean;
}>(
  "/api/chatbot/config"
);
```

If `isConfigured` is false, hide the Tawk widget or show a local support button.

To load the widget on a frontend page, include:

```html
<script src="/tawk-widget.js"></script>
```

The widget automatically reads `accessToken` from `localStorage`, calls `/api/chatbot/context`, and sends ecommerce context to Tawk visitor attributes.

Call authenticated context when a user is logged in:

```ts
const context = await apiJson<ChatbotContextResponse>("/api/chatbot/context");
```

Context DTO:

```ts
type ChatbotContextResponse = {
  user: {
    id: number;
    fullName: string;
    email: string;
    role: string;
  };
  cartItems: {
    productId: number;
    productName: string;
    quantity: number;
    price: number;
    variantSku: string | null;
  }[];
  recentOrders: {
    orderId: number;
    totalPrice: number;
    status: string;
    paymentStatus: string;
    createdAt: string;
  }[];
  roomDesigns: {
    roomDesignId: number;
    roomUploadId: number;
    name: string;
    roomType: string | null;
    imageUrl: string;
    createdAt: string;
  }[];
};
```

## Auth

### Register

```http
POST /api/auth/register
Content-Type: application/json
```

```json
{
  "firstName": "Karam",
  "lastName": "Customer",
  "email": "customer@example.com",
  "password": "Password123",
  "phone": "+962790000000",
  "profileImage": null,
  "role": "customer"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "customer@dreamroom.test",
  "password": "Password123"
}
```

### Refresh Token

```http
POST /api/auth/refresh
Content-Type: application/json
```

```json
{
  "refreshToken": "REFRESH_TOKEN"
}
```

### Revoke Refresh Token

```http
POST /api/auth/revoke
Content-Type: application/json
```

```json
{
  "refreshToken": "REFRESH_TOKEN"
}
```

### Logout

```http
POST /api/auth/logout
Authorization: Bearer ACCESS_TOKEN
Content-Type: application/json
```

```json
{
  "refreshToken": "REFRESH_TOKEN"
}
```

## Core CRUD Endpoints

Each resource supports:

```http
GET /api/{resource}
GET /api/{resource}/{id}
POST /api/{resource}
PUT /api/{resource}/{id}
DELETE /api/{resource}/{id}
```

Resources:

```text
users
addresses
ai-chats
ai-messages
categories
products
product-images
product-variants
cart
cart-items
favorites
orders
order-items
payments
reviews
room-uploads
room-designs
room-furniture-placements
```

## AI Room Design

The user must have furniture products in cart before starting AI Room Design.

### Upload Room And Create Design

```http
POST /AiRoom/UploadAndCreateDesign
Authorization: Bearer ACCESS_TOKEN
Content-Type: multipart/form-data
```

Form fields:

```text
roomImage: file
roomType: living_room
height: 3
width: 5
depth: 4
```

Returns:

```json
{
  "roomUploadId": 1,
  "roomDesignId": 1,
  "imageUrl": "https://res.cloudinary.com/...",
  "cartProducts": []
}
```

### Save Placement

Creates or updates placement for the same `roomDesignId` and `productId`.

```http
POST /AiRoom/SavePlacement
Authorization: Bearer ACCESS_TOKEN
Content-Type: application/json
```

```json
{
  "roomDesignId": 1,
  "productId": 1,
  "positionX": 120,
  "positionY": 300,
  "rotation": 0,
  "scale": 1.2,
  "zIndex": 2
}
```

### Update Placement

```http
POST /AiRoom/UpdatePlacement
Authorization: Bearer ACCESS_TOKEN
Content-Type: application/json
```

```json
{
  "placementId": 1,
  "positionX": 140,
  "positionY": 320,
  "rotation": 10,
  "scale": 1.1,
  "zIndex": 3
}
```

### Switch Product

The new product must already be in the user's cart.

```http
POST /AiRoom/SwitchProduct
Authorization: Bearer ACCESS_TOKEN
Content-Type: application/json
```

```json
{
  "roomDesignId": 1,
  "oldProductId": 1,
  "newProductId": 2
}
```

### Generate Realistic AI Design

```http
POST /AiRoom/GenerateRealisticDesign
Authorization: Bearer ACCESS_TOKEN
Content-Type: application/json
```

```json
{
  "roomDesignId": 1
}
```

Returns:

```json
{
  "generatedRoomImageId": 1,
  "generatedImageUrl": "https://res.cloudinary.com/...",
  "aiAnalysisJson": "{}"
}
```

## Chatbot

### Get Tawk Config

```http
GET /api/chatbot/config
```

Returns:

```json
{
  "propertyId": "YOUR_TAWK_PROPERTY_ID",
  "widgetId": "YOUR_TAWK_WIDGET_ID",
  "embedUrl": "https://embed.tawk.to/YOUR_TAWK_PROPERTY_ID/YOUR_TAWK_WIDGET_ID",
  "isConfigured": true
}
```

### Get Current User Context

```http
GET /api/chatbot/context
Authorization: Bearer ACCESS_TOKEN
```

Returns safe user context:

```json
{
  "user": {},
  "cartItems": [],
  "recentOrders": [],
  "roomDesigns": []
}
```

## Static Test Pages

```text
/ai-room-design.html
/tawk-widget.js
```

## Verified Local AI Test Flow

Tested on `http://localhost:5039` with seeded user:

```json
{
  "email": "customer@dreamroom.test",
  "password": "Password123"
}
```

### Dependency APIs Checked

```http
GET /api/products
GET /api/product-images
GET /api/product-variants
GET /api/cart
GET /api/cart-items
```

Confirmed cart has AI-eligible products:

```json
[
  { "productId": 1, "name": "Modern Beige Sofa", "quantity": 1 },
  { "productId": 2, "name": "Walnut Coffee Table", "quantity": 1 },
  { "productId": 4, "name": "Arc Floor Lamp", "quantity": 2 }
]
```

### Upload Room And Create Design

```http
POST /AiRoom/UploadAndCreateDesign
Authorization: Bearer ACCESS_TOKEN
Content-Type: multipart/form-data
```

Form fields:

```text
RoomImage: room-test.png
RoomType: living_room
Height: 3
Width: 5
Depth: 4
```

Response:

```json
{
  "roomUploadId": 2,
  "roomDesignId": 1,
  "imageUrl": "https://res.cloudinary.com/duruqfguc/image/upload/v1781208292/furniture/rooms/vjqaiotdodv0g6s3jtut.png",
  "cartProducts": [
    { "productId": 1, "name": "Modern Beige Sofa", "quantity": 1 },
    { "productId": 2, "name": "Walnut Coffee Table", "quantity": 1 },
    { "productId": 4, "name": "Arc Floor Lamp", "quantity": 2 }
  ]
}
```

### Save Placement

Request:

```json
{
  "roomDesignId": 1,
  "productId": 1,
  "positionX": 120,
  "positionY": 300,
  "rotation": 0,
  "scale": 1.2,
  "zIndex": 2
}
```

Response:

```json
{
  "id": 1,
  "roomDesignId": 1,
  "productId": 1,
  "positionX": 120,
  "positionY": 300,
  "rotation": 0,
  "scale": 1.2,
  "zIndex": 2
}
```

### Switch Product

Request:

```json
{
  "roomDesignId": 1,
  "oldProductId": 1,
  "newProductId": 2
}
```

Response:

```json
{
  "id": 1,
  "roomDesignId": 1,
  "productId": 2,
  "positionX": 120,
  "positionY": 300,
  "rotation": 0,
  "scale": 1.2,
  "zIndex": 2
}
```

### Update Placement

Request:

```json
{
  "placementId": 1,
  "positionX": 140,
  "positionY": 320,
  "rotation": 10,
  "scale": 1.1,
  "zIndex": 3
}
```

Response:

```json
{
  "id": 1,
  "roomDesignId": 1,
  "productId": 2,
  "positionX": 140,
  "positionY": 320,
  "rotation": 10,
  "scale": 1.1,
  "zIndex": 3
}
```

### Generate Realistic Design

Request:

```json
{
  "roomDesignId": 1
}
```

Response:

```json
{
  "generatedRoomImageId": 1,
  "generatedImageUrl": "https://res.cloudinary.com/duruqfguc/image/upload/v1781208489/furniture/generated/rv7yjnpkcii7bjynpirn.png",
  "aiAnalysisJson": "{\"roomType\":\"living_room\",\"roomLayout\":\"Estimated from uploaded image and user placements.\",\"wallColor\":\"approximate\",\"floorType\":\"approximate\",\"lighting\":\"approximate\",\"approximateWidth\":5.00,\"approximateHeight\":3.00,\"approximateDepth\":4.00,\"mode\":\"mock\"}"
}
```

OpenAI returned `429 insufficient_quota` during testing, so the API used the local fallback path and still uploaded a generated image record to Cloudinary. With a billed OpenAI key, the same endpoint will use real OpenAI analysis/image generation.

## Verified Real Image Test Flow

Product image URLs were updated to real public images:

```json
[
  {
    "productId": 1,
    "name": "Modern Beige Sofa",
    "imageUrl": "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=1000&q=80"
  },
  {
    "productId": 2,
    "name": "Walnut Coffee Table",
    "imageUrl": "https://images.unsplash.com/photo-1532372320572-cda25653a694?auto=format&fit=crop&w=1000&q=80"
  },
  {
    "productId": 4,
    "name": "Arc Floor Lamp",
    "imageUrl": "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?auto=format&fit=crop&w=1000&q=80"
  }
]
```

Real room upload response:

```json
{
  "roomUploadId": 3,
  "roomDesignId": 2,
  "imageUrl": "https://res.cloudinary.com/duruqfguc/image/upload/v1781209253/furniture/rooms/ohns5npq2ctpu1bxjsqi.jpg",
  "cartProducts": [
    {
      "productId": 1,
      "name": "Modern Beige Sofa",
      "imageUrl": "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=1000&q=80"
    },
    {
      "productId": 2,
      "name": "Walnut Coffee Table",
      "imageUrl": "https://images.unsplash.com/photo-1532372320572-cda25653a694?auto=format&fit=crop&w=1000&q=80"
    },
    {
      "productId": 4,
      "name": "Arc Floor Lamp",
      "imageUrl": "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?auto=format&fit=crop&w=1000&q=80"
    }
  ]
}
```

Real image generation response:

```json
{
  "generatedRoomImageId": 2,
  "generatedImageUrl": "https://res.cloudinary.com/duruqfguc/image/upload/v1781209256/furniture/generated/r7u24xgn5iwjetithbul.jpg",
  "aiAnalysisJson": "{\"roomType\":\"living_room\",\"roomLayout\":\"Estimated from uploaded image and user placements.\",\"wallColor\":\"approximate\",\"floorType\":\"approximate\",\"lighting\":\"approximate\",\"approximateWidth\":5.00,\"approximateHeight\":3.00,\"approximateDepth\":4.00,\"mode\":\"mock\"}"
}
```

This generated image is a real-size JPG because the uploaded room image was real. It is still marked `mode: mock` because OpenAI quota is unavailable.

## Verified Chatbot Test

```http
GET /api/chatbot/config
```

Response:

```json
{
  "propertyId": "",
  "widgetId": "",
  "embedUrl": "",
  "isConfigured": false
}
```

Tawk config is empty until `Tawk:PropertyId` and `Tawk:WidgetId` are configured.

```http
GET /api/chatbot/context
Authorization: Bearer ACCESS_TOKEN
```

Response:

```json
{
  "user": {
    "id": 1,
    "fullName": "Karam Customer",
    "email": "customer@dreamroom.test",
    "role": "customer"
  },
  "cartItems": [
    { "productId": 1, "productName": "Modern Beige Sofa", "quantity": 1, "price": 590.00, "variantSku": "SOFA-BEIGE-3S" },
    { "productId": 2, "productName": "Walnut Coffee Table", "quantity": 1, "price": 180.00, "variantSku": null },
    { "productId": 4, "productName": "Arc Floor Lamp", "quantity": 2, "price": 95.00, "variantSku": "LAMP-BLACK-STD" }
  ],
  "recentOrders": [
    { "orderId": 1, "totalPrice": 685.00, "status": "processing", "paymentStatus": "paid" }
  ],
  "roomDesigns": [
    { "roomDesignId": 1, "roomUploadId": 2, "name": "living_room Design", "roomType": "living_room" }
  ]
}
```
