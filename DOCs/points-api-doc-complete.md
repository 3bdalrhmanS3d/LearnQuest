# LearnQuest – PointsController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/points`

---

## 🔐 Authentication

All endpoints require a valid Bearer token:

```http
Authorization: Bearer <accessToken>
```

---

## 🔗 Endpoints Overview

| Endpoint                                    | Method | Description                          | Role                      |
| ------------------------------------------- | ------ | ------------------------------------ | ------------------------- |
| `/leaderboard/{courseId}`                   | GET    | Get course leaderboard               | All authenticated users   |
| `/my-ranking/{courseId}`                    | GET    | Get current user's ranking           | All authenticated users   |
| `/my-points`                                | GET    | Get user's points in all courses     | All authenticated users   |
| `/my-transactions/{courseId}`               | GET    | Get user's transaction history       | All authenticated users   |
| `/award-bonus`                              | POST   | Award bonus points to user           | Admin, Instructor         |
| `/deduct-points`                            | POST   | Deduct points from user              | Admin                     |
| `/course-transactions/{courseId}`           | GET    | Get course transaction history       | Admin, Instructor         |
| `/recent-transactions/{courseId}`           | GET    | Get recent course transactions       | Admin, Instructor         |
| `/course-stats/{courseId}`                  | GET    | Get course points statistics         | Admin, Instructor         |
| `/my-awarded-transactions`                  | GET    | Get transactions awarded by current user | Admin, Instructor         |
| `/update-ranks/{courseId}`                  | POST   | Manually update course rankings      | Admin                     |
| `/recalculate-user-points`                  | POST   | Recalculate user points              | Admin                     |
| `/user-ranking/{userId}/{courseId}`         | GET    | Get specific user ranking            | Admin, Instructor         |
| `/user-transactions/{userId}/{courseId}`    | GET    | Get specific user transactions       | Admin                     |

---

## 🎯 Points System Overview

The points system operates on a course-by-course basis with the following structure:

- **Quiz Points:** 10-25 points based on score performance
- **Course Completion Points:** 100 points for completing entire course
- **Bonus Points:** Awarded by instructors/admins for exceptional performance
- **Penalty Points:** Deducted by admins for violations or corrections
- **Rankings:** Calculated based on total points within each course

### Point Calculation Rules:
- Quiz scores 90-100%: 25 points
- Quiz scores 80-89%: 20 points  
- Quiz scores 70-79%: 15 points
- Quiz scores 60-69%: 10 points
- Quiz scores below 60%: 0 points
- Course completion: 100 points
- Perfect attendance bonus: 50 points
- Early submission bonus: 25 points

---

## 📊 Student Endpoints

### 1. Get Course Leaderboard (`GET /leaderboard/{courseId}`)

**Description:** Display the course leaderboard with user rankings.

**Path Parameters:**
- `courseId` (integer) - Course identifier

**Query Parameters:**
- `limit` (integer, optional) - Number of users to display (default: 100, max: 500)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Leaderboard retrieved successfully",
  "data": {
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "courseImage": "/uploads/courses/csharp_fundamentals.jpg",
    "totalEnrolledUsers": 247,
    "lastUpdated": "2025-06-27T18:30:00Z",
    "pointsDistribution": {
      "averagePoints": 185.4,
      "medianPoints": 172.0,
      "highestPoints": 425,
      "lowestPoints": 0
    },
    "rankings": [
      {
        "rank": 1,
        "userId": 123,
        "userName": "Ahmed Hassan",
        "userEmail": "ahmed.hassan@student.edu",
        "profilePhoto": "/uploads/profiles/user_123.jpg",
        "totalPoints": 425,
        "quizPoints": 275,
        "bonusPoints": 150,
        "penaltyPoints": 0,
        "courseCompletionPoints": 100,
        "isCurrentUser": false,
        "lastActivity": "2025-06-27T16:45:00Z",
        "badges": [
          {
            "badgeId": "quiz_master",
            "title": "Quiz Master",
            "description": "Scored 90%+ on all quizzes",
            "icon": "/icons/quiz_master.svg"
          }
        ],
        "statistics": {
          "completedQuizzes": 11,
          "averageQuizScore": 91.3,
          "totalQuizAttempts": 15,
          "courseCompletionPercentage": 100,
          "daysActive": 28,
          "studyStreak": 12
        }
      },
      {
        "rank": 2,
        "userId": 124,
        "userName": "Sara Ahmed",
        "userEmail": "sara.ahmed@student.edu",
        "profilePhoto": "/uploads/profiles/user_124.jpg",
        "totalPoints": 398,
        "quizPoints": 248,
        "bonusPoints": 100,
        "penaltyPoints": 0,
        "courseCompletionPoints": 100,
        "isCurrentUser": true,
        "lastActivity": "2025-06-27T17:20:00Z",
        "badges": [
          {
            "badgeId": "consistent_learner",
            "title": "Consistent Learner",
            "description": "Maintained 7-day study streak",
            "icon": "/icons/consistent_learner.svg"
          }
        ],
        "statistics": {
          "completedQuizzes": 10,
          "averageQuizScore": 88.7,
          "totalQuizAttempts": 14,
          "courseCompletionPercentage": 95,
          "daysActive": 25,
          "studyStreak": 8
        }
      }
    ],
    "currentUserInfo": {
      "userId": 124,
      "rank": 2,
      "totalPoints": 398,
      "pointsToNextRank": 27,
      "pointsFromPreviousRank": 56
    }
  }
}
```

**Errors:**
- `404 Not Found` - Course not found or user not enrolled
- `403 Forbidden` - Course access denied

---

### 2. Get My Ranking (`GET /my-ranking/{courseId}`)

**Description:** Get the current user's ranking and detailed points breakdown in a specific course.

**Path Parameters:**
- `courseId` (integer) - Course identifier

**Success Response (200):**

```json
{
  "success": true,
  "message": "User ranking retrieved successfully",
  "data": {
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "userId": 124,
    "userName": "Sara Ahmed",
    "currentRank": 2,
    "totalUsers": 247,
    "percentileRank": 99.2,
    "points": {
      "totalPoints": 398,
      "quizPoints": 248,
      "bonusPoints": 100,
      "penaltyPoints": 0,
      "courseCompletionPoints": 100,
      "breakdown": [
        {
          "source": "Quiz: Variables and Data Types",
          "points": 25,
          "earnedAt": "2025-06-15T10:30:00Z",
          "type": "Quiz"
        },
        {
          "source": "Perfect Attendance Bonus",
          "points": 50,
          "earnedAt": "2025-06-20T18:00:00Z",
          "type": "Bonus",
          "awardedBy": "Dr. Ahmed Hassan"
        },
        {
          "source": "Course Completion",
          "points": 100,
          "earnedAt": "2025-06-25T16:45:00Z",
          "type": "Completion"
        }
      ]
    },
    "progress": {
      "pointsToNextRank": 27,
      "pointsFromPreviousRank": 56,
      "nextRankUser": {
        "userId": 123,
        "userName": "Ahmed Hassan",
        "totalPoints": 425
      },
      "previousRankUser": {
        "userId": 125,
        "userName": "Omar Ali",
        "totalPoints": 342
      }
    },
    "achievements": [
      {
        "achievementId": "first_quiz",
        "title": "First Quiz Completed",
        "description": "Successfully completed your first quiz",
        "earnedAt": "2025-06-15T10:30:00Z",
        "points": 25
      }
    ],
    "trends": {
      "weeklyPointsGrowth": 45,
      "monthlyPointsGrowth": 198,
      "rankingChange": 3,
      "averagePointsPerDay": 14.2
    },
    "lastUpdated": "2025-06-27T17:20:00Z"
  }
}
```

---

### 3. Get My Points in All Courses (`GET /my-points`)

**Description:** Get the current user's points summary across all enrolled courses.

**Success Response (200):**

```json
{
  "success": true,
  "message": "User points retrieved successfully",
  "data": {
    "userId": 124,
    "userName": "Sara Ahmed",
    "totalPointsAllCourses": 1247,
    "totalCoursesEnrolled": 4,
    "completedCourses": 2,
    "averagePointsPerCourse": 311.75,
    "globalRank": 15,
    "totalUsers": 892,
    "courses": [
      {
        "courseId": 15,
        "courseName": "C# Programming Fundamentals",
        "courseImage": "/uploads/courses/csharp_fundamentals.jpg",
        "totalPoints": 398,
        "rank": 2,
        "totalUsers": 247,
        "isCompleted": true,
        "completedAt": "2025-06-25T16:45:00Z",
        "pointsBreakdown": {
          "quizPoints": 248,
          "bonusPoints": 100,
          "penaltyPoints": 0,
          "completionPoints": 100
        },
        "achievements": 5,
        "lastActivity": "2025-06-27T17:20:00Z"
      },
      {
        "courseId": 18,
        "courseName": "JavaScript Essentials",
        "courseImage": "/uploads/courses/javascript_essentials.jpg",
        "totalPoints": 312,
        "rank": 8,
        "totalUsers": 189,
        "isCompleted": true,
        "completedAt": "2025-06-10T14:30:00Z",
        "pointsBreakdown": {
          "quizPoints": 187,
          "bonusPoints": 25,
          "penaltyPoints": 0,
          "completionPoints": 100
        },
        "achievements": 3,
        "lastActivity": "2025-06-10T14:30:00Z"
      },
      {
        "courseId": 22,
        "courseName": "React.js Complete Guide",
        "courseImage": "/uploads/courses/react_complete.jpg",
        "totalPoints": 285,
        "rank": 5,
        "totalUsers": 156,
        "isCompleted": false,
        "completionPercentage": 78,
        "pointsBreakdown": {
          "quizPoints": 210,
          "bonusPoints": 75,
          "penaltyPoints": 0,
          "completionPoints": 0
        },
        "achievements": 4,
        "lastActivity": "2025-06-27T15:45:00Z"
      },
      {
        "courseId": 25,
        "courseName": "Node.js Backend Development",
        "courseImage": "/uploads/courses/nodejs_backend.jpg",
        "totalPoints": 252,
        "rank": 12,
        "totalUsers": 134,
        "isCompleted": false,
        "completionPercentage": 45,
        "pointsBreakdown": {
          "quizPoints": 152,
          "bonusPoints": 100,
          "penaltyPoints": 0,
          "completionPoints": 0
        },
        "achievements": 2,
        "lastActivity": "2025-06-26T11:20:00Z"
      }
    ],
    "overallStatistics": {
      "totalQuizzesCompleted": 48,
      "averageQuizScore": 86.4,
      "totalBonusPointsReceived": 300,
      "totalPenaltyPoints": 0,
      "studyStreakDays": 12,
      "totalStudyHours": 187
    },
    "recentPointsHistory": [
      {
        "date": "2025-06-27",
        "pointsEarned": 25,
        "source": "Quiz: Advanced React Hooks",
        "courseId": 22
      },
      {
        "date": "2025-06-26",
        "pointsEarned": 50,
        "source": "Participation Bonus",
        "courseId": 25
      }
    ]
  }
}
```

---

### 4. Get My Transaction History (`GET /my-transactions/{courseId}`)

**Description:** Get detailed transaction history for the current user in a specific course.

**Path Parameters:**
- `courseId` (integer) - Course identifier

**Query Parameters:**
- `pageNumber` (integer, optional) - Page number (default: 1)
- `pageSize` (integer, optional) - Page size (default: 20, max: 100)
- `type` (string, optional) - Filter by transaction type: "Quiz", "Bonus", "Penalty", "Completion"
- `startDate` (string, optional) - Filter from date (ISO format)
- `endDate` (string, optional) - Filter to date (ISO format)

**Success Response (200):**

```json
{
  "success": true,
  "message": "User transaction history retrieved successfully",
  "data": {
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "userId": 124,
    "summary": {
      "totalTransactions": 23,
      "totalPointsEarned": 398,
      "totalPointsDeducted": 0,
      "netPoints": 398,
      "firstTransaction": "2025-06-01T09:00:00Z",
      "lastTransaction": "2025-06-27T17:20:00Z"
    },
    "transactions": [
      {
        "transactionId": 1547,
        "type": "Quiz",
        "points": 25,
        "description": "Quiz: Advanced OOP Concepts",
        "details": {
          "quizId": 23,
          "quizTitle": "Object-Oriented Programming Quiz",
          "score": 91.5,
          "maxScore": 100,
          "attemptNumber": 1,
          "timeTaken": 2340
        },
        "timestamp": "2025-06-27T17:20:00Z",
        "awardedBy": "System",
        "isReversible": false
      },
      {
        "transactionId": 1546,
        "type": "Bonus",
        "points": 50,
        "description": "Excellent participation in class discussion",
        "details": {
          "reason": "Outstanding contribution to forum discussions",
          "category": "Participation",
          "approvalRequired": false
        },
        "timestamp": "2025-06-26T14:30:00Z",
        "awardedBy": "Dr. Ahmed Hassan",
        "awardedById": 456,
        "isReversible": true,
        "notes": "Student provided excellent examples and helped other students understand complex concepts"
      },
      {
        "transactionId": 1545,
        "type": "Completion",
        "points": 100,
        "description": "Course completion bonus",
        "details": {
          "completionDate": "2025-06-25T16:45:00Z",
          "finalGrade": 87.3,
          "certificateGenerated": true,
          "certificateId": "CERT-15-124-2025"
        },
        "timestamp": "2025-06-25T16:45:00Z",
        "awardedBy": "System",
        "isReversible": false
      },
      {
        "transactionId": 1544,
        "type": "Quiz",
        "points": 20,
        "description": "Quiz: Data Structures and Algorithms",
        "details": {
          "quizId": 22,
          "quizTitle": "Data Structures Quiz",
          "score": 83.2,
          "maxScore": 100,
          "attemptNumber": 2,
          "timeTaken": 1890
        },
        "timestamp": "2025-06-24T11:15:00Z",
        "awardedBy": "System",
        "isReversible": false
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalItems": 23,
      "totalPages": 2,
      "hasNext": true,
      "hasPrevious": false
    },
    "analytics": {
      "pointsByType": {
        "Quiz": 248,
        "Bonus": 100,
        "Penalty": 0,
        "Completion": 100
      },
      "pointsByMonth": [
        {
          "month": "2025-06",
          "points": 398,
          "transactions": 23
        }
      ],
      "averagePointsPerTransaction": 17.3,
      "bestPerformingCategory": "Quiz"
    }
  }
}
```

---

## 👨‍🏫 Instructor & Admin Operations

### 5. Award Bonus Points (`POST /award-bonus`)

**Authorization Required:** Admin or Instructor role

**Request Body:**

```json
{
  "userId": 124,
  "courseId": 15,
  "points": 50,
  "description": "Outstanding participation in class discussions",
  "category": "Participation",
  "reason": "Student consistently provided high-quality answers and helped other students",
  "expiresAt": null,
  "notifyUser": true,
  "requireApproval": false,
  "metadata": {
    "discussionId": 789,
    "helpfulnessRating": 4.8,
    "peersHelped": 12
  }
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Bonus points awarded successfully",
  "data": {
    "transactionId": 1548,
    "coursePointsId": 124,
    "userId": 124,
    "userName": "Sara Ahmed",
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "pointsAwarded": 50,
    "totalPoints": 448,
    "newRank": 2,
    "previousRank": 2,
    "pointsBreakdown": {
      "quizPoints": 248,
      "bonusPoints": 150,
      "penaltyPoints": 0,
      "completionPoints": 100,
      "totalPoints": 448
    },
    "awardedAt": "2025-06-27T18:45:00Z",
    "awardedBy": "Dr. Ahmed Hassan",
    "notification": {
      "sent": true,
      "type": "email_and_push",
      "message": "Congratulations! You've received 50 bonus points for outstanding participation."
    },
    "impact": {
      "rankingChange": 0,
      "pointsToNextRank": 27,
      "achievementsUnlocked": [
        {
          "achievementId": "participation_expert",
          "title": "Participation Expert",
          "description": "Received 100+ participation bonus points"
        }
      ]
    }
  }
}
```

**Errors:**
- `404 Not Found` - User or course not found
- `400 Bad Request` - Invalid points amount or user not enrolled
- `403 Forbidden` - Insufficient permissions

---

### 6. Deduct Points (`POST /deduct-points`)

**Authorization Required:** Admin role only

**Request Body:**

```json
{
  "userId": 124,
  "courseId": 15,
  "points": 25,
  "reason": "Plagiarism violation in assignment submission",
  "severity": "Medium",
  "appealable": true,
  "appealDeadline": "2025-07-05T23:59:00Z",
  "notifyUser": true,
  "requireConfirmation": true,
  "metadata": {
    "violationType": "Academic Misconduct",
    "evidenceId": "EVID-2025-156",
    "reviewedBy": "Academic Committee"
  }
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Points deducted successfully",
  "data": {
    "transactionId": 1549,
    "coursePointsId": 124,
    "userId": 124,
    "userName": "Sara Ahmed",
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "pointsDeducted": 25,
    "totalPoints": 423,
    "newRank": 2,
    "previousRank": 2,
    "pointsBreakdown": {
      "quizPoints": 248,
      "bonusPoints": 150,
      "penaltyPoints": 25,
      "completionPoints": 100,
      "totalPoints": 423
    },
    "deductedAt": "2025-06-27T19:00:00Z",
    "deductedBy": "System Administrator",
    "appeal": {
      "allowAppeals": true,
      "appealDeadline": "2025-07-05T23:59:00Z",
      "appealProcess": "Submit appeal through student portal",
      "appealId": "APPEAL-2025-124-15"
    },
    "notification": {
      "sent": true,
      "type": "email_and_push",
      "message": "Points have been deducted due to academic policy violation. You have the right to appeal."
    }
  }
}
```

---

### 7. Get Course Transaction History (`GET /course-transactions/{courseId}`)

**Authorization Required:** Admin or Instructor role

**Path Parameters:**
- `courseId` (integer) - Course identifier

**Query Parameters:**
- `pageNumber` (integer, optional) - Page number (default: 1)
- `pageSize` (integer, optional) - Page size (default: 50, max: 200)
- `type` (string, optional) - Filter by transaction type
- `userId` (integer, optional) - Filter by specific user
- `startDate` (string, optional) - Filter from date
- `endDate` (string, optional) - Filter to date
- `minPoints` (integer, optional) - Filter by minimum points
- `maxPoints` (integer, optional) - Filter by maximum points

**Success Response (200):**

```json
{
  "success": true,
  "message": "Course transaction history retrieved successfully",
  "data": {
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "summary": {
      "totalTransactions": 1247,
      "totalPointsAwarded": 45890,
      "totalPointsDeducted": 340,
      "netPointsDistributed": 45550,
      "uniqueUsers": 247,
      "averagePointsPerUser": 185.8,
      "dateRange": {
        "firstTransaction": "2025-06-01T09:00:00Z",
        "lastTransaction": "2025-06-27T19:00:00Z"
      }
    },
    "transactions": [
      {
        "transactionId": 1549,
        "userId": 124,
        "userName": "Sara Ahmed",
        "userEmail": "sara.ahmed@student.edu",
        "type": "Penalty",
        "points": -25,
        "description": "Academic misconduct penalty",
        "timestamp": "2025-06-27T19:00:00Z",
        "awardedBy": "System Administrator",
        "awardedById": 1,
        "details": {
          "severity": "Medium",
          "appealable": true,
          "appealDeadline": "2025-07-05T23:59:00Z"
        }
      },
      {
        "transactionId": 1548,
        "userId": 124,
        "userName": "Sara Ahmed",
        "userEmail": "sara.ahmed@student.edu",
        "type": "Bonus",
        "points": 50,
        "description": "Outstanding participation in class discussions",
        "timestamp": "2025-06-27T18:45:00Z",
        "awardedBy": "Dr. Ahmed Hassan",
        "awardedById": 456,
        "details": {
          "category": "Participation",
          "metadata": {
            "discussionId": 789,
            "helpfulnessRating": 4.8
          }
        }
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 50,
      "totalItems": 1247,
      "totalPages": 25,
      "hasNext": true
    },
    "analytics": {
      "transactionsByType": {
        "Quiz": 891,
        "Bonus": 289,
        "Penalty": 17,
        "Completion": 50
      },
      "pointsByType": {
        "Quiz": 32140,
        "Bonus": 8950,
        "Penalty": 340,
        "Completion": 5000
      },
      "topPerformers": [
        {
          "userId": 123,
          "userName": "Ahmed Hassan",
          "totalPoints": 425,
          "rank": 1
        }
      ],
      "trends": {
        "dailyPointsAverage": 245.7,
        "peakActivity": "14:00-16:00",
        "mostActiveDay": "Tuesday"
      }
    }
  }
}
```

---

### 8. Get Course Points Statistics (`GET /course-stats/{courseId}`)

**Authorization Required:** Admin or Instructor role

**Path Parameters:**
- `courseId` (integer) - Course identifier

**Query Parameters:**
- `period` (string, optional) - Time period: "week", "month", "quarter", "year", "all" (default: "month")
- `includeInactive` (boolean, optional) - Include inactive users (default: false)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Course points statistics retrieved successfully",
  "data": {
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "generatedAt": "2025-06-27T19:15:00Z",
    "period": "month",
    "overview": {
      "totalUsers": 247,
      "activeUsers": 198,
      "completedUsers": 52,
      "totalPointsDistributed": 45550,
      "averagePointsPerUser": 184.5,
      "medianPoints": 156.0,
      "pointsStandardDeviation": 89.7
    },
    "distribution": {
      "pointsRanges": [
        {
          "range": "0-50",
          "userCount": 23,
          "percentage": 9.3,
          "description": "Beginners"
        },
        {
          "range": "51-100",
          "userCount": 45,
          "percentage": 18.2,
          "description": "Developing"
        },
        {
          "range": "101-200",
          "userCount": 89,
          "percentage": 36.0,
          "description": "Progressing"
        },
        {
          "range": "201-300",
          "userCount": 67,
          "percentage": 27.1,
          "description": "Advanced"
        },
        {
          "range": "301+",
          "userCount": 23,
          "percentage": 9.3,
          "description": "Expert"
        }
      ],
      "quartiles": {
        "q1": 89,
        "q2": 156,
        "q3": 267,
        "iqr": 178
      }
    },
    "engagement": {
      "quizParticipationRate": 87.4,
      "averageQuizScore": 78.6,
      "courseCompletionRate": 21.1,
      "bonusPointsDistribution": {
        "totalBonus": 8950,
        "averageBonusPerUser": 36.2,
        "usersWithBonus": 156,
        "bonusParticipationRate": 63.2
      }
    },
    "timeAnalysis": {
      "pointsEarnedOverTime": [
        {
          "date": "2025-06-01",
          "totalPoints": 1245,
          "transactions": 45,
          "activeUsers": 198
        },
        {
          "date": "2025-06-02",
          "totalPoints": 1389,
          "transactions": 52,
          "activeUsers": 201
        }
      ],
      "peakActivityTimes": [
        {
          "timeSlot": "14:00-16:00",
          "averagePointsPerHour": 156.7,
          "transactionCount": 342
        },
        {
          "timeSlot": "10:00-12:00",
          "averagePointsPerHour": 134.2,
          "transactionCount": 298
        }
      ]
    },
    "topPerformers": {
      "byTotalPoints": [
        {
          "rank": 1,
          "userId": 123,
          "userName": "Ahmed Hassan",
          "totalPoints": 425,
          "pointsBreakdown": {
            "quiz": 275,
            "bonus": 150,
            "completion": 100
          }
        }
      ],
      "byQuizPerformance": [
        {
          "rank": 1,
          "userId": 145,
          "userName": "Fatima Ali",
          "averageQuizScore": 96.8,
          "quizzesTaken": 11,
          "perfectScores": 7
        }
      ],
      "byImprovement": [
        {
          "rank": 1,
          "userId": 167,
          "userName": "Omar Mahmoud",
          "improvementRate": 245.7,
          "initialScore": 45,
          "currentScore": 156,
          "timespan": "30 days"
        }
      ]
    },
    "insights": {
      "recommendations": [
        "Consider additional bonus opportunities for users in 0-50 range",
        "Peak activity is 2-4 PM - schedule important content during this time",
        "87% quiz participation rate is excellent - maintain current strategy"
      ],
      "concerns": [
        "21% course completion rate is below target of 35%",
        "23 users have earned 0 points - may need intervention"
      ],
      "achievements": [
        "Highest monthly engagement in course history",
        "Above-average bonus point distribution"
      ]
    }
  }
}
```

---

## 🔧 Administrative Operations

### 9. Update Course Rankings (`POST /update-ranks/{courseId}`)

**Authorization Required:** Admin role only

**Path Parameters:**
- `courseId` (integer) - Course identifier

**Request Body:**

```json
{
  "forceRecalculation": true,
  "includeInactiveUsers": false,
  "notifyUsers": true,
  "reason": "Manual ranking update due to system maintenance"
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Course rankings updated successfully",
  "data": {
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "updatedAt": "2025-06-27T19:30:00Z",
    "processedUsers": 247,
    "rankingChanges": 34,
    "summary": {
      "usersAffected": 34,
      "biggestRankImprovement": {
        "userId": 167,
        "userName": "Omar Mahmoud",
        "previousRank": 45,
        "newRank": 23,
        "improvement": 22
      },
      "biggestRankDrop": {
        "userId": 189,
        "userName": "Layla Hassan",
        "previousRank": 12,
        "newRank": 18,
        "drop": 6
      }
    },
    "notifications": {
      "sent": 34,
      "failed": 0,
      "types": ["email", "push"]
    },
    "nextUpdateScheduled": "2025-06-28T02:00:00Z"
  }
}
```

---

### 10. Recalculate User Points (`POST /recalculate-user-points`)

**Authorization Required:** Admin role only

**Request Body:**

```json
{
  "userId": 124,
  "courseId": 15,
  "includeQuizzes": true,
  "includeBonusPoints": true,
  "includePenalties": true,
  "includeCompletionPoints": true,
  "resetToZero": false,
  "reason": "Data inconsistency detected - manual recalculation requested"
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "User points recalculated successfully",
  "data": {
    "userId": 124,
    "userName": "Sara Ahmed",
    "courseId": 15,
    "courseName": "C# Programming Fundamentals",
    "recalculatedAt": "2025-06-27T19:45:00Z",
    "changes": {
      "previousPoints": 448,
      "newPoints": 423,
      "difference": -25,
      "previousRank": 2,
      "newRank": 3,
      "rankChange": -1
    },
    "breakdown": {
      "quizPoints": {
        "previous": 248,
        "new": 248,
        "change": 0,
        "transactionsProcessed": 11
      },
      "bonusPoints": {
        "previous": 150,
        "new": 150,
        "change": 0,
        "transactionsProcessed": 6
      },
      "penaltyPoints": {
        "previous": 0,
        "new": 25,
        "change": 25,
        "transactionsProcessed": 1
      },
      "completionPoints": {
        "previous": 100,
        "new": 100,
        "change": 0,
        "transactionsProcessed": 1
      }
    },
    "auditTrail": {
      "triggeredBy": "System Administrator",
      "reason": "Data inconsistency detected - manual recalculation requested",
      "backupCreated": true,
      "rollbackAvailable": true,
      "rollbackExpires": "2025-07-27T19:45:00Z"
    }
  }
}
```

---

## 🔧 Common Error Responses

- **401 Unauthorized** - Invalid or missing token
- **403 Forbidden** - Insufficient permissions for the operation
- **404 Not Found** - Course, user, or transaction not found
- **400 Bad Request** - Invalid input data or business rule violations
- **409 Conflict** - Operation conflicts with current state (e.g., points already awarded)
- **422 Unprocessable Entity** - Valid input but cannot process (e.g., insufficient points to deduct)
- **429 Too Many Requests** - Rate limiting applied
- **500 Internal Server Error** - Unexpected server errors

---

## 📝 Notes for Frontend Team

### Real-time Updates:
- Leaderboards update in real-time via WebSocket connections
- Rankings are recalculated automatically every hour
- Point changes trigger immediate rank updates
- Notifications are sent for significant ranking changes (±5 positions)

### Caching Strategy:
- Leaderboards are cached for 15 minutes
- User rankings are cached for 5 minutes
- Statistics are cached for 1 hour
- Admin operations bypass cache and force refresh

### Performance Considerations:
- Large leaderboards (500+ users) are paginated
- Complex statistics may take up to 10 seconds to generate
- Real-time updates are throttled to prevent spam
- Historical data beyond 1 year is archived

### Security & Privacy:
- User emails are only visible to instructors and admins
- Personal statistics require user consent or admin override
- Point transaction details include audit trails
- All point modifications are logged and immutable

### Integration Points:
- Points system integrates with quiz results automatically
- Course completion triggers point awards
- Achievement system depends on point thresholds
- Certificate generation requires minimum point threshold

---

*Last updated: 2025-06-27*