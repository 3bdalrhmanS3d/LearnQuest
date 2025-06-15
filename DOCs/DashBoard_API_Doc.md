# LearnQuest - DashboardController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/dashboard`

---

## 🔐 Authentication

All endpoints require Bearer token.

* Roles allowed: `Admin`, `Instructor`

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint               | Method | Description                            | Roles             |
| ---------------------- | ------ | -------------------------------------- | ----------------- |
| `/course-stats`        | GET    | Instructor course statistics           | Admin, Instructor |
| `/system-stats`        | GET    | System-wide statistics                 | Admin             |
| `/user-analytics`      | GET    | User verification & system metrics     | Admin             |
| `/recent-activity`     | GET    | Recent system or instructor activities | Admin, Instructor |
| `/performance-metrics` | GET    | Performance metrics with time filters  | Admin, Instructor |
| `/summary`             | GET    | Summary metrics for dashboard header   | Admin, Instructor |
| `/clear-cache`         | POST   | Force clear dashboard cache            | Admin             |

---

### 1️⃣ Course Stats (`GET /course-stats`)

**Response (Instructor Role):**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "role": "Instructor",
    "generatedAt": "2025-06-15T00:00:00Z",
    "totalCourses": 5,
    "courseStats": [
      {
        "courseId": 101,
        "courseName": "C# Fundamentals",
        "courseImage": "url/path",
        "studentCount": 50,
        "progressCount": 35
      }
    ],
    "mostEngagedCourse": {
      "courseId": 101,
      "courseName": "C# Fundamentals",
      "progressCount": 35
    }
  },
  "errors": null
}
```

---

### 2️⃣ System Stats (`GET /system-stats`)

**Response (Admin Only):**

```json
{
  "success": true,
  "data": {
    "totalUsers": 500,
    "activatedUsers": 450,
    "notActivatedUsers": 50,
    "totalRegularUsers": 400,
    "totalInstructors": 50,
    "totalAdmins": 50,
    "totalCourses": 120,
    "totalEnrollments": 1200,
    "totalRevenue": 65000.00,
    "lastUpdated": "2025-06-15T00:00:00Z"
  }
}
```

---

### 3️⃣ User Analytics (`GET /user-analytics`)

**Response:**

```json
{
  "success": true,
  "data": {
    "userMetrics": {
      "totalUsers": 500,
      "activatedUsers": 450,
      "pendingActivation": 50,
      "activationRate": 90.0
    },
    "systemMetrics": { /* Same as system-stats */ },
    "generatedAt": "2025-06-15T00:00:00Z"
  }
}
```

---

### 4️⃣ Recent Activity (`GET /recent-activity`)

**Response (Instructor):**

```json
{
  "success": true,
  "data": {
    "activities": [
      {
        "type": "Enrollment",
        "description": "New student enrolled in C# Fundamentals",
        "date": "2025-06-15T00:00:00Z",
        "courseId": 101,
        "courseName": "C# Fundamentals",
        "userId": 10,
        "userName": "Ahmed Ali"
      }
    ],
    "count": 1,
    "lastUpdated": "2025-06-15T00:00:00Z"
  }
}
```

**Response (Admin):**

```json
{
  "success": true,
  "data": {
    "activities": [ /* Admin actions logs */ ],
    "count": 50,
    "lastUpdated": "2025-06-15T00:00:00Z"
  }
}
```

---

### 5️⃣ Performance Metrics (`GET /performance-metrics?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd`)

**Response (Instructor):**

```json
{
  "success": true,
  "data": {
    "totalCourses": 5,
    "newEnrollments": 20,
    "totalStudents": 200,
    "revenue": 15000,
    "averageRating": 4.5,
    "period": {
      "startDate": "2025-06-01",
      "endDate": "2025-06-15"
    }
  }
}
```

**Response (Admin):**

```json
{
  "success": true,
  "data": {
    "totalUsers": 500,
    "activeCourses": 120,
    "newUsers": 30,
    "period": {
      "startDate": "2025-06-01",
      "endDate": "2025-06-15"
    }
  }
}
```

---

### 6️⃣ Dashboard Summary (`GET /summary`)

**Response (Instructor):**

```json
{
  "success": true,
  "data": {
    "totalCourses": 5,
    "activeCourses": 4,
    "totalEnrollments": 250,
    "totalRevenue": 25000,
    "averageRating": 4.4,
    "lastUpdated": "2025-06-15T00:00:00Z"
  }
}
```

**Response (Admin):**

```json
{
  "success": true,
  "data": {
    "totalUsers": 500,
    "totalInstructors": 50,
    "activeCourses": 120,
    "totalRevenue": 65000,
    "lastUpdated": "2025-06-15T00:00:00Z"
  }
}
```

---

### 7️⃣ Clear Cache (`POST /clear-cache`)

**Admin only.**

**Response:**

```json
{
  "success": true,
  "message": "Dashboard cache cleared successfully",
  "data": null
}
```

---

## 🔧 Common Error Responses

* 401: Unauthorized (missing or invalid token)
* 403: Forbidden (role restriction)
* 400: Validation errors (invalid query params)
* 500: Internal server errors (unexpected exceptions)

---

*Last Updated: 2025-06-15*
