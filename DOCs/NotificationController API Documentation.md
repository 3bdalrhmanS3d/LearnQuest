# LearnQuest – NotificationController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/notification`

---

## 🔐 Authentication

All endpoints require a valid Bearer token:

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                          | Method | Description                         | Role                      |
| --------------------------------- | ------ | ----------------------------------- | ------------------------- |
| `/`                               | GET    | Get paginated notifications         | All authenticated users   |
| `/{notificationId}`               | GET    | Get specific notification details   | All authenticated users   |
| `/{notificationId}/mark-read`     | POST   | Mark notification as read           | All authenticated users   |
| `/{notificationId}/mark-unread`   | POST   | Mark notification as unread         | All authenticated users   |
| `/mark-all-read`                  | POST   | Mark all notifications as read      | All authenticated users   |
| `/bulk-action`                    | POST   | Perform bulk actions                | All authenticated users   |
| `/unread-count`                   | GET    | Get unread notifications count      | All authenticated users   |
| `/preferences`                    | GET    | Get notification preferences        | All authenticated users   |
| `/preferences`                    | PUT    | Update notification preferences     | All authenticated users   |
| `/subscribe`                      | POST   | Subscribe to notification types     | All authenticated users   |
| `/unsubscribe`                    | POST   | Unsubscribe from notification types | All authenticated users   |
| `/send`                           | POST   | Send notification to user           | Admin, Instructor         |
| `/broadcast`                      | POST   | Send broadcast notification         | Admin only                |
| `/templates`                      | GET    | Get notification templates          | Admin, Instructor         |

---

## 📱 Core Notification Operations

### 1. Get Notifications (`GET /`)

**Description:** Get paginated notifications for the current user with filtering options.

**Query Parameters:**
- `pageNumber` (integer, optional) - Page number (default: 1)
- `pageSize` (integer, optional) - Page size (default: 20, max: 50)
- `isRead` (boolean, optional) - Filter by read status
- `type` (string, optional) - Filter by notification type
- `priority` (string, optional) - Filter by priority level
- `fromDate` (string, optional) - Filter from date (ISO format)
- `toDate` (string, optional) - Filter to date (ISO format)
- `courseId` (integer, optional) - Filter by course ID

**Success Response (200):**

```json
{
  "success": true,
  "message": "Notifications retrieved successfully",
  "data": {
    "notifications": [
      {
        "notificationId": 123,
        "title": "New Quiz Available",
        "message": "A new quiz has been added to your C# Programming course",
        "type": "CourseUpdate",
        "priority": "Medium",
        "isRead": false,
        "createdAt": "2025-06-27T10:30:00Z",
        "readAt": null,
        "expiresAt": "2025-07-27T10:30:00Z",
        "actionUrl": "/courses/15/quizzes/23",
        "actionText": "Take Quiz",
        "icon": "quiz",
        "category": "Academic",
        "metadata": {
          "courseId": 15,
          "courseName": "C# Programming",
          "quizId": 23,
          "quizTitle": "OOP Concepts Quiz"
        },
        "sender": {
          "userId": 456,
          "fullName": "Dr. Ahmed Hassan",
          "role": "Instructor",
          "profilePhoto": "/uploads/profiles/instructor_456.jpg"
        }
      },
      {
        "notificationId": 124,
        "title": "Course Completed!",
        "message": "Congratulations! You have successfully completed the JavaScript Essentials course",
        "type": "Achievement",
        "priority": "High",
        "isRead": true,
        "createdAt": "2025-06-26T16:45:00Z",
        "readAt": "2025-06-26T17:00:00Z",
        "expiresAt": null,
        "actionUrl": "/certificates/download/789",
        "actionText": "Download Certificate",
        "icon": "certificate",
        "category": "Achievement",
        "metadata": {
          "courseId": 18,
          "courseName": "JavaScript Essentials",
          "certificateId": 789,
          "completionDate": "2025-06-26T16:45:00Z"
        }
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalItems": 47,
      "totalPages": 3,
      "hasNext": true,
      "hasPrevious": false
    },
    "summary": {
      "totalNotifications": 47,
      "unreadCount": 12,
      "readCount": 35,
      "expiredCount": 3
    }
  }
}
```

**Notification Types:**
- `CourseUpdate` - Course content or structure changes
- `QuizAvailable` - New quiz available
- `Assignment` - Assignment related notifications
- `Achievement` - Achievements and milestones
- `SystemUpdate` - System maintenance or updates
- `Reminder` - Deadline or schedule reminders
- `Social` - Comments, replies, mentions
- `Security` - Security-related alerts
- `Payment` - Payment and billing notifications
- `General` - General announcements

**Priority Levels:**
- `Low` - Non-urgent information
- `Medium` - Standard notifications
- `High` - Important updates
- `Critical` - Urgent notifications requiring immediate attention

---

### 2. Get Notification Details (`GET /{notificationId}`)

**Path Parameter:** `notificationId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Notification details retrieved successfully",
  "data": {
    "notificationId": 123,
    "title": "New Quiz Available",
    "message": "A new quiz has been added to your C# Programming course. This quiz covers Object-Oriented Programming concepts and is required to proceed to the next level.",
    "fullContent": "Dear Student,\n\nWe're excited to announce that a new quiz on Object-Oriented Programming has been added to your C# Programming course...",
    "type": "QuizAvailable",
    "priority": "Medium",
    "isRead": false,
    "createdAt": "2025-06-27T10:30:00Z",
    "readAt": null,
    "expiresAt": "2025-07-27T10:30:00Z",
    "actionUrl": "/courses/15/quizzes/23",
    "actionText": "Take Quiz",
    "icon": "quiz",
    "category": "Academic",
    "metadata": {
      "courseId": 15,
      "courseName": "C# Programming",
      "quizId": 23,
      "quizTitle": "OOP Concepts Quiz",
      "passingScore": 70,
      "timeLimit": 30,
      "totalQuestions": 15
    },
    "sender": {
      "userId": 456,
      "fullName": "Dr. Ahmed Hassan",
      "role": "Instructor",
      "profilePhoto": "/uploads/profiles/instructor_456.jpg",
      "email": "ahmed.hassan@learnquest.com"
    },
    "relatedNotifications": [
      {
        "notificationId": 120,
        "title": "Course Content Updated",
        "createdAt": "2025-06-26T14:20:00Z"
      }
    ]
  }
}
```

---

### 3. Mark as Read (`POST /{notificationId}/mark-read`)

**Path Parameter:** `notificationId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Notification marked as read successfully",
  "data": {
    "notificationId": 123,
    "isRead": true,
    "readAt": "2025-06-27T16:00:00Z",
    "unreadCount": 11
  }
}
```

---

### 4. Mark as Unread (`POST /{notificationId}/mark-unread`)

**Path Parameter:** `notificationId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Notification marked as unread successfully",
  "data": {
    "notificationId": 123,
    "isRead": false,
    "readAt": null,
    "unreadCount": 12
  }
}
```

---

### 5. Mark All as Read (`POST /mark-all-read`)

**Request Body (Optional):**

```json
{
  "type": "CourseUpdate",
  "courseId": 15,
  "olderThan": "2025-06-20T00:00:00Z"
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "All notifications marked as read successfully",
  "data": {
    "markedCount": 12,
    "newUnreadCount": 0,
    "timestamp": "2025-06-27T16:05:00Z"
  }
}
```

---

### 6. Bulk Action (`POST /bulk-action`)

**Description:** Perform bulk operations on multiple notifications.

**Request Body:**

```json
{
  "action": "MarkRead",
  "notificationIds": [123, 124, 125, 126],
  "filters": {
    "type": "CourseUpdate",
    "priority": "Low",
    "olderThan": "2025-06-20T00:00:00Z"
  }
}
```

**Available Actions:**
- `MarkRead` - Mark notifications as read
- `MarkUnread` - Mark notifications as unread
- `Delete` - Delete notifications
- `Archive` - Archive notifications

**Success Response (200):**

```json
{
  "success": true,
  "message": "Bulk action completed successfully",
  "data": {
    "action": "MarkRead",
    "processedCount": 4,
    "successCount": 4,
    "failedCount": 0,
    "newUnreadCount": 8,
    "results": [
      {
        "notificationId": 123,
        "success": true
      },
      {
        "notificationId": 124,
        "success": true
      }
    ]
  }
}
```

---

## 📊 Notification Statistics

### 7. Get Unread Count (`GET /unread-count`)

**Query Parameters:**
- `type` (string, optional) - Filter by notification type
- `priority` (string, optional) - Filter by priority level
- `courseId` (integer, optional) - Filter by course ID

**Success Response (200):**

```json
{
  "success": true,
  "message": "Unread count retrieved successfully",
  "data": {
    "totalUnread": 12,
    "byType": {
      "CourseUpdate": 5,
      "QuizAvailable": 3,
      "Achievement": 2,
      "Reminder": 1,
      "General": 1
    },
    "byPriority": {
      "Critical": 0,
      "High": 2,
      "Medium": 7,
      "Low": 3
    },
    "byCourse": [
      {
        "courseId": 15,
        "courseName": "C# Programming",
        "unreadCount": 6
      },
      {
        "courseId": 18,
        "courseName": "JavaScript Essentials",
        "unreadCount": 3
      }
    ],
    "lastUpdated": "2025-06-27T16:00:00Z"
  }
}
```

---

## ⚙️ Notification Preferences

### 8. Get Preferences (`GET /preferences`)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Notification preferences retrieved successfully",
  "data": {
    "userId": 123,
    "preferences": {
      "email": {
        "enabled": true,
        "types": {
          "CourseUpdate": true,
          "QuizAvailable": true,
          "Achievement": true,
          "Assignment": true,
          "Reminder": true,
          "Security": true,
          "SystemUpdate": false,
          "Social": false
        },
        "frequency": "Immediate",
        "quietHours": {
          "enabled": true,
          "startTime": "22:00",
          "endTime": "08:00",
          "timezone": "Africa/Cairo"
        }
      },
      "push": {
        "enabled": true,
        "types": {
          "CourseUpdate": false,
          "QuizAvailable": true,
          "Achievement": true,
          "Assignment": true,
          "Reminder": true,
          "Security": true,
          "SystemUpdate": false,
          "Social": true
        },
        "allowedDevices": ["mobile", "desktop"]
      },
      "inApp": {
        "enabled": true,
        "types": {
          "CourseUpdate": true,
          "QuizAvailable": true,
          "Achievement": true,
          "Assignment": true,
          "Reminder": true,
          "Security": true,
          "SystemUpdate": true,
          "Social": true
        },
        "autoMarkRead": false,
        "showPreview": true
      }
    },
    "courseSpecific": [
      {
        "courseId": 15,
        "courseName": "C# Programming",
        "emailEnabled": true,
        "pushEnabled": false,
        "inAppEnabled": true
      }
    ],
    "updatedAt": "2025-06-15T14:30:00Z"
  }
}
```

---

### 9. Update Preferences (`PUT /preferences`)

**Request Body:**

```json
{
  "email": {
    "enabled": true,
    "types": {
      "CourseUpdate": true,
      "QuizAvailable": true,
      "Achievement": true,
      "Assignment": true,
      "Reminder": true,
      "Security": true,
      "SystemUpdate": false,
      "Social": false
    },
    "frequency": "Daily",
    "quietHours": {
      "enabled": true,
      "startTime": "23:00",
      "endTime": "07:00",
      "timezone": "Africa/Cairo"
    }
  },
  "push": {
    "enabled": true,
    "types": {
      "QuizAvailable": true,
      "Achievement": true,
      "Reminder": true,
      "Security": true
    },
    "allowedDevices": ["mobile"]
  },
  "inApp": {
    "enabled": true,
    "autoMarkRead": false,
    "showPreview": true
  },
  "courseSpecific": [
    {
      "courseId": 15,
      "emailEnabled": false,
      "pushEnabled": true,
      "inAppEnabled": true
    }
  ]
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Notification preferences updated successfully",
  "data": {
    "updated": true,
    "timestamp": "2025-06-27T16:10:00Z",
    "changesApplied": [
      "Email frequency changed to Daily",
      "Push notifications limited to mobile devices",
      "Course-specific settings updated for C# Programming"
    ]
  }
}
```

---

## 📡 Subscription Management

### 10. Subscribe (`POST /subscribe`)

**Request Body:**

```json
{
  "types": ["CourseUpdate", "QuizAvailable", "Achievement"],
  "channels": ["email", "push", "inApp"],
  "courseIds": [15, 18],
  "immediate": true
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Successfully subscribed to notifications",
  "data": {
    "subscribedTypes": ["CourseUpdate", "QuizAvailable", "Achievement"],
    "subscribedChannels": ["email", "push", "inApp"],
    "subscribedCourses": [15, 18],
    "effectiveDate": "2025-06-27T16:15:00Z"
  }
}
```

---

### 11. Unsubscribe (`POST /unsubscribe`)

**Request Body:**

```json
{
  "types": ["Social", "SystemUpdate"],
  "channels": ["email"],
  "courseIds": [20],
  "reason": "Too frequent",
  "temporary": false
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Successfully unsubscribed from notifications",
  "data": {
    "unsubscribedTypes": ["Social", "SystemUpdate"],
    "unsubscribedChannels": ["email"],
    "unsubscribedCourses": [20],
    "effectiveDate": "2025-06-27T16:20:00Z",
    "resubscribeUrl": "/notifications/resubscribe?token=abc123"
  }
}
```

---

## 👨‍🏫 Instructor & Admin Operations

### 12. Send Notification (`POST /send`)

**Authorization Required:** Instructor or Admin role

**Request Body:**

```json
{
  "recipientIds": [123, 124, 125],
  "title": "Assignment Deadline Reminder",
  "message": "This is a reminder that your final project is due in 3 days.",
  "type": "Reminder",
  "priority": "High",
  "actionUrl": "/courses/15/assignments/5",
  "actionText": "View Assignment",
  "expiresAt": "2025-07-15T23:59:00Z",
  "metadata": {
    "courseId": 15,
    "assignmentId": 5,
    "dueDate": "2025-07-15T23:59:00Z"
  },
  "channels": ["email", "push", "inApp"],
  "scheduleFor": "2025-06-28T09:00:00Z"
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Notification sent successfully",
  "data": {
    "notificationId": 456,
    "recipientCount": 3,
    "deliveryStatus": {
      "email": 3,
      "push": 2,
      "inApp": 3
    },
    "scheduledFor": "2025-06-28T09:00:00Z",
    "estimatedDelivery": "2025-06-28T09:05:00Z"
  }
}
```

---

### 13. Broadcast Notification (`POST /broadcast`)

**Authorization Required:** Admin role only

**Request Body:**

```json
{
  "targetAudience": {
    "roles": ["RegularUser", "Instructor"],
    "courseIds": [15, 18, 20],
    "activeWithinDays": 30,
    "excludeUserIds": [789]
  },
  "title": "System Maintenance Notice",
  "message": "The system will be under maintenance on July 1st from 2:00 AM to 6:00 AM EET.",
  "type": "SystemUpdate",
  "priority": "High",
  "actionUrl": "/maintenance-info",
  "actionText": "Learn More",
  "expiresAt": "2025-07-01T06:00:00Z",
  "channels": ["email", "inApp"],
  "scheduleFor": "2025-06-30T18:00:00Z"
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Broadcast notification scheduled successfully",
  "data": {
    "broadcastId": 789,
    "estimatedRecipients": 1247,
    "targetCriteria": {
      "roles": ["RegularUser", "Instructor"],
      "courses": 3,
      "activeUsers": 1247
    },
    "scheduledFor": "2025-06-30T18:00:00Z",
    "estimatedDelivery": "2025-06-30T18:30:00Z"
  }
}
```

---

### 14. Get Templates (`GET /templates`)

**Authorization Required:** Instructor or Admin role

**Query Parameters:**
- `category` (string, optional) - Filter by template category
- `type` (string, optional) - Filter by notification type

**Success Response (200):**

```json
{
  "success": true,
  "message": "Notification templates retrieved successfully",
  "data": {
    "templates": [
      {
        "templateId": 1,
        "name": "Quiz Available",
        "category": "Academic",
        "type": "QuizAvailable",
        "title": "New Quiz: {{quizTitle}}",
        "message": "A new quiz '{{quizTitle}}' is now available in your {{courseName}} course.",
        "variables": [
          {
            "name": "quizTitle",
            "type": "string",
            "required": true,
            "description": "The title of the quiz"
          },
          {
            "name": "courseName",
            "type": "string",
            "required": true,
            "description": "The name of the course"
          }
        ],
        "defaultPriority": "Medium",
        "supportedChannels": ["email", "push", "inApp"],
        "isActive": true
      }
    ],
    "categories": [
      "Academic",
      "Administrative",
      "System",
      "Marketing",
      "Security"
    ]
  }
}
```

---

## 🔧 Common Error Responses

- **401 Unauthorized** - Invalid or missing token
- **403 Forbidden** - Insufficient permissions
- **404 Not Found** - Notification or resource not found
- **400 Bad Request** - Invalid input data or validation errors
- **429 Too Many Requests** - Rate limit exceeded for sending notifications
- **500 Internal Server Error** - Unexpected server errors

---

## 📝 Notes for Frontend Team

- Notifications support real-time updates via WebSocket connection
- All timestamps are in UTC format
- Maximum notification message length is 1000 characters
- Images and attachments are supported via metadata URLs
- Notification preferences are cached and updated in real-time
- Push notifications require proper service worker setup
- Email notifications respect user's quiet hours settings
- Expired notifications are automatically archived after 30 days
- Bulk operations are limited to 100 notifications per request
- Rate limiting applies: 10 notification sends per minute for instructors, 50 for admins

---

*Last updated: 2025-06-27*