# LearnQuest - ProfileController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/profile`

---

## 🔐 Authentication

All endpoints require a valid Bearer token.

**Allowed Roles:** `Admin`, `Instructor`, `RegularUser`

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                       | Method | Description                           |
| ------------------------------ | ------ | ------------------------------------- |
| `/dashboard`                   | GET    | Load user dashboard info              |
| `/`                            | GET    | Get current user profile              |
| `/update`                      | POST   | Update user profile                   |
| `/pay-course`                  | POST   | Register course payment               |
| `/confirm-payment/{paymentId}` | POST   | Confirm payment and enroll            |
| `/my-courses`                  | GET    | Get enrolled courses                  |
| `/favorite-courses`            | GET    | Get favorite courses                  |
| `/favorites/{courseId}`        | POST   | Add course to favorites               |
| `/favorites/{courseId}`        | DELETE | Remove course from favorites          |
| `/upload-photo`                | POST   | Upload profile photo                  |
| `/delete-photo`                | DELETE | Delete profile photo                  |
| `/change-name`                 | POST   | Change user full name                 |
| `/change-password`             | POST   | Change user password                  |
| `/stats`                       | GET    | User statistics & progress            |
| `/recent-activities`           | GET    | Get recent user activities (limit 10) |

---

### 1️⃣ Get Dashboard (`GET /dashboard`)

**Response 200 OK:**

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

**Response 200 OK:**

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

**Errors:** `401 Unauthorized`, `404 Not Found`, `400 Bad Request`, `500 Internal Server Error`

---

### 3️⃣ Update Profile (`POST /update`)

**Request Body:**

```json
{
  "birthDate": "1995-01-01",
  "edu": "Master's",
  "national": "Egypt"
}
```

**Response 200 OK:**

```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": null
}
```

**Errors:** Validation errors (`400 Bad Request`), Invalid dates

---

### 4️⃣ Pay For Course (`POST /pay-course`)

**Request Body:**

```json
{
  "courseId": 101,
  "amount": 100.00,
  "transactionId": "TXN123456"
}
```

**Response 200 OK:**

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

**Errors:** Validation errors (`400 Bad Request`), Course not found (`404 Not Found`)

---

### 5️⃣ Confirm Payment (`POST /confirm-payment/{paymentId}`)

**Response 200 OK:**

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

**Errors:** Payment not found (`404 Not Found`), Invalid confirmation (`400 Bad Request`)

---

### 6️⃣ My Courses (`GET /my-courses`)

**Response 200 OK:**

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

**Response 200 OK:**

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

### 8️⃣ Add To Favorites (`POST /favorites/{courseId}`)

**Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "message": "Course added to favorites successfully",
    "courseId": 202,
    "addedAt": "2025-06-15T00:00:00Z"
  }
}
```

**Errors:** Course not found (`404 Not Found`), Already in favorites (`400 Bad Request`)

---

### 9️⃣ Remove From Favorites (`DELETE /favorites/{courseId}`)

**Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "message": "Course removed from favorites successfully",
    "courseId": 202,
    "removedAt": "2025-06-15T00:00:00Z"
  }
}
```

**Errors:** Not in favorites (`404 Not Found`)

---

### 🔟 Upload Profile Photo (`POST /upload-photo`)

* **Request:** `multipart/form-data`
* Allowed formats: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
* Max file size: 5 MB

**Response 200 OK:**

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

**Errors:**

* `400 Bad Request` (no file, invalid type)
* `413 Payload Too Large`

---

### 1️⃣1️⃣ Delete Profile Photo (`DELETE /delete-photo`)

**Response 200 OK:**

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

### 1️⃣2️⃣ Change Name (`POST /change-name`)

**Request Body:**

```json
{
  "newFullName": "Jane Smith",
  "changeReason": "Updated my official name"
}
```

**Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "message": "User name changed successfully",
    "newName": "Jane Smith",
    "changedAt": "2025-06-15T00:00:00Z",
    "requiresReLogin": true
  }
}
```

**Errors:** Validation errors (`400 Bad Request`), User not found (`404 Not Found`)

---

### 1️⃣3️⃣ Change Password (`POST /change-password`)

**Request Body:**

```json
{
  "currentPassword": "OldPass!23",
  "newPassword": "NewPass!45",
  "confirmPassword": "NewPass!45",
  "changeReason": "Routine update"
}
```

**Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "message": "Password changed successfully. Please login again.",
    "changedAt": "2025-06-15T00:00:00Z",
    "requiresReLogin": true,
    "allDevicesLoggedOut": true
  }
}
```

**Errors:** Validation errors (`400 Bad Request`), Invalid current password (`400 Bad Request`), User not found (`404 Not Found`)

---

### 1️⃣4️⃣ User Stats (`GET /stats`)

**Response 200 OK:**

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
      "completionRate": 0.75
    }
  }
}
```

---

### 1️⃣5️⃣ Recent Activities (`GET /recent-activities`)

**Response 200 OK:**

```json
{
  "success": true,
  "message": "Recent activities retrieved successfully",
  "data": [
    {
      "activityType": "Login",
      "description": "User logged in",
      "timestamp": "2025-06-15T00:00:00Z",
      "ipAddress": "127.0.0.1",
      "userAgent": "PostmanRuntime/7.32.3",
      "timeAgo": "Just now",
      "activityIcon": "log-in",
      "formattedTimestamp": "Jun 15, 2025 00:00"
    }
    // … up to 10
  ]
}
```

---

## 🔧 Common Error Responses

* **401 Unauthorized**
  Invalid or missing token.

* **403 Forbidden**
  Insufficient role/permissions.

* **404 Not Found**
  Resource not found (user, course, payment).

* **400 Bad Request**
  Validation or business rule errors.

* **413 Payload Too Large**
  Uploaded file exceeds size limit.

* **500 Internal Server Error**
  Unexpected errors.

---

*Last Updated: 2025-06-15*

```
```
