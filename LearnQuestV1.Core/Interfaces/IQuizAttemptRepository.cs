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
    }
}
