using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Interfaces
{
    public interface IQuizRepository : IBaseRepo<Quiz>
    {
        Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(int courseId);
        Task<IEnumerable<Quiz>> GetQuizzesByInstructorIdAsync(int instructorId);
        Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
        Task<Quiz?> GetQuizWithAttemptsAsync(int quizId);
        Task<IEnumerable<Quiz>> GetQuizzesByTypeAsync(QuizType quizType, int? entityId = null);
        Task<bool> HasUserPassedQuizAsync(int quizId, int userId);
        Task<int> GetUserAttemptCountAsync(int quizId, int userId);
        Task<QuizAttempt?> GetBestAttemptAsync(int quizId, int userId);
        Task<IEnumerable<QuizAttempt>> GetUserAttemptsAsync(int quizId, int userId);
        Task<bool> CanUserAttemptQuizAsync(int quizId, int userId);
        Task<IEnumerable<Quiz>> GetRequiredQuizzesForContentAsync(int contentId);
        Task<IEnumerable<Quiz>> GetRequiredQuizzesForSectionAsync(int sectionId);
        Task<IEnumerable<Quiz>> GetRequiredQuizzesForLevelAsync(int levelId);
        Task<IEnumerable<Quiz>> GetRequiredQuizzesForCourseAsync(int courseId);
        Task<bool> IsQuizAccessibleToUserAsync(int quizId, int userId);
    }
}
