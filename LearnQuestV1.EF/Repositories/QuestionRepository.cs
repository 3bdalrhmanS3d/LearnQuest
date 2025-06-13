using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.Quiz;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.EF.Repositories
{
    public class QuestionRepository : BaseRepo<Question>, IQuestionRepository
    {
        public QuestionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Question>> GetQuestionsByCourseIdAsync(int courseId)
        {
            return await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Include(q => q.Content)
                .Include(q => q.QuestionOptions.OrderBy(o => o.OrderIndex))
                .Where(q => q.CourseId == courseId && q.IsActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetQuestionsByInstructorIdAsync(int instructorId)
        {
            return await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Content)
                .Include(q => q.QuestionOptions.OrderBy(o => o.OrderIndex))
                .Where(q => q.InstructorId == instructorId && q.IsActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Question?> GetQuestionWithOptionsAsync(int questionId)
        {
            return await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Include(q => q.Content)
                .Include(q => q.QuestionOptions.OrderBy(o => o.OrderIndex))
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.IsActive);
        }

        public async Task<IEnumerable<Question>> GetQuestionsByContentIdAsync(int contentId)
        {
            return await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Include(q => q.QuestionOptions.OrderBy(o => o.OrderIndex))
                .Where(q => q.ContentId == contentId && q.IsActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetAvailableQuestionsForQuizAsync(int courseId, int instructorId)
        {
            return await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Content)
                .Include(q => q.QuestionOptions.OrderBy(o => o.OrderIndex))
                .Where(q => q.CourseId == courseId &&
                          q.InstructorId == instructorId &&
                          q.IsActive)
                .OrderBy(q => q.QuestionText)
                .ToListAsync();
        }

        public async Task<bool> IsQuestionUsedInActiveQuizAsync(int questionId)
        {
            return await _context.QuizQuestions
                .Include(qq => qq.Quiz)
                .AnyAsync(qq => qq.QuestionId == questionId &&
                              qq.Quiz.IsActive &&
                              !qq.Quiz.IsDeleted);
        }

        public async Task<int> GetQuestionUsageCountAsync(int questionId)
        {
            return await _context.QuizQuestions
                .Include(qq => qq.Quiz)
                .CountAsync(qq => qq.QuestionId == questionId &&
                                qq.Quiz.IsActive &&
                                !qq.Quiz.IsDeleted);
        }

        public async Task<IEnumerable<Question>> SearchQuestionsAsync(string searchTerm, int courseId, int instructorId)
        {
            var normalizedSearchTerm = searchTerm.ToLower().Trim();

            return await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Content)
                .Include(q => q.QuestionOptions.OrderBy(o => o.OrderIndex))
                .Where(q => q.CourseId == courseId &&
                          q.InstructorId == instructorId &&
                          q.IsActive &&
                          (q.QuestionText.ToLower().Contains(normalizedSearchTerm) ||
                           (q.Explanation != null && q.Explanation.ToLower().Contains(normalizedSearchTerm)) ||
                           (q.CodeSnippet != null && q.CodeSnippet.ToLower().Contains(normalizedSearchTerm))))
                .OrderBy(q => q.QuestionText)
                .ToListAsync();
        }

        public async Task<bool> CanInstructorAccessQuestionAsync(int questionId, int instructorId)
        {
            return await _context.Questions
                .AnyAsync(q => q.QuestionId == questionId &&
                             q.InstructorId == instructorId &&
                             q.IsActive);
        }
    }
}