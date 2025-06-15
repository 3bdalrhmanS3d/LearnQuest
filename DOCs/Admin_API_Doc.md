# LearnQuest - AdminController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/admin`

---

## 🔐 Authentication

All endpoints require a valid Bearer token with Admin role.

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                            | Method | Description                            | Role  |
| ----------------------------------- | ------ | -------------------------------------- | ----- |
| `/dashboard`                        | GET    | Welcome message and token validation   | Admin |
| `/all-users`                        | GET    | Retrieve users grouped by verification | Admin |
| `/get-basic-user-info/{userId}`     | GET    | Basic user profile and visit info      | Admin |
| `/make-instructor/{userId}`         | POST   | Promote user to Instructor             | Admin |
| `/make-admin/{userId}`              | POST   | Promote user to Admin                  | Admin |
| `/make-regular-user/{userId}`       | POST   | Demote user to Regular User            | Admin |
| `/delete-user/{userId}`             | DELETE | Soft delete user                       | Admin |
| `/recover-user/{userId}`            | POST   | Recover soft-deleted user              | Admin |
| `/toggle-user-activation/{userId}`  | POST   | Enable/disable user                    | Admin |
| `/all-admin-actions`                | GET    | List admin actions                     | Admin |
| `/get-history-user?userId={userId}` | GET    | User visit history                     | Admin |
| `/send-notification`                | POST   | Send notifications to user             | Admin |
| `/get-user-info`                    | GET    | Current admin info                     | Admin |

---

### 1. Admin Dashboard (`GET /dashboard`)

**Description:** Verifies admin token and returns welcome message.

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "Welcome to Admin Dashboard!",
  "data": {
    "adminId": 42,
    "timestamp": "2025-06-14T14:00:00Z"
  },
  "errors": null,
  "timestamp": "2025-06-15T00:00:00Z",
  "requestId": "abc123"
}
```

**Errors:**

* 401 Unauthorized
* 403 Forbidden

---

### 2. Get All Users (`GET /all-users`)

**Description:** List of verified and unverified users.

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "activatedCount": 1,
    "activatedUsers": [
      {
        "userId": 1,
        "fullName": "System Administrator",
        "emailAddress": "admin@learnquest.com",
        "role": "Admin",
        "isVerified": true,
        "createdAt": "2025-06-15T00:02:10.8907662",
        "isActive": true,
        "profilePhoto": null,
        "isSystemProtected": false
      }
    ],
    "notActivatedCount": 0,
    "notActivatedUsers": []
  },
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "e8911635"
}
```

**Errors:**

* 401 Unauthorized
* 500 Internal Server Error

---

### 3. Get Basic User Info (`GET /get-basic-user-info/{userId}`)

**Description:** Basic profile, details and last login.

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "user": {
      "userId": 1002,
      "fullName": "Jane Doe",
      "emailAddress": "jane@example.com",
      "role": "RegularUser",
      "isActive": true,
      "createdAt": "2025-02-01T10:00:00Z",
      "lastLoginAt": "2025-06-14T08:30:00Z",
      "details": {
        "birthDate": "1990-05-20",
        "educationLevel": "Bachelor's",
        "nationality": "Egypt",
        "createdAt": "2025-02-01T10:00:00Z"
      }
    }
  },
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "req-456"
}
```

**Errors:**

* 400 Bad Request
* 401 Unauthorized
* 404 Not Found
* 500 Internal Server Error

---

### 4. Role Management

#### Promote to Instructor (`POST /make-instructor/{userId}`)

#### Promote to Admin (`POST /make-admin/{userId}`)

#### Demote to Regular User (`POST /make-regular-user/{userId}`)

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "User promoted successfully",
  "data": null,
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "req-789"
}
```

**Errors:**

* 400 Invalid operation
* 404 Not Found
* 401 Unauthorized
* 500 Internal Server Error

---

### 5. Soft Delete & Recover

#### Delete User (`DELETE /delete-user/{userId}`)

#### Recover User (`POST /recover-user/{userId}`)

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "User deleted/recovered successfully",
  "data": null,
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "req-001"
}
```

**Errors:**

* 400 Invalid operation
* 404 Not Found
* 401 Unauthorized
* 500 Internal Server Error

---

### 6. Toggle User Activation (`POST /toggle-user-activation/{userId}`)

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "User activation toggled successfully",
  "data": null,
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "req-002"
}
```

**Errors:**

* 400 Invalid operation
* 404 Not Found
* 401 Unauthorized
* 500 Internal Server Error

---

### 7. Admin Action Logs (`GET /all-admin-actions`)

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "count": 50,
    "logs": [
      {
        "logId": 1,
        "adminName": "Admin User",
        "adminEmail": "admin@example.com",
        "targetUserName": "Jane Doe",
        "targetUserEmail": "jane@example.com",
        "actionType": "MakeInstructor",
        "actionDetails": "User promoted to Instructor",
        "actionDate": "2025-06-14T09:00:00Z",
        "ipAddress": "192.168.1.10"
      }
    ]
  },
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "req-003"
}
```

**Errors:**

* 401 Unauthorized
* 500 Internal Server Error

---

### 8. User Visit History (`GET /get-history-user?userId={userId}`)

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "Success",
  "data": [
    { "id": 1, "userId": 1002, "lastVisit": "2025-06-14T08:30:00Z" }
  ],
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "req-004"
}
```

**Errors:**

* 401 Unauthorized
* 500 Internal Server Error

---

### 9. Send Notification (`POST /send-notification`)

**Request Body:**

```json
{
  "userId": 123,
  "templateType": "AccountActivated", 
  "subject": "Custom Subject", 
  "message": "Custom Message"
}
```

**Success Response:**

```json
{
  "success": true,
  "message": "Notification sent successfully",
  "data": null,
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "req-005"
}
```

**Errors:**

* 400 Invalid input
* 404 User not found
* 401 Unauthorized
* 500 Internal Server Error

---

### 10. Get Current Admin Info (`GET /get-user-info`)

**Request Body:** None

**Success Response:**

```json
{
  "success": true,
  "message": "Admin retrieved successfully!",
  "data": {
    "user": {
      "userId": 42,
      "fullName": "Admin User",
      "emailAddress": "admin@learnquest.com",
      "role": "Admin",
      "isActive": true,
      "createdAt": "2025-02-01T10:00:00Z",
      "lastLoginAt": "2025-06-14T08:30:00Z",
      "details": {
        "birthDate": "1990-01-01",
        "educationLevel": "Master's",
        "nationality": "US",
        "createdAt": "2025-02-01T10:00:00Z"
      }
    }
  },
  "errors": null,
  "timestamp": "2025-06-15T02:53:34.5382427Z",
  "requestId": "req-006"
}
```

**Errors:**

* 401 Unauthorized
* 404 Admin not found

---

*Last updated: 2025-06-15*
