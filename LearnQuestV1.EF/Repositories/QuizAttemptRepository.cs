using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.Quiz;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.EF.Repositories
{
    public class QuizAttemptRepository : BaseRepo<QuizAttempt>, IQuizAttemptRepository
    {
        public QuizAttemptRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByQuizIdAsync(int quizId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId)
                .OrderByDescending(qa => qa.StartedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByUserIdAsync(int userId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.Course)
                .Include(qa => qa.User)
                .Where(qa => qa.UserId == userId)
                .OrderByDescending(qa => qa.StartedAt)
                .ToListAsync();
        }

        public async Task<QuizAttempt?> GetAttemptWithAnswersAsync(int attemptId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.Course)
                .Include(qa => qa.User)
                .Include(qa => qa.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.QuestionOptions)
                .Include(qa => qa.UserAnswers)
                    .ThenInclude(ua => ua.SelectedOption)
                .FirstOrDefaultAsync(qa => qa.AttemptId == attemptId);
        }

        public async Task<IEnumerable<QuizAttempt>> GetUserAttemptsForQuizAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId && qa.UserId == userId)
                .OrderByDescending(qa => qa.StartedAt)
                .ToListAsync();
        }

        public async Task<QuizAttempt?> GetBestAttemptForUserAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId && qa.UserId == userId)
                .OrderByDescending(qa => qa.ScorePercentage)
                .ThenByDescending(qa => qa.StartedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetAttemptCountForUserAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .CountAsync(qa => qa.QuizId == quizId && qa.UserId == userId);
        }

        public async Task<bool> HasUserPassedQuizAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .AnyAsync(qa => qa.QuizId == quizId &&
                              qa.UserId == userId &&
                              qa.Passed);
        }

        public async Task<IEnumerable<QuizAttempt>> GetRecentAttemptsAsync(int instructorId, int count = 10)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.Course)
                .Include(qa => qa.User)
                .Where(qa => qa.Quiz.InstructorId == instructorId)
                .OrderByDescending(qa => qa.StartedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<QuizAttempt?> GetActiveAttemptAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .FirstOrDefaultAsync(qa => qa.QuizId == quizId &&
                                         qa.UserId == userId &&
                                         qa.CompletedAt == null);
        }

        public async Task<QuizAttempt?> GetActiveAttemptWithQuizAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)          // هنا نقوم بتحميل الـ Quiz
                .FirstOrDefaultAsync(qa =>
                    qa.QuizId == quizId &&
                    qa.UserId == userId &&
                    qa.CompletedAt == null);
        }

        public async Task<bool> CanUserStartNewAttemptAsync(int quizId, int userId)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.QuizId == quizId &&
                                        q.IsActive &&
                                        !q.IsDeleted);

            if (quiz == null) return false;

            // Check if user already passed
            if (await HasUserPassedQuizAsync(quizId, userId))
                return false;

            // Check if there's an active attempt
            var activeAttempt = await GetActiveAttemptAsync(quizId, userId);
            if (activeAttempt != null) return false;

            // Check attempt limit
            var attemptCount = await GetAttemptCountForUserAsync(quizId, userId);
            return attemptCount < quiz.MaxAttempts;
        }
        public async Task<IEnumerable<QuizAttempt>> GetUserExamAttemptsAsync(int userId, int? courseId = null)
        {
            var query = _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.Course)
                .Include(qa => qa.User)
                .Where(qa => qa.UserId == userId && qa.Quiz.QuizType == QuizType.ExamQuiz);

            if (courseId.HasValue)
                query = query.Where(qa => qa.Quiz.CourseId == courseId.Value);

            return await query
                .OrderByDescending(qa => qa.StartedAt)
                .ToListAsync();
        }

        public async Task<QuizAttempt?> GetExamAttemptDetailAsync(int attemptId, int userId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.Course)
                .Include(qa => qa.User)
                .Include(qa => qa.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.QuestionOptions)
                .Include(qa => qa.UserAnswers)
                    .ThenInclude(ua => ua.SelectedOption)
                .Where(qa => qa.AttemptId == attemptId && qa.UserId == userId && qa.Quiz.QuizType == QuizType.ExamQuiz)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<QuizAttempt>> GetExamAttemptsByQuizIdAsync(int quizId, int pageNumber, int pageSize)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId && qa.CompletedAt.HasValue)
                .OrderByDescending(qa => qa.StartedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<QuizAttempt?> GetBestExamAttemptAsync(int examId, int userId)
        {
            return await _context.QuizAttempts
            .Include(qa => qa.Quiz)
            .Include(qa => qa.User)
            .Include(qa => qa.UserAnswers)
                .ThenInclude(ua => ua.Question)
                    .ThenInclude(q => q.QuestionOptions)
            .Include(qa => qa.UserAnswers)
                .ThenInclude(ua => ua.SelectedOption)
            .Where(qa => qa.QuizId == examId && qa.UserId == userId && qa.CompletedAt.HasValue)
            .OrderByDescending(qa => qa.ScorePercentage)
            .ThenByDescending(qa => qa.StartedAt)
            .FirstOrDefaultAsync();
        }

    }
}