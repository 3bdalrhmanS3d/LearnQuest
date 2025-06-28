using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Exam
{
    public class ExamRegistrationDto
    {
        [Required(ErrorMessage = "Session ID is required")]
        public int SessionId { get; set; }

        [StringLength(200, ErrorMessage = "Preferred seating cannot exceed 200 characters")]
        public string? PreferredSeating { get; set; }

        [StringLength(500, ErrorMessage = "Special accommodations cannot exceed 500 characters")]
        public string? SpecialAccommodations { get; set; }

        public EmergencyContactDto? EmergencyContact { get; set; }

        [Required(ErrorMessage = "Agreement must be signed")]
        public bool AgreementSigned { get; set; }

        public IdentityDocumentDto? IdentityDocument { get; set; }
    }

    public class EmergencyContactDto
    {
        [Required(ErrorMessage = "Emergency contact name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public required string Phone { get; set; }

        [StringLength(50, ErrorMessage = "Relationship cannot exceed 50 characters")]
        public string? Relationship { get; set; }
    }

    public class IdentityDocumentDto
    {
        [Required(ErrorMessage = "Document type is required")]
        [StringLength(50, ErrorMessage = "Document type cannot exceed 50 characters")]
        public required string Type { get; set; }

        [Required(ErrorMessage = "Document number is required")]
        [StringLength(50, ErrorMessage = "Document number cannot exceed 50 characters")]
        public required string Number { get; set; }
    }

    // === PROCTORING DTOs ===

    public class StartProctoringDto
    {
        [Required(ErrorMessage = "Session ID is required")]
        public int SessionId { get; set; }

        public ProctoringSettingsDto ProctoringSettings { get; set; } = new();
    }

    public class ProctoringSettingsDto
    {
        [StringLength(20, ErrorMessage = "Monitoring level cannot exceed 20 characters")]
        public string MonitoringLevel { get; set; } = "High"; // Low, Medium, High

        public AlertThresholdsDto AlertThresholds { get; set; } = new();
        public AutoInterventionsDto AutoInterventions { get; set; } = new();
    }

    public class AlertThresholdsDto
    {
        [Range(1, 60, ErrorMessage = "Face detection loss threshold must be between 1 and 60 seconds")]
        public int FaceDetectionLoss { get; set; } = 5;

        [Range(1, 10, ErrorMessage = "Multiple faces threshold must be between 1 and 10")]
        public int MultipleFacesDetected { get; set; } = 3;

        [Range(1, 30, ErrorMessage = "Eye gaze deviation threshold must be between 1 and 30")]
        public int EyeGazeDeviation { get; set; } = 10;

        [Range(1, 10, ErrorMessage = "Suspicious audio threshold must be between 1 and 10")]
        public int SuspiciousAudio { get; set; } = 3;
    }

    public class AutoInterventionsDto
    {
        public bool PauseOnFaceLoss { get; set; } = true;
        public bool AlertOnTabSwitch { get; set; } = true;
        public bool RecordSuspiciousActivity { get; set; } = true;
    }

    // === ENHANCED EXAM TAKING DTOs ===

    public class StartExamEnhancedDto
    {
        [Required(ErrorMessage = "Session ID is required")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Confirmation code is required")]
        [StringLength(50, ErrorMessage = "Confirmation code cannot exceed 50 characters")]
        public required string ConfirmationCode { get; set; }

        public IdentityVerificationDto? IdentityVerification { get; set; }
        public SystemCheckDto? SystemCheck { get; set; }
    }

    public class IdentityVerificationDto
    {
        [StringLength(500, ErrorMessage = "Photo URL cannot exceed 500 characters")]
        public string? PhotoUrl { get; set; }

        [StringLength(50, ErrorMessage = "Document number cannot exceed 50 characters")]
        public string? DocumentNumber { get; set; }
    }

    public class SystemCheckDto
    {
        public bool WebcamWorking { get; set; }
        public bool MicrophoneWorking { get; set; }
        public bool ScreenSharing { get; set; }
        public bool BrowserCompatible { get; set; }

        [Range(0, 1000, ErrorMessage = "Internet speed must be between 0 and 1000 Mbps")]
        public decimal InternetSpeed { get; set; }
    }

    public class SubmitExamEnhancedDto
    {
        [Required(ErrorMessage = "Attempt ID is required")]
        public int AttemptId { get; set; }

        [Required(ErrorMessage = "Session ID is required")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Answers are required")]
        public List<EnhancedExamAnswerDto> Answers { get; set; } = new();

        [Range(0, int.MaxValue, ErrorMessage = "Total time spent must be non-negative")]
        public int TotalTimeSpent { get; set; }

        [Required(ErrorMessage = "Submission type is required")]
        [StringLength(20, ErrorMessage = "Submission type cannot exceed 20 characters")]
        public required string SubmissionType { get; set; }

        [Required(ErrorMessage = "Integrity statement is required")]
        [StringLength(500, ErrorMessage = "Integrity statement cannot exceed 500 characters")]
        public required string IntegrityStatement { get; set; }

        public ProctoringESummaryDto? ProctoringESummary { get; set; }
    }

    public class EnhancedExamAnswerDto
    {
        [Required(ErrorMessage = "Question ID is required")]
        public int QuestionId { get; set; }

        public List<int>? SelectedChoiceIds { get; set; }

        [StringLength(5000, ErrorMessage = "Essay answer cannot exceed 5000 characters")]
        public string? EssayAnswer { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Time spent must be non-negative")]
        public int TimeSpent { get; set; }

        public bool Flagged { get; set; }

        [StringLength(200, ErrorMessage = "Note cannot exceed 200 characters")]
        public string? Note { get; set; }

        public int? WordCount { get; set; }
    }

    public class ProctoringESummaryDto
    {
        public int SuspiciousEvents { get; set; }
        public int Warnings { get; set; }
        public int TabSwitches { get; set; }
        public int ScreenShareInterruptions { get; set; }
    }

    // === EMERGENCY & COMMUNICATION DTOs ===

    public class BroadcastMessageDto
    {
        [Required(ErrorMessage = "Session ID is required")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public required string Message { get; set; }

        [StringLength(20, ErrorMessage = "Message type cannot exceed 20 characters")]
        public string MessageType { get; set; } = "Info"; // Info, Warning, Emergency

        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

        public bool RequireAcknowledgment { get; set; } = false;
        public List<int>? TargetUserIds { get; set; } // If null, broadcast to all
    }

    // === RESPONSE DTOs ===

    public class ExamRegistrationResponseDto
    {
        public int RegistrationId { get; set; }
        public int ExamId { get; set; }
        public int SessionId { get; set; }
        public required string ExamTitle { get; set; }
        public ExamSessionDetailsDto SessionDetails { get; set; }
        public DateTime RegisteredAt { get; set; }
        public required string ConfirmationCode { get; set; }
        public required string CheckinInstructions { get; set; }
        public List<string> Requirements { get; set; } = new();
        public List<ReminderDto> Reminders { get; set; } = new();
    }

    public class ExamSessionDetailsDto
    {
        public required string SessionName { get; set; }
        public DateTime StartDateTime { get; set; }
        public int Duration { get; set; }
        public string? Location { get; set; }
    }

    public class ReminderDto
    {
        public required string Type { get; set; }
        public DateTime ScheduledFor { get; set; }
        public required string Message { get; set; }
    }

    public class ProctoringSessionResponseDto
    {
        public required string ProctoringSessionId { get; set; }
        public int ExamId { get; set; }
        public int SessionId { get; set; }
        public DateTime StartedAt { get; set; }
        public required string MonitoringLevel { get; set; }
        public int ActiveParticipants { get; set; }
        public required string MonitoringDashboardUrl { get; set; }
        public EmergencyControlsDto EmergencyControls { get; set; }
    }

    public class EmergencyControlsDto
    {
        public required string PauseAllExams { get; set; }
        public required string BroadcastMessage { get; set; }
        public required string EvacuateSession { get; set; }
    }

    public class ExamCertificateDto
    {
        public required string CertificateId { get; set; }
        public int ExamId { get; set; }
        public int AttemptId { get; set; }
        public int UserId { get; set; }
        public required string ExamTitle { get; set; }
        public required string StudentName { get; set; }
        public decimal Score { get; set; }
        public required string Grade { get; set; }
        public decimal PassingScore { get; set; }
        public DateTime CompletedAt { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public required string CertificateUrl { get; set; }
        public required string VerificationUrl { get; set; }
        public required string VerificationCode { get; set; }
        public required string DigitalSignature { get; set; }
        public List<string> Skills { get; set; } = new();
        public CertificateMetadataDto Metadata { get; set; }
    }

    public class CertificateMetadataDto
    {
        public required string Issuer { get; set; }
        public required string Credential { get; set; }
        public required string Level { get; set; }
        public int Credits { get; set; }
    }

    // === ENUMS ===

    public enum MonitoringLevel
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum MessageType
    {
        Info = 1,
        Warning = 2,
        Emergency = 3
    }

    public enum Priority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }
}
