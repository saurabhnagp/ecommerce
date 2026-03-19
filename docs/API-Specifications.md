# AmCart API Specifications

## API Documentation v1.0

---

## Table of Contents

1. [Overview](#1-overview)
2. [Authentication](#2-authentication)
3. [Common Headers](#3-common-headers)
4. [Error Handling](#4-error-handling)
5. [User Service API](#5-user-service-api)
6. [Product Service API](#6-product-service-api)
7. [Cart Service API](#7-cart-service-api)
8. [Order Service API](#8-order-service-api)
9. [Payment Service API](#9-payment-service-api)
10. [Search Service API](#10-search-service-api)
11. [Review Service API](#11-review-service-api)
12. [Notification Service API](#12-notification-service-api)

---

## 1. Overview

### 1.1 Base URL

| Environment | Base URL |
|-------------|----------|
| Development | `http://localhost:8000/api/v1` |
| Staging | `https://api-staging.amcart.com/api/v1` |
| Production | `https://api.amcart.com/api/v1` |

### 1.2 API Versioning

All APIs are versioned using URL path versioning: `/api/v1/`, `/api/v2/`

### 1.3 Content Type

```
Content-Type: application/json
Accept: application/json
```

### 1.4 Rate Limiting

| Endpoint Type | Rate Limit | Window |
|---------------|------------|--------|
| Authentication | 10 requests | 1 minute |
| Public APIs | 100 requests | 1 minute |
| Authenticated APIs | 200 requests | 1 minute |
| Admin APIs | 500 requests | 1 minute |

Rate limit headers in response:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1703001600
```

---

## 2. Authentication

### 2.1 JWT Token Format

```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "role": "customer",
  "iat": 1703001600,
  "exp": 1703005200
}
```

### 2.2 Token Lifetime

| Token Type | Lifetime |
|------------|----------|
| Access Token | 15 minutes |
| Refresh Token | 7 days |

### 2.3 Authentication Header

```
Authorization: Bearer <access_token>
```

---

## 3. Common Headers

### 3.1 Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | Conditional | JWT Bearer token for authenticated endpoints |
| `Content-Type` | Yes | `application/json` |
| `Accept` | Yes | `application/json` |
| `X-Request-ID` | No | Unique request identifier for tracing |
| `X-Device-ID` | No | Client device identifier |
| `Accept-Language` | No | Preferred language (default: `en`) |

### 3.2 Response Headers

| Header | Description |
|--------|-------------|
| `X-Request-ID` | Request identifier (echoed or generated) |
| `X-Response-Time` | Processing time in milliseconds |
| `X-RateLimit-*` | Rate limiting information |

---

## 4. Error Handling

### 4.1 Error Response Format

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "email",
        "message": "Invalid email format"
      }
    ],
    "traceId": "abc123-def456"
  }
}
```

### 4.2 Error Codes

| HTTP Status | Error Code | Description |
|-------------|------------|-------------|
| 400 | `BAD_REQUEST` | Invalid request format |
| 400 | `VALIDATION_ERROR` | Request validation failed |
| 401 | `UNAUTHORIZED` | Missing or invalid authentication |
| 401 | `TOKEN_EXPIRED` | JWT token has expired |
| 403 | `FORBIDDEN` | Insufficient permissions |
| 404 | `NOT_FOUND` | Resource not found |
| 409 | `CONFLICT` | Resource already exists |
| 422 | `UNPROCESSABLE_ENTITY` | Business logic error |
| 429 | `RATE_LIMITED` | Too many requests |
| 500 | `INTERNAL_ERROR` | Server error |
| 503 | `SERVICE_UNAVAILABLE` | Service temporarily unavailable |

---

## 5. User Service API

**Base Path**: `/api/v1`

### 5.1 Authentication Endpoints

#### POST /auth/register

Register a new user account.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecureP@ss123",
  "name": "John Doe",
  "phone": "+91-9876543210",
  "gender": "male"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "name": "John Doe",
      "phone": "+91-9876543210",
      "gender": "male",
      "role": "customer",
      "isVerified": false,
      "createdAt": "2024-12-19T10:30:00Z"
    },
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
      "expiresIn": 900
    }
  },
  "message": "Registration successful. Please verify your email."
}
```

**Validation Rules:**
| Field | Rules |
|-------|-------|
| email | Required, valid email format, unique |
| password | Required, min 8 chars, 1 uppercase, 1 number, 1 special |
| name | Required, min 2 chars, max 100 chars |
| phone | Optional, valid phone format |
| gender | Optional, enum: male, female, other |

---

#### POST /auth/login

Authenticate user and get tokens.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecureP@ss123",
  "rememberMe": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "name": "John Doe",
      "role": "customer",
      "avatarUrl": "https://cdn.amcart.com/avatars/user123.jpg"
    },
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
      "expiresIn": 900
    }
  }
}
```

**Error Response (401):**
```json
{
  "success": false,
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Invalid email or password"
  }
}
```

---

#### POST /auth/refresh

Refresh access token using refresh token.

**Request:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 900
  }
}
```

---

#### POST /auth/forgot-password

Request password reset email.

**Request:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "If the email exists, a password reset link has been sent."
}
```

---

#### POST /auth/reset-password

Reset password with token.

**Request:**
```json
{
  "token": "reset-token-from-email",
  "password": "NewSecureP@ss456",
  "confirmPassword": "NewSecureP@ss456"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Password has been reset successfully."
}
```

---

#### POST /auth/logout

Logout and invalidate refresh token.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Logged out successfully."
}
```

---

### 5.2 User Profile Endpoints

#### GET /users/me

Get current user profile.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "name": "John Doe",
    "phone": "+91-9876543210",
    "gender": "male",
    "avatarUrl": "https://cdn.amcart.com/avatars/user123.jpg",
    "role": "customer",
    "isVerified": true,
    "createdAt": "2024-12-19T10:30:00Z",
    "updatedAt": "2024-12-19T14:00:00Z"
  }
}
```

---

#### PUT /users/me

Update current user profile.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "name": "John Smith",
  "phone": "+91-9876543211",
  "gender": "male"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "name": "John Smith",
    "phone": "+91-9876543211",
    "gender": "male",
    "updatedAt": "2024-12-19T15:00:00Z"
  },
  "message": "Profile updated successfully."
}
```

---

#### PUT /users/me/password

Change password.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "currentPassword": "SecureP@ss123",
  "newPassword": "NewSecureP@ss456",
  "confirmPassword": "NewSecureP@ss456"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Password changed successfully."
}
```

---

### 5.3 Address Endpoints

#### GET /users/me/addresses

Get all addresses for current user.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "addr-uuid-1",
      "type": "shipping",
      "isDefault": true,
      "fullName": "John Doe",
      "phone": "+91-9876543210",
      "addressLine1": "123 Main Street",
      "addressLine2": "Apt 4B",
      "city": "Mumbai",
      "state": "Maharashtra",
      "postalCode": "400001",
      "country": "India",
      "createdAt": "2024-12-19T10:30:00Z"
    }
  ]
}
```

---

#### POST /users/me/addresses

Add a new address.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "type": "shipping",
  "isDefault": false,
  "fullName": "John Doe",
  "phone": "+91-9876543210",
  "addressLine1": "456 Park Avenue",
  "addressLine2": "",
  "city": "Delhi",
  "state": "Delhi",
  "postalCode": "110001",
  "country": "India"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "addr-uuid-2",
    "type": "shipping",
    "isDefault": false,
    "fullName": "John Doe",
    "phone": "+91-9876543210",
    "addressLine1": "456 Park Avenue",
    "city": "Delhi",
    "state": "Delhi",
    "postalCode": "110001",
    "country": "India",
    "createdAt": "2024-12-19T16:00:00Z"
  },
  "message": "Address added successfully."
}
```

---

#### PUT /users/me/addresses/{addressId}

Update an address.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "addressLine1": "789 New Street",
  "isDefault": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "addr-uuid-2",
    "addressLine1": "789 New Street",
    "isDefault": true,
    "updatedAt": "2024-12-19T17:00:00Z"
  },
  "message": "Address updated successfully."
}
```

---

#### DELETE /users/me/addresses/{addressId}

Delete an address.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Address deleted successfully."
}
```

---

## 6. Product Service API

**Base Path**: `/api/v1`

### 6.1 Product Endpoints

#### GET /products

Get products with filtering, sorting, and pagination.

**Query Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page (max 100) |
| `sort` | string | `createdAt:desc` | Sort field and order |
| `category` | string | - | Category slug |
| `brand` | string | - | Brand slug |
| `gender` | string | - | `men`, `women` |
| `minPrice` | decimal | - | Minimum price |
| `maxPrice` | decimal | - | Maximum price |
| `colors` | string | - | Comma-separated colors |
| `sizes` | string | - | Comma-separated sizes |
| `inStock` | bool | - | Filter in-stock items |
| `isOnSale` | bool | - | Filter sale items |
| `isNew` | bool | - | Filter new arrivals |

**Example:** `GET /products?category=t-shirts&gender=men&minPrice=500&maxPrice=2000&sort=price:asc&page=1&pageSize=20`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "prod-uuid-1",
        "sku": "TSH-BLU-001",
        "name": "Classic Blue T-Shirt",
        "slug": "classic-blue-t-shirt",
        "description": "Comfortable cotton t-shirt...",
        "basePrice": 999.00,
        "salePrice": 799.00,
        "isOnSale": true,
        "isNew": false,
        "isFeatured": true,
        "category": {
          "id": "cat-uuid",
          "name": "T-Shirts",
          "slug": "t-shirts"
        },
        "brand": {
          "id": "brand-uuid",
          "name": "AmCart Basics",
          "slug": "amcart-basics"
        },
        "images": [
          {
            "url": "https://cdn.amcart.com/products/tsh-001-1.jpg",
            "alt": "Front view",
            "isPrimary": true
          }
        ],
        "colors": ["Blue", "Black", "White"],
        "sizes": ["S", "M", "L", "XL"],
        "rating": 4.5,
        "reviewCount": 128,
        "inStock": true
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalItems": 150,
      "totalPages": 8,
      "hasNext": true,
      "hasPrevious": false
    },
    "filters": {
      "categories": [
        { "slug": "t-shirts", "name": "T-Shirts", "count": 45 }
      ],
      "brands": [
        { "slug": "amcart-basics", "name": "AmCart Basics", "count": 30 }
      ],
      "colors": [
        { "name": "Blue", "count": 25 },
        { "name": "Black", "count": 40 }
      ],
      "sizes": [
        { "name": "M", "count": 120 },
        { "name": "L", "count": 110 }
      ],
      "priceRange": {
        "min": 299,
        "max": 4999
      }
    }
  }
}
```

---

#### GET /products/{slug}

Get single product by slug.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "prod-uuid-1",
    "sku": "TSH-BLU-001",
    "name": "Classic Blue T-Shirt",
    "slug": "classic-blue-t-shirt",
    "description": "Comfortable cotton t-shirt perfect for everyday wear. Made from 100% organic cotton with a relaxed fit.",
    "basePrice": 999.00,
    "salePrice": 799.00,
    "isOnSale": true,
    "discountPercentage": 20,
    "isNew": false,
    "isFeatured": true,
    "category": {
      "id": "cat-uuid",
      "name": "T-Shirts",
      "slug": "t-shirts",
      "path": ["Men", "Clothing", "T-Shirts"]
    },
    "brand": {
      "id": "brand-uuid",
      "name": "AmCart Basics",
      "slug": "amcart-basics",
      "logo": "https://cdn.amcart.com/brands/amcart-basics.png"
    },
    "images": [
      {
        "id": "img-1",
        "url": "https://cdn.amcart.com/products/tsh-001-1.jpg",
        "thumbnailUrl": "https://cdn.amcart.com/products/tsh-001-1-thumb.jpg",
        "alt": "Front view",
        "isPrimary": true
      },
      {
        "id": "img-2",
        "url": "https://cdn.amcart.com/products/tsh-001-2.jpg",
        "thumbnailUrl": "https://cdn.amcart.com/products/tsh-001-2-thumb.jpg",
        "alt": "Back view",
        "isPrimary": false
      }
    ],
    "variants": [
      {
        "id": "var-uuid-1",
        "sku": "TSH-BLU-001-M-BLU",
        "color": "Blue",
        "size": "M",
        "priceModifier": 0,
        "stockQuantity": 25,
        "inStock": true
      },
      {
        "id": "var-uuid-2",
        "sku": "TSH-BLU-001-L-BLU",
        "color": "Blue",
        "size": "L",
        "priceModifier": 0,
        "stockQuantity": 0,
        "inStock": false
      }
    ],
    "attributes": {
      "material": "100% Organic Cotton",
      "fit": "Regular Fit",
      "care": "Machine wash cold",
      "origin": "Made in India"
    },
    "rating": 4.5,
    "reviewCount": 128,
    "createdAt": "2024-11-01T10:00:00Z",
    "updatedAt": "2024-12-19T10:00:00Z"
  }
}
```

---

#### GET /products/new

Get new arrivals.

**Query Parameters:** `page`, `pageSize`

**Response:** Same as GET /products

---

#### GET /products/featured

Get featured products.

**Query Parameters:** `page`, `pageSize`

**Response:** Same as GET /products

---

#### GET /products/sale

Get products on sale.

**Query Parameters:** `page`, `pageSize`

**Response:** Same as GET /products

---

### 6.2 Category Endpoints

#### GET /categories

Get category tree.

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "cat-men",
      "name": "Men",
      "slug": "men",
      "image": "https://cdn.amcart.com/categories/men.jpg",
      "children": [
        {
          "id": "cat-men-clothing",
          "name": "Clothing",
          "slug": "men-clothing",
          "children": [
            {
              "id": "cat-men-tshirts",
              "name": "T-Shirts",
              "slug": "men-t-shirts",
              "productCount": 150
            },
            {
              "id": "cat-men-shirts",
              "name": "Shirts",
              "slug": "men-shirts",
              "productCount": 200
            }
          ]
        },
        {
          "id": "cat-men-accessories",
          "name": "Accessories",
          "slug": "men-accessories",
          "children": [
            {
              "id": "cat-men-watches",
              "name": "Watches",
              "slug": "men-watches",
              "productCount": 75
            }
          ]
        }
      ]
    },
    {
      "id": "cat-women",
      "name": "Women",
      "slug": "women",
      "image": "https://cdn.amcart.com/categories/women.jpg",
      "children": []
    }
  ]
}
```

---

#### GET /categories/{slug}/products

Get products by category.

**Query Parameters:** Same as GET /products

**Response:** Same as GET /products

---

### 6.3 Brand Endpoints

#### GET /brands

Get all brands.

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "brand-uuid-1",
      "name": "AmCart Basics",
      "slug": "amcart-basics",
      "logo": "https://cdn.amcart.com/brands/amcart-basics.png",
      "productCount": 250
    },
    {
      "id": "brand-uuid-2",
      "name": "Premium Collection",
      "slug": "premium-collection",
      "logo": "https://cdn.amcart.com/brands/premium.png",
      "productCount": 100
    }
  ]
}
```

---

## 7. Cart Service API

**Base Path**: `/api/v1`

### 7.1 Cart Endpoints

#### GET /cart

Get current user's cart.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "cart-uuid",
    "userId": "user-uuid",
    "items": [
      {
        "id": "item-uuid-1",
        "productId": "prod-uuid-1",
        "variantId": "var-uuid-1",
        "product": {
          "name": "Classic Blue T-Shirt",
          "slug": "classic-blue-t-shirt",
          "image": "https://cdn.amcart.com/products/tsh-001-1.jpg"
        },
        "variant": {
          "color": "Blue",
          "size": "M",
          "sku": "TSH-BLU-001-M-BLU"
        },
        "quantity": 2,
        "unitPrice": 799.00,
        "subtotal": 1598.00,
        "inStock": true,
        "stockQuantity": 25
      }
    ],
    "summary": {
      "itemCount": 2,
      "subtotal": 1598.00,
      "discount": 0,
      "couponCode": null,
      "couponDiscount": 0,
      "shipping": 0,
      "tax": 0,
      "total": 1598.00
    },
    "updatedAt": "2024-12-19T10:00:00Z"
  }
}
```

---

#### POST /cart/items

Add item to cart.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "productId": "prod-uuid-1",
  "variantId": "var-uuid-1",
  "quantity": 1
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "item": {
      "id": "item-uuid-2",
      "productId": "prod-uuid-1",
      "variantId": "var-uuid-1",
      "quantity": 1,
      "unitPrice": 799.00,
      "subtotal": 799.00
    },
    "cart": {
      "itemCount": 3,
      "total": 2397.00
    }
  },
  "message": "Item added to cart."
}
```

**Error Response (400):**
```json
{
  "success": false,
  "error": {
    "code": "INSUFFICIENT_STOCK",
    "message": "Only 5 items available in stock."
  }
}
```

---

#### PUT /cart/items/{itemId}

Update cart item quantity.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "quantity": 3
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "item": {
      "id": "item-uuid-1",
      "quantity": 3,
      "subtotal": 2397.00
    },
    "cart": {
      "itemCount": 4,
      "total": 3196.00
    }
  },
  "message": "Cart updated."
}
```

---

#### DELETE /cart/items/{itemId}

Remove item from cart.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "cart": {
      "itemCount": 1,
      "total": 799.00
    }
  },
  "message": "Item removed from cart."
}
```

---

#### DELETE /cart

Clear entire cart.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Cart cleared."
}
```

---

#### POST /cart/coupon

Apply coupon code.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "couponCode": "SAVE20"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "coupon": {
      "code": "SAVE20",
      "type": "percentage",
      "value": 20,
      "description": "20% off on all items"
    },
    "summary": {
      "subtotal": 1598.00,
      "couponDiscount": 319.60,
      "total": 1278.40
    }
  },
  "message": "Coupon applied successfully."
}
```

**Error Response (400):**
```json
{
  "success": false,
  "error": {
    "code": "INVALID_COUPON",
    "message": "This coupon code is invalid or expired."
  }
}
```

---

#### DELETE /cart/coupon

Remove applied coupon.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Coupon removed."
}
```

---

## 8. Order Service API

**Base Path**: `/api/v1`

### 8.1 Order Endpoints

#### POST /orders

Create order from cart.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "shippingAddressId": "addr-uuid-1",
  "billingAddressId": "addr-uuid-1",
  "notes": "Please leave at the door",
  "paymentMethod": "razorpay"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "order": {
      "id": "order-uuid",
      "orderNumber": "ORD-2024-001234",
      "status": "pending_payment",
      "items": [
        {
          "productId": "prod-uuid-1",
          "productName": "Classic Blue T-Shirt",
          "variantId": "var-uuid-1",
          "color": "Blue",
          "size": "M",
          "quantity": 2,
          "unitPrice": 799.00,
          "subtotal": 1598.00
        }
      ],
      "shippingAddress": {
        "fullName": "John Doe",
        "addressLine1": "123 Main Street",
        "city": "Mumbai",
        "state": "Maharashtra",
        "postalCode": "400001",
        "country": "India",
        "phone": "+91-9876543210"
      },
      "summary": {
        "subtotal": 1598.00,
        "discount": 319.60,
        "couponCode": "SAVE20",
        "shipping": 0,
        "tax": 0,
        "total": 1278.40
      },
      "createdAt": "2024-12-19T10:00:00Z"
    },
    "payment": {
      "razorpayOrderId": "order_ABC123XYZ",
      "amount": 127840,
      "currency": "INR",
      "keyId": "rzp_test_xxxxx"
    }
  },
  "message": "Order created. Please complete payment."
}
```

---

#### GET /orders

Get user's order history.

**Headers:** `Authorization: Bearer <access_token>`

**Query Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page |
| `status` | string | - | Filter by status |

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "order-uuid",
        "orderNumber": "ORD-2024-001234",
        "status": "delivered",
        "itemCount": 2,
        "total": 1278.40,
        "placedAt": "2024-12-15T10:00:00Z",
        "deliveredAt": "2024-12-18T14:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalItems": 15,
      "totalPages": 2
    }
  }
}
```

---

#### GET /orders/{orderId}

Get order details.

**Headers:** `Authorization: Bearer <access_token>`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "order-uuid",
    "orderNumber": "ORD-2024-001234",
    "status": "delivered",
    "statusHistory": [
      {
        "status": "pending_payment",
        "timestamp": "2024-12-15T10:00:00Z"
      },
      {
        "status": "confirmed",
        "timestamp": "2024-12-15T10:05:00Z"
      },
      {
        "status": "processing",
        "timestamp": "2024-12-15T12:00:00Z"
      },
      {
        "status": "shipped",
        "timestamp": "2024-12-16T09:00:00Z",
        "metadata": {
          "trackingNumber": "TRK123456789",
          "carrier": "FedEx"
        }
      },
      {
        "status": "delivered",
        "timestamp": "2024-12-18T14:00:00Z"
      }
    ],
    "items": [
      {
        "productId": "prod-uuid-1",
        "productName": "Classic Blue T-Shirt",
        "productSlug": "classic-blue-t-shirt",
        "productImage": "https://cdn.amcart.com/products/tsh-001-1.jpg",
        "variantId": "var-uuid-1",
        "color": "Blue",
        "size": "M",
        "quantity": 2,
        "unitPrice": 799.00,
        "subtotal": 1598.00
      }
    ],
    "shippingAddress": {
      "fullName": "John Doe",
      "addressLine1": "123 Main Street",
      "city": "Mumbai",
      "state": "Maharashtra",
      "postalCode": "400001",
      "country": "India",
      "phone": "+91-9876543210"
    },
    "billingAddress": {
      "fullName": "John Doe",
      "addressLine1": "123 Main Street",
      "city": "Mumbai",
      "state": "Maharashtra",
      "postalCode": "400001",
      "country": "India"
    },
    "payment": {
      "method": "razorpay",
      "status": "completed",
      "transactionId": "pay_ABC123XYZ",
      "paidAt": "2024-12-15T10:05:00Z"
    },
    "summary": {
      "subtotal": 1598.00,
      "discount": 319.60,
      "couponCode": "SAVE20",
      "shipping": 0,
      "tax": 0,
      "total": 1278.40
    },
    "notes": "Please leave at the door",
    "placedAt": "2024-12-15T10:00:00Z",
    "deliveredAt": "2024-12-18T14:00:00Z"
  }
}
```

---

#### POST /orders/{orderId}/cancel

Cancel an order.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "reason": "Changed my mind"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "orderNumber": "ORD-2024-001234",
    "status": "cancelled",
    "refund": {
      "status": "initiated",
      "amount": 1278.40,
      "estimatedDays": 5
    }
  },
  "message": "Order cancelled. Refund will be processed within 5-7 business days."
}
```

**Error Response (400):**
```json
{
  "success": false,
  "error": {
    "code": "ORDER_NOT_CANCELLABLE",
    "message": "Order cannot be cancelled as it has already been shipped."
  }
}
```

---

## 9. Payment Service API

**Base Path**: `/api/v1`

### 9.1 Payment Endpoints

#### POST /payments/create-order

Create Razorpay order for payment.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "orderId": "order-uuid"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "razorpayOrderId": "order_ABC123XYZ",
    "amount": 127840,
    "currency": "INR",
    "keyId": "rzp_test_xxxxx",
    "prefill": {
      "name": "John Doe",
      "email": "user@example.com",
      "contact": "+91-9876543210"
    },
    "theme": {
      "color": "#3B82F6"
    }
  }
}
```

---

#### POST /payments/verify

Verify payment after Razorpay callback.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "razorpayOrderId": "order_ABC123XYZ",
  "razorpayPaymentId": "pay_ABC123XYZ",
  "razorpaySignature": "signature_string"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "orderId": "order-uuid",
    "orderNumber": "ORD-2024-001234",
    "paymentId": "payment-uuid",
    "status": "confirmed",
    "message": "Payment successful! Your order has been confirmed."
  }
}
```

**Error Response (400):**
```json
{
  "success": false,
  "error": {
    "code": "PAYMENT_VERIFICATION_FAILED",
    "message": "Payment verification failed. Please contact support."
  }
}
```

---

#### POST /payments/webhook

Razorpay webhook handler (Server-to-Server).

**Headers:** `X-Razorpay-Signature: <webhook_signature>`

**Request:** Razorpay webhook payload

**Response (200 OK):**
```json
{
  "status": "ok"
}
```

---

## 10. Search Service API

**Base Path**: `/api/v1`

### 10.1 Search Endpoints

#### GET /search

Full-text product search.

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `q` | string | Search query (required) |
| `page` | int | Page number |
| `pageSize` | int | Items per page |
| `category` | string | Filter by category |
| `brand` | string | Filter by brand |
| `minPrice` | decimal | Minimum price |
| `maxPrice` | decimal | Maximum price |
| `colors` | string | Comma-separated colors |
| `sizes` | string | Comma-separated sizes |
| `sort` | string | Sort option |

**Example:** `GET /search?q=blue+cotton+shirt&category=men-shirts&minPrice=500&maxPrice=2000`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "query": "blue cotton shirt",
    "items": [
      {
        "id": "prod-uuid-1",
        "name": "Classic <em>Blue</em> <em>Cotton</em> <em>Shirt</em>",
        "slug": "classic-blue-cotton-shirt",
        "description": "Premium <em>cotton</em> <em>shirt</em> in <em>blue</em>...",
        "price": 1499.00,
        "salePrice": 1199.00,
        "image": "https://cdn.amcart.com/products/shirt-001.jpg",
        "rating": 4.6,
        "reviewCount": 85,
        "score": 12.5
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalItems": 45,
      "totalPages": 3
    },
    "facets": {
      "categories": [
        { "slug": "men-shirts", "name": "Shirts", "count": 30 },
        { "slug": "men-casual-shirts", "name": "Casual Shirts", "count": 15 }
      ],
      "brands": [
        { "slug": "amcart-basics", "name": "AmCart Basics", "count": 20 }
      ],
      "colors": [
        { "name": "Blue", "count": 45 },
        { "name": "Light Blue", "count": 12 }
      ],
      "sizes": [
        { "name": "M", "count": 40 },
        { "name": "L", "count": 38 }
      ],
      "priceRanges": [
        { "label": "Under ₹500", "min": 0, "max": 500, "count": 5 },
        { "label": "₹500 - ₹1000", "min": 500, "max": 1000, "count": 20 },
        { "label": "₹1000 - ₹2000", "min": 1000, "max": 2000, "count": 15 }
      ]
    },
    "suggestions": [
      "blue cotton formal shirt",
      "blue cotton casual shirt"
    ]
  }
}
```

---

#### GET /search/autocomplete

Search suggestions as user types.

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `q` | string | Partial search query |
| `limit` | int | Max suggestions (default: 10) |

**Example:** `GET /search/autocomplete?q=blu&limit=5`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "suggestions": [
      {
        "text": "blue t-shirt",
        "type": "product",
        "count": 45
      },
      {
        "text": "blue jeans",
        "type": "product",
        "count": 30
      },
      {
        "text": "blue formal shirt",
        "type": "product",
        "count": 25
      },
      {
        "text": "Bluetooth Accessories",
        "type": "category",
        "slug": "bluetooth-accessories"
      }
    ]
  }
}
```

---

## 11. Review Service API

**Base Path**: `/api/v1`

### 11.1 Review Endpoints

#### GET /reviews/product/{productId}

Get reviews for a product.

**Query Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Reviews per page |
| `sort` | string | `createdAt:desc` | Sort option |
| `rating` | int | - | Filter by rating (1-5) |

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "summary": {
      "averageRating": 4.5,
      "totalReviews": 128,
      "ratingDistribution": {
        "5": 70,
        "4": 35,
        "3": 15,
        "2": 5,
        "1": 3
      }
    },
    "items": [
      {
        "id": "review-uuid-1",
        "userId": "user-uuid",
        "userName": "John D.",
        "userAvatar": "https://cdn.amcart.com/avatars/user123.jpg",
        "rating": 5,
        "title": "Excellent quality!",
        "content": "The fabric is really soft and comfortable. Perfect fit!",
        "images": [
          "https://cdn.amcart.com/reviews/rev-001-1.jpg"
        ],
        "isVerifiedPurchase": true,
        "helpful": {
          "up": 15,
          "down": 2
        },
        "createdAt": "2024-12-15T10:00:00Z",
        "replies": [
          {
            "userName": "AmCart Support",
            "content": "Thank you for your kind words!",
            "createdAt": "2024-12-16T09:00:00Z"
          }
        ]
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalItems": 128,
      "totalPages": 13
    }
  }
}
```

---

#### POST /reviews

Create a review.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "productId": "prod-uuid-1",
  "rating": 5,
  "title": "Love this product!",
  "content": "Great quality and fast delivery. Highly recommend!",
  "images": [
    "https://cdn.amcart.com/user-uploads/temp/img1.jpg"
  ]
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "review-uuid-new",
    "productId": "prod-uuid-1",
    "rating": 5,
    "title": "Love this product!",
    "content": "Great quality and fast delivery. Highly recommend!",
    "status": "pending",
    "createdAt": "2024-12-19T10:00:00Z"
  },
  "message": "Review submitted. It will be visible after moderation."
}
```

---

#### POST /reviews/{reviewId}/helpful

Mark review as helpful.

**Headers:** `Authorization: Bearer <access_token>`

**Request:**
```json
{
  "helpful": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "helpful": {
      "up": 16,
      "down": 2
    }
  }
}
```

---

## 12. Notification Service API

**Base Path**: `/api/v1` (Internal use only)

### 12.1 Internal Endpoints

#### POST /notifications/send

Send notification (internal service call).

**Request:**
```json
{
  "userId": "user-uuid",
  "type": "order_shipped",
  "channel": "email",
  "template": "order-shipped",
  "data": {
    "orderNumber": "ORD-2024-001234",
    "trackingNumber": "TRK123456789",
    "carrier": "FedEx",
    "estimatedDelivery": "2024-12-22"
  }
}
```

**Response (202 Accepted):**
```json
{
  "success": true,
  "data": {
    "notificationId": "notif-uuid",
    "status": "queued"
  }
}
```

---

## Appendix A: Status Codes

### Order Status

| Status | Description |
|--------|-------------|
| `pending_payment` | Order created, awaiting payment |
| `confirmed` | Payment received, order confirmed |
| `processing` | Order being prepared |
| `shipped` | Order shipped |
| `out_for_delivery` | Out for delivery |
| `delivered` | Order delivered |
| `cancelled` | Order cancelled |
| `refunded` | Order refunded |

### Payment Status

| Status | Description |
|--------|-------------|
| `pending` | Awaiting payment |
| `completed` | Payment successful |
| `failed` | Payment failed |
| `refunded` | Payment refunded |

---

## Appendix B: Webhook Events

### Order Events

| Event | Description |
|-------|-------------|
| `order.created` | New order created |
| `order.confirmed` | Order confirmed |
| `order.shipped` | Order shipped |
| `order.delivered` | Order delivered |
| `order.cancelled` | Order cancelled |

### Payment Events

| Event | Description |
|-------|-------------|
| `payment.completed` | Payment successful |
| `payment.failed` | Payment failed |
| `payment.refunded` | Refund processed |

---

**END OF API SPECIFICATIONS**

