// إنشاء ملف جديد: ExamValidationAttributes.cs في مجلد Utilities

using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.Utilities
{
    /// <summary>
    /// Custom validation attribute for exam time limits
    /// </summary>
    public class ExamTimeLimitAttribute : ValidationAttribute
    {
        private readonly int _minMinutes;
        private readonly int _maxMinutes;

        public ExamTimeLimitAttribute(int minMinutes = 30, int maxMinutes = 480)
        {
            _minMinutes = minMinutes;
            _maxMinutes = maxMinutes;
            ErrorMessage = $"Exam time limit must be between {_minMinutes} and {_maxMinutes} minutes";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true; // Allow null for optional fields

            if (value is int timeLimit)
            {
                return timeLimit >= _minMinutes && timeLimit <= _maxMinutes;
            }

            return false;
        }
    }

    /// <summary>
    /// Custom validation attribute for exam passing scores
    /// </summary>
    public class ExamPassingScoreAttribute : ValidationAttribute
    {
        public ExamPassingScoreAttribute()
        {
            ErrorMessage = "Passing score must be between 0 and 100";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (value is int score)
            {
                return score >= 0 && score <= 100;
            }

            return false;
        }
    }

    /// <summary>
    /// Custom validation attribute for exam attempt limits
    /// </summary>
    public class ExamMaxAttemptsAttribute : ValidationAttribute
    {
        private readonly int _maxAttempts;

        public ExamMaxAttemptsAttribute(int maxAttempts = 5)
        {
            _maxAttempts = maxAttempts;
            ErrorMessage = $"Maximum attempts cannot exceed {_maxAttempts}";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (value is int attempts)
            {
                return attempts >= 1 && attempts <= _maxAttempts;
            }

            return false;
        }
    }

    /// <summary>
    /// Validation attribute for exam scheduling
    /// </summary>
    public class ExamScheduleValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not DateTime scheduleTime) return true;

            // Must be in the future
            if (scheduleTime <= DateTime.UtcNow.AddMinutes(30))
            {
                ErrorMessage = "Exam must be scheduled at least 30 minutes in the future";
                return false;
            }

            // Cannot be more than 1 year in the future
            if (scheduleTime > DateTime.UtcNow.AddYears(1))
            {
                ErrorMessage = "Exam cannot be scheduled more than 1 year in advance";
                return false;
            }

            return true;
        }
    }

    /// Centralized error messages for exam operations
    /// </summary>
    public static class ExamErrorMessages
    {
        // Access Control
        public const string ExamNotFound = "Exam not found or access denied";
        public const string ExamAccessDenied = "You are not authorized to access this exam";
        public const string CourseAccessDenied = "Access denied to this course";
        public const string InstructorAccessRequired = "Only instructors can perform this action";

        // Exam Availability
        public const string ExamNotAvailable = "This exam is not currently available";
        public const string ExamInactive = "This exam has been deactivated";
        public const string ExamMaxAttemptsReached = "Maximum number of attempts reached for this exam";
        public const string ExamAlreadyInProgress = "You have an exam attempt already in progress";
        public const string ExamRequirementsNotMet = "Required content must be completed before taking this exam";

        // Exam Timing
        public const string ExamTimeExpired = "Time limit for this exam has expired";
        public const string ExamNotStarted = "This exam has not started yet";
        public const string ExamAlreadyCompleted = "This exam has already been completed";
        public const string InvalidExamSchedule = "Invalid exam schedule - start time must be before end time";

        // Exam Content
        public const string ExamNoQuestions = "This exam has no questions configured";
        public const string ExamInvalidQuestions = "Some questions in this exam are invalid or deleted";
        public const string ExamAnswersRequired = "All questions must be answered before submitting";
        public const string ExamInvalidAnswers = "Invalid answers provided for some questions";

        // Exam Creation/Update
        public const string ExamTitleRequired = "Exam title is required";
        public const string ExamInvalidLevel = "Invalid level specified for this course";
        public const string ExamInvalidCourse = "Invalid course specified";
        public const string ExamDuplicateTitle = "An exam with this title already exists in this course";

        // Exam Submission
        public const string ExamSubmissionFailed = "Failed to submit exam. Please try again";
        public const string ExamNoActiveAttempt = "No active exam attempt found";
        public const string ExamAlreadySubmitted = "This exam attempt has already been submitted";
        public const string ExamSubmissionTimeout = "Exam submission timed out. Please check your connection";

        // Security & Proctoring
        public const string ExamSecurityViolation = "Security violation detected during exam";
        public const string ExamProctoringRequired = "This exam requires proctoring supervision";
        public const string ExamTechnicalIssue = "Technical issue detected. Please contact support";
        public const string ExamSuspiciousActivity = "Suspicious activity detected during exam";

        // Registration & Sessions
        public const string ExamRegistrationClosed = "Registration for this exam session is closed";
        public const string ExamSessionFull = "This exam session is full";
        public const string ExamAlreadyRegistered = "You are already registered for this exam session";
        public const string ExamRegistrationNotFound = "Exam registration not found";

        // General
        public const string ExamUnexpectedError = "An unexpected error occurred. Please try again";
        public const string ExamServiceUnavailable = "Exam service is temporarily unavailable";
        public const string ExamInvalidOperation = "Invalid operation for current exam state";
    }

    /// <summary>
    /// Success messages for exam operations
    /// </summary>
    public static class ExamSuccessMessages
    {
        public const string ExamCreated = "Exam created successfully";
        public const string ExamUpdated = "Exam updated successfully";
        public const string ExamDeleted = "Exam deleted successfully";
        public const string ExamActivated = "Exam activated successfully";
        public const string ExamDeactivated = "Exam deactivated successfully";
        public const string ExamScheduled = "Exam scheduled successfully";
        public const string ExamStarted = "Exam attempt started successfully";
        public const string ExamSubmitted = "Exam submitted successfully";
        public const string ExamRegistered = "Successfully registered for exam session";
        public const string ExamUnregistered = "Successfully unregistered from exam session";
    }
}