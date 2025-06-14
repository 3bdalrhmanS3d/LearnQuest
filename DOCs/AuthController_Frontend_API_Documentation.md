# 📄 LearnQuest - AuthController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/Auth`

---

## 🔗 Endpoints Overview

| Action                   | Endpoint                    | Method | Auth Required | Description                                 |
| ------------------------ | --------------------------- | ------ | ------------- | ------------------------------------------- |
| Signup                   | `/signup`                   | POST   | No            | Register new user / send verification code  |
| Verify Account           | `/verify-account`           | POST   | No            | Activate account with verification code     |
| Resend Verification Code | `/resend-verification-code` | POST   | No            | Send a new verification code to email       |
| Signin                   | `/signin`                   | POST   | No            | Authenticate user and issue tokens          |
| Refresh Token            | `/refresh-token`            | POST   | No            | Obtain new access & refresh tokens          |
| Auto Login               | `/auto-login`               | POST   | No            | Long-term login via autoLoginToken cookie   |
| Forget Password          | `/forget-password`          | POST   | No            | Initiate password reset                     |
| Reset Password           | `/reset-password`           | POST   | No            | Complete password reset with verification   |
| Logout                   | `/logout`                   | POST   | Yes           | Invalidate access token (and clear cookies) |

---

## 1. 📝 Signup

### Request

```http
POST /api/Auth/signup
Content-Type: application/json

{
  "firstName": "string",        // required
  "lastName": "string",         // required
  "emailAddress": "user@example.com", // required, valid email
  "password": "P@ssw0rd!",     // required, meets password policy
  "userConfPassword": "P@ssw0rd!" // required, must match password
}
```

### Responses

* **201 Created** (or 200 OK)

  ```json
  {
    "success": true,
    "code": "AUTH_012",
    "message": "Verification code sent.",
    "data": null,
    "timestamp": "2025-06-14T12:34:56Z"
  }
  ```
* **400 Bad Request**

  * Validation errors (missing fields, weak password, mismatch)
  * Example:

  ```json
  {
    "success": false,
    "code": "AUTH_002",
    "message": "Password does not meet requirements."
  }
  ```
* **409 Conflict**

  * User already exists and verified

  ```json
  {
    "success": false,
    "code": "AUTH_008",
    "message": "User already exists and is verified."
  }
  ```

> **Notes:**
>
> * Password must include uppercase, lowercase, digit, special char, length 8–128.
> * After signup, server sets `EmailForVerification` cookie (HttpOnly) with expiry.

---

## 2. ✅ Verify Account

### Request

```http
POST /api/Auth/verify-account
Content-Type: application/json
Cookie: EmailForVerification=<user-email>

{
  "verificationCode": "123456"   // 6-digit code sent via email
}
```

### Responses

* **200 OK**

  ```json
  {
    "success": true,
    "code": "AUTH_013",
    "message": "Account activated successfully.",
    "data": null,
    "timestamp": "2025-06-14T12:45:00Z"
  }
  ```
* **400 Bad Request**

  * Missing or expired cookie
  * Invalid/expired code

  ```json
  {
    "success": false,
    "code": "AUTH_005",
    "message": "Invalid or expired verification code."
  }
  ```

> **Notes:**
>
> * On success, server deletes `EmailForVerification` cookie.

---

## 3. 🔄 Resend Verification Code

### Request

```http
POST /api/Auth/resend-verification-code
Cookie: EmailForVerification=<user-email>
```

*No body required.*

### Responses

* **200 OK**

  ```json
  {
    "success": true,
    "code": "AUTH_012",
    "message": "Verification code sent.",
    "data": null,
    "timestamp": "2025-06-14T12:50:00Z"
  }
  ```
* **429 Too Many Requests**

  * Cooldown not expired

  ```json
  {
    "success": false,
    "code": "AUTH_010",
    "message": "Please wait X minute(s) before requesting a new code."
  }
  ```
* **400 Bad Request**

  * No cookie found

---

## 4. 🔑 Signin

### Request

```http
POST /api/Auth/signin
Content-Type: application/json

{
  "email": "user@example.com",  // required
  "password": "P@ssw0rd!",     // required
  "rememberMe": true            // optional, default=false
}
```

### Responses

* **200 OK**

  ```json
  {
    "success": true,
    "code": "AUTH_012",
    "message": "Operation completed successfully.",
    "data": {
      "userId": 123,
      "fullName": "John Doe",
      "email": "user@example.com",
      "role": 1,                   // 0=Admin,1=RegularUser
      "accessToken": "<JWT>",
      "refreshToken": "<REFRESH>",
      "tokenExpiresAt": "2025-06-14T13:45:00Z",
      "autoLoginToken": "<AUTO_LOGIN>" // only if rememberMe=true
    },
    "timestamp": "2025-06-14T12:55:00Z"
  }
  ```
* **401 Unauthorized**

  ```json
  {
    "success": false,
    "code": "AUTH_001",
    "message": "Invalid credentials."
  }
  ```
* **423 Locked**

  * Account locked due to repeated failures

  ```json
  {
    "success": false,
    "code": "AUTH_003",
    "message": "Account locked. Try again later."
  }
  ```

> **Headers to set on client:**
>
> * `Authorization: Bearer <accessToken>` for protected calls
> * If `rememberMe`, also store `AutoLoginToken` in HttpOnly cookie

---

## 5. 🔁 Refresh Token

### Request

```http
POST /api/Auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "<REFRESH_TOKEN>"
}
```

### Responses

* **200 OK**

  ```json
  {
    "success": true,
    "code": "AUTH_014",
    "message": "Token refreshed successfully.",
    "data": {
      "accessToken": "<NEW_JWT>",
      "refreshToken": "<NEW_REFRESH>",
      "tokenExpiresAt": "2025-06-14T14:00:00Z"
    },
    "timestamp": "2025-06-14T13:00:00Z"
  }
  ```
* **401 Unauthorized**

  ```json
  {
    "success": false,
    "code": "AUTH_005",
    "message": "Invalid token."
  }
  ```

---

## 6. 🔓 Auto Login

### Request

```http
POST /api/Auth/auto-login
Content-Type: application/json

{
  "autoLoginToken": "<AUTO_LOGIN_TOKEN>"
}
```

### Responses

* **200 OK** Returns same schema as Signin (with new tokens)
* **401 Unauthorized** Invalid or expired auto-login token

---

## 7. 🔐 Forget Password

### Request

```http
POST /api/Auth/forget-password
Content-Type: application/json

{
  "emailAddress": "user@example.com"
}
```

### Responses

* **200 OK** (always true)

  ```json
  {
    "success": true,
    "code": "AUTH_011",
    "message": "Verification code sent to your email.",
    "data": null,
    "timestamp": "2025-06-14T13:05:00Z"
  }
  ```

> **Note:** Always returns success to protect against email enumeration.

---

## 8. 🔄 Reset Password

### Request

```http
POST /api/Auth/reset-password
Content-Type: application/json

{
  "emailAddress": "user@example.com",
  "verificationCode": "123456",
  "newPassword": "NewP@ssw0rd!",
  "confirmPassword": "NewP@ssw0rd!"
}
```

### Responses

* **200 OK**

  ```json
  {
    "success": true,
    "code": "AUTH_015",
    "message": "Password reset successfully.",
    "data": null,
    "timestamp": "2025-06-14T13:10:00Z"
  }
  ```
* **400 Bad Request** Invalid code or password mismatch

---

## 9. 🚪 Logout

### Request

```http
POST /api/Auth/logout
Authorization: Bearer <accessToken>
```

### Responses

* **200 OK**

  ```json
  {
    "success": true,
    "code": "AUTH_016",
    "message": "Logout successful.",
    "data": null,
    "timestamp": "2025-06-14T13:15:00Z"
  }
  ```
* **401 Unauthorized** Invalid or missing token

> **Note:** On success, client should also delete any stored cookies (`AutoLoginToken`).

---

## 📌 Global Guidelines

1. **Response Format:** All responses follow `SecureAuthResponse<T>`:

   ```json
   {
     "success": bool,
     "code": "AUTH_xxx",
     "message": "...",
     "data": T | null,
     "timestamp": "ISO-8601 UTC"
   }
   ```
2. **Error Handling:** Always check `success`. On failure, use `code` and `message` to inform users.
3. **Headers & Cookies:**

   * Set `Authorization: Bearer <token>` on protected calls.
   * Respect HttpOnly and Secure flags on cookies (`EmailForVerification`, `AutoLoginToken`).
4. **Token Expiry:** Use the `tokenExpiresAt` timestamp to refresh or logout before expiry.
5. **CORS:** Frontend origin `http://localhost:3000` is allowed.

---

> *Generated on 2025-06-14*
