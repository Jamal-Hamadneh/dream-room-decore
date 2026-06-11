# Dream Room Decore API Docs

Base URL for local development:

```text
http://localhost:5039
```

Most endpoints require JWT:

```http
Authorization: Bearer YOUR_ACCESS_TOKEN
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

### Get Chatwoot Config

```http
GET /api/chatbot/config
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
/chatwoot-widget.js
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
  "baseUrl": "",
  "websiteToken": ""
}
```

Chatwoot config is empty until `Chatwoot:BaseUrl` and `Chatwoot:WebsiteToken` are configured.

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
