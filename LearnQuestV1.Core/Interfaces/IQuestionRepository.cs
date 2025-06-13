using LearnQuestV1.Core.Models.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Interfaces
{
    public interface IQuestionRepository : IBaseRepo<Question>
    {
        Task<IEnumerable<Question>> GetQuestionsByCourseIdAsync(int courseId);
        Task<IEnumerable<Question>> GetQuestionsByInstructorIdAsync(int instructorId);
        Task<Question?> GetQuestionWithOptionsAsync(int questionId);
        Task<IEnumerable<Question>> GetQuestionsByContentIdAsync(int contentId);
        Task<IEnumerable<Question>> GetAvailableQuestionsForQuizAsync(int courseId, int instructorId);
        Task<bool> IsQuestionUsedInActiveQuizAsync(int questionId);
        Task<int> GetQuestionUsageCountAsync(int questionId);
        Task<IEnumerable<Question>> SearchQuestionsAsync(string searchTerm, int courseId, int instructorId);
        Task<bool> CanInstructorAccessQuestionAsync(int questionId, int instructorId);
    }
}
