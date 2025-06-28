using LearnQuestV1.Core.DTOs.Quiz;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.Quiz;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IQuizService
    {
        #region Quiz Management
        Task<QuizResponseDto> CreateQuizAsync(CreateQuizDto createQuizDto, int instructorId);
        Task<QuizResponseDto> CreateQuizWithQuestionsAsync(CreateQuizWithQuestionsDto createDto, int instructorId);
        Task<QuizResponseDto?> UpdateQuizAsync(UpdateQuizDto updateQuizDto, int instructorId);
        Task<bool> DeleteQuizAsync(int quizId, int instructorId);
        Task<bool> ToggleQuizStatusAsync(int quizId, int instructorId);
        Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
        #endregion

        #region Quiz Retrieval
        Task<QuizResponseDto?> GetQuizByIdAsync(int quizId, int? userId = null);
        Task<IEnumerable<QuizSummaryDto>> GetQuizzesByCourseAsync(int courseId, int? userId = null);
        Task<IEnumerable<QuizSummaryDto>> GetQuizzesByInstructorAsync(int instructorId);
        Task<IEnumerable<QuizResponseDto>> GetQuizzesByTypeAsync(QuizType quizType, int? entityId = null, int? userId = null);
        #endregion

        #region Question Management
        Task<QuestionResponseDto> CreateQuestionAsync(CreateQuestionDto createQuestionDto, int instructorId);
        Task<QuestionResponseDto?> UpdateQuestionAsync(UpdateQuestionDto updateQuestionDto, int instructorId);
        Task<bool> DeleteQuestionAsync(int questionId, int instructorId);
        Task<bool> AddQuestionsToQuizAsync(int quizId, List<int> questionIds, int instructorId);
        Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId, int instructorId);
        Task<bool> ReorderQuizQuestionsAsync(int quizId, Dictionary<int, int> questionOrders, int instructorId);
        #endregion

        #region Question Retrieval
        Task<QuestionResponseDto?> GetQuestionByIdAsync(int questionId, int instructorId);
        Task<IEnumerable<QuestionSummaryDto>> GetQuestionsByCourseAsync(int courseId, int instructorId);
        Task<IEnumerable<QuestionSummaryDto>> GetAvailableQuestionsForQuizAsync(int courseId, int instructorId);
        Task<IEnumerable<QuestionSummaryDto>> SearchQuestionsAsync(string searchTerm, int courseId, int instructorId);
        #endregion

        #region Quiz Question Management (Enhanced)
        /// <summary>
        /// Get quiz questions for student attempt (answers hidden)
        /// </summary>
        Task<IEnumerable<QuizQuestionResponseDto>> GetQuizQuestionsForAttemptAsync(int quizId, int userId);

        /// <summary>
        /// Get quiz questions with answers revealed (instructor view)
        /// </summary>
        Task<IEnumerable<QuizQuestionResponseDto>> GetQuizQuestionsWithAnswersAsync(int quizId, int instructorId);

        /// <summary>
        /// Get detailed statistics for a specific question
        /// </summary>
        Task<QuizQuestionStatsDto?> GetQuestionStatisticsAsync(int questionId, int instructorId);

        /// <summary>
        /// Perform bulk operations on quiz questions
        /// </summary>
        Task<bool> BulkUpdateQuizQuestionsAsync(int quizId, BulkQuestionOperationDto operation, int instructorId);
        #endregion

        #region Quiz Taking
        Task<QuizAttemptResponseDto> StartQuizAttemptAsync(int quizId, int userId);
        Task<QuizAttemptResponseDto> SubmitQuizAsync(SubmitQuizDto submitDto, int userId);
        Task<QuizAttemptResponseDto?> GetCurrentQuizAttemptAsync(int quizId, int userId);
        Task<QuizAttemptResponseDto?> GetQuizAttemptAsync(int attemptId, int userId);
        Task<IEnumerable<QuizAttemptResponseDto>> GetUserQuizAttemptsAsync(int quizId, int userId);
        #endregion

        #region Quiz Analytics & Reports
        Task<IEnumerable<QuizAttemptResponseDto>> GetQuizAttemptsAsync(int quizId, int instructorId);
        Task<QuizAttemptResponseDto?> GetAttemptDetailsAsync(int attemptId, int instructorId);
        Task<IEnumerable<QuizAttemptResponseDto>> GetRecentAttemptsAsync(int instructorId, int count = 10);
        #endregion

        #region Validation & Access Control
        Task<bool> CanUserAccessQuizAsync(int quizId, int userId);
        Task<bool> CanUserAttemptQuizAsync(int quizId, int userId);
        Task<bool> HasUserPassedQuizAsync(int quizId, int userId);
        Task<bool> CanInstructorAccessQuizAsync(int quizId, int instructorId);
        Task<bool> CanInstructorAccessQuestionAsync(int questionId, int instructorId);
        #endregion

        #region Progress & Requirements
        Task<IEnumerable<QuizSummaryDto>> GetRequiredQuizzesForProgressAsync(int contentId, int? sectionId, int? levelId, int? courseId, int userId);
        Task<bool> AreRequiredQuizzesCompletedAsync(int contentId, int? sectionId, int? levelId, int? courseId, int userId);
        #endregion
    }
}