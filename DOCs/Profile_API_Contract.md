# LearnQuest - ProfileController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/profile`

---

## 🔐 Authentication

All endpoints require Bearer token.

* Roles allowed: `Admin`, `Instructor`, `RegularUser`

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                       | Method | Description                |
| ------------------------------ | ------ | -------------------------- |
| `/dashboard`                   | GET    | Load user dashboard info   |
| `/`                            | GET    | Get current user profile   |
| `/update`                      | POST   | Update user profile        |
| `/pay-course`                  | POST   | Register course payment    |
| `/confirm-payment/{paymentId}` | POST   | Confirm payment and enroll |
| `/my-courses`                  | GET    | Get enrolled courses       |
| `/favorite-courses`            | GET    | Get favorite courses       |
| `/upload-photo`                | POST   | Upload profile photo       |
| `/delete-photo`                | DELETE | Delete profile photo       |
| `/stats`                       | GET    | User statistics & progress |

---

### 1️⃣ Get Dashboard (`GET /dashboard`)

**Response:**

```json
{
  "success": true,
  "message": "Dashboard loaded successfully",
  "data": {
    "message": "Welcome to your dashboard!",
    "userId": 1,
    "timestamp": "2025-06-15T00:00:00Z",
    "sessionInfo": {
      "ipAddress": "127.0.0.1",
      "userAgent": "PostmanRuntime/7.32.3"
    }
  }
}
```

---

### 2️⃣ Get Profile (`GET /`)

**Response:**

```json
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "userId": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "role": "RegularUser",
    "profilePhoto": "/uploads/profile-pictures/user_1.png",
    "birthDate": "1995-01-01",
    "educationLevel": "Bachelor's",
    "nationality": "USA",
    "createdAt": "2025-01-01T10:00:00Z"
  }
}
```

**Errors:** 401, 404, 400, 500

---

### 3️⃣ Update Profile (`POST /update`)

**Request:**

```json
{
  "birthDate": "1995-01-01",
  "edu": "Master's",
  "national": "Egypt"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": null
}
```

**Errors:** Validation Errors, Invalid Dates

---

### 4️⃣ Pay For Course (`POST /pay-course`)

**Request:**

```json
{
  "courseId": 101,
  "amount": 100.00,
  "transactionId": "TXN123456"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "message": "Payment recorded successfully. Awaiting confirmation",
    "courseId": 101,
    "amount": 100.00,
    "transactionId": "TXN123456",
    "status": "Pending"
  }
}
```

**Errors:** Validation, Course not found

---

### 5️⃣ Confirm Payment (`POST /confirm-payment/{paymentId}`)

**Response:**

```json
{
  "success": true,
  "data": {
    "message": "Payment confirmed. Course enrollment successful",
    "paymentId": 202,
    "status": "Completed",
    "enrollmentDate": "2025-06-15T00:00:00Z"
  }
}
```

**Errors:** Payment not found, Invalid confirmation

---

### 6️⃣ My Courses (`GET /my-courses`)

**Response:**

```json
{
  "success": true,
  "data": {
    "count": 3,
    "courses": [
      { "courseId": 101, "courseName": "C# Fundamentals" }
    ],
    "lastUpdated": "2025-06-15T00:00:00Z"
  }
}
```

---

### 7️⃣ Favorite Courses (`GET /favorite-courses`)

**Response:**

```json
{
  "success": true,
  "data": {
    "count": 2,
    "favorites": [
      { "courseId": 202, "courseName": "ASP.NET Core" }
    ],
    "lastUpdated": "2025-06-15T00:00:00Z"
  }
}
```

---

### 8️⃣ Upload Profile Photo (`POST /upload-photo`)

* **Request:** `multipart/form-data`
* Allowed formats: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
* Max file size: 5MB

**Response:**

```json
{
  "success": true,
  "data": {
    "message": "Profile photo uploaded successfully",
    "fileName": "user_1_20250615.jpg",
    "fileSize": 1000000,
    "uploadedAt": "2025-06-15T00:00:00Z"
  }
}
```

**Errors:** 400, 413 Payload Too Large, Invalid file type

---

### 9️⃣ Delete Profile Photo (`DELETE /delete-photo`)

**Response:**

```json
{
  "success": true,
  "data": {
    "message": "Profile photo deleted. Default restored",
    "deletedAt": "2025-06-15T00:00:00Z",
    "defaultPhoto": "/uploads/profile-pictures/default.png"
  }
}
```

---

### 🔟 User Stats (`GET /stats`)

**Response:**

```json
{
  "success": true,
  "data": {
    "enrolledCourses": 5,
    "completedSections": 15,
    "progressDetails": [
      { "courseId": 101, "progressPercentage": 80 }
    ],
    "lastUpdated": "2025-06-15T00:00:00Z",
    "summary": {
      "totalActiveCourses": 5,
      "averageProgress": 75,
      "completionRate": 3
    }
  }
}
```

---

## 🔧 Common Error Responses

* 401: Unauthorized (invalid token)
* 404: Not Found (user/profile/payment not found)
* 400: Validation errors
* 500: Internal server errors

---

*Last Updated: 2025-06-15*
