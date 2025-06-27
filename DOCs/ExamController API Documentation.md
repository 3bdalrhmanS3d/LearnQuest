# LearnQuest – ExamController API Documentation (Frontend Contract)

> **Base URL:** `https://localhost:7217/api/exam`

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
| `/`                                         | POST   | Create new exam                      | Instructor                |
| `/with-questions`                           | POST   | Create exam with questions           | Instructor                |
| `/{examId}`                                 | PUT    | Update existing exam                 | Instructor                |
| `/{examId}`                                 | DELETE | Delete exam                          | Instructor                |
| `/{examId}`                                 | GET    | Get exam details                     | All authenticated users   |
| `/{examId}/schedule`                        | POST   | Schedule exam session                | Instructor                |
| `/{examId}/sessions`                        | GET    | Get exam sessions                    | Instructor, Admin         |
| `/{examId}/sessions/{sessionId}`            | GET    | Get session details                  | All authenticated users   |
| `/{examId}/register`                        | POST   | Register for exam session           | RegularUser               |
| `/{examId}/unregister`                      | DELETE | Unregister from exam session        | RegularUser               |
| `/{examId}/start`                           | POST   | Start exam attempt                   | RegularUser               |
| `/{examId}/submit`                          | POST   | Submit exam attempt                  | RegularUser               |
| `/{examId}/proctor/start`                   | POST   | Start proctoring session             | Instructor, Admin         |
| `/{examId}/proctor/monitor`                 | GET    | Monitor exam session                 | Instructor, Admin         |
| `/{examId}/attempts`                        | GET    | Get exam attempts                    | All authenticated users   |
| `/{examId}/attempts/{attemptId}`            | GET    | Get specific attempt details         | All authenticated users   |
| `/{examId}/attempts/{attemptId}/review`     | GET    | Review exam attempt                  | Instructor, Admin         |
| `/{examId}/results`                         | GET    | Get exam results                     | Instructor, Admin         |
| `/{examId}/statistics`                      | GET    | Get exam statistics                  | Instructor, Admin         |
| `/{examId}/certificate/{attemptId}`         | GET    | Get exam certificate                 | All authenticated users   |

---

## 📝 Exam Management (Instructor Only)

### 1. Create Exam (`POST /`)

**Authorization Required:** Instructor role

**Request Body:**

```json
{
  "examTitle": "Final Programming Certification Exam",
  "description": "Comprehensive examination covering all programming concepts taught in the course",
  "courseId": 15,
  "examType": "Final",
  "duration": 120,
  "passingScore": 75.0,
  "maxAttempts": 1,
  "isProctored": true,
  "allowedDevices": ["desktop"],
  "browserLockdown": true,
  "recordScreen": true,
  "recordAudio": false,
  "recordWebcam": true,
  "preventCopyPaste": true,
  "shuffleQuestions": true,
  "shuffleChoices": true,
  "showResults": "AfterGrading",
  "allowReview": false,
  "certificateTemplate": "programming_cert_template_v2",
  "instructions": "This is a proctored exam. Ensure you are in a quiet environment with good internet connection.",
  "prerequisites": [
    {
      "type": "Quiz",
      "entityId": 23,
      "mustPass": true
    },
    {
      "type": "Assignment",
      "entityId": 12,
      "mustPass": true
    }
  ],
  "schedulingSettings": {
    "requiresRegistration": true,
    "registrationDeadline": "2025-07-10T23:59:00Z",
    "maxParticipantsPerSession": 50,
    "timeZone": "Africa/Cairo"
  }
}
```

**Exam Types:**
- `Midterm` - Mid-term examination
- `Final` - Final examination
- `Certification` - Certification exam
- `Assessment` - General assessment
- `Placement` - Placement test

**Show Results Options:**
- `Immediately` - Show results immediately after submission
- `AfterGrading` - Show results after manual grading is complete
- `AfterDeadline` - Show results after exam deadline
- `Never` - Never show results to students

**Success Response (201):**

```json
{
  "success": true,
  "message": "Exam created successfully",
  "data": {
    "examId": 45,
    "examTitle": "Final Programming Certification Exam",
    "description": "Comprehensive examination covering all programming concepts",
    "courseId": 15,
    "examType": "Final",
    "duration": 120,
    "passingScore": 75.0,
    "maxAttempts": 1,
    "isProctored": true,
    "schedulingSettings": {
      "requiresRegistration": true,
      "registrationDeadline": "2025-07-10T23:59:00Z",
      "maxParticipantsPerSession": 50
    },
    "createdAt": "2025-06-27T18:00:00Z",
    "instructorId": 456,
    "questionCount": 0,
    "totalPoints": 0,
    "status": "Draft"
  }
}
```

---

### 2. Create Exam with Questions (`POST /with-questions`)

**Authorization Required:** Instructor role

**Request Body:**

```json
{
  "exam": {
    "examTitle": "Advanced JavaScript Certification",
    "description": "Advanced JavaScript concepts and patterns",
    "courseId": 18,
    "examType": "Certification",
    "duration": 90,
    "passingScore": 80.0,
    "maxAttempts": 2,
    "isProctored": true,
    "browserLockdown": true,
    "recordWebcam": true,
    "shuffleQuestions": true
  },
  "questions": [
    {
      "questionText": "Explain the concept of closures in JavaScript with a practical example.",
      "questionType": "Essay",
      "points": 10,
      "difficultyLevel": "Advanced",
      "timeLimit": 15,
      "explanation": "Closures allow functions to access variables from outer scope even after the outer function has returned",
      "rubric": [
        {
          "criterion": "Understanding of closures",
          "points": 4,
          "description": "Demonstrates clear understanding of closure concept"
        },
        {
          "criterion": "Practical example",
          "points": 3,
          "description": "Provides a working code example"
        },
        {
          "criterion": "Explanation clarity",
          "points": 3,
          "description": "Clear and well-structured explanation"
        }
      ]
    },
    {
      "questionText": "Which of the following statements about async/await are correct?",
      "questionType": "MultipleAnswer",
      "points": 5,
      "difficultyLevel": "Intermediate",
      "timeLimit": 5,
      "choices": [
        {
          "choiceText": "async/await is syntactic sugar over Promises",
          "isCorrect": true,
          "explanation": "Correct! async/await provides a cleaner syntax for Promise-based code"
        },
        {
          "choiceText": "await can only be used inside async functions",
          "isCorrect": true,
          "explanation": "Correct! await keyword requires an async function context"
        },
        {
          "choiceText": "async functions always return a Promise",
          "isCorrect": true,
          "explanation": "Correct! async functions implicitly return a Promise"
        },
        {
          "choiceText": "await makes JavaScript synchronous",
          "isCorrect": false,
          "explanation": "Incorrect! await pauses execution but doesn't make JavaScript synchronous"
        }
      ]
    }
  ]
}
```

**Success Response (201):**

```json
{
  "success": true,
  "message": "Exam created with questions successfully",
  "data": {
    "examId": 46,
    "examTitle": "Advanced JavaScript Certification",
    "questionCount": 2,
    "totalPoints": 15,
    "estimatedDuration": 20,
    "createdAt": "2025-06-27T18:15:00Z",
    "status": "Draft"
  }
}
```

---

## 📅 Exam Scheduling

### 3. Schedule Exam Session (`POST /{examId}/schedule`)

**Path Parameter:** `examId` (integer)
**Authorization Required:** Instructor role

**Request Body:**

```json
{
  "sessionName": "Morning Session - Batch A",
  "startDateTime": "2025-07-15T09:00:00Z",
  "endDateTime": "2025-07-15T11:30:00Z",
  "timeZone": "Africa/Cairo",
  "maxParticipants": 25,
  "location": "Computer Lab 1",
  "proctor": {
    "userId": 789,
    "name": "Dr. Sarah Ahmed",
    "email": "sarah.ahmed@learnquest.com"
  },
  "registrationOpenAt": "2025-07-01T00:00:00Z",
  "registrationCloseAt": "2025-07-10T23:59:00Z",
  "allowLateRegistration": false,
  "notificationSettings": {
    "sendRegistrationConfirmation": true,
    "sendReminderNotifications": true,
    "reminderTimes": [48, 24, 2]
  }
}
```

**Success Response (201):**

```json
{
  "success": true,
  "message": "Exam session scheduled successfully",
  "data": {
    "sessionId": 123,
    "examId": 45,
    "sessionName": "Morning Session - Batch A",
    "startDateTime": "2025-07-15T09:00:00Z",
    "endDateTime": "2025-07-15T11:30:00Z",
    "maxParticipants": 25,
    "currentRegistrations": 0,
    "registrationOpenAt": "2025-07-01T00:00:00Z",
    "registrationCloseAt": "2025-07-10T23:59:00Z",
    "status": "Scheduled",
    "proctorAssigned": true,
    "createdAt": "2025-06-27T18:30:00Z"
  }
}
```

---

### 4. Get Exam Sessions (`GET /{examId}/sessions`)

**Path Parameter:** `examId` (integer)
**Authorization Required:** Instructor or Admin role

**Query Parameters:**
- `status` (string, optional) - Filter by session status
- `startDate` (string, optional) - Filter sessions from date
- `endDate` (string, optional) - Filter sessions to date

**Success Response (200):**

```json
{
  "success": true,
  "message": "Exam sessions retrieved successfully",
  "data": {
    "examId": 45,
    "examTitle": "Final Programming Certification Exam",
    "totalSessions": 3,
    "sessions": [
      {
        "sessionId": 123,
        "sessionName": "Morning Session - Batch A",
        "startDateTime": "2025-07-15T09:00:00Z",
        "endDateTime": "2025-07-15T11:30:00Z",
        "maxParticipants": 25,
        "registeredCount": 18,
        "checkedInCount": 0,
        "completedCount": 0,
        "status": "Scheduled",
        "location": "Computer Lab 1",
        "proctor": {
          "name": "Dr. Sarah Ahmed",
          "email": "sarah.ahmed@learnquest.com"
        },
        "registrationStatus": {
          "isOpen": true,
          "openAt": "2025-07-01T00:00:00Z",
          "closeAt": "2025-07-10T23:59:00Z"
        }
      },
      {
        "sessionId": 124,
        "sessionName": "Afternoon Session - Batch B",
        "startDateTime": "2025-07-15T14:00:00Z",
        "endDateTime": "2025-07-15T16:30:00Z",
        "maxParticipants": 25,
        "registeredCount": 22,
        "checkedInCount": 0,
        "completedCount": 0,
        "status": "Scheduled",
        "location": "Computer Lab 2"
      }
    ]
  }
}
```

---

## 🎓 Student Exam Operations

### 5. Register for Exam (`POST /{examId}/register`)

**Path Parameter:** `examId` (integer)
**Authorization Required:** RegularUser role

**Request Body:**

```json
{
  "sessionId": 123,
  "preferredSeating": "Front row",
  "specialAccommodations": "Extra time due to disability",
  "emergencyContact": {
    "name": "John Doe",
    "phone": "+20123456789",
    "relationship": "Father"
  },
  "agreementSigned": true,
  "identityDocument": {
    "type": "National ID",
    "number": "29512345678901"
  }
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Successfully registered for exam session",
  "data": {
    "registrationId": 456,
    "examId": 45,
    "sessionId": 123,
    "examTitle": "Final Programming Certification Exam",
    "sessionDetails": {
      "sessionName": "Morning Session - Batch A",
      "startDateTime": "2025-07-15T09:00:00Z",
      "duration": 120,
      "location": "Computer Lab 1"
    },
    "registeredAt": "2025-06-27T18:45:00Z",
    "confirmationCode": "EXAM-45-123-456",
    "checkinInstructions": "Please arrive 30 minutes early with a valid ID",
    "requirements": [
      "Valid government-issued photo ID",
      "Laptop with webcam and microphone",
      "Stable internet connection",
      "Quiet environment"
    ],
    "reminders": [
      {
        "type": "Email",
        "scheduledFor": "2025-07-13T09:00:00Z",
        "message": "Exam reminder - 2 days before"
      },
      {
        "type": "SMS",
        "scheduledFor": "2025-07-15T07:00:00Z",
        "message": "Exam today - Check in starts at 8:30 AM"
      }
    ]
  }
}
```

---

### 6. Start Exam Attempt (`POST /{examId}/start`)

**Path Parameter:** `examId` (integer)
**Authorization Required:** RegularUser role

**Request Body:**

```json
{
  "sessionId": 123,
  "confirmationCode": "EXAM-45-123-456",
  "identityVerification": {
    "photoUrl": "/uploads/identity/user_123_exam_45.jpg",
    "documentNumber": "29512345678901"
  },
  "systemCheck": {
    "webcamWorking": true,
    "microphoneWorking": true,
    "screenSharing": true,
    "browserCompatible": true,
    "internetSpeed": 25.6
  }
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Exam attempt started successfully",
  "data": {
    "attemptId": 789,
    "examId": 45,
    "sessionId": 123,
    "examTitle": "Final Programming Certification Exam",
    "duration": 120,
    "startedAt": "2025-07-15T09:05:00Z",
    "mustFinishBy": "2025-07-15T11:05:00Z",
    "totalQuestions": 50,
    "totalPoints": 100,
    "proctoring": {
      "isActive": true,
      "proctorId": 789,
      "sessionToken": "prct_abc123xyz",
      "recordingStarted": true,
      "monitoringLevel": "High"
    },
    "securitySettings": {
      "browserLockdown": true,
      "preventTabSwitching": true,
      "preventRightClick": true,
      "preventCopyPaste": true,
      "fullScreenRequired": true
    },
    "questions": [
      {
        "questionId": 201,
        "questionNumber": 1,
        "questionText": "What is the time complexity of binary search?",
        "questionType": "MultipleChoice",
        "points": 2,
        "timeLimit": 3,
        "choices": [
          {
            "choiceId": 801,
            "choiceText": "O(n)"
          },
          {
            "choiceId": 802,
            "choiceText": "O(log n)"
          },
          {
            "choiceId": 803,
            "choiceText": "O(n log n)"
          },
          {
            "choiceId": 804,
            "choiceText": "O(n²)"
          }
        ]
      }
    ],
    "navigation": {
      "allowBackward": false,
      "allowSkip": true,
      "showProgress": true,
      "showTimeRemaining": true,
      "autoSubmit": true
    }
  }
}
```

---

### 7. Submit Exam Attempt (`POST /{examId}/submit`)

**Path Parameter:** `examId` (integer)
**Authorization Required:** RegularUser role

**Request Body:**

```json
{
  "attemptId": 789,
  "sessionId": 123,
  "answers": [
    {
      "questionId": 201,
      "selectedChoiceIds": [802],
      "timeSpent": 125,
      "flagged": false
    },
    {
      "questionId": 202,
      "essayAnswer": "Polymorphism in object-oriented programming allows objects of different types to be treated as objects of a common base type...",
      "timeSpent": 780,
      "wordCount": 245,
      "flagged": true,
      "note": "Review this answer"
    }
  ],
  "totalTimeSpent": 7140,
  "submissionType": "Final",
  "integrityStatement": "I affirm that this work is my own and I have not received unauthorized assistance",
  "proctoringSummary": {
    "suspiciousEvents": 0,
    "warnings": 1,
    "tabSwitches": 0,
    "screenShareInterruptions": 0
  }
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Exam submitted successfully",
  "data": {
    "attemptId": 789,
    "examId": 45,
    "sessionId": 123,
    "submittedAt": "2025-07-15T11:00:00Z",
    "totalTimeSpent": 7140,
    "autoGradedScore": 75,
    "manualGradingRequired": true,
    "estimatedTotalScore": 85,
    "gradingStatus": "Pending",
    "expectedResultsDate": "2025-07-18T17:00:00Z",
    "confirmationNumber": "EXAM-SUB-789-2025",
    "proctorReport": {
      "overallRating": "Clean",
      "suspiciousEvents": 0,
      "integrityScore": 98.5,
      "notes": "Professional conduct throughout the exam"
    },
    "nextSteps": [
      "Results will be available within 3 business days",
      "Check your email for official notification",
      "Certificate will be generated automatically upon passing"
    ]
  }
}
```

---

## 👨‍🏫 Proctoring & Monitoring

### 8. Start Proctoring Session (`POST /{examId}/proctor/start`)

**Path Parameter:** `examId` (integer)
**Authorization Required:** Instructor or Admin role

**Request Body:**

```json
{
  "sessionId": 123,
  "proctorSettings": {
    "monitoringLevel": "High",
    "alertThresholds": {
      "faceDetectionLoss": 5,
      "multipeFacesDetected": 3,
      "eyeGazeDeviation": 10,
      "suspiciousAudio": 3
    },
    "autoInterventions": {
      "pauseOnFaceLoss": true,
      "alertOnTabSwitch": true,
      "recordSuspiciousActivity": true
    }
  }
}
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Proctoring session started successfully",
  "data": {
    "proctorSessionId": "prct_123_2025071509",
    "examId": 45,
    "sessionId": 123,
    "startedAt": "2025-07-15T09:00:00Z",
    "monitoringLevel": "High",
    "activeParticipants": 18,
    "monitoringDashboardUrl": "/proctor/dashboard/123",
    "emergencyControls": {
      "pauseAllExams": "/api/exam/45/proctor/pause-all",
      "broadcastMessage": "/api/exam/45/proctor/broadcast",
      "evacuateSession": "/api/exam/45/proctor/evacuate"
    }
  }
}
```

---

### 9. Monitor Exam Session (`GET /{examId}/proctor/monitor`)

**Path Parameter:** `examId` (integer)
**Authorization Required:** Instructor or Admin role

**Query Parameters:**
- `sessionId` (integer, required) - Session to monitor
- `alertsOnly` (boolean, optional) - Show only participants with alerts

**Success Response (200):**

```json
{
  "success": true,
  "message": "Exam monitoring data retrieved successfully",
  "data": {
    "sessionId": 123,
    "sessionStatus": "Active",
    "startedAt": "2025-07-15T09:00:00Z",
    "timeRemaining": 3600,
    "overview": {
      "totalParticipants": 18,
      "activeAttempts": 16,
      "completedAttempts": 0,
      "flaggedParticipants": 2,
      "technicalIssues": 1
    },
    "participants": [
      {
        "userId": 123,
        "userName": "Ahmed Hassan",
        "attemptId": 789,
        "status": "Active",
        "progress": 45.5,
        "timeSpent": 3240,
        "currentQuestion": 23,
        "lastActivity": "2025-07-15T10:15:00Z",
        "integrityScore": 98.5,
        "alerts": [],
        "biometrics": {
          "faceDetected": true,
          "eyeGazeOnScreen": true,
          "multipleFaces": false,
          "audioLevel": 25.3
        },
        "technology": {
          "connectionStatus": "Stable",
          "bandwidthQuality": "Good",
          "webcamStatus": "Active",
          "screenShareStatus": "Active"
        }
      },
      {
        "userId": 124,
        "userName": "Sara Ahmed",
        "attemptId": 790,
        "status": "Flagged",
        "progress": 38.2,
        "timeSpent": 2890,
        "currentQuestion": 19,
        "lastActivity": "2025-07-15T10:14:00Z",
        "integrityScore": 76.8,
        "alerts": [
          {
            "type": "FaceDetectionLoss",
            "timestamp": "2025-07-15T10:12:00Z",
            "severity": "Medium",
            "description": "Face not detected for 8 seconds",
            "action": "Warning sent"
          },
          {
            "type": "SuspiciousAudio",
            "timestamp": "2025-07-15T10:08:00Z",
            "severity": "Low",
            "description": "Conversation detected in background",
            "action": "Recorded for review"
          }
        ]
      }
    ],
    "systemAlerts": [
      {
        "type": "NetworkIssue",
        "timestamp": "2025-07-15T10:10:00Z",
        "affectedUsers": 3,
        "description": "Temporary connectivity issues detected",
        "resolution": "Monitoring connection quality"
      }
    ]
  }
}
```

---

## 📊 Results & Analytics

### 10. Get Exam Results (`GET /{examId}/results`)

**Path Parameter:** `examId` (integer)
**Authorization Required:** Instructor or Admin role

**Query Parameters:**
- `sessionId` (integer, optional) - Filter by session
- `status` (string, optional) - Filter by grading status
- `exportFormat` (string, optional) - Export format: "json", "csv", "pdf"

**Success Response (200):**

```json
{
  "success": true,
  "message": "Exam results retrieved successfully",
  "data": {
    "examId": 45,
    "examTitle": "Final Programming Certification Exam",
    "summary": {
      "totalAttempts": 47,
      "gradedAttempts": 45,
      "pendingGrading": 2,
      "passedAttempts": 32,
      "failedAttempts": 13,
      "passRate": 71.1,
      "averageScore": 78.4,
      "highestScore": 96.5,
      "lowestScore": 42.0
    },
    "results": [
      {
        "attemptId": 789,
        "userId": 123,
        "userName": "Ahmed Hassan",
        "sessionId": 123,
        "submittedAt": "2025-07-15T11:00:00Z",
        "totalScore": 87.5,
        "totalPoints": 100,
        "isPassed": true,
        "gradingStatus": "Completed",
        "timeSpent": 7140,
        "integrityScore": 98.5,
        "proctorRating": "Clean",
        "certificateGenerated": true,
        "breakdown": {
          "autoGradedScore": 75.0,
          "manualGradedScore": 12.5,
          "totalPossible": 100
        }
      }
    ],
    "statistics": {
      "questionAnalysis": [
        {
          "questionId": 201,
          "correctRate": 89.4,
          "averageScore": 1.8,
          "totalPoints": 2,
          "difficulty": "Easy"
        }
      ],
      "timeAnalysis": {
        "averageTimeSpent": 6847,
        "fastestCompletion": 5423,
        "slowestCompletion": 7200
      }
    }
  }
}
```

---

### 11. Get Exam Certificate (`GET /{examId}/certificate/{attemptId}`)

**Path Parameters:** 
- `examId` (integer)
- `attemptId` (integer)

**Success Response (200):**

```json
{
  "success": true,
  "message": "Certificate retrieved successfully",
  "data": {
    "certificateId": "CERT-45-789-2025",
    "examId": 45,
    "attemptId": 789,
    "userId": 123,
    "examTitle": "Final Programming Certification Exam",
    "studentName": "Ahmed Hassan",
    "score": 87.5,
    "grade": "A",
    "passingScore": 75.0,
    "completedAt": "2025-07-15T11:00:00Z",
    "issuedAt": "2025-07-16T10:00:00Z",
    "expiresAt": "2027-07-16T10:00:00Z",
    "certificateUrl": "/certificates/download/CERT-45-789-2025.pdf",
    "verificationUrl": "https://learnquest.com/verify/CERT-45-789-2025",
    "verificationCode": "LQ-VER-2025-789456",
    "digitalSignature": "sha256:a1b2c3d4e5f6...",
    "skills": [
      "Advanced Programming Concepts",
      "Object-Oriented Programming",
      "Data Structures and Algorithms",
      "Software Design Patterns"
    ],
    "metadata": {
      "issuer": "LearnQuest Educational Platform",
      "credential": "Programming Certification",
      "level": "Advanced",
      "credits": 40
    }
  }
}
```

---

## 🔧 Common Error Responses

- **401 Unauthorized** - Invalid or missing token
- **403 Forbidden** - Insufficient permissions or access denied
- **404 Not Found** - Exam, session, or attempt not found
- **400 Bad Request** - Invalid input data or validation errors
- **409 Conflict** - Already registered, exam in progress, or scheduling conflicts
- **423 Locked** - Exam locked due to security issues or technical problems
- **429 Too Many Requests** - Rate limit exceeded
- **500 Internal Server Error** - Unexpected server errors

---

## 🔒 Security & Integrity Features

### Proctoring Technology:
- **Webcam Monitoring** - Continuous face detection and behavior analysis
- **Screen Recording** - Full screen capture with activity tracking
- **Audio Analysis** - Background noise and conversation detection
- **Browser Lockdown** - Prevents tab switching and external applications
- **Identity Verification** - Photo ID matching and biometric verification
- **Network Monitoring** - Connection stability and bandwidth analysis

### Anti-Cheating Measures:
- **Question Randomization** - Different question sets per attempt
- **Choice Shuffling** - Random order of answer choices
- **Time Limits** - Per-question and overall time constraints
- **Copy/Paste Prevention** - Disabled clipboard operations
- **Right-Click Disable** - Prevented context menu access
- **Tab Switch Detection** - Alerts on focus loss
- **Multiple Device Detection** - Prevents multiple session access

### Data Security:
- **Encrypted Storage** - All exam data encrypted at rest
- **Secure Transmission** - TLS 1.3 for all communications
- **Access Logging** - Comprehensive audit trails
- **Data Retention** - Configurable retention policies
- **GDPR Compliance** - Data protection and privacy controls

---

## 📝 Notes for Frontend Team

- Exams require more robust security than regular quizzes
- Implement proper proctoring SDK integration for monitoring
- Handle webcam/microphone permissions gracefully
- Provide clear technical requirements before exam starts
- Implement offline detection and auto-reconnection
- Support screen sharing for proctoring requirements
- All timestamps are in UTC format
- File uploads for questions support images and documents
- Certificate generation is automatic upon passing
- Implement proper error handling for technical failures
- Support real-time communication with proctors during exams

---

*Last updated: 2025-06-27*