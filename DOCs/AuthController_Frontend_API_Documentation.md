# 📄 LearnQuest - AuthController API Documentation (Frontend Contract)

> This document serves as a full API reference for the frontend team (React) to integrate with the authentication system.

---

## 🌐 Base URL

``` 
[https://localhost:7217/api/Auth](https://localhost:7217/api/Auth)

```

---

## 1️⃣ Signup

**Endpoint:** `POST /signup`

**Request Body:**

```json
{
  "firstName": "string (required)",
  "lastName": "string (required)",
  "emailAddress": "string (valid email, required)",
  "password": "string (required)",
  "userConfPassword": "string (required, must match password)"
}
```

**Success Response:**

```json
{
  "success": true,
  "code": "AUTH_012",
  "message": "Operation completed successfully.",
  "data": null,
  "timestamp": "..."
}
```

**Possible Errors:**

* User already exists and verified:

```json
{
  "success": false,
  "code": "AUTH_008",
  "message": "User already exists and is verified."
}
```

* Validation error: 400 BadRequest

---

## 2️⃣ Verify Account

**Endpoint:** `POST /verify-account`

**Request Body:**

```json
{
  "emailAddress": "string (required)",
  "verificationCode": "string (required)"
}
```

**Success Response:** Same as above.

**Possible Errors:**

```json
{
  "success": false,
  "code": "AUTH_005",
  "message": "Invalid or expired verification code."
}
```

---

## 3️⃣ Resend Verification Code

**Endpoint:** `POST /resend-verification-code`

**Request Body:** *No body required*

**Success Response:** Same as signup.

**Possible Errors:**

* If no email cookie exists → returns BadRequest.

---

## 4️⃣ Signin

**Endpoint:** `POST /signin`

**Request Body:**

```json
{
  "email": "string (required)",
  "password": "string (required)",
  "rememberMe": true/false
}
```

**Success Response:**

```json
{
  "success": true,
  "code": "AUTH_012",
  "message": "Operation completed successfully.",
  "data": {
    "userId": int,
    "fullName": "string",
    "email": "string",
    "role": int,
    "accessToken": "JWT_TOKEN",
    "refreshToken": "REFRESH_TOKEN",
    "tokenExpiresAt": "UTC datetime",
    "autoLoginToken": "AUTO_LOGIN_TOKEN"
  }
}
```

**Possible Errors:**

```json
{
  "success": false,
  "code": "AUTH_001",
  "message": "Invalid credentials."
}
```

---

## 5️⃣ Refresh Token

**Endpoint:** `POST /refresh-token`

**Request Body:**

```json
{
  "refreshToken": "string (required)"
}
```

**Success Response:**

```json
{
  "success": true,
  "data": {
    "accessToken": "JWT_TOKEN",
    "refreshToken": "REFRESH_TOKEN",
    "tokenExpiresAt": "UTC datetime"
  }
}
```

**Errors:**

```json
{
  "success": false,
  "code": "AUTH_005",
  "message": "Invalid token."
}
```

---

## 6️⃣ Auto Login

**Endpoint:** `POST /auto-login`

**Request Body:**

```json
{
  "autoLoginToken": "string (required)"
}
```

**Success Response:** Same as Signin.

**Errors:** Same as Refresh Token.

---

## 7️⃣ Forget Password

**Endpoint:** `POST /forget-password`

**Request Body:**

```json
{
  "emailAddress": "string (required)"
}
```

**Success Response:**

```json
{
  "success": true,
  "code": "AUTH_011",
  "message": "Verification code sent to your email."
}
```

**Errors:** Account not found.

```json
{
  "success": false,
  "code": "AUTH_008",
  "message": "Account not found."
}
```

---

## 8️⃣ Reset Password

**Endpoint:** `POST /reset-password`

**Request Body:**

```json
{
  "emailAddress": "string",
  "verificationCode": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

**Success:** Same as signup.

**Errors:** Invalid token, mismatched passwords, etc.

---

## 9️⃣ Logout

**Endpoint:** `POST /logout`

**Authorization Required:** Yes
(Bearer Token in Header)

**Success Response:** Same as signup.

**Errors:**

```json
{
  "success": false,
  "code": "AUTH_005",
  "message": "Invalid token."
}
```

---

## 🔐 Global Notes

* 🔸 All responses follow a consistent `SecureAuthResponse` format.
* 🔸 `accessToken` → required for authorization.
* 🔸 `refreshToken` → used for renewing tokens.
* 🔸 `autoLoginToken` → used for long-term "Remember Me" scenarios.
* 🔸 Always validate `success` field on frontend.
* 🔸 Use timestamp if needed for debug or error tracking.
