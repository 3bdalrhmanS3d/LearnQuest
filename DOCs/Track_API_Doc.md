# LearnQuest – TrackController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/track`

---

## 🔐 Authentication

All endpoints require a valid Bearer token with **Admin** role:

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                            | Method | Description                           | Role  |
| ----------------------------------- | ------ | ------------------------------------- | ----- |
| `/create`                           | POST   | Create a new track                    | Admin |
| `/upload-image/{trackId}`           | POST   | Upload or replace track image         | Admin |
| `/update`                           | PUT    | Update track name/description         | Admin |
| `/delete/{trackId}`                 | DELETE | Delete a track and its associations   | Admin |
| `/add-course`                       | POST   | Add a course to a track               | Admin |
| `/remove-course?trackId=&courseId=` | DELETE | Remove a course from a track          | Admin |
| `/all`                              | GET    | Retrieve all tracks                   | Admin |
| `/details/{trackId}`                | GET    | Retrieve details for a specific track | Admin |

---

### 1. Create Track (`POST /create`)

**Description:** Creates a new track.

**Request Body:**

```json
{
  "trackName": "Full-Stack Development",
  "description": "A curated path covering frontend and backend skills",
  "courseIds": [5, 8, 12]
}
```

**Success Response (200):**

```json
{
  "message": "Track created successfully",
  "trackId": 3
}
```

**Errors:**

* `400 Bad Request` for validation failures
* `401 Unauthorized` if missing/invalid token
* `404 Not Found` if any `courseIds` do not exist

---

### 2. Upload Track Image (`POST /upload-image/{trackId}`)

**Description:** Uploads or replaces the image associated with a track.

**Path Parameter:**

* `trackId` (integer) – ID of the track

**Form Data:**

* `file` (IFormFile) – image file to upload

**Success Response (200):**

```json
{
  "message": "Track image uploaded successfully"
}
```

---

### 3. Update Track (`PUT /update`)

**Description:** Updates name or description of an existing track.

**Request Body:**

```json
{
  "trackId": 3,
  "trackName": "Advanced Full-Stack",
  "description": "Includes DevOps and testing modules"
}
```

**Success Response (200):**

```json
{
  "message": "Track updated successfully"
}
```

---

### 4. Delete Track (`DELETE /delete/{trackId}`)

**Description:** Deletes a track and removes all its course associations.

**Path Parameter:**

* `trackId` (integer) – ID of the track

**Success Response (200):**

```json
{
  "message": "Track deleted successfully"
}
```

---

### 5. Add Course to Track (`POST /add-course`)

**Description:** Adds an existing course to a track.

**Request Body:**

```json
{
  "trackId": 3,
  "courseId": 15
}
```

**Success Response (200):**

```json
{
  "message": "Course added to track successfully"
}
```

---

### 6. Remove Course from Track (`DELETE /remove-course?trackId={trackId}&courseId={courseId}`)

**Description:** Removes a course from a track.

**Query Parameters:**

* `trackId` (integer)
* `courseId` (integer)

**Success Response (200):**

```json
{
  "message": "Course removed from track successfully"
}
```

---

### 7. Get All Tracks (`GET /all`)

**Description:** Retrieves all tracks in the system.

**Success Response (200):**

```json
{
  "message": "Tracks retrieved successfully",
  "data": [
    { "trackId": 1, "trackName": "Data Science", "description": "…" },
    { "trackId": 2, "trackName": "Web Development", "description": "…" }
  ],
  "count": 2
}
```

**Note:** Returns an empty list with count 0 if no tracks exist.

---

### 8. Get Track Details (`GET /details/{trackId}`)

**Description:** Retrieves full information for a single track, including its courses.

**Path Parameter:**

* `trackId` (integer)

**Success Response (200):**

```json
{
  "message": "Track details retrieved successfully",
  "data": {
    "trackId": 3,
    "trackName": "Full-Stack Development",
    "description": "…",
    "courses": [
      { "courseId": 5, "title": "HTML & CSS Basics" },
      { "courseId": 8, "title": "ASP.NET Core Fundamentals" }
    ]
  }
}
```

**Errors:**

* `404 Not Found` if the track does not exist

*Last updated: 2025-06-17*
