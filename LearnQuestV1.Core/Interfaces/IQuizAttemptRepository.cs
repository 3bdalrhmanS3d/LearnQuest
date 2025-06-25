using LearnQuestV1.Core.Models.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Interfaces
{
    public interface IQuizAttemptRepository : IBaseRepo<QuizAttempt>
    {
        Task<IEnumerable<QuizAttempt>> GetAttemptsByQuizIdAsync(int quizId);
        Task<IEnumerable<QuizAttempt>> GetAttemptsByUserIdAsync(int userId);
        Task<QuizAttempt?> GetAttemptWithAnswersAsync(int attemptId);
        Task<IEnumerable<QuizAttempt>> GetUserAttemptsForQuizAsync(int quizId, int userId);
        Task<QuizAttempt?> GetBestAttemptForUserAsync(int quizId, int userId);
        Task<int> GetAttemptCountForUserAsync(int quizId, int userId);
        Task<bool> HasUserPassedQuizAsync(int quizId, int userId);
        Task<IEnumerable<QuizAttempt>> GetRecentAttemptsAsync(int instructorId, int count = 10);
        Task<QuizAttempt?> GetActiveAttemptAsync(int quizId, int userId);
        Task<bool> CanUserStartNewAttemptAsync(int quizId, int userId);
        Task<QuizAttempt?> GetActiveAttemptWithQuizAsync(int quizId, int userId);
        Task<IEnumerable<QuizAttempt>> GetUserExamAttemptsAsync(int userId, int? courseId = null);
        
        Task<QuizAttempt?> GetExamAttemptDetailAsync(int attemptId, int userId);
        Task<IEnumerable<QuizAttempt>> GetExamAttemptsByQuizIdAsync(int quizId, int pageNumber, int pageSize);


        Task<QuizAttempt?> GetBestExamAttemptAsync(int examId, int userId);

    }
}
