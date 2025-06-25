using AutoMapper;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.DTOs.Exam;
using LearnQuestV1.Core.DTOs.Quiz;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.Quiz;
using LearnQuestV1.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<ExamService> _logger;
        private readonly IQuizService _quizService;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IQuizRepository _quizRepository;

        public ExamService(IUnitOfWork uow, IMapper mapper, ILogger<ExamService> logger, IQuizService quizService, IQuizAttemptRepository quizAttemptRepository, IQuizRepository quizRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
            _quizService = quizService;
            _quizAttemptRepository = quizAttemptRepository;
            _quizRepository = quizRepository;
        }

        #region Exam Management

        public async Task<ExamResponseDto> CreateExamAsync(CreateExamDto createExamDto, int instructorId)
        {
            try
            {
                // Validate instructor access to course
                var course = await _uow.Courses.GetByIdAsync(createExamDto.CourseId);
                if (course == null || course.InstructorId != instructorId)
                    throw new UnauthorizedAccessException("Access denied to this course");

                // Validate level if provided
                if (createExamDto.LevelId.HasValue)
                {
                    var level = await _uow.Levels.GetByIdAsync(createExamDto.LevelId.Value);
                    if (level == null || level.CourseId != createExamDto.CourseId)
                        throw new ArgumentException("Invalid level for this course");
                }

                // Create quiz with ExamQuiz type
                var createQuizDto = new CreateQuizDto
                {
                    Title = createExamDto.Title,
                    Description = createExamDto.Description,
                    QuizType = QuizType.ExamQuiz,
                    LevelId = createExamDto.LevelId,
                    CourseId = createExamDto.CourseId,
                    MaxAttempts = createExamDto.MaxAttempts,
                    PassingScore = createExamDto.PassingScore,
                    IsRequired = createExamDto.IsRequired,
                    TimeLimitInMinutes = createExamDto.TimeLimitInMinutes
                };

                var quiz = await _quizService.CreateQuizAsync(createQuizDto, instructorId);

                // Convert to ExamResponseDto
                var examResponse = await ConvertQuizToExamResponseAsync(quiz);

                _logger.LogInformation("Exam created successfully with ID: {ExamId} by instructor: {InstructorId}",
                    examResponse.ExamId, instructorId);

                return examResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam for instructor: {InstructorId}", instructorId);
                throw;
            }
        }

        public async Task<ExamResponseDto> CreateExamWithQuestionsAsync(CreateExamWithQuestionsDto createDto, int instructorId)
        {
            try
            {
                // Create quiz with questions using QuizService
                var createQuizDto = new CreateQuizWithQuestionsDto
                {
                    Quiz = new CreateQuizDto
                    {
                        Title = createDto.Exam.Title,
                        Description = createDto.Exam.Description,
                        QuizType = QuizType.ExamQuiz,
                        LevelId = createDto.Exam.LevelId,
                        CourseId = createDto.Exam.CourseId,
                        MaxAttempts = createDto.Exam.MaxAttempts,
                        PassingScore = createDto.Exam.PassingScore,
                        IsRequired = createDto.Exam.IsRequired,
                        TimeLimitInMinutes = createDto.Exam.TimeLimitInMinutes
                    },
                    ExistingQuestionIds = createDto.ExistingQuestionIds,
                    NewQuestions = createDto.NewQuestions.Select(nq => new CreateQuestionDto
                    {
                        QuestionText = nq.QuestionText,
                        QuestionType = nq.QuestionType,
                        CourseId = createDto.Exam.CourseId,
                        HasCode = nq.HasCode,
                        CodeSnippet = nq.CodeSnippet,
                        ProgrammingLanguage = nq.ProgrammingLanguage,
                        Points = nq.Points,
                        Explanation = nq.Explanation,
                        Options = nq.Options.Select(o => new CreateQuestionOptionDto
                        {
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect,
                            OrderIndex = o.OrderIndex
                        }).ToList()
                    }).ToList()
                };

                var quiz = await _quizService.CreateQuizWithQuestionsAsync(createQuizDto, instructorId);
                var examResponse = await ConvertQuizToExamResponseAsync(quiz);

                _logger.LogInformation("Exam with questions created successfully with ID: {ExamId} by instructor: {InstructorId}",
                    examResponse.ExamId, instructorId);

                return examResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam with questions for instructor: {InstructorId}", instructorId);
                throw;
            }
        }

        public async Task<ExamResponseDto?> UpdateExamAsync(UpdateExamDto updateExamDto, int instructorId)
        {
            try
            {
                var updateQuizDto = new UpdateQuizDto
                {
                    QuizId = updateExamDto.ExamId,
                    Title = updateExamDto.Title,
                    Description = updateExamDto.Description,
                    MaxAttempts = updateExamDto.MaxAttempts,
                    PassingScore = updateExamDto.PassingScore,
                    IsRequired = updateExamDto.IsRequired,
                    TimeLimitInMinutes = updateExamDto.TimeLimitInMinutes,
                    IsActive = updateExamDto.IsActive
                };

                var quiz = await _quizService.UpdateQuizAsync(updateQuizDto, instructorId);
                if (quiz == null) return null;

                return await ConvertQuizToExamResponseAsync(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam: {ExamId} by instructor: {InstructorId}",
                    updateExamDto.ExamId, instructorId);
                throw;
            }
        }

        public async Task<bool> DeleteExamAsync(int examId, int instructorId)
        {
            try
            {
                return await _quizService.DeleteQuizAsync(examId, instructorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exam: {ExamId} by instructor: {InstructorId}", examId, instructorId);
                throw;
            }
        }

        public async Task<ExamResponseDto?> GetExamByIdAsync(int examId, int userId, string userRole)
        {
            try
            {
                var quiz = await _quizService.GetQuizByIdAsync(examId, userId);
                if (quiz == null || quiz.QuizType != QuizType.ExamQuiz) return null;

                return await ConvertQuizToExamResponseAsync(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam: {ExamId} for user: {UserId}", examId, userId);
                throw;
            }
        }

        public async Task<IEnumerable<ExamSummaryDto>> GetExamsByCourseAsync(int courseId, int instructorId)
        {
            try
            {
                var quizzes = await _quizService.GetQuizzesByCourseAsync(courseId, instructorId);
                var examQuizzes = quizzes.Where(q => q.QuizType == QuizType.ExamQuiz);

                return examQuizzes.Select(ConvertQuizToExamSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exams for course: {CourseId} by instructor: {InstructorId}",
                    courseId, instructorId);
                throw;
            }
        }

        public async Task<IEnumerable<ExamSummaryDto>> GetExamsByLevelAsync(int levelId, int userId)
        {
            try
            {
                var quizzes = await _quizService.GetQuizzesByTypeAsync(QuizType.ExamQuiz, levelId, userId);
                return quizzes.Select(q => new ExamSummaryDto
                {
                    ExamId = q.QuizId,
                    Title = q.Title,
                    ExamType = ConvertToExamType(q),      // إذا كان لديك دالة تناسب QuizResponseDto
                    TotalQuestions = q.TotalQuestions,          // تأكد أنّ QuizResponseDto يملك هذه الخاصية
                    TotalPoints = q.TotalPoints,
                    PassingScore = q.PassingScore,
                    IsRequired = q.IsRequired,
                    CreatedAt = q.CreatedAt,
                    IsActive = q.IsActive,
                    UserAttempts = q.UserAttempts,
                    HasPassed = q.HasPassed,
                    CanAttempt = q.CanAttempt,
                    IsAvailable = q.CanAttempt,
                    IsScheduled = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exams for level: {LevelId} for user: {UserId}", levelId, userId);
                throw;
            }
        }


        public async Task<bool> ActivateExamAsync(int examId, int instructorId)
        {
            try
            {
                var quiz = await _quizService.GetQuizByIdAsync(examId, instructorId);
                if (quiz == null || quiz.IsActive) return false;
                return await _quizService.ToggleQuizStatusAsync(examId, instructorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating exam: {ExamId} by instructor: {InstructorId}", examId, instructorId);
                throw;
            }
        }

        public async Task<bool> DeactivateExamAsync(int examId, int instructorId)
        {
            try
            {
                var quiz = await _quizService.GetQuizByIdAsync(examId, instructorId);
                if (quiz == null || !quiz.IsActive) return false;
                return await _quizService.ToggleQuizStatusAsync(examId, instructorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating exam: {ExamId} by instructor: {InstructorId}", examId, instructorId);
                throw;
            }
        }

        #endregion

        #region Exam Access & Validation

        public async Task<bool> CanUserAccessExamAsync(int examId, int userId)
        {
            try
            {
                return await _quizService.CanUserAccessQuizAsync(examId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking exam access for exam: {ExamId} and user: {UserId}", examId, userId);
                throw;
            }
        }

        public async Task<bool> IsExamAvailableAsync(int examId, int userId)
        {
            try
            {
                var quiz = await _uow.Quizzes.GetByIdAsync(examId);
                if (quiz == null || quiz.QuizType != QuizType.ExamQuiz || !quiz.IsActive)
                    return false;

                // Check if user has remaining attempts
                var remainingAttempts = await GetRemainingAttemptsAsync(examId, userId);
                if (remainingAttempts <= 0) return false;

                // Check if user has completed required content
                return await HasUserCompletedRequiredContentAsync(examId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking exam availability for exam: {ExamId} and user: {UserId}", examId, userId);
                throw;
            }
        }

        public async Task<bool> HasUserCompletedRequiredContentAsync(int examId, int userId)
        {
            try
            {
                var quiz = await _uow.Quizzes.GetByIdAsync(examId);
                if (quiz == null) return false;

                if (quiz.LevelId.HasValue)
                {
                    var up = await _uow.UserProgresses
                        .FirstOrDefaultAsync(up => up.UserId == userId
                                                && up.CurrentLevelId == quiz.LevelId.Value);
                    return up?.CompletedAt.HasValue == true;
                }

                return await _uow.UserProgresses
                    .Where(up => up.UserId == userId && up.CourseId == quiz.CourseId)
                    .AllAsync(up => up.CompletedAt.HasValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking required content for exam: {ExamId}, user: {UserId}",
                    examId, userId);
                throw;
            }
        }


        public async Task<int> GetRemainingAttemptsAsync(int examId, int userId)
        {
            try
            {
                var quiz = await _uow.Quizzes.GetByIdAsync(examId);
                if (quiz == null) return 0;

                var attemptCount = await _uow.QuizAttempts.CountAsync(qa => qa.QuizId == examId && qa.UserId == userId);
                return Math.Max(0, quiz.MaxAttempts - attemptCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining attempts for exam: {ExamId} and user: {UserId}", examId, userId);
                throw;
            }
        }

        #endregion

        #region Exam Taking

        public async Task<ExamAttemptResponseDto> StartExamAttemptAsync(int examId, int userId)
        {
            try
            {
                var attemptResponse = await _quizService.StartQuizAttemptAsync(examId, userId);
                return ConvertQuizAttemptToExamAttempt(attemptResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting exam attempt for exam: {ExamId} and user: {UserId}", examId, userId);
                throw;
            }
        }

        public async Task<ExamAttemptResponseDto?> GetCurrentExamAttemptAsync(int examId, int userId)
        {
            try
            {
                var attempt = await _quizService.GetCurrentQuizAttemptAsync(examId, userId);
                return attempt != null
                    ? ConvertQuizAttemptToExamAttempt(attempt)
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current exam attempt for exam: {ExamId} and user: {UserId}", examId, userId);
                throw;
            }
        }

        public async Task<ExamResultDto> SubmitExamAsync(SubmitExamDto submitDto, int userId)
        {
            try
            {
                var submitQuizDto = new SubmitQuizDto
                {
                    QuizId = submitDto.ExamId,
                    Answers = submitDto.Answers.Select(a => new SubmitAnswerDto
                    {
                        QuestionId = a.QuestionId,
                        SelectedOptionId = a.SelectedOptionId,
                        BooleanAnswer = a.BooleanAnswer
                    }).ToList()
                };

                var quizResult = await _quizService.SubmitQuizAsync(submitQuizDto, userId);
                return ConvertQuizResultToExamResult(quizResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting exam: {ExamId} for user: {UserId}", submitDto.ExamId, userId);
                throw;
            }
        }

        public async Task<bool> IsExamInProgressAsync(int examId, int userId)
        {
            try
            {
                var activeAttempt = await _uow.QuizAttempts
                    .FirstOrDefaultAsync(qa => qa.QuizId == examId && qa.UserId == userId && qa.CompletedAt == null);

                return activeAttempt != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking exam progress for exam: {ExamId} and user: {UserId}", examId, userId);
                throw;
            }
        }

        public async Task<TimeSpan?> GetRemainingTimeAsync(int examId, int userId)
        {
            try
            {
                // حقن IQuizAttemptRepository في ExamService بدل _uow.QuizAttempts
                var activeAttempt = await _quizAttemptRepository
                    .GetActiveAttemptWithQuizAsync(examId, userId);

                if (activeAttempt?.Quiz?.TimeLimitInMinutes == null) return null;

                var elapsedTime = DateTime.UtcNow - activeAttempt.StartedAt;
                var timeLimit = TimeSpan.FromMinutes(activeAttempt.Quiz.TimeLimitInMinutes.Value);
                var remainingTime = timeLimit - elapsedTime;

                return remainingTime > TimeSpan.Zero ? remainingTime : TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining time for exam: {ExamId} and user: {UserId}", examId, userId);
                throw;
            }
        }

        #endregion

        #region Exam Results & History

        public async Task<IEnumerable<ExamAttemptSummaryDto>> GetUserExamAttemptsAsync(int userId, int? courseId = null)
        {
            try
            {
                var attempts = await _quizAttemptRepository
                        .GetUserExamAttemptsAsync(userId, courseId);

                return attempts.Select(ConvertToExamAttemptSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam attempts for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<ExamAttemptDetailDto?> GetExamAttemptDetailAsync(int attemptId, int userId)
        {
            try
            {
                var attempt = await _quizAttemptRepository
                    .GetExamAttemptDetailAsync(attemptId, userId);


                return attempt != null ? ConvertToExamAttemptDetail(attempt) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam attempt detail: {AttemptId} for user: {UserId}", attemptId, userId);
                throw;
            }
        }

        public async Task<ExamResultDto?> GetBestExamResultAsync(int examId, int userId)
        {
            try
            {
                var bestAttempt = await _quizAttemptRepository
                       .GetBestExamAttemptAsync(examId, userId);


                return bestAttempt != null ? ConvertToExamResult(bestAttempt) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best exam result for exam: {ExamId} and user: {UserId}", examId, userId);
                throw;
            }
        }

        public async Task<bool> HasUserPassedExamAsync(int examId, int userId)
        {
            try
            {
                return await _uow.QuizAttempts
                    .AnyAsync(qa => qa.QuizId == examId && qa.UserId == userId && qa.Passed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user passed exam: {ExamId} for user: {UserId}", examId, userId);
                throw;
            }
        }

        #endregion

        #region Exam Statistics (Instructor)

        public async Task<ExamStatisticsDto?> GetExamStatisticsAsync(int examId, int instructorId)
        {
            try
            {
                var quiz = await _quizRepository.GetExamWithAttemptsAsync(examId, instructorId);


                if (quiz == null) return null;

                var attempts = quiz.QuizAttempts
                    .Where(qa => qa.CompletedAt.HasValue)
                    .ToList();

                if (!attempts.Any())
                {
                    return new ExamStatisticsDto
                    {
                        ExamId = examId,
                        ExamTitle = quiz.Title,
                        TotalAttempts = 0,
                        UniqueStudents = 0,
                        PassedAttempts = 0,
                        PassRate = 0,
                        AverageScore = 0,
                        HighestScore = 0,
                        LowestScore = 0,
                        QuestionStatistics = new List<ExamQuestionStatDto>(),
                        ScoreDistribution = new List<ExamScoreDistributionDto>()
                    };
                }

                var statistics = new ExamStatisticsDto
                {
                    ExamId = examId,
                    ExamTitle = quiz.Title,
                    TotalAttempts = attempts.Count(),
                    UniqueStudents = attempts.Select(a => a.UserId).Distinct().Count(),
                    PassedAttempts = attempts.Count(a => a.Passed),
                    PassRate = (decimal)attempts.Count(a => a.Passed) / attempts.Count * 100,
                    AverageScore = attempts.Average(a => a.ScorePercentage),
                    HighestScore = (int)attempts.Max(a => a.ScorePercentage),
                    LowestScore = (int)attempts.Min(a => a.ScorePercentage),
                    AverageTimeMinutes =attempts.Average(a => a.TimeTakenInMinutes),
                    QuestionStatistics = CalculateQuestionStatistics(attempts),
                    ScoreDistribution = CalculateScoreDistribution(attempts)
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam statistics for exam: {ExamId} by instructor: {InstructorId}",
                    examId, instructorId);
                throw;
            }
        }

        public async Task<IEnumerable<ExamAttemptSummaryDto>> GetExamAttemptsAsync(int examId, int instructorId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var quiz = await _uow.Quizzes.FirstOrDefaultAsync(q => q.QuizId == examId && q.InstructorId == instructorId);
                if (quiz == null) return Enumerable.Empty<ExamAttemptSummaryDto>();

                var attempts = await _quizAttemptRepository
                        .GetExamAttemptsByQuizIdAsync(examId, pageNumber, pageSize);

                return attempts.Select(ConvertToExamAttemptSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam attempts for exam: {ExamId} by instructor: {InstructorId}",
                    examId, instructorId);
                throw;
            }
        }

        public async Task<CourseExamPerformanceDto> GetCourseExamPerformanceAsync(int courseId, int instructorId)
        {
            try
            {
                var course = await _uow.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId && c.InstructorId == instructorId);
                if (course == null) throw new UnauthorizedAccessException("Access denied to this course");

                var exams = await _quizRepository.GetExamQuizzesByCourseAsync(courseId, instructorId);

                var allAttempts = exams.SelectMany(e => e.QuizAttempts.Where(qa => qa.CompletedAt.HasValue)).ToList();

                var performance = new CourseExamPerformanceDto
                {
                    CourseId = courseId,
                    CourseName = course.CourseName,
                    TotalExams = exams.Count(),
                    TotalAttempts = allAttempts.Count(),
                    UniqueStudents = allAttempts.Select(a => a.UserId).Distinct().Count(),
                    OverallPassRate = allAttempts.Any() ? (decimal)allAttempts.Count(a => a.Passed) / allAttempts.Count * 100 : 0,
                    AverageScore = allAttempts.Any() ? (decimal)allAttempts.Average(a => a.ScorePercentage) : 0,
                    ExamPerformances = exams.Select(e => new ExamPerformanceSummaryDto
                    {
                        ExamId = e.QuizId,
                        ExamTitle = e.Title,
                        ExamType = ConvertToExamType(e),
                        Attempts = e.QuizAttempts.Count(qa => qa.CompletedAt.HasValue),
                        PassRate = e.QuizAttempts.Any(qa => qa.CompletedAt.HasValue)
                            ? (decimal)e.QuizAttempts.Count(qa => qa.Passed) / e.QuizAttempts.Count(qa => qa.CompletedAt.HasValue) * 100
                            : 0,
                        AverageScore = e.QuizAttempts.Any(qa => qa.CompletedAt.HasValue)
                            ? (decimal)e.QuizAttempts.Where(qa => qa.CompletedAt.HasValue).Average(qa => qa.ScorePercentage)
                            : 0
                    }).ToList()
                };

                return performance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course exam performance for course: {CourseId} by instructor: {InstructorId}",
                    courseId, instructorId);
                throw;
            }
        }

        public async Task<IEnumerable<ExamQuestionAnalyticsDto>> GetExamQuestionAnalyticsAsync(int examId, int instructorId)
        {
            try
            {
                var quiz = await _quizRepository
                .GetExamWithQuestionsAndAttemptsAsync(examId, instructorId);
                if (quiz == null) return Enumerable.Empty<ExamQuestionAnalyticsDto>();


                var completedAttempts = quiz.QuizAttempts.Where(qa => qa.CompletedAt.HasValue).ToList();

                return quiz.QuizQuestions.Select(qq => CalculateQuestionAnalytics(qq.Question, completedAttempts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam question analytics for exam: {ExamId} by instructor: {InstructorId}",
                    examId, instructorId);
                throw;
            }
        }

        #endregion

        #region Exam Scheduling (Future Enhancement)

        public async Task<bool> ScheduleExamAsync(int examId, ScheduleExamDto scheduleDto, int instructorId)
        {
            try
            {
                var quiz = await _uow.Quizzes.FirstOrDefaultAsync(q => q.QuizId == examId && q.InstructorId == instructorId);
                if (quiz == null) return false;

                // This would require additional fields in the Quiz model for scheduling
                // For now, we'll log the scheduling request
                _logger.LogInformation("Exam scheduling requested for exam: {ExamId} from {StartTime} to {EndTime}",
                    examId, scheduleDto.StartTime, scheduleDto.EndTime);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling exam: {ExamId} by instructor: {InstructorId}", examId, instructorId);
                throw;
            }
        }

        public async Task<IEnumerable<ScheduledExamDto>> GetScheduledExamsAsync(int userId, int? courseId = null)
        {
            try
            {
                // This is a placeholder implementation
                // In a real scenario, you'd have scheduling data in the database
                return Enumerable.Empty<ScheduledExamDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scheduled exams for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CancelScheduledExamAsync(int examId, int instructorId)
        {
            // Placeholder implementation
            return true;
        }

        public async Task<bool> IsExamScheduledAsync(int examId)
        {
            // Placeholder implementation
            return false;
        }

        #endregion

        #region Exam Question Management

        public async Task<bool> AddQuestionToExamAsync(int examId, int questionId, int instructorId, int? customPoints = null)
        {
            try
            {
                return await _quizService.AddQuestionsToQuizAsync(
                    examId,
                    new List<int> { questionId },
                    instructorId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error adding question to exam: {ExamId} by instructor: {InstructorId}",
                    examId, instructorId);
                throw;
            }
        }

        public async Task<bool> RemoveQuestionFromExamAsync(int examId, int questionId, int instructorId)
        {
            try
            {
                return await _quizService.RemoveQuestionFromQuizAsync(examId, questionId, instructorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing question from exam: {ExamId} by instructor: {InstructorId}", examId, instructorId);
                throw;
            }
        }

        public async Task<bool> ReorderExamQuestionsAsync(int examId, Dictionary<int, int> questionOrders, int instructorId)
        {
            try
            {
                return await _quizService.ReorderQuizQuestionsAsync(examId, questionOrders, instructorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering exam questions for exam: {ExamId} by instructor: {InstructorId}", examId, instructorId);
                throw;
            }
        }

        public async Task<IEnumerable<QuestionSummaryDto>> GetAvailableQuestionsForExamAsync(int courseId, int instructorId)
        {
            try
            {
                return await _quizService.GetAvailableQuestionsForQuizAsync(courseId, instructorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available questions for course: {CourseId} by instructor: {InstructorId}", courseId, instructorId);
                throw;
            }
        }

        #endregion

        #region Security & Proctoring (Future Enhancement)

        public async Task<bool> ValidateExamSecurityAsync(int examId, int userId, string sessionToken)
        {
            // Placeholder for security validation
            return true;
        }

        public async Task<ExamProctoringDto?> GetExamProctoringStatusAsync(int attemptId, int instructorId)
        {
            // Placeholder for proctoring functionality
            return null;
        }

        public async Task<bool> FlagSuspiciousActivityAsync(int attemptId, string activityType, string details)
        {
            // Placeholder for activity flagging
            return true;
        }

        #endregion

        #region Private Helper Methods

        private async Task<ExamResponseDto> ConvertQuizToExamResponseAsync(QuizResponseDto quiz)
        {
            return new ExamResponseDto
            {
                ExamId = quiz.QuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                ExamType = ConvertToExamType(quiz),
                LevelId = quiz.LevelId,
                LevelName = quiz.LevelName,
                CourseId = quiz.CourseId,
                CourseName = quiz.CourseName,
                InstructorId = quiz.InstructorId,
                InstructorName = quiz.InstructorName,
                MaxAttempts = quiz.MaxAttempts,
                PassingScore = quiz.PassingScore,
                IsRequired = quiz.IsRequired,
                TimeLimitInMinutes = quiz.TimeLimitInMinutes,
                CreatedAt = quiz.CreatedAt,
                UpdatedAt = quiz.UpdatedAt,
                IsActive = quiz.IsActive,
                TotalQuestions = quiz.TotalQuestions,
                TotalPoints = quiz.TotalPoints,
                UserAttempts = quiz.UserAttempts,
                BestScore = quiz.BestScore,
                HasPassed = quiz.HasPassed,
                CanAttempt = quiz.CanAttempt,
                RemainingAttempts = quiz.UserAttempts.HasValue ? Math.Max(0, quiz.MaxAttempts - quiz.UserAttempts.Value) : quiz.MaxAttempts,
                IsAvailable = quiz.CanAttempt,
                IsScheduled = false, // Default for now
                RequireProctoring = false, // Default for now
                ShuffleQuestions = true, // Default for now
                ShowResultsImmediately = false // Default for now
            };
        }

        private ExamSummaryDto ConvertQuizToExamSummary(QuizSummaryDto quiz)
        {
            return new ExamSummaryDto
            {
                ExamId = quiz.QuizId,
                Title = quiz.Title,
                ExamType = ConvertToExamType(quiz),
                TotalQuestions = quiz.TotalQuestions,
                TotalPoints = quiz.TotalPoints,
                PassingScore = quiz.PassingScore,
                IsRequired = quiz.IsRequired,
                CreatedAt = quiz.CreatedAt,
                IsActive = quiz.IsActive,
                UserAttempts = quiz.UserAttempts,
                HasPassed = quiz.HasPassed,
                CanAttempt = quiz.CanAttempt,
                IsAvailable = quiz.CanAttempt,
                IsScheduled = false // Default for now
            };
        }

        private ExamAttemptResponseDto ConvertQuizAttemptToExamAttempt(QuizAttemptResponseDto quizAttempt)
        {
            return new ExamAttemptResponseDto
            {
                AttemptId = quizAttempt.AttemptId,
                ExamId = quizAttempt.QuizId,
                ExamTitle = quizAttempt.QuizTitle,
                UserId = quizAttempt.UserId,
                UserName = quizAttempt.UserName,
                StartedAt = quizAttempt.StartedAt,
                CompletedAt = quizAttempt.CompletedAt,
                AttemptNumber = quizAttempt.AttemptNumber,
                TimeLimitInMinutes = quizAttempt.TimeLimitInMinutes,
                RemainingTime = quizAttempt.RemainingTime,
                IsActive = quizAttempt.IsActive,
                Questions = quizAttempt.Questions.Select(q => new ExamQuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    HasCode = q.HasCode,
                    CodeSnippet = q.CodeSnippet,
                    ProgrammingLanguage = q.ProgrammingLanguage,
                    Points = q.Points,
                    OrderIndex = q.QuestionId,
                    Options = q.Options.Select(o => new ExamQuestionOptionDto
                    {
                        OptionId = o.OptionId,
                        OptionText = o.OptionText,
                        OrderIndex = o.OrderIndex
                    }).ToList()
                }).ToList()
            };
        }

        private ExamResultDto ConvertQuizResultToExamResult(QuizAttemptResponseDto quizResult)
        {
            return new ExamResultDto
            {
                AttemptId = quizResult.AttemptId,
                ExamId = quizResult.QuizId,
                ExamTitle = quizResult.QuizTitle,
                UserId = quizResult.UserId,
                UserName = quizResult.UserName,
                Score = quizResult.Score,
                TotalPoints = quizResult.TotalPoints,
                ScorePercentage = quizResult.ScorePercentage,
                Passed = quizResult.Passed,
                StartedAt = quizResult.StartedAt,
                CompletedAt = quizResult.CompletedAt,
                AttemptNumber = quizResult.AttemptNumber,
                TimeTakenInMinutes = quizResult.TimeTakenInMinutes,
                Answers = quizResult.Answers.Select(a => new ExamAnswerResultDto
                {
                    UserAnswerId = a.UserAnswerId,
                    QuestionId = a.QuestionId,
                    QuestionText = a.QuestionText,
                    SelectedOptionId = a.SelectedOptionId,
                    SelectedOptionText = a.SelectedOptionText,
                    BooleanAnswer = a.BooleanAnswer,
                    IsCorrect = a.IsCorrect,
                    PointsEarned = a.PointsEarned,
                    AnsweredAt = a.AnsweredAt,
                    CorrectAnswerText = a.CorrectAnswerText,
                    Explanation = a.Explanation
                }).ToList()
            };
        }

        private ExamAttemptSummaryDto ConvertToExamAttemptSummary(QuizAttempt attempt)
        {
            return new ExamAttemptSummaryDto
            {
                AttemptId = attempt.AttemptId,
                ExamId = attempt.QuizId,
                ExamTitle = attempt.Quiz.Title,
                Score = attempt.Score,
                TotalPoints = attempt.TotalPoints,
                ScorePercentage = attempt.ScorePercentage,
                Passed = attempt.Passed,
                StartedAt = attempt.StartedAt,
                CompletedAt = attempt.CompletedAt,
                AttemptNumber = attempt.AttemptNumber,
                UserName = attempt.User.FullName
            };
        }

        private ExamAttemptDetailDto ConvertToExamAttemptDetail(QuizAttempt attempt)
        {
            return new ExamAttemptDetailDto
            {
                AttemptId = attempt.AttemptId,
                ExamId = attempt.QuizId,
                ExamTitle = attempt.Quiz.Title,
                UserId = attempt.UserId,
                UserName = attempt.User.FullName,
                Score = attempt.Score,
                TotalPoints = attempt.TotalPoints,
                ScorePercentage = attempt.ScorePercentage,
                Passed = attempt.Passed,
                StartedAt = attempt.StartedAt,
                CompletedAt = attempt.CompletedAt ?? DateTime.UtcNow,
                AttemptNumber = attempt.AttemptNumber,
                TimeTakenInMinutes = attempt.TimeTakenInMinutes,
                Answers = attempt.UserAnswers.Select(ua => new ExamAnswerResultDto
                {
                    UserAnswerId = ua.UserAnswerId,
                    QuestionId = ua.QuestionId,
                    QuestionText = ua.Question.QuestionText,
                    SelectedOptionId = ua.SelectedOptionId,
                    SelectedOptionText = ua.SelectedOption?.OptionText,
                    BooleanAnswer = ua.BooleanAnswer,
                    IsCorrect = ua.IsCorrect,
                    PointsEarned = ua.PointsEarned,
                    AnsweredAt = ua.AnsweredAt,
                    CorrectAnswerText = ua.Question.QuestionOptions.FirstOrDefault(o => o.IsCorrect)?.OptionText,
                    Explanation = ua.Question.Explanation
                }).ToList()
            };
        }

        private ExamResultDto ConvertToExamResult(QuizAttempt attempt)
        {
            return new ExamResultDto
            {
                AttemptId = attempt.AttemptId,
                ExamId = attempt.QuizId,
                ExamTitle = attempt.Quiz.Title,
                UserId = attempt.UserId,
                UserName = attempt.User.FullName,
                Score = attempt.Score,
                TotalPoints = attempt.TotalPoints,
                ScorePercentage = attempt.ScorePercentage,
                Passed = attempt.Passed,
                StartedAt = attempt.StartedAt,
                CompletedAt = attempt.CompletedAt ?? DateTime.UtcNow,
                AttemptNumber = attempt.AttemptNumber,
                TimeTakenInMinutes = attempt.TimeTakenInMinutes,
                Answers = attempt.UserAnswers.Select(ua => new ExamAnswerResultDto
                {
                    UserAnswerId = ua.UserAnswerId,
                    QuestionId = ua.QuestionId,
                    QuestionText = ua.Question.QuestionText,
                    SelectedOptionId = ua.SelectedOptionId,
                    SelectedOptionText = ua.SelectedOption?.OptionText,
                    BooleanAnswer = ua.BooleanAnswer,
                    IsCorrect = ua.IsCorrect,
                    PointsEarned = ua.PointsEarned,
                    AnsweredAt = ua.AnsweredAt,
                    CorrectAnswerText = ua.Question.QuestionOptions.FirstOrDefault(o => o.IsCorrect)?.OptionText,
                    Explanation = ua.Question.Explanation
                }).ToList()
            };
        }

        private ExamType ConvertToExamType(Quiz quiz)
        {
            return quiz.LevelId.HasValue ? ExamType.LevelExam : ExamType.FinalExam;
        }

        private ExamType ConvertToExamType(QuizResponseDto quiz)
        {
            return quiz.LevelId.HasValue ? ExamType.LevelExam : ExamType.FinalExam;
        }

        private ExamType ConvertToExamType(QuizSummaryDto quiz)
        {
            return quiz.QuizType == QuizType.LevelQuiz ? ExamType.LevelExam : ExamType.FinalExam;
        }

        private List<ExamQuestionStatDto> CalculateQuestionStatistics(List<QuizAttempt> attempts)
        {
            var questionStats = new List<ExamQuestionStatDto>();

            var allAnswers = attempts.SelectMany(a => a.UserAnswers).GroupBy(ua => ua.QuestionId);

            foreach (var questionGroup in allAnswers)
            {
                var answers = questionGroup.ToList();
                var correctAnswers = answers.Count(a => a.IsCorrect);

                questionStats.Add(new ExamQuestionStatDto
                {
                    QuestionId = questionGroup.Key,
                    QuestionText = answers.First().Question.QuestionText,
                    TotalAnswered = answers.Count,
                    CorrectAnswers = correctAnswers,
                    CorrectPercentage = answers.Count > 0 ? (decimal)correctAnswers / answers.Count * 100 : 0,
                    DifficultyLevel = CalculateDifficultyLevel(correctAnswers, answers.Count)
                });
            }

            return questionStats;
        }

        private List<ExamScoreDistributionDto> CalculateScoreDistribution(List<QuizAttempt> attempts)
        {
            var scoreRanges = new[]
            {
                new { Range = "0-20%", Min = 0, Max = 20 },
                new { Range = "21-40%", Min = 21, Max = 40 },
                new { Range = "41-60%", Min = 41, Max = 60 },
                new { Range = "61-80%", Min = 61, Max = 80 },
                new { Range = "81-100%", Min = 81, Max = 100 }
            };

            return scoreRanges.Select(range => new ExamScoreDistributionDto
            {
                ScoreRange = range.Range,
                StudentCount = attempts.Count(a => a.ScorePercentage >= range.Min && a.ScorePercentage <= range.Max),
                Percentage = attempts.Count > 0
                    ? (decimal)attempts.Count(a => a.ScorePercentage >= range.Min && a.ScorePercentage <= range.Max) / attempts.Count * 100
                    : 0
            }).ToList();
        }

        private ExamQuestionAnalyticsDto CalculateQuestionAnalytics(Question question, List<QuizAttempt> attempts)
        {
            var allAnswers = attempts.SelectMany(a => a.UserAnswers).Where(ua => ua.QuestionId == question.QuestionId).ToList();
            var correctAnswers = allAnswers.Count(a => a.IsCorrect);
            var correctPercentage = allAnswers.Count > 0 ? (decimal)correctAnswers / allAnswers.Count * 100 : 0;

            return new ExamQuestionAnalyticsDto
            {
                QuestionId = question.QuestionId,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Points = question.Points,
                TotalAnswered = allAnswers.Count,
                CorrectAnswers = correctAnswers,
                CorrectPercentage = correctPercentage,
                DifficultyIndex = CalculateDifficultyLevel(correctAnswers, allAnswers.Count),
                DifficultyLevel = GetDifficultyDescription(CalculateDifficultyLevel(correctAnswers, allAnswers.Count)),
                OptionAnalytics = question.QuestionOptions.Select(o => new ExamOptionAnalyticsDto
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect,
                    SelectionCount = allAnswers.Count(a => a.SelectedOptionId == o.OptionId),
                    SelectionPercentage = allAnswers.Count > 0
                        ? (decimal)allAnswers.Count(a => a.SelectedOptionId == o.OptionId) / allAnswers.Count * 100
                        : 0
                }).ToList()
            };
        }

        private decimal CalculateDifficultyLevel(int correctAnswers, int totalAnswers)
        {
            if (totalAnswers == 0) return 0;
            return (decimal)correctAnswers / totalAnswers;
        }

        private string GetDifficultyDescription(decimal difficultyIndex)
        {
            return difficultyIndex switch
            {
                >= 0.8m => "Easy",
                >= 0.6m => "Medium",
                >= 0.4m => "Hard",
                _ => "Very Hard"
            };
        }

        //public Task<ExamAttemptResponseDto?> GetCurrentExamAttemptAsync(int examId, int userId)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}