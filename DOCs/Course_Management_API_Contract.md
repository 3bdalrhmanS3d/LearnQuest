# Course Management API Contract

> **Base URL:** `/api/courses`
> **Authentication:** Bearer JWT, roles `Instructor` or `Admin` (some endpoints Admin-only)

---

## 1. List Courses

### `GET /api/courses`

Returns a paginated list of courses, scoped by role:

* **Admin**: can view all courses; may filter by instructor, term, active status
* **Instructor**: sees only their own courses

#### Query Parameters

| Name           | Type   | Required | Default | Description                                         |
| -------------- | ------ | -------- | ------- | --------------------------------------------------- |
| `pageNumber`   | int    | No       | 1       | Page index (1-based)                                |
| `pageSize`     | int    | No       | 10      | Items per page (max 50)                             |
| `searchTerm`   | string | No       | —       | Case-insensitive search against name/description    |
| `isActive`     | bool   | No       | —       | Filter by `IsActive` flag                           |
| `instructorId` | int    | No       | —       | **Admin only**: view courses of specific instructor |

#### Success (200)

```json
{
  "message": "Courses retrieved successfully",
  "data": [
    {
      "courseId": 123,
      "courseName": "Intro to C#",
      "courseImage": "/uploads/courses/…jpg",
      "coursePrice": 49.99,
      "isActive": true,
      "createdAt": "2025-06-01T12:34:56Z",
      "instructorName": "Alice Smith",
      "enrollmentCount": 42,
      "averageRating": 4.5,
      "reviewCount": 10,
      "levelsCount": 5,
      "sectionsCount": 12,
      "contentsCount": 48
    },
    // …
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "hasMore": true
  }
}
```

#### Errors

* **401 Unauthorized**

  ```json
  { "message": "Invalid or missing token." }
  ```
* **500 Internal Server Error**

  ```json
  { "message": "An unexpected error occurred while retrieving courses." }
  ```

---

## 2. Get Course Overview

### `GET /api/courses/{courseId}/overview`

Comprehensive statistics for one course.

#### Path Parameters

| Name       | Type | Required | Description       |
| ---------- | ---- | -------- | ----------------- |
| `courseId` | int  | Yes      | Course identifier |

#### Success (200)

```json
{
  "message": "Course overview retrieved successfully",
  "data": {
    "courseId": 123,
    "courseName": "Intro to C#",
    "description": "Learn the basics of C#",
    "courseImage": "/uploads/courses/…jpg",
    "coursePrice": 49.99,
    "isActive": true,
    "createdAt": "2025-06-01T12:34:56Z",
    "instructorName": "Alice Smith",
    "instructorId": 42,
    "enrollmentCount": 42,
    "activeEnrollmentCount": 5,
    "completedEnrollmentCount": 30,
    "totalRevenue": 2099.58,
    "levelsCount": 5,
    "sectionsCount": 12,
    "contentsCount": 48,
    "quizzesCount": 10,
    "totalDurationMinutes": 720,
    "averageRating": 4.5,
    "reviewCount": 10,
    "reviewSummary": {
      "fiveStar": 6,
      "fourStar": 3,
      "threeStar": 1,
      "twoStar": 0,
      "oneStar": 0
    },
    "recentEnrollments": 3,
    "recentCompletions": 2,
    "recentRevenue": 199.98
  }
}
```

#### Errors

* **401 Unauthorized**
* **404 Not Found** (invalid `courseId`)
* **500 Internal Server Error**

---

## 3. Get Course Details

### `GET /api/courses/{courseId}/details`

Returns the content structure (levels, sections, skills, about items).

#### Path Parameters

| Name       | Type | Required | Description       |
| ---------- | ---- | -------- | ----------------- |
| `courseId` | int  | Yes      | Course identifier |

#### Success (200)

```json
{
  "message": "Course details retrieved successfully",
  "data": {
    "courseId": 123,
    "courseName": "Intro to C#",
    "description": "Learn the basics of C#",
    "courseImage": "/uploads/courses/…jpg",
    "coursePrice": 49.99,
    "isActive": true,
    "createdAt": "2025-06-01T12:34:56Z",
    "instructorName": "Alice Smith",
    "instructorId": 42,
    "aboutCourses": [
      { "aboutCourseId": 1, "aboutCourseText": "You will learn …", "outcomeType": "Learn" },
      // …
    ],
    "courseSkills": [ "C#", ".NET", "OOP" ],
    "levels": [
      {
        "levelId": 10,
        "levelName": "Basics",
        "levelOrder": 1,
        "sectionsCount": 3,
        "contentsCount": 12,
        "quizzesCount": 2,
        "isVisible": true
      },
      // …
    ]
  }
}
```

#### Errors

* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 4. Get Course Enrollments

### `GET /api/courses/{courseId}/enrollments`

Paginated list of students enrolled.

#### Path Parameters

| Name       | Type | Required | Description       |
| ---------- | ---- | -------- | ----------------- |
| `courseId` | int  | Yes      | Course identifier |

#### Query Parameters

| Name         | Type | Required | Default | Description              |
| ------------ | ---- | -------- | ------- | ------------------------ |
| `pageNumber` | int  | No       | 1       | Page index               |
| `pageSize`   | int  | No       | 20      | Items per page (max 100) |

#### Success (200)

```json
{
  "message": "Course enrollments retrieved successfully",
  "data": [
    {
      "userId": 77,
      "fullName": "Bob Jones",
      "emailAddress": "bob@example.com",
      "enrolledAt": "2025-06-10T09:15:00Z"
    },
    // …
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 20,
    "hasMore": false
  }
}
```

#### Errors

* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 5. Get Course Reviews

### `GET /api/courses/{courseId}/reviews`

Returns ratings breakdown and comments summary.

#### Path Parameters

| Name       | Type | Required | Description       |
| ---------- | ---- | -------- | ----------------- |
| `courseId` | int  | Yes      | Course identifier |

#### Success (200)

```json
{
  "message": "Course reviews retrieved successfully",
  "data": {
    "averageRating": 4.5,
    "reviewCount": 10,
    "ratings": {
      "5": 6,
      "4": 3,
      "3": 1,
      "2": 0,
      "1": 0
    },
    "latestReviews": [
      {
        "userId": 88,
        "fullName": "Carol Lee",
        "rating": 5,
        "comment": "Great course!",
        "createdAt": "2025-06-12T14:20:00Z"
      }
      // …
    ]
  }
}
```

#### Errors

* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 6. Get Course Analytics

### `GET /api/courses/{courseId}/analytics`

Time-series or period metrics (enrollments, revenue, completion).

#### Path Parameters

| Name       | Type | Required | Description       |
| ---------- | ---- | -------- | ----------------- |
| `courseId` | int  | Yes      | Course identifier |

#### Query Parameters

| Name        | Type      | Required | Description                        |
| ----------- | --------- | -------- | ---------------------------------- |
| `startDate` | DateTime? | No       | ISO 8601 start of analytics window |
| `endDate`   | DateTime? | No       | ISO 8601 end of analytics window   |

#### Success (200)

```json
{
  "message": "Course analytics retrieved successfully",
  "data": {
    "timeSeries": [
      {
        "date": "2025-06-01",
        "newEnrollments": 2,
        "revenue": 99.98,
        "completions": 1
      },
      // …
    ],
    "summary": {
      "totalEnrollments": 42,
      "totalRevenue": 2099.58,
      "totalCompletions": 30
    }
  }
}
```

#### Errors

* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 7. Create Course

### `POST /api/courses`

Creates a new course (optionally with image).

#### Request

* **Content-Type:** `multipart/form-data`
* **Body Fields:**

| Name        | Type        | Required | Description                                 |
| ----------- | ----------- | -------- | ------------------------------------------- |
| `input`     | JSON object | Yes      | Course metadata (see **CreateCourseDto**)   |


##### **CreateCourseDto** JSON Schema

```jsonc
{
  "courseName": "string",          // 1–200 chars
  "description": "string",
  "coursePrice": 0.0,              // decimal
  "isActive": true,                // bool
  "CourseImage" : "file",
  "aboutCourseInputs": [           // optional
    {
      "aboutCourseText": "string",
      "outcomeType": "Learn"       // "Learn", "Practice", etc.
    }
  ],
  "courseSkillInputs": [ "C#", ".NET" ]  // optional list of strings
}
```

#### Success (201)

* **Location Header:** URL of the new resource
* **Body:**

```json
{
  "message": "Course created successfully",
  "courseId": 123
}
```

#### Errors

* **400 Bad Request** (validation failure)
* **401 Unauthorized**
* **404 Not Found** (e.g. instructor user missing)
* **500 Internal Server Error**

---

## 8. Update Course

### `PUT /api/courses/{courseId}`

Updates metadata only (no image).

#### Path Parameters

| Name       | Type | Required | Description       |
| ---------- | ---- | -------- | ----------------- |
| `courseId` | int  | Yes      | Course identifier |

#### Request Body (`UpdateCourseDto`)

```json
{
  "courseName": "string",      // optional fields can be null/omitted
  "description": "string",
  "coursePrice": 0.0,
  "isActive": true
}
```

#### Success (200)

```json
{ "message": "Course updated successfully" }
```

#### Errors

* **400 Bad Request**
* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 9. Delete Course (Soft)

### `DELETE /api/courses/{courseId}`

Marks course as deleted.

#### Success (200)

```json
{ "message": "Course deleted successfully" }
```

#### Errors

* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 10. Toggle Course Status

### `PATCH /api/courses/{courseId}/toggle-status`

Switches `IsActive` true⇄false.

#### Success (200)

```json
{ "message": "Course status toggled successfully" }
```

#### Errors

* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 11. Upload Course Image

### `POST /api/courses/{courseId}/upload-image`

Uploads or replaces the course’s cover image.

#### Request

* **Content-Type:** `multipart/form-data`
* **Fields:**

  * `file`: image file (max 5MB, JPG/PNG/GIF/WebP)

#### Success (200)

```json
{
  "message": "Course image uploaded successfully",
  "imageUrl": "/uploads/courses/course_123_....jpg"
}
```

#### Errors

* **400 Bad Request** (no file, invalid type/size)
* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 12. Get Available Skills

### `GET /api/courses/skills`

Lookup for tag-style course skills.

#### Query Parameters

| Name         | Type   | Required | Default | Description          |
| ------------ | ------ | -------- | ------- | -------------------- |
| `searchTerm` | string | No       | —       | Filter by skill name |
| `pageNumber` | int    | No       | 1       |                      |
| `pageSize`   | int    | No       | 50      | Max 100              |

#### Success (200)

```json
{
  "message": "Available skills retrieved successfully",
  "data": [
    "C#",
    ".NET",
    "JavaScript",
    // …
  ]
}
```

#### Errors

* **500 Internal Server Error**

---

## 13. Bulk Course Action (Admin Only)

### `POST /api/courses/bulk-action`

Perform batch operations: activate, deactivate, delete, toggle.

#### Request Body (`BulkCourseActionDto`)

```json
{
  "action": "Activate",          // e.g. "Activate", "Deactivate", "Delete"
  "courseIds": [123, 456, 789]
}
```

#### Success (200)

```json
{
  "message": "Bulk action completed",
  "data": {
    "successCount": 3,
    "failureDetails": [
      { "courseId": 999, "error": "Not found" }
    ]
  }
}
```

#### Errors

* **400 Bad Request**
* **401 Unauthorized**
* **500 Internal Server Error**

---

## 14. Transfer Course Ownership (Admin Only)

### `POST /api/courses/{courseId}/transfer-ownership`

Moves a course to another instructor.

#### Path Parameters

| Name       | Type | Required | Description       |
| ---------- | ---- | -------- | ----------------- |
| `courseId` | int  | Yes      | Course identifier |

#### Request Body

```json
{
  "newInstructorId": 88
}
```

#### Success (200)

```json
{ "message": "Course ownership transferred successfully" }
```

#### Errors

* **400 Bad Request**
* **401 Unauthorized**
* **404 Not Found**
* **500 Internal Server Error**

---

## 15. Search Courses (Admin Only)

### `GET /api/courses/search`

Free-text search across courses.

#### Query Parameters

| Name         | Type   | Required | Default | Description       |
| ------------ | ------ | -------- | ------- | ----------------- |
| `searchTerm` | string | Yes      | —       | Keywords to match |
| `pageNumber` | int    | No       | 1       |                   |
| `pageSize`   | int    | No       | 10      | Max 50            |

#### Success (200)

```json
{
  "message": "Search completed successfully",
  "data": [ /* CourseCDto array */ ],
  "searchTerm": "c#",
  "pagination": { "pageNumber":1,"pageSize":10,"hasMore":false }
}
```

#### Errors

* **400 Bad Request** (`searchTerm` empty)
* **401 Unauthorized**
* **500 Internal Server Error**

---

### Notes for Front-End Team

* All endpoints return JSON with a top-level `message` and either `data` (on success) or `message` (on errors).
* Use HTTP status codes to drive UI flows: **200**, **201**, **400**, **401**, **404**, **500**.
* Endpoints that upload files require `multipart/form-data`.
* Remember role-based behavior: some endpoints restrict to **Admin** only.
* Pagination: `hasMore` indicates whether a next page likely exists.

Feel free to reach out for any clarifications!
