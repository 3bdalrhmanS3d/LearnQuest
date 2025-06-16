# LearnQuest – LevelController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/levels`

---

## 🔐 Authentication

All endpoints require a valid Bearer token with **Instructor** or **Admin** role (except Admin-only or unconditional endpoints):

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                        | Method | Description                                     | Role              |
| ------------------------------- | ------ | ----------------------------------------------- | ----------------- |
| `/Create`                       | POST   | Create a new level in a course                  | Instructor, Admin |
| `/{levelId}`                    | PUT    | Update an existing level                        | Instructor, Admin |
| `/{levelId}`                    | DELETE | Soft-delete a level                             | Instructor, Admin |
| `/{levelId}/details`            | GET    | Retrieve detailed information of a level        | Instructor, Admin |
| `course/{courseId}`             | GET    | Get all levels for a specific course            | Instructor, Admin |
| `/{levelId}/progress`           | GET    | Get student progress for a level                | Instructor, Admin |
| `/{levelId}/stats`              | GET    | Get statistical data for a level                | Instructor, Admin |
| `/{levelId}/analytics`          | GET    | Get analytics for a level (optional date range) | Instructor, Admin |
| `/{levelId}/toggle-visibility`  | PATCH  | Toggle visibility of a level                    | Instructor, Admin |
| `/reorder`                      | PUT    | Reorder multiple levels                         | Instructor, Admin |
| `/my-levels`                    | GET    | Get levels created by current instructor        | Instructor, Admin |
| `/search`                       | GET    | Search and filter levels                        | Instructor, Admin |
| `/{levelId}/copy`               | POST   | Copy an existing level to another course        | Instructor, Admin |
| `/bulk-action`                  | POST   | Perform bulk actions on levels                  | Instructor, Admin |
| `instructor/{instructorId}`     | GET    | Get levels by specific instructor               | Admin-only        |
| `/{levelId}/transfer-ownership` | POST   | Transfer level ownership to another instructor  | Admin-only        |
| `/admin/all`                    | GET    | Get all levels for admin view                   | Admin-only        |

---

### 1. Create Level (`POST /Create`)

**Request Body:**

```json
{
  "levelName": "Basics of Algebra",
  "courseId": 5,
  "isVisible": true,
  "order": 1
}
```

**Success Response (201):**

```json
{
  "message": "Level created successfully",
  "levelId": 12
}
```

---

### 2. Update Level (`PUT /{levelId}`)

**Path Parameter:** `levelId` (integer)

**Request Body:**

```json
{
  "levelId": 12,
  "levelName": "Advanced Algebra",
  "courseId": 5,
  "isVisible": false,
  "order": 1
}
```

**Success Response (200):**

```json
{
  "message": "Level updated successfully"
}
```

---

### 3. Delete Level (`DELETE /{levelId}`)

**Path Parameter:** `levelId` (integer)

**Success Response (200):**

```json
{
  "message": "Level deleted successfully"
}
```

---

### 4. Get Level Details (`GET /{levelId}/details`)

**Path Parameter:** `levelId` (integer)

**Success Response (200):**

```json
{
  "message": "Level details retrieved successfully",
  "data": {
    "levelId": 12,
    "levelName": "Basics of Algebra",
    "courseId": 5,
    "isVisible": true,
    "order": 1,
    "createdAt": "2025-06-16T10:00:00Z"
  }
}
```

---

### 5. Get Course Levels (`GET /course/{courseId}`)

**Path Parameter:** `courseId` (integer)

**Query Params:** `includeHidden` (boolean, default: false)

**Success Response (200):**

```json
{
  "message": "Course levels retrieved successfully",
  "count": 3,
  "data": [ /* LevelSummaryDto[] */ ]
}
```

---

### 6. Get Level Progress (`GET /{levelId}/progress?pageNumber=1&pageSize=20`)

**Success Response (200):**

```json
{
  "message": "Level progress retrieved successfully",
  "data": [ /* LevelProgressDto[] */ ],
  "pagination": { "pageNumber": 1, "pageSize": 20, "hasMore": false }
}
```

---

### 7. Get Level Stats (`GET /{levelId}/stats`)

**Success Response (200):**

```json
{
  "message": "Level statistics retrieved successfully",
  "data": { /* LevelStatsDto */ }
}
```

---

### 8. Get Level Analytics (`GET /{levelId}/analytics?startDate=2025-06-01&endDate=2025-06-15`)

**Success Response (200):**

```json
{
  "message": "Level analytics retrieved successfully",
  "data": { /* LevelAnalyticsDto */ }
}
```

---

### 9. Toggle Visibility (`PATCH /{levelId}/toggle-visibility`)

**Success Response (200):**

```json
{
  "message": "Level visibility toggled successfully",
  "data": { "levelId": 12, "isNowVisible": false }
}
```

---

### 10. Reorder Levels (`PUT /reorder`)

**Request Body:** `ReorderLevelDto[]`

**Success Response (200):**

```json
{
  "message": "Levels reordered successfully"
}
```

---

### 11. Get My Levels (`GET /my-levels?pageNumber=1&pageSize=20`)

**Success Response (200):**

```json
{
  "message": "Instructor levels retrieved successfully",
  "data": [ /* LevelSummaryDto[] */ ],
  "pagination": { "pageNumber":1, "pageSize":20, "hasMore": false }
}
```

---

### 12. Search Levels (`GET /search?SearchTerm=algebra&CourseId=5&pageNumber=1&pageSize=20`)

**Success Response (200):**

```json
{
  "message": "Level search completed successfully",
  "data": [ /* LevelSummaryDto[] */ ],
  "filter": { /* echo filter */ },
  "pagination": { /* pagination info */ }
}
```

---

### 13. Copy Level (`POST /{levelId}/copy`)

**Request Body:** `CopyLevelDto`

**Success Response (201):**

```json
{
  "message": "Level copied successfully",
  "levelId": 20
}
```

---

### 14. Bulk Action (`POST /bulk-action`)

**Request Body:** `BulkLevelActionDto`

**Success Response (200):**

```json
{
  "message": "Bulk action completed",
  "data": { "successCount": 2, "failedIds": [] }
}
```

---

### 15. Get Levels by Instructor (`GET /instructor/{instructorId}?pageNumber=1&pageSize=20`)

**Success Response (200):**

```json
{
  "message": "Instructor levels retrieved successfully",
  "instructorId": 7,
  "data": [ /* LevelSummaryDto[] */ ],
  "pagination": { /* pagination info */ }
}
```

---

### 16. Transfer Ownership (`POST /{levelId}/transfer-ownership`)

**Request Body:**

```json
{
  "newInstructorId": 8
}
```

**Success Response (200):**

```json
{
  "message": "Level ownership transferred successfully"
}
```

---

### 17. Get All Levels for Admin (`GET /admin/all?pageNumber=1&pageSize=20&searchTerm=beta`)

**Success Response (200):**

```json
{
  "message": "All levels retrieved successfully",
  "data": [ /* LevelSummaryDto[] */ ],
  "searchTerm": "beta",
  "pagination": { /* pagination info */ }
}
```

*Last updated: 2025-06-16*
