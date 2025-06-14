# 📄 LearnQuest - AdminController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/admin`

---

## 🔐 Authentication

All endpoints in this controller require a valid **Bearer** token with **Admin** role.
Include header:

```
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Action                              | Endpoint                            | Method | Auth | Description                                |
| ----------------------------------- | ----------------------------------- | ------ | ---- | ------------------------------------------ |
| Admin Dashboard                     | `/dashboard`                        | GET    | Yes  | Welcome message and basic admin info       |
| Get All Users                       | `/all-users`                        | GET    | Yes  | Retrieve all users grouped by verification |
| Get Basic User Info                 | `/get-basic-user-info/{userId}`     | GET    | Yes  | Fetch core profile & last login of a user  |
| Promote to Instructor               | `/make-instructor/{userId}`         | POST   | Yes  | Elevate user to **Instructor** role        |
| Promote to Admin                    | `/make-admin/{userId}`              | POST   | Yes  | Elevate user to **Admin** role             |
| Demote to Regular User              | `/make-regular-user/{userId}`       | POST   | Yes  | Demote user to **RegularUser** role        |
| Soft Delete User                    | `/delete-user/{userId}`             | DELETE | Yes  | Mark user as deleted (soft delete)         |
| Recover Soft-Deleted User           | `/recover-user/{userId}`            | POST   | Yes  | Restore a previously soft-deleted user     |
| Toggle User Activation              | `/toggle-user-activation/{userId}`  | POST   | Yes  | Enable/disable a user account              |
| Get Admin Actions Log               | `/all-admin-actions`                | GET    | Yes  | Retrieve recent admin action logs          |
| Get User Visit History              | `/get-history-user?userId={userId}` | GET    | Yes  | Fetch visit history entries for a user     |
| Send Notification to User(s)        | `/send-notification`                | POST   | Yes  | Send email & in-app notification           |
| Get Current Admin Info (from token) | `/get-user-info`                    | GET    | Yes  | Retrieve admin’s own profile from token    |

---

## 1️⃣ Admin Dashboard

**Endpoint:** `GET /dashboard`

**Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "message": "Welcome to Admin Dashboard!",
    "adminId": 42,
    "timestamp": "2025-06-14T14:00:00Z"
  },
  "error": null
}
```

**Errors:**

* `401 Unauthorized` invalid/missing token
* `403 Forbidden` non-admin role

---

## 2️⃣ Get All Users (Grouped)

**Endpoint:** `GET /all-users`

**Description:** Returns counts and arrays of activated vs not activated users. Cached for 2 minutes.

**Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "ActivatedCount": 10,
    "ActivatedUsers": [ /* Array of AdminUserDto */ ],
    "NotActivatedCount": 3,
    "NotActivatedUsers": [ /* Array of AdminUserDto */ ]
  },
  "error": null
}
```

**AdminUserDto**:

```json
{
  "userId": 1002,
  "fullName": "Jane Doe",
  "emailAddress": "jane@example.com",
  "role": "RegularUser",
  "isVerified": true,
  "createdAt": "2025-02-01T10:00:00Z",
  "isActive": true
}
```

**Errors:**

* `401 Unauthorized` invalid token
* `500 Internal Server Error` on failure

---

## 3️⃣ Get Basic User Info

**Endpoint:** `GET /get-basic-user-info/{userId}`

**URL Parameters:**

* `userId` (int, required)

**Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "user": {
      "userId": 1002,
      "fullName": "Jane Doe",
      "emailAddress": "jane@example.com",
      "role": "RegularUser",
      "isActive": true,
      "createdAt": "2025-02-01T10:00:00Z",
      "lastLoginAt": "2025-06-14T08:30:00Z", // nullable
      "details": {
        "birthDate": "1990-05-20",
        "educationLevel": "Bachelor's",
        "nationality": "Egypt",
        "createdAt": "2025-02-01T10:00:00Z"
      }
    }
  },
  "error": null
}
```

**Errors:**

* `400 Bad Request` invalid userId
* `401 Unauthorized` invalid token
* `404 Not Found` user does not exist
* `500 Internal Server Error`

---

## 4️⃣ Role Management

### Promote to Instructor

* **Endpoint:** `POST /make-instructor/{userId}`
* **Response 200:** `{ "success": true, "data": null }`
* **Errors:** `400`, `404`, `401`, `500`

### Promote to Admin

* **Endpoint:** `POST /make-admin/{userId}`
* **Response 200:** `{ "success": true, "data": null }`

### Demote to Regular User

* **Endpoint:** `POST /make-regular-user/{userId}`
* **Response 200:** `{ "success": true, "data": null }`

*All three follow the same pattern:*

* URL param `userId`
* Cache invalidated for the user
* Logged via AdminActionLogger

---

## 5️⃣ Soft Delete & Recover

### Soft Delete User

* **Endpoint:** `DELETE /delete-user/{userId}`
* **Response 200:** `{ "success": true, "data": null }`
* **Errors:** `400`, `404`, `401`, `500`

### Recover Soft-Deleted User

* **Endpoint:** `POST /recover-user/{userId}`
* **Response 200:** `{ "success": true, "data": null }`

*Invalid operations throw `400`.*

---

## 6️⃣ Toggle Activation

* **Endpoint:** `POST /toggle-user-activation/{userId}`
* **Response 200:** `{ "success": true, "data": null }`

---

## 7️⃣ Admin Action Logs

* **Endpoint:** `GET /all-admin-actions`
* **Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "Count": 50,
    "Logs": [
      {
        "logId": 1,
        "adminName": "Admin User",
        "adminEmail": "admin@example.com",
        "targetUserName": "Jane Doe",
        "targetUserEmail": "jane@example.com",
        "actionType": "MakeInstructor",
        "actionDetails": "User jane@example.com promoted to Instructor",
        "actionDate": "2025-06-14T09:00:00Z",
        "ipAddress": "192.168.1.10"
      }
    ]
  },
  "error": null
}
```

* Cached for 1 minute

---

## 8️⃣ User Visit History

* **Endpoint:** `GET /get-history-user?userId={userId}`
* **Response 200 OK:** array of `UserVisitHistory`

```json
[
  { "id": 1, "userId": 1002, "lastVisit": "2025-06-14T08:30:00Z" },
  ...
]
```

---

## 9️⃣ Send Notification

* **Endpoint:** `POST /send-notification`
* **Request Body:**

  ```json
  {
    "userId": 1002,                // required
    "templateType": "AccountActivated", // optional enum
    "subject": "string",          // required if no template
    "message": "string"           // required if no template
  }
  ```
* **Response 200:** `{ "success": true, "data": null }`
* **Errors:** `400`, `404`, `401`, `500`

---

## 🔟 Get Current Admin Info

* **Endpoint:** `GET /get-user-info`
* **Response 200 OK:**

```json
{
  "success": true,
  "data": {
    "message": "Admin retrieved successfully!",
    "user": { /* BasicUserInfoDto */ }
  },
  "error": null
}
```

---

> *Generated on 2025-06-14*
