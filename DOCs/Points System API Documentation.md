# LearnQuest - Points System API Documentation

> **Base URL:** `https://localhost:7217/api/points`

---

## 🔐 Authentication

All endpoints require a valid Bearer token.

```http
Authorization: Bearer <accessToken>
```

---

## 🎯 Points System Overview

النظام يعمل على أساس النقاط للكورسات فقط (ليس للمستويات أو الأقسام):

- **نقاط الكويزات:** 10-25 نقطة حسب الدرجة
- **نقاط إكمال الكورس:** 100 نقطة
- **نقاط إضافية:** يمنحها المدرس/الإدارة
- **خصم النقاط:** للإدارة فقط

---

## 🔗 Endpoints Overview

| Endpoint | Method | Role | Description |
|----------|--------|------|-------------|
| `/leaderboard/{courseId}` | GET | All | عرض لوحة المتصدرين للكورس |
| `/my-ranking/{courseId}` | GET | All | ترتيب المستخدم الحالي |
| `/my-points` | GET | All | نقاط المستخدم في جميع الكورسات |
| `/my-transactions/{courseId}` | GET | All | سجل معاملات النقاط للمستخدم |
| `/award-bonus` | POST | Admin/Instructor | منح نقاط إضافية |
| `/deduct-points` | POST | Admin | خصم نقاط |
| `/course-transactions/{courseId}` | GET | Admin/Instructor | سجل معاملات الكورس |
| `/recent-transactions/{courseId}` | GET | Admin/Instructor | آخر المعاملات |
| `/course-stats/{courseId}` | GET | Admin/Instructor | إحصائيات النقاط |
| `/my-awarded-transactions` | GET | Admin/Instructor | المعاملات التي منحها المستخدم |
| `/update-ranks/{courseId}` | POST | Admin | تحديث الترتيب يدوياً |
| `/recalculate-user-points` | POST | Admin | إعادة حساب نقاط المستخدم |
| `/user-ranking/{userId}/{courseId}` | GET | Admin/Instructor | ترتيب مستخدم محدد |
| `/user-transactions/{userId}/{courseId}` | GET | Admin | سجل معاملات مستخدم محدد |

---

## 📊 Student Endpoints

### 1. Get Course Leaderboard (`GET /leaderboard/{courseId}`)

**Description:** عرض لوحة المتصدرين للكورس مع ترتيب المستخدمين

**Path Parameters:**
- `courseId` (integer) - معرف الكورس

**Query Parameters:**
- `limit` (integer, optional) - عدد المستخدمين المعروضين (افتراضي: 100)

**Success Response (200):**
```json
{
  "success": true,
  "message": "Leaderboard retrieved successfully",
  "data": {
    "courseId": 1,
    "courseName": "C# Programming",
    "courseImage": "path/to/image.jpg",
    "totalEnrolledUsers": 150,
    "lastUpdated": "2025-06-18T10:30:00Z",
    "rankings": [
      {
        "rank": 1,
        "userId": 101,
        "userName": "أحمد محمد",
        "userEmail": "ahmed@example.com",
        "profilePhoto": "path/to/photo.jpg",
        "totalPoints": 285,
        "quizPoints": 185,
        "bonusPoints": 100,
        "penaltyPoints": 0,
        "isCurrentUser": false,
        "lastActivity": "2025-06-18T09:15:00Z",
        "completedQuizzes": 8,
        "averageQuizScore": 87.5,
        "totalQuizAttempts": 12
      }
    ]
  }
}
```

---

### 2. Get My Ranking (`GET /my-ranking/{courseId}`)

**Description:** الحصول على ترتيب المستخدم الحالي في الكورس

**Success Response (200):**
```json
{
  "success": true,
  "message": "User ranking retrieved successfully",
  "data": {
    "rank": 5,
    "userId": 42,
    "userName": "محمد أحمد",
    "totalPoints": 220,
    "quizPoints": 160,
    "bonusPoints": 60,
    "penaltyPoints": 0,
    "isCurrentUser": true,
    "lastActivity": "2025-06-18T08:20:00Z",
    "completedQuizzes": 6,
    "averageQuizScore": 82.3,
    "totalQuizAttempts": 8
  }
}
```

---

### 3. Get My Points in All Courses (`GET /my-points`)

**Description:** الحصول على نقاط المستخدم في جميع الكورسات المسجل بها

**Success Response (200):**
```json
{
  "success": true,
  "message": "User points retrieved successfully",
  "data": [
    {
      "coursePointsId": 1,
      "userId": 42,
      "userName": "محمد أحمد",
      "courseId": 1,
      "courseName": "C# Programming",
      "totalPoints": 220,
      "quizPoints": 160,
      "bonusPoints": 60,
      "penaltyPoints": 0,
      "currentRank": 5,
      "lastUpdated": "2025-06-18T08:20:00Z",
      "createdAt": "2025-06-10T14:30:00Z"
    }
  ]
}
```

---

### 4. Get My Transaction History (`GET /my-transactions/{courseId}`)

**Description:** سجل معاملات النقاط للمستخدم في كورس محدد

**Success Response (200):**
```json
{
  "success": true,
  "message": "Transaction history retrieved successfully",
  "data": [
    {
      "transactionId": 101,
      "userId": 42,
      "userName": "محمد أحمد",
      "courseId": 1,
      "courseName": "C# Programming",
      "pointsChanged": 20,
      "pointsAfterTransaction": 220,
      "source": "QuizCompletion",
      "transactionType": "Earned",
      "quizAttemptId": 55,
      "quizName": "OOP Basics Quiz",
      "awardedByUserId": null,
      "awardedByName": null,
      "description": "Points earned from quiz completion",
      "createdAt": "2025-06-18T08:20:00Z"
    }
  ]
}
```

---

## 👨‍🏫 Instructor/Admin Endpoints

### 5. Award Bonus Points (`POST /award-bonus`)

**Description:** منح نقاط إضافية لمستخدم (للمدرس والإدارة)

**Request Body:**
```json
{
  "userId": 42,
  "courseId": 1,
  "points": 50,
  "description": "Outstanding participation in discussions"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Bonus points awarded successfully",
  "data": {
    "coursePointsId": 1,
    "userId": 42,
    "totalPoints": 270,
    "quizPoints": 160,
    "bonusPoints": 110,
    "penaltyPoints": 0,
    "currentRank": 4,
    "lastUpdated": "2025-06-18T10:45:00Z"
  }
}
```

---

### 6. Deduct Points (`POST /deduct-points`)

**Description:** خصم نقاط من مستخدم (للإدارة فقط)

**Request Body:**
```json
{
  "userId": 42,
  "courseId": 1,
  "points": 20,
  "reason": "Academic misconduct penalty"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Points deducted successfully",
  "data": {
    "coursePointsId": 1,
    "userId": 42,
    "totalPoints": 200,
    "quizPoints": 160,
    "bonusPoints": 60,
    "penaltyPoints": 20,
    "currentRank": 6,
    "lastUpdated": "2025-06-18T10:50:00Z"
  }
}
```

---

### 7. Get Course Points Statistics (`GET /course-stats/{courseId}`)

**Description:** إحصائيات شاملة لنظام النقاط في الكورس

**Success Response (200):**
```json
{
  "success": true,
  "message": "Course points statistics retrieved successfully",
  "data": {
    "courseId": 1,
    "courseName": "C# Programming",
    "totalUsers": 150,
    "usersWithPoints": 120,
    "totalPointsAwarded": 18500,
    "averagePoints": 154.17,
    "highestPoints": 285,
    "lowestPoints": 10,
    "topUser": {
      "rank": 1,
      "userId": 101,
      "userName": "أحمد محمد",
      "totalPoints": 285
    },
    "pointsBySource": [
      {
        "source": "QuizCompletion",
        "totalPoints": 12500,
        "percentage": 67.57
      },
      {
        "source": "CourseCompletion",
        "totalPoints": 4000,
        "percentage": 21.62
      },
      {
        "source": "BonusPoints",
        "totalPoints": 2000,
        "percentage": 10.81
      }
    ]
  }
}
```

---

## 🎯 Point Values

| Action | Points | Condition |
|--------|--------|-----------|
| Quiz Pass (60-79%) | 10 | النجاح في الكويز |
| Quiz Good (80-89%) | 15 | درجة جيدة |
| Quiz Excellent (90-99%) | 20 | درجة ممتازة |
| Quiz Perfect (100%) | 25 | الدرجة الكاملة |
| Course Completion | 100 | إكمال جميع أقسام الكورس |
| Bonus Points | Variable | يحددها المدرس/الإدارة |

---

## 🏆 Ranking System

- الترتيب بناءً على إجمالي النقاط
- في حالة التعادل: أعلى نقاط كويزات
- في حالة التعادل الثاني: الأسبق في التسجيل
- يتم تحديث الترتيب تلقائياً عند تغيير النقاط

---

## ❌ Error Responses

**400 Bad Request:**
```json
{
  "success": false,
  "message": "Invalid request data"
}
```

**401 Unauthorized:**
```json
{
  "success": false,
  "message": "Authentication required"
}
```

**403 Forbidden:**
```json
{
  "success": false,
  "message": "Insufficient permissions"
}
```

**404 Not Found:**
```json
{
  "success": false,
  "message": "Course not found"
}
```

**500 Internal Server Error:**
```json
{
  "success": false,
  "message": "Internal server error"
}
```

---

> *Generated on 2025-06-18*