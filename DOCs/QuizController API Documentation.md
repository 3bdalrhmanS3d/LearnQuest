# LearnQuest – QuizController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/quiz`

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
| `/`                                         | POST   | Create new quiz                      | Instructor                |
| `/with-questions`                           | POST   | Create quiz with questions           | Instructor                |
| `/{quizId}`                                 | PUT    | Update existing quiz                 | Instructor                |
| `/{quizId}`                                 | DELETE | Delete quiz                          | Instructor                |
| `/{quizId}`                                 | GET    | Get quiz details                     | All authenticated users   |
| `/questions`                                | POST   | Create new question                  | Instructor                |
| `/questions/{questionId}`                   | PUT    | Update existing question             | Instructor                |
| `/questions/{questionId}`                   | DELETE | Delete question                      | Instructor                |
| `/questions/{questionId}`                   | GET    | Get question details                 | All authenticated users   |
| `/{quizId}/questions`                       | GET    | Get quiz questions                   | All authenticated users   |
| `/{quizId}/start`                           | POST   | Start quiz attempt                   | RegularUser               |
| `/{quizId}/submit`                          | POST   | Submit quiz attempt                  | RegularUser               |
| `/{quizId}/attempts`                        | GET    | Get user's quiz attempts             | All authenticated users   |
| `/{quizId}/attempts/{attemptId}`            | GET    | Get specific attempt details         | All authenticated users   |
| `/{quizId}/statistics`                      | GET    | Get quiz statistics                  | Instructor, Admin         |
| `/{quizId}/results`                         | GET    | Get quiz results summary             | Instructor, Admin         |
| `/by-type`                                  | GET    | Get quizzes by type and entity       | All authenticated users   |
| `/required-completion-check`                | GET    | Check required quiz completion       | All authenticated users   |
| `/{quizId}/can-access`                      | GET    | Check if user can access quiz        | All authenticated users   |
| `/{quizId}/can-attempt`                     | GET    | Check if user can attempt quiz       | RegularUser               |
| `/{quizId}/has-passed`                      | GET    | Check if user has passed quiz        | RegularUser               |

---

## 📝 Quiz Management (Instructor Only)

### 1. Create Quiz (`POST /`)

**Authorization Required:** Instructor role

**Request Body:**

```json
{
  "quizTitle": "Object-Oriented Programming Quiz",
  "description": "Test your understanding of OOP concepts in C#",
  "quizType": "Section",
  "entityId": 12,
  "timeLimit": 45,
  "passingScore": 70.0,
  "maxAttempts": 3,
  "isRandomized": true,
  "showCorrectAnswers": true,
  "showCorrectAnswersAfter": "Submission",
  "isActive": true,
  "availableFrom": "2025-06-28T09:00:00Z",
  "availableUntil": "2025-07-28T23:59:00Z",
  "instructions": "Read each question carefully. You have 45 minutes to complete this quiz.",
  "difficultyLevel": "Intermediate",
  "tags": ["OOP", "CSharp", "Programming"]
}
```

**Quiz Types:**
- `Content` - Associated with specific content
- `Section` - Associated with specific section
- `Level` - Associated with specific level
- `Course` - Associated with entire course

**Show Correct Answers Options:**
- `Never` - Never show correct answers
- `Submission` - Show after submission
- `Passing` - Show only after passing
- `Always` - Always show during review

**Success Response (201):**

```json
{
  "success": true,
  "message": "Quiz created successfully",
  "data": {
    "quizId": 23,
    "quizTitle": "Object-Oriented Programming Quiz",
    "description": "Test your understanding of OOP concepts in C#",
    "quizType": "Section",
    "entityId": 12,
    "timeLimit": 45,
    "passingScore": 70.0,
    "maxAttempts": 3,
    "isRandomized": true,
    "showCorrectAnswers": true,
    "showCorrectAnswersAfter": "Submission",
    "isActive": true,
    "availableFrom": "2025-06-28T09:00:00Z",
    "availableUntil": "2025-07-28T23:59:00Z",
    "difficultyLevel": "Intermediate",
    "createdAt": "2025-06-27T16:30:00Z",
    "instructorId": 456,
    "questionCount": 0,
    "totalPoints": 0
  }
}
```

---

### 2. Create Quiz with Questions (`POST /with-questions`)

**Authorization Required:** Instructor role

**Request Body:**

```json
{
  "quiz": {
    "quizTitle": "JavaScript Fundamentals Quiz",
    "description": "Test your basic JavaScript knowledge",
    "quizType": "Section",
    "entityId": 15,
    "timeLimit": 30,
    "passingScore": 75.0,
    "maxAttempts": 2,
    "isRandomized": false,
    "showCorrectAnswers": true,
    "showCorrectAnswersAfter": "Passing",
    "isActive": true,
    "availableFrom": "2025-06-28T10:00:00Z",
    "availableUntil": "2025-08-28T23:59:00Z"
  },
  "questions": [
    {
      "questionText": "What is the correct way to declare a variable in JavaScript?",
      "questionType": "MultipleChoice",
      "points": 2,
      "difficultyLevel": "Beginner",
      "explanation": "In modern JavaScript, 'let' and 'const' are preferred over 'var'",
      "choices": [
        {
          "choiceText": "var x = 5;",
          "isCorrect": false,
          "explanation": "While valid, 'var' has function scope issues"
        },
        {
          "choiceText": "let x = 5;",
          "isCorrect": true,
          "explanation": "Correct! 'let' provides block scope"
        },
        {
          "choiceText": "x = 5;",
          "isCorrect": false,
          "explanation": "This creates a global variable, which is not recommended"
        },
        {
          "choiceText": "const x = 5;",
          "isCorrect": true,
          "explanation": "Correct! 'const' is used for constants"
        }
      ]
    },
    {
      "questionText": "Explain the difference between 'let' and 'const' in JavaScript.",
      "questionType": "Essay",
      "points": 5,
      "difficultyLevel": "Intermediate",
      "explanation": "Key differences include mutability and reassignment capabilities",
      "maxLength": 500,
      "keywords": ["scope", "reassignment", "immutable", "block"]
    }
  ]
}
```

**Question Types:**
- `MultipleChoice` - Single correct answer
- `MultipleAnswer` - Multiple correct answers
- `TrueFalse` - True or false question
- `Essay` - Text-based answer
- `FillInBlank` - Fill in the missing words
- `Matching` - Match pairs of items
- `Ordering` - Put items in correct order

**Success Response (201):**

```json
{
  "success": true,
  "message": "Quiz created with questions successfully",
  "data": {
    "quizId": 24,
    "quizTitle": "JavaScript Fundamentals Quiz",
    "questionCount": 2,
    "totalPoints": 7,
    "estimatedDuration": 30,
    "createdAt": "2025-06-27T16:45:00Z"
  }
}
```

---

### 3. Update Quiz (`PUT /{quizId}`)

**Path Parameter:** `quizId` (integer)
**Authorization Required:** Instructor role (quiz owner)

**Request Body:**

```json
{
  "quizId": 23,
  "quizTitle": "Advanced OOP Concepts Quiz",
  "description": "Updated description with more comprehensive coverage",
  "timeLimit": 60,
  "passingScore": 75.0,
  "maxAttempts": 2,
  "isRandomized": true,
  "showCorrectAnswers": true,
  "showCorrectAnswersAfter": "Passing",
  "isActive": true,
  "availableFrom": "2025-06-29T09:00:00Z",
  "availableUntil": "2025-07-29T23:59:00Z"
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Quiz updated successfully",
  "data": {
    "quizId": 23,
    "updatedFields": [
      "quizTitle",
      "timeLimit",
      "passingScore",
      "availableFrom"
    ],
    "updatedAt": "2025-06-27T16:50:00Z"
  }
}
```

---

### 4. Delete Quiz (`DELETE /{quizId}`)

**Path Parameter:** `quizId` (integer)
**Authorization Required:** Instructor role (quiz owner)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Quiz deleted successfully",
  "data": {
    "quizId": 23,
    "deletedAt": "2025-06-27T16:55:00Z",
    "affectedAttempts": 15,
    "notificationsSent": 8
  }
}
```

---

## 📊 Question Management (Instructor Only)

### 5. Create Question (`POST /questions`)

**Authorization Required:** Instructor role

**Request Body:**

```json
{
  "quizId": 23,
  "questionText": "Which principle of OOP allows a class to inherit properties from another class?",
  "questionType": "MultipleChoice",
  "points": 3,
  "difficultyLevel": "Intermediate",
  "explanation": "Inheritance allows child classes to inherit properties and methods from parent classes",
  "order": 1,
  "choices": [
    {
      "choiceText": "Encapsulation",
      "isCorrect": false,
      "explanation": "Encapsulation is about data hiding and bundling"
    },
    {
      "choiceText": "Inheritance",
      "isCorrect": true,
      "explanation": "Correct! Inheritance allows classes to inherit from other classes"
    },
    {
      "choiceText": "Polymorphism",
      "isCorrect": false,
      "explanation": "Polymorphism is about multiple forms of the same method"
    },
    {
      "choiceText": "Abstraction",
      "isCorrect": false,
      "explanation": "Abstraction is about hiding complex implementation details"
    }
  ]
}
```

**Success Response (201):**

```json
{
  "success": true,
  "message": "Question created successfully",
  "data": {
    "questionId": 156,
    "quizId": 23,
    "questionText": "Which principle of OOP allows a class to inherit properties from another class?",
    "questionType": "MultipleChoice",
    "points": 3,
    "order": 1,
    "choiceCount": 4,
    "createdAt": "2025-06-27T17:00:00Z"
  }
}
```

---

### 6. Update Question (`PUT /questions/{questionId}`)

**Path Parameters:** `questionId` (integer)
**Authorization Required:** Instructor role (quiz owner)

**Request Body:**

```json
{
  "questionId": 156,
  "questionText": "Which OOP principle allows a derived class to inherit properties and methods from a base class?",
  "questionType": "MultipleChoice",
  "points": 4,
  "difficultyLevel": "Intermediate",
  "explanation": "Updated explanation with more detail about inheritance",
  "order": 1
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Question updated successfully",
  "data": {
    "questionId": 156,
    "updatedFields": [
      "questionText",
      "points",
      "explanation"
    ],
    "updatedAt": "2025-06-27T17:05:00Z"
  }
}
```

---

## 🎯 Quiz Taking (Student Operations)

### 7. Start Quiz Attempt (`POST /{quizId}/start`)

**Path Parameter:** `quizId` (integer)
**Authorization Required:** RegularUser role

**Success Response (200):**

```json
{
  "success": true,
  "message": "Quiz attempt started successfully",
  "data": {
    "attemptId": 789,
    "quizId": 23,
    "quizTitle": "Object-Oriented Programming Quiz",
    "timeLimit": 45,
    "startedAt": "2025-06-27T17:10:00Z",
    "expiresAt": "2025-06-27T17:55:00Z",
    "totalQuestions": 15,
    "totalPoints": 45,
    "attemptNumber": 1,
    "maxAttempts": 3,
    "instructions": "Read each question carefully. You have 45 minutes to complete this quiz.",
    "questions": [
      {
        "questionId": 156,
        "questionText": "Which OOP principle allows a derived class to inherit properties and methods from a base class?",
        "questionType": "MultipleChoice",
        "points": 4,
        "order": 1,
        "choices": [
          {
            "choiceId": 623,
            "choiceText": "Encapsulation"
          },
          {
            "choiceId": 624,
            "choiceText": "Inheritance"
          },
          {
            "choiceId": 625,
            "choiceText": "Polymorphism"
          },
          {
            "choiceId": 626,
            "choiceText": "Abstraction"
          }
        ]
      }
    ],
    "navigationSettings": {
      "allowBackward": true,
      "showProgress": true,
      "showTimeRemaining": true,
      "autoSubmitOnTimeout": true
    }
  }
}
```

**Errors:**
- `400 Bad Request` - Already has an active attempt, max attempts reached, or quiz not available
- `403 Forbidden` - Quiz access denied or prerequisites not met

---

### 8. Submit Quiz Attempt (`POST /{quizId}/submit`)

**Path Parameter:** `quizId` (integer)
**Authorization Required:** RegularUser role

**Request Body:**

```json
{
  "attemptId": 789,
  "answers": [
    {
      "questionId": 156,
      "selectedChoiceIds": [624],
      "timeSpent": 45
    },
    {
      "questionId": 157,
      "essayAnswer": "Polymorphism allows objects of different types to be treated as objects of a common base type...",
      "timeSpent": 180
    }
  ],
  "totalTimeSpent": 2340,
  "submitType": "Final"
}
```

**Submit Types:**
- `Draft` - Save progress without final submission
- `Final` - Final submission for grading

**Success Response (200):**

```json
{
  "success": true,
  "message": "Quiz submitted successfully",
  "data": {
    "attemptId": 789,
    "quizId": 23,
    "score": 38,
    "totalPoints": 45,
    "percentage": 84.4,
    "isPassed": true,
    "passingScore": 70.0,
    "submittedAt": "2025-06-27T17:45:00Z",
    "timeSpent": 2340,
    "gradingStatus": "Completed",
    "feedback": "Excellent work! You demonstrated a strong understanding of OOP concepts.",
    "results": [
      {
        "questionId": 156,
        "isCorrect": true,
        "pointsEarned": 4,
        "pointsPossible": 4,
        "feedback": "Correct! Inheritance is the right answer."
      },
      {
        "questionId": 157,
        "isCorrect": false,
        "pointsEarned": 2,
        "pointsPossible": 5,
        "feedback": "Good explanation but missing key concepts about runtime polymorphism."
      }
    ],
    "nextAttemptAvailableAt": null,
    "certificateEarned": false,
    "pointsAwarded": 15,
    "achievements": [
      {
        "achievementId": 7,
        "title": "Quiz Master",
        "description": "Scored 80% or higher on your first attempt"
      }
    ]
  }
}
```

---

## 📈 Quiz Statistics & Results

### 9. Get Quiz Statistics (`GET /{quizId}/statistics`)

**Path Parameter:** `quizId` (integer)
**Authorization Required:** Instructor or Admin role

**Query Parameters:**
- `startDate` (string, optional) - Filter attempts from date
- `endDate` (string, optional) - Filter attempts to date
- `includeDetails` (boolean, optional) - Include detailed breakdown

**Success Response (200):**

```json
{
  "success": true,
  "message": "Quiz statistics retrieved successfully",
  "data": {
    "quizId": 23,
    "quizTitle": "Object-Oriented Programming Quiz",
    "period": {
      "startDate": "2025-06-01T00:00:00Z",
      "endDate": "2025-06-27T23:59:59Z"
    },
    "overview": {
      "totalAttempts": 147,
      "uniqueStudents": 52,
      "passedAttempts": 89,
      "failedAttempts": 58,
      "passRate": 60.5,
      "averageScore": 73.2,
      "averageTimeSpent": 2156,
      "completionRate": 94.2
    },
    "scoreDistribution": [
      { "range": "90-100", "count": 23, "percentage": 15.6 },
      { "range": "80-89", "count": 31, "percentage": 21.1 },
      { "range": "70-79", "count": 35, "percentage": 23.8 },
      { "range": "60-69", "count": 28, "percentage": 19.0 },
      { "range": "0-59", "count": 30, "percentage": 20.4 }
    ],
    "questionAnalysis": [
      {
        "questionId": 156,
        "questionText": "Which OOP principle allows inheritance...",
        "correctAnswerRate": 82.3,
        "averageTimeSpent": 45,
        "difficultyRating": "Easy",
        "mostSelectedChoice": "Inheritance (82.3%)",
        "commonMistakes": [
          {
            "choice": "Polymorphism",
            "selectedBy": 15.6,
            "reason": "Confusion between inheritance and polymorphism"
          }
        ]
      }
    ],
    "timeAnalysis": {
      "averageCompletionTime": 2156,
      "fastestCompletion": 1204,
      "slowestCompletion": 2640,
      "timeoutCount": 8,
      "earlySubmissions": 23
    },
    "trends": {
      "scoreImprovement": 5.2,
      "timeEfficiency": 8.7,
      "retakeSuccessRate": 78.5
    }
  }
}
```

---

### 10. Get User's Quiz Attempts (`GET /{quizId}/attempts`)

**Path Parameter:** `quizId` (integer)

**Query Parameters:**
- `pageNumber` (integer, optional) - Page number (default: 1)
- `pageSize` (integer, optional) - Page size (default: 10)
- `status` (string, optional) - Filter by status: "Completed", "InProgress", "Abandoned"

**Success Response (200):**

```json
{
  "success": true,
  "message": "Quiz attempts retrieved successfully",
  "data": {
    "quizId": 23,
    "quizTitle": "Object-Oriented Programming Quiz",
    "userStats": {
      "totalAttempts": 2,
      "bestScore": 84.4,
      "bestScoreAttempt": 1,
      "isPassed": true,
      "remainingAttempts": 1,
      "averageScore": 76.7
    },
    "attempts": [
      {
        "attemptId": 789,
        "attemptNumber": 1,
        "status": "Completed",
        "score": 38,
        "totalPoints": 45,
        "percentage": 84.4,
        "isPassed": true,
        "startedAt": "2025-06-27T17:10:00Z",
        "submittedAt": "2025-06-27T17:45:00Z",
        "timeSpent": 2340,
        "gradingStatus": "Completed"
      },
      {
        "attemptId": 790,
        "attemptNumber": 2,
        "status": "Completed",
        "score": 31,
        "totalPoints": 45,
        "percentage": 68.9,
        "isPassed": false,
        "startedAt": "2025-06-28T10:15:00Z",
        "submittedAt": "2025-06-28T10:52:00Z",
        "timeSpent": 2220,
        "gradingStatus": "Completed"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 10,
      "totalItems": 2,
      "hasNext": false
    }
  }
}
```

---

## 🔍 Quiz Discovery & Validation

### 11. Get Quizzes by Type (`GET /by-type`)

**Query Parameters:**
- `quizType` (string, required) - Quiz type: "Content", "Section", "Level", "Course"
- `entityId` (integer, optional) - Entity ID to filter by

**Success Response (200):**

```json
{
  "success": true,
  "message": "Quizzes retrieved successfully",
  "data": {
    "quizType": "Section",
    "entityId": 12,
    "quizzes": [
      {
        "quizId": 23,
        "quizTitle": "Object-Oriented Programming Quiz",
        "description": "Test your understanding of OOP concepts",
        "timeLimit": 45,
        "questionCount": 15,
        "totalPoints": 45,
        "passingScore": 70.0,
        "maxAttempts": 3,
        "difficultyLevel": "Intermediate",
        "isActive": true,
        "availableFrom": "2025-06-28T09:00:00Z",
        "availableUntil": "2025-07-28T23:59:00Z",
        "userStats": {
          "hasAttempted": true,
          "bestScore": 84.4,
          "isPassed": true,
          "attemptsUsed": 2,
          "canAttempt": true
        }
      }
    ]
  }
}
```

---

### 12. Check Required Quiz Completion (`GET /required-completion-check`)

**Query Parameters:**
- `contentId` (integer, optional) - Content ID
- `sectionId` (integer, optional) - Section ID  
- `levelId` (integer, optional) - Level ID
- `courseId` (integer, optional) - Course ID

**Success Response (200):**

```json
{
  "success": true,
  "message": "Required quiz completion check completed",
  "data": {
    "allRequiredCompleted": false,
    "requiredQuizzes": [
      {
        "quizId": 23,
        "quizTitle": "OOP Fundamentals Quiz",
        "isRequired": true,
        "isPassed": true,
        "bestScore": 84.4,
        "passingScore": 70.0
      },
      {
        "quizId": 25,
        "quizTitle": "Advanced OOP Quiz",
        "isRequired": true,
        "isPassed": false,
        "bestScore": 65.2,
        "passingScore": 70.0,
        "canRetake": true,
        "attemptsRemaining": 1
      }
    ],
    "blockingReason": "Required quiz 'Advanced OOP Quiz' not passed",
    "nextAvailableContent": null
  }
}
```

---

### 13. Check Quiz Access (`GET /{quizId}/can-access`)

**Path Parameter:** `quizId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Quiz access check completed",
  "data": {
    "canAccess": true,
    "accessReason": "All prerequisites met",
    "prerequisites": [
      {
        "type": "Content",
        "entityId": 67,
        "title": "Introduction to OOP",
        "isMet": true
      }
    ],
    "restrictions": {
      "timeWindow": {
        "isWithinWindow": true,
        "availableFrom": "2025-06-28T09:00:00Z",
        "availableUntil": "2025-07-28T23:59:00Z"
      },
      "attempts": {
        "hasAttemptsLeft": true,
        "attemptsUsed": 2,
        "maxAttempts": 3
      },
      "courseEnrollment": {
        "isEnrolled": true,
        "enrollmentStatus": "Active"
      }
    }
  }
}
```

---

## 🔧 Common Error Responses

- **401 Unauthorized** - Invalid or missing token
- **403 Forbidden** - Insufficient permissions or access denied
- **404 Not Found** - Quiz, question, or attempt not found
- **400 Bad Request** - Invalid input data, already started, or validation errors
- **409 Conflict** - Quiz already submitted or attempt limit reached
- **422 Unprocessable Entity** - Business rule violations
- **500 Internal Server Error** - Unexpected server errors

---

## 📝 Notes for Frontend Team

- Quiz attempts expire based on the `timeLimit` setting
- Questions can be randomized if `isRandomized` is true
- Essay questions require manual grading by instructors
- Multiple choice questions support multiple correct answers
- Quiz progress is auto-saved every 30 seconds during attempt
- Time remaining should be displayed and synced with server
- Handle connection loss gracefully with auto-reconnection
- Support offline mode for quiz taking (sync when reconnected)
- Implement proper validation for required fields
- All timestamps are in UTC format
- File attachments are supported for questions via metadata
- Quiz results may include rich feedback with formatting

---

*Last updated: 2025-06-27*