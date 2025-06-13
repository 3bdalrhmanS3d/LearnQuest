using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.Quiz;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.EF.Repositories
{
    public class QuizRepository : BaseRepo<Quiz>, IQuizRepository
    {
        public QuizRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(int courseId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Include(q => q.Content)
                .Include(q => q.Section)
                .Include(q => q.Level)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                .Where(q => q.CourseId == courseId && !q.IsDeleted)
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByInstructorIdAsync(int instructorId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Content)
                .Include(q => q.Section)
                .Include(q => q.Level)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                .Where(q => q.InstructorId == instructorId && !q.IsDeleted)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Include(q => q.Content)
                .Include(q => q.Section)
                .Include(q => q.Level)
                .Include(q => q.QuizQuestions.OrderBy(qq => qq.OrderIndex))
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(question => question.QuestionOptions.OrderBy(o => o.OrderIndex))
                .FirstOrDefaultAsync(q => q.QuizId == quizId && !q.IsDeleted);
        }

        public async Task<Quiz?> GetQuizWithAttemptsAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Include(q => q.QuizAttempts)
                    .ThenInclude(qa => qa.User)
                .FirstOrDefaultAsync(q => q.QuizId == quizId && !q.IsDeleted);
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByTypeAsync(QuizType quizType, int? entityId = null)
        {
            var query = _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Where(q => q.QuizType == quizType && !q.IsDeleted);

            query = quizType switch
            {
                QuizType.ContentQuiz => query.Where(q => entityId == null || q.ContentId == entityId),
                QuizType.SectionQuiz => query.Where(q => entityId == null || q.SectionId == entityId),
                QuizType.LevelQuiz => query.Where(q => entityId == null || q.LevelId == entityId),
                QuizType.CourseQuiz => query.Where(q => entityId == null || q.CourseId == entityId),
                _ => query
            };

            return await query.OrderBy(q => q.CreatedAt).ToListAsync();
        }

        public async Task<bool> HasUserPassedQuizAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .AnyAsync(qa => qa.QuizId == quizId &&
                              qa.UserId == userId &&
                              qa.Passed);
        }

        public async Task<int> GetUserAttemptCountAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .CountAsync(qa => qa.QuizId == quizId && qa.UserId == userId);
        }

        public async Task<QuizAttempt?> GetBestAttemptAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Where(qa => qa.QuizId == quizId && qa.UserId == userId)
                .OrderByDescending(qa => qa.ScorePercentage)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<QuizAttempt>> GetUserAttemptsAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId && qa.UserId == userId)
                .OrderByDescending(qa => qa.StartedAt)
                .ToListAsync();
        }

        public async Task<bool> CanUserAttemptQuizAsync(int quizId, int userId)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.QuizId == quizId && !q.IsDeleted && q.IsActive);

            if (quiz == null) return false;

            // Check if user has already passed
            if (await HasUserPassedQuizAsync(quizId, userId))
                return false;

            // Check attempt limit
            var attemptCount = await GetUserAttemptCountAsync(quizId, userId);
            return attemptCount < quiz.MaxAttempts;
        }

        public async Task<IEnumerable<Quiz>> GetRequiredQuizzesForContentAsync(int contentId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Where(q => q.ContentId == contentId &&
                          q.IsRequired &&
                          q.IsActive &&
                          !q.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetRequiredQuizzesForSectionAsync(int sectionId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Where(q => q.SectionId == sectionId &&
                          q.IsRequired &&
                          q.IsActive &&
                          !q.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetRequiredQuizzesForLevelAsync(int levelId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Where(q => q.LevelId == levelId &&
                          q.IsRequired &&
                          q.IsActive &&
                          !q.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetRequiredQuizzesForCourseAsync(int courseId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Where(q => q.CourseId == courseId &&
                          q.QuizType == QuizType.CourseQuiz &&
                          q.IsRequired &&
                          q.IsActive &&
                          !q.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsQuizAccessibleToUserAsync(int quizId, int userId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                    .ThenInclude(c => c.CourseEnrollments)
                .FirstOrDefaultAsync(q => q.QuizId == quizId && !q.IsDeleted);

            if (quiz == null || !quiz.IsActive) return false;

            // Check if user is enrolled in the course
            return quiz.Course.CourseEnrollments.Any(ce => ce.UserId == userId);
        }
    }
}
