# LearnQuest – ProgressController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/progress`

---

## 🔐 Authentication

All endpoints require a valid Bearer token with **RegularUser**, **Instructor**, or **Admin** role:

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                              | Method | Description                           | Role                      |
| ------------------------------------- | ------ | ------------------------------------- | ------------------------- |
| `/user-stats`                         | GET    | Get user overall progress statistics  | RegularUser, Instructor, Admin |
| `/course-completion/{courseId}`       | GET    | Check if user completed course        | RegularUser, Instructor, Admin |
| `/start-content/{contentId}`          | POST   | Start tracking content progress       | RegularUser, Instructor, Admin |
| `/complete-content/{contentId}`       | POST   | Mark content as completed             | RegularUser, Instructor, Admin |
| `/complete-section/{sectionId}`       | POST   | Complete section and get next step    | RegularUser, Instructor, Admin |
| `/course-progress/{courseId}`         | GET    | Get detailed course progress          | RegularUser, Instructor, Admin |
| `/section-progress/{sectionId}`       | GET    | Get section completion progress       | RegularUser, Instructor, Admin |
| `/content-progress/{contentId}`       | GET    | Get content viewing progress          | RegularUser, Instructor, Admin |
| `/learning-path/{userId}`             | GET    | Get user's learning path              | RegularUser, Instructor, Admin |
| `/achievements/{userId}`              | GET    | Get user achievements and milestones  | RegularUser, Instructor, Admin |
| `/weekly-activity/{userId}`           | GET    | Get weekly learning activity          | RegularUser, Instructor, Admin |

---

## 📊 User Progress Statistics

### 1. Get User Stats (`GET /user-stats`)

**Description:** Get comprehensive progress statistics for the current user.

**Success Response (200):**

```json
{
  "success": true,
  "message": "User statistics retrieved successfully",
  "data": {
    "userId": 123,
    "userName": "John Doe",
    "profilePhoto": "/uploads/profiles/user_123.jpg",
    "overallProgress": {
      "totalCoursesEnrolled": 8,
      "coursesCompleted": 3,
      "coursesInProgress": 4,
      "coursesNotStarted": 1,
      "totalContentViewed": 247,
      "totalTimeSpent": 1840,
      "averageCompletionRate": 0.68
    },
    "currentStreak": {
      "days": 12,
      "startDate": "2025-06-01T00:00:00Z",
      "lastActivityDate": "2025-06-27T15:30:00Z"
    },
    "weeklyStats": {
      "hoursThisWeek": 14.5,
      "contentCompletedThisWeek": 23,
      "quizzesPassedThisWeek": 5
    },
    "achievements": [
      {
        "achievementId": 1,
        "title": "First Course Completed",
        "description": "Completed your first course",
        "earnedAt": "2025-06-15T10:00:00Z",
        "badgeIcon": "/icons/first-course.svg"
      }
    ],
    "recentActivity": [
      {
        "activityType": "ContentCompleted",
        "contentTitle": "Introduction to Variables",
        "courseName": "C# Programming",
        "completedAt": "2025-06-27T14:30:00Z"
      }
    ]
  }
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `500 Internal Server Error` - Server error

---

## 🎓 Course Completion Tracking

### 2. Check Course Completion (`GET /course-completion/{courseId}`)

**Path Parameter:** `courseId` (integer)

**Description:** Check if the current user has completed a specific course.

**Success Response (200):**

```json
{
  "success": true,
  "message": "Course completion status retrieved successfully",
  "data": {
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "isCompleted": true,
    "completionDate": "2025-06-20T16:45:00Z",
    "completionPercentage": 100,
    "timeSpent": 2847,
    "certificateUrl": "/certificates/user_123_course_15.pdf",
    "finalGrade": 87.5,
    "passGrade": 70,
    "progressSummary": {
      "totalLevels": 6,
      "completedLevels": 6,
      "totalSections": 24,
      "completedSections": 24,
      "totalContent": 156,
      "completedContent": 156,
      "totalQuizzes": 12,
      "passedQuizzes": 11
    }
  }
}
```

**Errors:**
- `404 Not Found` - Course not found
- `401 Unauthorized` - Invalid token

---

## 📹 Content Progress Tracking

### 3. Start Content (`POST /start-content/{contentId}`)

**Path Parameter:** `contentId` (integer)

**Description:** Mark the beginning of content consumption for progress tracking.

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content started successfully",
  "data": {
    "contentId": 67,
    "contentTitle": "Object-Oriented Programming",
    "startedAt": "2025-06-27T15:00:00Z",
    "estimatedDuration": 25,
    "prerequisitesMet": true,
    "nextContentId": 68
  }
}
```

**Errors:**
- `400 Bad Request` - Prerequisites not met or already started
- `404 Not Found` - Content not found

---

### 4. Complete Content (`POST /complete-content/{contentId}`)

**Path Parameter:** `contentId` (integer)

**Request Body:**

```json
{
  "timeSpent": 1847,
  "completionPercentage": 95,
  "rating": 4,
  "notes": "Great explanation of OOP concepts"
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content marked as completed successfully",
  "data": {
    "contentId": 67,
    "completedAt": "2025-06-27T15:30:00Z",
    "timeSpent": 1847,
    "pointsEarned": 15,
    "sectionProgress": {
      "sectionId": 12,
      "completionPercentage": 75,
      "remainingContent": 2
    },
    "nextRecommendation": {
      "contentId": 68,
      "contentTitle": "Inheritance and Polymorphism",
      "estimatedDuration": 30
    }
  }
}
```

---

## 📚 Section Progress

### 5. Complete Section (`POST /complete-section/{sectionId}`)

**Path Parameter:** `sectionId` (integer)

**Description:** Mark a section as completed and get the next learning step.

**Success Response (200):**

```json
{
  "success": true,
  "message": "Section completed successfully",
  "data": {
    "sectionId": 12,
    "sectionName": "OOP Fundamentals",
    "completedAt": "2025-06-27T15:45:00Z",
    "pointsEarned": 50,
    "certificateEarned": false,
    "levelProgress": {
      "levelId": 4,
      "levelName": "Advanced Programming",
      "completionPercentage": 60,
      "remainingSections": 3
    },
    "nextStep": {
      "type": "Quiz",
      "entityId": 23,
      "title": "OOP Concepts Quiz",
      "description": "Test your understanding of object-oriented programming",
      "isOptional": false
    },
    "achievements": [
      {
        "achievementId": 5,
        "title": "OOP Master",
        "description": "Completed all OOP fundamentals",
        "earnedAt": "2025-06-27T15:45:00Z"
      }
    ]
  }
}
```

---

### 6. Get Section Progress (`GET /section-progress/{sectionId}`)

**Path Parameter:** `sectionId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Section progress retrieved successfully",
  "data": {
    "sectionId": 12,
    "sectionName": "OOP Fundamentals",
    "completionPercentage": 75,
    "isCompleted": false,
    "startedAt": "2025-06-25T10:00:00Z",
    "estimatedCompletionDate": "2025-06-28T16:00:00Z",
    "contentProgress": [
      {
        "contentId": 65,
        "contentTitle": "Introduction to Classes",
        "contentType": "Video",
        "isCompleted": true,
        "completedAt": "2025-06-25T10:30:00Z",
        "timeSpent": 1520,
        "rating": 5
      },
      {
        "contentId": 66,
        "contentTitle": "Constructors and Methods",
        "contentType": "Video",
        "isCompleted": true,
        "completedAt": "2025-06-26T14:15:00Z",
        "timeSpent": 1840,
        "rating": 4
      },
      {
        "contentId": 67,
        "contentTitle": "Object-Oriented Programming",
        "contentType": "Video",
        "isCompleted": false,
        "currentProgress": 45,
        "timeSpent": 890
      }
    ],
    "quizzes": [
      {
        "quizId": 23,
        "quizTitle": "OOP Concepts Quiz",
        "isCompleted": false,
        "isRequired": true,
        "passingScore": 70
      }
    ]
  }
}
```

---

## 🎯 Detailed Progress Views

### 7. Get Course Progress (`GET /course-progress/{courseId}`)

**Path Parameter:** `courseId` (integer)

**Query Parameters:**
- `includeDetails` (boolean, optional) - Include detailed breakdown (default: true)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Course progress retrieved successfully",
  "data": {
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "courseImage": "/uploads/courses/csharp_fundamentals.jpg",
    "overallProgress": {
      "completionPercentage": 68,
      "isCompleted": false,
      "enrolledAt": "2025-06-01T09:00:00Z",
      "lastActivityAt": "2025-06-27T15:30:00Z",
      "totalTimeSpent": 14570,
      "estimatedTimeRemaining": 6840
    },
    "levelProgress": [
      {
        "levelId": 1,
        "levelName": "C# Basics",
        "order": 1,
        "completionPercentage": 100,
        "isCompleted": true,
        "completedAt": "2025-06-10T16:00:00Z",
        "sectionsCount": 4,
        "completedSections": 4
      },
      {
        "levelId": 2,
        "levelName": "Control Structures",
        "order": 2,
        "completionPercentage": 75,
        "isCompleted": false,
        "sectionsCount": 6,
        "completedSections": 4,
        "currentSection": {
          "sectionId": 12,
          "sectionName": "Loops and Iterations",
          "progress": 45
        }
      }
    ],
    "statistics": {
      "totalContent": 156,
      "completedContent": 106,
      "totalQuizzes": 12,
      "completedQuizzes": 8,
      "passedQuizzes": 7,
      "averageQuizScore": 82.5,
      "pointsEarned": 1240,
      "achievementsUnlocked": 5
    },
    "nextRecommendation": {
      "type": "Content",
      "entityId": 67,
      "title": "Object-Oriented Programming",
      "description": "Learn the fundamentals of OOP in C#"
    }
  }
}
```

---

### 8. Get Content Progress (`GET /content-progress/{contentId}`)

**Path Parameter:** `contentId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Content progress retrieved successfully",
  "data": {
    "contentId": 67,
    "contentTitle": "Object-Oriented Programming",
    "contentType": "Video",
    "sectionName": "OOP Fundamentals",
    "courseName": "C# Programming",
    "progress": {
      "isStarted": true,
      "isCompleted": false,
      "startedAt": "2025-06-27T15:00:00Z",
      "completionPercentage": 45,
      "timeSpent": 890,
      "totalDuration": 1500,
      "lastPosition": 675,
      "viewCount": 3
    },
    "interactions": {
      "rating": null,
      "isFavorited": false,
      "notes": "",
      "bookmarks": [
        {
          "timePosition": 320,
          "note": "Important concept about classes",
          "createdAt": "2025-06-27T15:10:00Z"
        }
      ]
    },
    "prerequisites": {
      "allMet": true,
      "required": [
        {
          "contentId": 65,
          "title": "Introduction to Classes",
          "isCompleted": true
        }
      ]
    }
  }
}
```

---

## 🛤️ Learning Path & Analytics

### 9. Get Learning Path (`GET /learning-path/{userId}`)

**Path Parameter:** `userId` (integer)

**Query Parameters:**
- `includeCompleted` (boolean, optional) - Include completed items (default: false)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Learning path retrieved successfully",
  "data": {
    "userId": 123,
    "generatedAt": "2025-06-27T16:00:00Z",
    "currentFocus": {
      "courseId": 15,
      "courseName": "C# Programming",
      "levelId": 2,
      "levelName": "Control Structures",
      "sectionId": 12,
      "sectionName": "OOP Fundamentals",
      "nextContentId": 67,
      "nextContentTitle": "Object-Oriented Programming"
    },
    "recommendedPath": [
      {
        "step": 1,
        "type": "Content",
        "entityId": 67,
        "title": "Object-Oriented Programming",
        "estimatedDuration": 25,
        "difficulty": "Intermediate",
        "priority": "High"
      },
      {
        "step": 2,
        "type": "Quiz",
        "entityId": 23,
        "title": "OOP Concepts Quiz",
        "estimatedDuration": 15,
        "difficulty": "Intermediate",
        "priority": "Medium"
      }
    ],
    "upcomingDeadlines": [
      {
        "type": "Assignment",
        "title": "Final Project Submission",
        "courseId": 15,
        "dueDate": "2025-07-15T23:59:00Z",
        "daysRemaining": 18
      }
    ],
    "learningInsights": {
      "preferredLearningTime": "14:00-16:00",
      "averageSessionDuration": 45,
      "strongestTopics": ["Variables", "Conditions"],
      "improvementAreas": ["Loops", "Functions"]
    }
  }
}
```

---

### 10. Get Achievements (`GET /achievements/{userId}`)

**Path Parameter:** `userId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "User achievements retrieved successfully",
  "data": {
    "userId": 123,
    "totalAchievements": 12,
    "totalPoints": 2450,
    "currentLevel": 5,
    "nextLevelPoints": 3000,
    "achievements": [
      {
        "achievementId": 1,
        "title": "First Steps",
        "description": "Started your first course",
        "category": "Getting Started",
        "points": 50,
        "badgeIcon": "/icons/first-steps.svg",
        "isUnlocked": true,
        "unlockedAt": "2025-06-01T09:30:00Z"
      },
      {
        "achievementId": 2,
        "title": "Quick Learner",
        "description": "Complete 5 contents in one day",
        "category": "Learning Speed",
        "points": 100,
        "badgeIcon": "/icons/quick-learner.svg",
        "isUnlocked": true,
        "unlockedAt": "2025-06-15T18:45:00Z"
      },
      {
        "achievementId": 3,
        "title": "Quiz Master",
        "description": "Pass 10 quizzes with 90%+ score",
        "category": "Excellence",
        "points": 200,
        "badgeIcon": "/icons/quiz-master.svg",
        "isUnlocked": false,
        "progress": {
          "current": 7,
          "required": 10
        }
      }
    ],
    "recentlyUnlocked": [
      {
        "achievementId": 5,
        "title": "Consistency Champion",
        "unlockedAt": "2025-06-27T10:00:00Z"
      }
    ]
  }
}
```

---

### 11. Get Weekly Activity (`GET /weekly-activity/{userId}`)

**Path Parameter:** `userId` (integer)

**Query Parameters:**
- `startDate` (string, optional) - Start date (ISO format)
- `weeks` (integer, optional) - Number of weeks to retrieve (default: 4)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Weekly activity retrieved successfully",
  "data": {
    "userId": 123,
    "period": {
      "startDate": "2025-06-01T00:00:00Z",
      "endDate": "2025-06-27T23:59:59Z",
      "totalWeeks": 4
    },
    "weeklyData": [
      {
        "weekNumber": 26,
        "startDate": "2025-06-23T00:00:00Z",
        "endDate": "2025-06-29T23:59:59Z",
        "statistics": {
          "totalTimeSpent": 875,
          "contentCompleted": 12,
          "quizzesTaken": 3,
          "quizzesPassed": 2,
          "averageSessionDuration": 58,
          "activeDays": 5,
          "pointsEarned": 180
        },
        "dailyActivity": [
          {
            "date": "2025-06-23",
            "timeSpent": 0,
            "contentCompleted": 0,
            "isActive": false
          },
          {
            "date": "2025-06-24",
            "timeSpent": 125,
            "contentCompleted": 2,
            "isActive": true
          }
        ]
      }
    ],
    "trends": {
      "timeSpentTrend": "increasing",
      "contentCompletionTrend": "stable",
      "consistencyScore": 0.78,
      "weekOverWeekGrowth": 0.15
    }
  }
}
```

---

## 🔧 Common Error Responses

- **401 Unauthorized** - Invalid or missing token
- **403 Forbidden** - Insufficient permissions
- **404 Not Found** - Resource not found (course, content, section, user)
- **400 Bad Request** - Invalid input data or business rule violation
- **500 Internal Server Error** - Unexpected server errors

---

## 📝 Notes for Frontend Team

- All timestamps are in UTC format
- Progress percentages are decimal values (0.0 to 1.0)
- Time spent is in seconds
- Points are integer values
- User can only access their own progress data unless they have Instructor/Admin role
- Content progress is automatically saved every 30 seconds during viewing
- Section completion requires all mandatory content to be finished
- Course completion requires all levels and mandatory quizzes to be completed
- Achievements are calculated in real-time and cached for performance

---

*Last updated: 2025-06-27*