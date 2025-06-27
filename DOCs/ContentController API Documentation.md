# LearnQuest – ContentController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/contents`

---

## 🔐 Authentication

All endpoints require a valid Bearer token with **Instructor** or **Admin** role:

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                        | Method | Description                           | Role              |
| ------------------------------- | ------ | ------------------------------------- | ----------------- |
| `/`                             | POST   | Create new content                    | Instructor, Admin |
| `/{contentId}`                  | PUT    | Update existing content               | Instructor, Admin |
| `/{contentId}`                  | DELETE | Delete content                        | Instructor, Admin |
| `/{contentId}`                  | GET    | Get content details                   | Instructor, Admin |
| `/upload-file`                  | POST   | Upload video/document file            | Instructor, Admin |
| `/upload-multiple-files`        | POST   | Upload multiple files at once         | Instructor, Admin |
| `/reorder`                      | PUT    | Reorder multiple content items        | Instructor, Admin |
| `/{contentId}/toggle-visibility`| PATCH  | Toggle content visibility             | Instructor, Admin |
| `/bulk-visibility-toggle`       | POST   | Bulk toggle visibility                | Instructor, Admin |
| `/section/{sectionId}`          | GET    | Get all contents in section           | Instructor, Admin |
| `/{contentId}/stats`            | GET    | Get content statistics                | Instructor, Admin |
| `/{contentId}/analytics`        | GET    | Get detailed content analytics        | Instructor, Admin |
| `/search`                       | GET    | Search and filter contents            | Instructor, Admin |
| `/my-contents`                  | GET    | Get instructor's contents             | Instructor, Admin |
| `/duplicate/{contentId}`        | POST   | Duplicate existing content            | Instructor, Admin |
| `/bulk-action`                  | POST   | Perform bulk actions                  | Instructor, Admin |

---

## 📝 Core CRUD Operations

### 1. Create Content (`POST /`)

**Description:** Creates a new content item under a given section.

**Request Body:**

```json
{
  "contentTitle": "Introduction to Variables",
  "contentDescription": "Learn about different types of variables in programming",
  "sectionId": 15,
  "contentType": "Video",
  "order": 1,
  "isVisible": true,
  "videoUrl": "/uploads/videos/intro-variables.mp4",
  "videoDurationMinutes": 25,
  "documentUrl": null,
  "documentSize": null,
  "textContent": null,
  "metadata": {
    "difficulty": "Beginner",
    "tags": ["programming", "variables", "basics"]
  }
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content created successfully",
  "data": {
    "contentId": 45,
    "message": "Content 'Introduction to Variables' created successfully"
  }
}
```

**Errors:**
- `400 Bad Request` - Invalid input data
- `401 Unauthorized` - Invalid or missing token
- `404 Not Found` - Section not found or not accessible
- `403 Forbidden` - User doesn't own the section

---

### 2. Update Content (`PUT /{contentId}`)

**Path Parameter:** `contentId` (integer)

**Request Body:**

```json
{
  "contentId": 45,
  "contentTitle": "Advanced Variable Concepts",
  "contentDescription": "Deep dive into variable scope and memory management",
  "sectionId": 15,
  "contentType": "Video",
  "order": 1,
  "isVisible": true,
  "videoUrl": "/uploads/videos/advanced-variables.mp4",
  "videoDurationMinutes": 35
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content updated successfully",
  "data": null
}
```

---

### 3. Delete Content (`DELETE /{contentId}`)

**Path Parameter:** `contentId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content deleted successfully",
  "data": null
}
```

**Errors:**
- `404 Not Found` - Content not found
- `403 Forbidden` - User doesn't own the content

---

### 4. Get Content Details (`GET /{contentId}`)

**Path Parameter:** `contentId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content details retrieved successfully",
  "data": {
    "contentId": 45,
    "contentTitle": "Introduction to Variables",
    "contentDescription": "Learn about different types of variables",
    "contentType": "Video",
    "sectionId": 15,
    "sectionName": "Programming Fundamentals",
    "order": 1,
    "isVisible": true,
    "videoUrl": "/uploads/videos/intro-variables.mp4",
    "videoDurationMinutes": 25,
    "documentUrl": null,
    "textContent": null,
    "createdAt": "2025-06-15T10:00:00Z",
    "updatedAt": "2025-06-15T14:30:00Z",
    "viewCount": 127,
    "completionRate": 0.85,
    "metadata": {
      "difficulty": "Beginner",
      "tags": ["programming", "variables"]
    }
  }
}
```

---

## 📁 File Management

### 5. Upload File (`POST /upload-file`)

**Description:** Uploads a video or document file to the server.

**Content-Type:** `multipart/form-data`

**Form Data:**
- `file` (IFormFile) - File to upload
- `type` (ContentType) - "Video" or "Doc"

**Success Response (200):**

```json
{
  "success": true,
  "message": "File uploaded successfully",
  "data": {
    "url": "/uploads/videos/content_20250615_143022.mp4",
    "fileName": "intro-variables.mp4",
    "contentType": "Video",
    "fileSize": 157680640,
    "duration": 1580
  }
}
```

**Errors:**
- `400 Bad Request` - No file provided or invalid content type
- `413 Payload Too Large` - File size exceeds limit
- `415 Unsupported Media Type` - Invalid file format

---

### 6. Upload Multiple Files (`POST /upload-multiple-files`)

**Description:** Uploads multiple files at once for bulk content creation.

**Content-Type:** `multipart/form-data`

**Form Data:**
- `files` (IFormFile[]) - Array of files to upload
- `type` (ContentType) - "Video" or "Doc"

**Success Response (200):**

```json
{
  "success": true,
  "message": "Files uploaded successfully",
  "data": {
    "successfulUploads": 3,
    "failedUploads": 0,
    "results": [
      {
        "fileName": "lesson1.mp4",
        "url": "/uploads/videos/lesson1_20250615.mp4",
        "success": true
      },
      {
        "fileName": "lesson2.mp4", 
        "url": "/uploads/videos/lesson2_20250615.mp4",
        "success": true
      }
    ]
  }
}
```

---

## 🔄 Content Organization

### 7. Reorder Contents (`PUT /reorder`)

**Description:** Reorders multiple content items in one call.

**Request Body:**

```json
[
  {
    "contentId": 45,
    "newOrder": 3
  },
  {
    "contentId": 46,
    "newOrder": 1
  },
  {
    "contentId": 47,
    "newOrder": 2
  }
]
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Contents reordered successfully",
  "data": null
}
```

---

### 8. Toggle Content Visibility (`PATCH /{contentId}/toggle-visibility`)

**Path Parameter:** `contentId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content visibility toggled successfully",
  "data": {
    "contentId": 45,
    "isNowVisible": false
  }
}
```

---

### 9. Bulk Visibility Toggle (`POST /bulk-visibility-toggle`)

**Description:** Toggle visibility for multiple content items.

**Request Body:**

```json
{
  "contentIds": [45, 46, 47],
  "setVisible": false
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Bulk visibility toggle completed",
  "data": {
    "successfulActions": 3,
    "failedActions": 0,
    "results": [
      {
        "contentId": 45,
        "success": true,
        "newVisibility": false
      }
    ]
  }
}
```

---

## 📋 Content Retrieval

### 10. Get Section Contents (`GET /section/{sectionId}`)

**Path Parameter:** `sectionId` (integer)

**Query Parameters:**
- `includeHidden` (boolean, optional) - Include hidden content (default: false)
- `sortBy` (string, optional) - Sort field: "order", "title", "createdAt"
- `sortDirection` (string, optional) - "asc" or "desc" (default: "asc")

**Success Response (200):**

```json
{
  "success": true,
  "message": "Section contents retrieved successfully",
  "data": {
    "sectionId": 15,
    "sectionName": "Programming Fundamentals",
    "contentCount": 5,
    "contents": [
      {
        "contentId": 45,
        "contentTitle": "Introduction to Variables",
        "contentType": "Video",
        "order": 1,
        "isVisible": true,
        "videoDurationMinutes": 25,
        "viewCount": 127,
        "createdAt": "2025-06-15T10:00:00Z"
      }
    ]
  }
}
```

---

## 📊 Analytics and Statistics

### 11. Get Content Statistics (`GET /{contentId}/stats`)

**Path Parameter:** `contentId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content statistics retrieved successfully",
  "data": {
    "contentId": 45,
    "contentTitle": "Introduction to Variables",
    "totalViews": 247,
    "uniqueViewers": 189,
    "completionRate": 0.76,
    "averageWatchTime": 18.5,
    "likes": 34,
    "dislikes": 2,
    "comments": 12,
    "lastViewedAt": "2025-06-15T16:45:00Z",
    "viewsByDay": [
      {
        "date": "2025-06-10",
        "views": 23
      }
    ],
    "deviceBreakdown": {
      "desktop": 156,
      "mobile": 78,
      "tablet": 13
    }
  }
}
```

---

### 12. Get Content Analytics (`GET /{contentId}/analytics`)

**Path Parameter:** `contentId` (integer)

**Query Parameters:**
- `startDate` (string, optional) - Start date for analytics (ISO format)
- `endDate` (string, optional) - End date for analytics (ISO format)
- `granularity` (string, optional) - "day", "week", "month" (default: "day")

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content analytics retrieved successfully",
  "data": {
    "contentId": 45,
    "period": {
      "startDate": "2025-06-01T00:00:00Z",
      "endDate": "2025-06-15T23:59:59Z"
    },
    "summary": {
      "totalViews": 247,
      "uniqueViewers": 189,
      "averageEngagement": 0.68,
      "completionRate": 0.76
    },
    "viewsOverTime": [
      {
        "date": "2025-06-01",
        "views": 15,
        "uniqueViewers": 12
      }
    ],
    "engagementMetrics": {
      "averageWatchPercentage": 68.5,
      "dropOffPoints": [
        {
          "timeInSeconds": 180,
          "dropOffPercentage": 0.15
        }
      ]
    }
  }
}
```

---

## 🔍 Search and Filtering

### 13. Search Contents (`GET /search`)

**Query Parameters:**
- `searchTerm` (string, required) - Search keywords
- `sectionId` (integer, optional) - Filter by section
- `contentType` (string, optional) - Filter by type: "Video", "Doc", "Text"
- `isVisible` (boolean, optional) - Filter by visibility
- `pageNumber` (integer, optional) - Page number (default: 1)
- `pageSize` (integer, optional) - Page size (default: 20, max: 50)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content search completed successfully",
  "data": {
    "searchTerm": "variables",
    "totalResults": 8,
    "contents": [
      {
        "contentId": 45,
        "contentTitle": "Introduction to Variables",
        "contentDescription": "Learn about different types of variables",
        "contentType": "Video",
        "sectionName": "Programming Fundamentals",
        "order": 1,
        "isVisible": true,
        "createdAt": "2025-06-15T10:00:00Z"
      }
    ],
    "pagination": {
      "pageNumber": 1,
      "pageSize": 20,
      "hasMore": false
    }
  }
}
```

---

### 14. Get My Contents (`GET /my-contents`)

**Description:** Get content items created by the current instructor.

**Query Parameters:**
- `pageNumber` (integer, optional) - Page number (default: 1)
- `pageSize` (integer, optional) - Page size (default: 20)
- `contentType` (string, optional) - Filter by type
- `sortBy` (string, optional) - Sort field
- `sortDirection` (string, optional) - "asc" or "desc"

**Success Response (200):**

```json
{
  "success": true,
  "message": "Instructor contents retrieved successfully",
  "data": {
    "totalContents": 127,
    "contents": [
      {
        "contentId": 45,
        "contentTitle": "Introduction to Variables",
        "contentType": "Video",
        "sectionName": "Programming Fundamentals",
        "courseName": "C# Programming",
        "isVisible": true,
        "viewCount": 247,
        "createdAt": "2025-06-15T10:00:00Z"
      }
    ],
    "pagination": {
      "pageNumber": 1,
      "pageSize": 20,
      "hasMore": true
    }
  }
}
```

---

## 🔄 Advanced Operations

### 15. Duplicate Content (`POST /duplicate/{contentId}`)

**Path Parameter:** `contentId` (integer)

**Request Body:**

```json
{
  "newTitle": "Introduction to Variables (Copy)",
  "targetSectionId": 16,
  "copyFiles": true
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content duplicated successfully",
  "data": {
    "originalContentId": 45,
    "newContentId": 78,
    "newTitle": "Introduction to Variables (Copy)"
  }
}
```

---

### 16. Bulk Action (`POST /bulk-action`)

**Description:** Perform bulk operations on multiple content items.

**Request Body:**

```json
{
  "action": "Delete",
  "contentIds": [45, 46, 47],
  "parameters": {
    "confirm": true
  }
}
```

**Available Actions:** `Delete`, `Hide`, `Show`, `ChangeSection`, `UpdateOrder`

**Success Response (200):**

```json
{
  "success": true,
  "message": "Bulk action completed",
  "data": {
    "action": "Delete",
    "successCount": 3,
    "failedCount": 0,
    "results": [
      {
        "contentId": 45,
        "success": true,
        "message": "Content deleted successfully"
      }
    ]
  }
}
```

---

## 🔧 Common Error Responses

- **401 Unauthorized** - Invalid or missing token
- **403 Forbidden** - Insufficient permissions or not content owner
- **404 Not Found** - Content, section, or resource not found
- **400 Bad Request** - Validation errors or invalid input data
- **413 Payload Too Large** - File size exceeds maximum limit
- **415 Unsupported Media Type** - Invalid file format
- **429 Too Many Requests** - Rate limit exceeded
- **500 Internal Server Error** - Unexpected server errors

---

## 📝 Notes for Frontend Team

- All file upload endpoints support progress tracking via XMLHttpRequest
- Content files are stored in `/wwwroot/uploads/` with organized subdirectories
- Video files support common formats: MP4, WebM, AVI (max 500MB)
- Document files support: PDF, DOC, DOCX, PPT, PPTX (max 100MB)
- All timestamps are in UTC format
- File URLs are relative and should be prefixed with the base URL
- Content ordering is 1-based (1, 2, 3, ...)
- Visibility changes affect student access immediately
- Search functionality supports partial text matching and filtering

---

*Last updated: 2025-06-27*