using LearnQuestV1.Api.DTOs.Instructor;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves the dashboard data for a given instructor (by their userId).
        /// </summary>
        /// <param name="instructorId">The instructor’s userId (from the JWT token).</param>
        /// <returns>A DashboardDto containing course counts and stats.</returns>
        Task<DashboardDto> GetDashboardAsync();
    }
}
