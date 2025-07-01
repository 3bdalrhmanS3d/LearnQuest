# 🚀 LearnQuest V1 - Advanced Learning Management System

![LearnQuest](https://img.shields.io/badge/LearnQuest-v1.0-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-9.0.5-green.svg)

LearnQuest V1 is a comprehensive Learning Management System (LMS) designed for modern educational institutions. Built with cutting-edge technologies, it provides a secure, scalable, and feature-rich platform for online learning and course management.

## 📋 Table of Contents

- [🌟 Key Features](#-key-features)
- [🏗️ Architecture](#️-architecture)
- [🛠️ Technology Stack](#️-technology-stack)
- [⚡ Quick Start](#-quick-start)
- [🔧 Installation & Setup](#-installation--setup)
- [🔑 Authentication & Security](#-authentication--security)
- [📚 API Documentation](#-api-documentation)
- [🎯 User Roles & Permissions](#-user-roles--permissions)
- [🎓 Course Management](#-course-management)
- [📊 Analytics & Reporting](#-analytics--reporting)
- [🔒 Exam & Proctoring System](#-exam--proctoring-system)
- [💾 Database Schema](#-database-schema)
- [🚀 Deployment](#-deployment)
- [🤝 Contributing](#-contributing)
- [📞 Support](#-support)

## 🌟 Key Features

### 📚 **Comprehensive Course Management**

- **Multi-level Course Structure**: Courses → Levels → Sections → Content
- **Rich Content Types**: Videos, documents, interactive materials
- **Course Tracks**: Organized learning paths
- **Skills Management**: Skill tracking and validation
- **Prerequisites**: Course dependency management

### 👥 **Advanced User Management**

- **Multi-role System**: Admin, Instructor, Student
- **Enhanced Authentication**: JWT with refresh tokens
- **Account Security**: Email verification, password reset
- **User Profiles**: Detailed profile management
- **Learning Analytics**: Personal progress tracking

### 🏆 **Examination & Certification System**

- **Secure Exam Engine**: Time-limited, randomized questions
- **Advanced Proctoring**: Webcam monitoring, screen recording
- **Anti-Cheating Measures**: Browser lockdown, activity monitoring
- **Automated Certificates**: Digital certificates with verification
- **Multiple Question Types**: MCQ, Essay, True/False

### 📊 **Analytics & Dashboard**

- **Real-time Dashboards**: Role-based analytics
- **Performance Metrics**: Course completion, engagement rates
- **Predictive Analytics**: Learning outcome predictions
- **Detailed Reports**: Exportable data reports
- **System Health Monitoring**: Performance tracking

### 🔔 **Real-time Communication**

- **SignalR Integration**: Live notifications
- **Course Forums**: Student-instructor interaction
- **Messaging System**: Internal communication
- **Progress Notifications**: Automated updates

### 🔒 **Enterprise Security**

- **Advanced Authentication**: JWT + refresh token strategy
- **Role-based Access Control**: Granular permissions
- **Security Audit Logs**: Comprehensive activity tracking
- **Data Encryption**: At-rest and in-transit
- **GDPR Compliance**: Data protection standards

## 🏗️ Architecture

LearnQuest follows a **Clean Architecture** pattern with clear separation of concerns:

```
LearnQuestV1/
├── 🎯 LearnQuestV1.Api/          # Presentation Layer
│   ├── Controllers/              # API Controllers
│   ├── DTOs/                    # Data Transfer Objects
│   ├── Services/                # Business Logic
│   ├── Middlewares/             # Custom Middleware
│   └── Configuration/           # App Configuration
│
├── 🧠 LearnQuestV1.Core/        # Domain Layer
│   ├── Models/                  # Domain Entities
│   ├── Enums/                   # System Enumerations
│   ├── Interfaces/              # Contracts
│   └── DTOs/                    # Core DTOs
│
└── 💾 LearnQuestV1.EF/          # Data Access Layer
    ├── Application/             # DbContext
    ├── Repositories/            # Data Repositories
    ├── UnitOfWork/             # Unit of Work Pattern
    └── Migrations/             # Database Migrations
```

## 🛠️ Technology Stack

### **Backend**

- **Framework**: ASP.NET Core 8.0
- **Database**: Entity Framework Core 9.0.5 + SQL Server
- **Authentication**: JWT Bearer Tokens
- **Mapping**: AutoMapper 14.0.0
- **Email**: MailKit 4.12.1
- **Caching**: In-Memory Caching
- **Documentation**: Swagger/OpenAPI
- **Real-time**: SignalR

### **Security**

- **Password Hashing**: PBKDF2 with SHA-256
- **Token Management**: JWT with refresh tokens
- **CORS**: Configured for React frontend
- **HTTPS**: TLS 1.3 enforcement
- **Rate Limiting**: Request throttling

### **Monitoring & Logging**

- **Logging**: Serilog with file rotation
- **Health Checks**: Database connectivity
- **Audit Trails**: Security action logging
- **Performance**: Request/response monitoring

### **Frontend Integration**

- **CORS Policy**: Configured for React (localhost:3000)
- **API Design**: RESTful with standardized responses
- **WebSocket**: SignalR for real-time features

## ⚡ Quick Start

### Prerequisites

- **.NET 8.0 SDK** or later
- **SQL Server** (Local/Express/Cloud)
- **Visual Studio 2022** or **VS Code**

### 1. Clone the Repository

```bash
git clone https://github.com/3bdalrhmanS3d/LearnQuest.git
cd learnquest
```

### 2. Database Setup

```bash
# Update connection string in appsettings.json
# Run migrations
dotnet ef database update -p LearnQuest.EF -s LearnQuest.Api
```

### 3. Run the API

```bash
cd LearnQuest.Api
dotnet run
```

### 4. Access the Application

- **API**: <https://localhost:7217>
- **Swagger UI**: <https://localhost:7217/swagger>
- **Health Check**: <https://localhost:7217/health>

## 🔧 Installation & Setup

### **Step 1: Environment Configuration**

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=LearnQ_DBV3;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True"
  },
  "JWT": {
    "ValidIss": "https://localhost:7217/",
    "ValidAud": "https://localhost:7217",
    "SecretKey": "YourSuperSecretKeyForJwtTokenThatIsLongEnoughToBeSecure123!"
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@learnquest.com",
    "EnableSsl": true
  },
  "Security": {
    "MaxFailedAttempts": 5,
    "LockoutDurationMinutes": 30,
    "RequireEmailConfirmation": true
  }
}
```

### **Step 2: Database Initialization**

```bash
# Install EF tools
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate -p LearnQuestV1.EF -s LearnQuestV1.Api

# Update database
dotnet ef database update -p LearnQuestV1.EF -s LearnQuestV1.Api
```

### **Step 3: Seed Default Data**

The application automatically seeds:

- **Default Admin**: <admin@learnquest.com> / Yg1rb76y@Yg1rb76y
- **Sample Courses**: Programming, Data Science tracks
- **Demo Users**: 50 sample users for testing

### **Step 4: Configure CORS for Frontend**

Update `Program.cs` CORS settings:

```csharp
options.AddPolicy("AllowReactApp", policy =>
    policy.WithOrigins("http://localhost:3000") // Your frontend URL
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials());
```

## 🔑 Authentication & Security

### **JWT Token Strategy**

- **Access Token**: 1-hour expiry, contains user claims
- **Refresh Token**: 7-day expiry, secure HTTP-only
- **Auto-login Token**: Optional persistent login

### **Security Features**

- **Account Lockout**: After 5 failed attempts
- **Email Verification**: Required for new accounts
- **Password Requirements**: Complex password policy
- **Security Auditing**: All actions logged
- **Session Management**: Multiple device detection

### **API Authentication**

```javascript
// Frontend example
const token = localStorage.getItem('accessToken');
fetch('/api/courses', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
```

## 📚 API Documentation

### **Base URL**: `https://localhost:7217/api`

### **Core Endpoints**

#### **Authentication** (`/api/auth`)

- `POST /signup` - Register new user
- `POST /signin` - User login
- `POST /verify-account` - Email verification
- `POST /refresh-token` - Token refresh
- `POST /forgot-password` - Password reset

#### **Course Management** (`/api/courses`)

- `GET /browse` - Browse available courses
- `GET /{id}` - Get course details
- `POST /` - Create course (Instructor)
- `PUT /{id}` - Update course (Instructor)
- `DELETE /{id}` - Delete course (Instructor)

#### **User Profile** (`/api/profile`)

- `GET /` - Get user profile
- `POST /update` - Update profile
- `GET /my-courses` - Enrolled courses
- `POST /upload-photo` - Profile picture

#### **Dashboard** (`/api/dashboard`)

- `GET /course-stats` - Course statistics
- `GET /system-stats` - System analytics (Admin)
- `GET /performance-metrics` - Performance data

#### **Exam System** (`/api/exam`)

- `POST /{examId}/register` - Register for exam
- `POST /{examId}/start` - Start exam attempt
- `POST /{examId}/submit` - Submit exam
- `GET /{examId}/certificate/{attemptId}` - Get certificate

### **Response Format**

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* Response data */ },
  "errors": [], // Validation errors if any
  "timestamp": "2025-07-01T00:00:00Z"
}
```

## 🎯 User Roles & Permissions

### **🔧 Admin**

- **System Management**: Full system control
- **User Management**: Create/modify all users
- **Course Oversight**: Access to all courses
- **Analytics**: System-wide reports
- **Security**: Audit logs and security settings

### **👨‍🏫 Instructor**

- **Course Creation**: Create and manage courses
- **Content Management**: Upload materials, create exams
- **Student Monitoring**: Track student progress
- **Grading**: Manual grading and feedback
- **Analytics**: Course-specific reports

### **👨‍🎓 Student (RegularUser)**

- **Course Enrollment**: Browse and enroll in courses
- **Learning Progress**: Track personal progress
- **Exam Taking**: Participate in assessments
- **Certificates**: Download earned certificates
- **Profile Management**: Update personal information

## 🎓 Course Management

### **Course Structure**

```
Course
├── About Course Items (Features, Prerequisites)
├── Course Skills (Technologies/Skills taught)
├── Levels (Course modules)
│   └── Sections (Grouped content)
│       └── Content Items (Videos, documents, etc.)
├── Enrollments (Student registrations)
├── Reviews & Ratings
└── Exams/Quizzes
```

### **Content Types**

- **Video Content**: Streaming video lessons
- **Document Content**: PDFs, presentations
- **Interactive Content**: Hands-on exercises
- **Code Examples**: Programming snippets
- **External Resources**: Links and references

### **Progress Tracking**

- **Completion Status**: Per content item
- **Time Spent**: Learning session tracking
- **Bookmarks**: Save important content
- **Notes**: Personal annotations

## 📊 Analytics & Reporting

### **Student Analytics**

- **Learning Streak**: Consecutive study days
- **Progress Metrics**: Completion percentages
- **Time Investment**: Hours spent learning
- **Achievement Badges**: Milestone rewards
- **Performance Trends**: Grade improvements

### **Instructor Analytics**

- **Course Performance**: Enrollment and completion rates
- **Student Engagement**: Activity and participation
- **Content Effectiveness**: Most/least engaging content
- **Revenue Tracking**: Course earnings
- **Student Feedback**: Reviews and ratings

### **Admin Analytics**

- **System Health**: Performance metrics
- **User Growth**: Registration trends
- **Course Popularity**: Top performing courses
- **Revenue Reports**: Financial analytics
- **Security Metrics**: Security incident tracking

## 🔒 Exam & Proctoring System

### **Advanced Security Features**

- **Browser Lockdown**: Prevents tab switching
- **Screen Recording**: Full session capture
- **Webcam Monitoring**: Facial recognition
- **Audio Analysis**: Background noise detection
- **Activity Tracking**: Mouse/keyboard monitoring

### **Anti-Cheating Measures**

- **Question Randomization**: Unique question sets
- **Time Limits**: Per-question constraints
- **Copy/Paste Prevention**: Clipboard disabled
- **Multiple Device Detection**: Session validation
- **Suspicious Activity Alerts**: Real-time monitoring

### **Proctoring Dashboard**

- **Live Monitoring**: Real-time student view
- **Alert System**: Automated suspicious activity alerts
- **Emergency Controls**: Pause/evacuate sessions
- **Communication Tools**: Chat with students
- **Incident Reporting**: Detailed violation logs

### **Certificate System**

- **Digital Certificates**: Secure PDF generation
- **Verification System**: QR code validation
- **Blockchain Integration**: Immutable records (planned)
- **Skills Validation**: Industry-recognized credentials

## 💾 Database Schema

### **Core Entities**

#### **User Management**

- `Users` - Core user information
- `UserDetails` - Extended profile data
- `AccountVerification` - Email verification
- `RefreshTokens` - Token management
- `UserVisitHistory` - Login tracking

#### **Course Structure**

- `Courses` - Course master data
- `CourseTracks` - Learning paths
- `Levels` - Course modules
- `Sections` - Content groupings
- `Contents` - Individual content items
- `AboutCourses` - Course descriptions
- `CourseSkills` - Skill associations

#### **Learning & Progress**

- `CourseEnrollments` - Student registrations
- `UserProgress` - Learning progress
- `UserContentActivity` - Content interactions
- `UserBookmarks` - Saved content
- `StudySessions` - Learning sessions

#### **Assessment System**

- `Quizzes` - Exam definitions
- `Questions` - Question bank
- `QuizAttempts` - Exam attempts
- `UserAnswers` - Student responses

#### **Financial**

- `Payments` - Payment records
- `PaymentTransactions` - Transaction logs
- `Discounts` - Coupon system

### **Key Relationships**

- **User** → **CourseEnrollments** → **Course** (Many-to-Many)
- **Course** → **Levels** → **Sections** → **Contents** (Hierarchical)
- **User** → **QuizAttempts** → **Quiz** (Exam tracking)
- **User** → **UserProgress** → **Course** (Progress tracking)

## 🚀 Deployment

### **Development Environment**

```bash
# Clone repository
git clone https://github.com/3bdalrhmanS3d/LearnQuest.git

# Restore packages
dotnet restore

# Update database
dotnet ef database update -p LearnQuest.EF -s LearnQuest.Api

# Run application
dotnet run --project LearnQuest.Api
```

### **Performance Considerations**

- **Connection Pooling**: Configure for high traffic
- **Caching Strategy**: Redis for distributed caching
- **CDN Integration**: For static content delivery
- **Load Balancing**: Multiple instance deployment
- **Database Optimization**: Index optimization and query tuning

## 🤝 Contributing

We welcome contributions to LearnQuest! Please follow these guidelines:

### **Development Process**

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### **Code Standards**

- Follow **C# coding conventions**
- Write **comprehensive unit tests**
- Update **API documentation**
- Ensure **security best practices**

### **Pull Request Requirements**

- **Clear description** of changes
- **Tests** for new functionality
- **Documentation** updates
- **Security** impact assessment

## 📞 Support

### **Getting Help**

- **Documentation**: Check this README and API docs
- **Issues**: Report bugs via GitHub Issues
- **Discussions**: Community Q&A and feature requests
- **Email**: <abdalrhmansaad24@gmail.com>

### **System Requirements**

- **.NET 8.0** or later
- **SQL Server 2019** or later
- **4GB RAM** minimum (8GB recommended)
- **2 CPU cores** minimum
- **10GB** storage space

### **Common Issues**

- **Database Connection**: Check connection string format
- **JWT Errors**: Verify secret key configuration
- **CORS Issues**: Ensure frontend URL is whitelisted
- **Email Delivery**: Verify SMTP settings

--- 
**Built with ❤️ by the LearnQuest BackEnd Team**

