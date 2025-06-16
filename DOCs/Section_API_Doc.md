# LearnQuest – SectionController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/sections`

---

## 🔐 Authentication

All endpoints require a valid Bearer token with **Instructor** or **Admin** role (except Admin-only or unconditional endpoints):

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                          | Method | Description                                      | Role              |
| --------------------------------- | ------ | ------------------------------------------------ | ----------------- |
| `/Create`                         | POST   | Create a new section                             | Instructor, Admin |
| `/{sectionId}`                    | PUT    | Update an existing section                       | Instructor, Admin |
| `/{sectionId}`                    | DELETE | Soft-delete a section                            | Instructor, Admin |
| `/{sectionId}/details`            | GET    | Get detailed info of a section                   | Instructor, Admin |
| `level/{levelId}`                 | GET    | Get all sections under a level                   | Instructor, Admin |
| `/{sectionId}/progress`           | GET    | Get student progress data in a section           | Instructor, Admin |
| `/{sectionId}/stats`              | GET    | Get statistical data for a section               | Instructor, Admin |
| `/{sectionId}/analytics`          | GET    | Get advanced analytics (optional date range)     | Instructor, Admin |
| `/{sectionId}/contents`           | GET    | Get list of contents in a section                | Instructor, Admin |
| `/{sectionId}/toggle-visibility`  | PATCH  | Toggle section visibility                        | Instructor, Admin |
| `reorder`                         | PUT    | Reorder multiple sections                        | Instructor, Admin |
| `my-sections`                     | GET    | Get sections created by current instructor       | Instructor, Admin |
| `search`                          | GET    | Search/filter sections                           | Instructor, Admin |
| `/{sectionId}/copy`               | POST   | Copy an existing section                         | Instructor, Admin |
| `bulk-action`                     | POST   | Perform bulk actions on sections                 | Instructor, Admin |
| `instructor/{instructorId}`       | GET    | Get sections by specific instructor              | Admin-only        |
| `/{sectionId}/transfer-ownership` | POST   | Transfer section ownership to another instructor | Admin-only        |
| `admin/all`                       | GET    | Get all sections (Admin view)                    | Admin-only        |

---

### 1. Create Section (`POST /Create`)

**Request Body:**

```json
{
  "sectionName": "Stomatal Function",
  "description": "Mechanism of gas exchange in leaves",
  "levelId": 3,
  "isVisible": true,
  "order": 3
}
```

**Success Response (201):**

```json
{
  "message": "Section created successfully",
  "sectionId": 12
}
```

---

### 2. Update Section (`PUT /{sectionId}`)

**Path Parameter:** `sectionId` (integer)

**Request Body:**

```json
{
  "sectionId": 12,
  "sectionName": "Stomatal Regulation",
  "description": "How stomata open and close",
  "levelId": 3,
  "isVisible": false,
  "order": 3
}
```

**Success Response (200):**

```json
{
  "message": "Section updated successfully"
}
```

---

### 3. Delete Section (`DELETE /{sectionId}`)

**Path Parameter:** `sectionId` (integer)

**Success Response (200):**

```json
{
  "message": "Section deleted successfully"
}
```

---

### 4. Get Section Details (`GET /{sectionId}/details`)

**Path Parameter:** `sectionId` (integer)

**Success Response (200):**

```json
{
  "message": "Section details retrieved successfully",
  "data": {
    "sectionId": 12,
    "sectionName": "Stomatal Function",
    "description": "Mechanism of gas exchange in leaves",
    "levelId": 3,
    "isVisible": true,
    "order": 3,
    "createdAt": "2025-06-16T09:00:00Z"
  }
}
```

---

### 5. Get Level Sections (`GET /level/{levelId}`)

**Path Parameter:** `levelId` (integer)

**Query Params:** `includeHidden` (boolean, default: false)

**Success Response (200):**

```json
{
  "message": "Level sections retrieved successfully",
  "count": 2,
  "data": [ /* SectionSummaryDto[] */ ]
}
```

---

### 6. Get Section Progress (`GET /{sectionId}/progress?pageNumber=1&pageSize=20`)

**Success Response (200):**

```json
{
  "message": "Section progress retrieved successfully",
  "data": [ /* SectionProgressDto[] */ ],
  "pagination": { "pageNumber": 1, "pageSize": 20, "hasMore": false }
}
```

---

### 7. Get Section Stats (`GET /{sectionId}/stats`)

**Success Response (200):**

```json
{
  "message": "Section statistics retrieved successfully",
  "data": { /* SectionStatsDto */ }
}
```

---

### 8. Get Section Analytics (`GET /{sectionId}/analytics?startDate=2025-06-01&endDate=2025-06-15`)

**Success Response (200):**

```json
{
  "message": "Section analytics retrieved successfully",
  "data": { /* SectionAnalyticsDto */ }
}
```

---

### 9. Get Section Contents (`GET /{sectionId}/contents`)

**Success Response (200):**

```json
{
  "message": "Section contents retrieved successfully",
  "count": 5,
  "data": [ /* ContentOverviewDto[] */ ]
}
```

---

### 10. Toggle Visibility (`PATCH /{sectionId}/toggle-visibility`)

**Success Response (200):**

```json
{
  "message": "Section visibility toggled successfully",
  "data": { "sectionId": 12, "isNowVisible": false }
}
```

---

### 11. Reorder Sections (`PUT /reorder`)

**Request Body:** `ReorderSectionDto[]`

**Success Response (200):**

```json
{
  "message": "Sections reordered successfully"
}
```

---

### 12. Get My Sections (`GET /my-sections?pageNumber=1&pageSize=20`)

**Success Response (200):**

```json
{
  "message": "Instructor sections retrieved successfully",
  "data": [ /* SectionSummaryDto[] */ ],
  "pagination": { "pageNumber":1, "pageSize":20, "hasMore": false }
}
```

---

### 13. Search Sections (`GET /search?SearchTerm=photosynthesis&LevelId=3&pageNumber=1&pageSize=20`)

**Success Response (200):**

```json
{
  "message": "Section search completed successfully",
  "data": [ /* SectionSummaryDto[] */ ],
  "filter": { /* echo filter */ },
  "pagination": { /* pagination info */ }
}
```

---

### 14. Copy Section (`POST /{sectionId}/copy`)

**Request Body:** `CopySectionDto`

**Success Response (201):**

```json
{
  "message": "Section copied successfully",
  "sectionId": 45
}
```

---

### 15. Bulk Action (`POST /bulk-action`)

**Request Body:** `BulkSectionActionDto`

**Success Response (200):**

```json
{
  "message": "Bulk action completed",
  "data": { "successCount": 3, "failedIds": [] }
}
```

---

### 16. Get Sections by Instructor (`GET /instructor/{instructorId}?pageNumber=1&pageSize=20`)

**Success Response (200):**

```json
{
  "message": "Instructor sections retrieved successfully",
  "instructorId": 5,
  "data": [ /* SectionSummaryDto[] */ ],
  "pagination": { /* pagination info */ }
}
```

---

### 17. Transfer Ownership (`POST /{sectionId}/transfer-ownership`)

**Request Body:**

```json
{
  "newInstructorId": 7
}
```

**Success Response (200):**

```json
{
  "message": "Section ownership transferred successfully"
}
```

---

### 18. Get All Sections for Admin (`GET /admin/all?pageNumber=1&pageSize=20&searchTerm=alpha`)

**Success Response (200):**

```json
{
  "message": "All sections retrieved successfully",
  "data": [ /* SectionSummaryDto[] */ ],
  "searchTerm": "alpha",
  "pagination": { /* pagination info */ }
}
```

*Last updated: 2025-06-16*
