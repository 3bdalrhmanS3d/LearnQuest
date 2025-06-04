using LearnQuestV1.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //Task<int> SaveChangesAsync();
        //Task<int> SaveChanges();
        //Task BeginTransactionAsync();
        //Task CommitTransactionAsync();
        //Task RollbackTransactionAsync();

        IBaseRepo<User> Users { get; }
        IBaseRepo<UserDetail> UserDetails { get; }
        IBaseRepo<AccountVerification> AccountVerifications { get; }
        IBaseRepo<RefreshToken> RefreshTokens { get; }
        IBaseRepo<UserVisitHistory> UserVisitHistories { get; }
        IBaseRepo<BlacklistToken> BlacklistTokens { get; }

        IBaseRepo<Course> Courses { get; }
        IBaseRepo<AboutCourse> AboutCourses { get; }
        IBaseRepo<CourseSkill> CourseSkills { get; }
        IBaseRepo<Level> Levels { get; }
        IBaseRepo<Section> Sections { get; }
        IBaseRepo<Content> Contents { get; }
        IBaseRepo<CourseEnrollment> CourseEnrollments { get; }
        IBaseRepo<CourseFeedback> CourseFeedbacks { get; }
        IBaseRepo<CourseReview> CourseReviews { get; }
        IBaseRepo<CourseTrack> CourseTracks { get; }
        IBaseRepo<CourseTrackCourse> CourseTrackCourses { get; }

        IBaseRepo<FavoriteCourse> FavoriteCourses { get; }
        IBaseRepo<Payment> Payments { get; }
        IBaseRepo<UserCoursePoint> UserCoursePoints { get; }
        IBaseRepo<UserProgress> UserProgresses { get; }
        IBaseRepo<UserContentActivity> UserContentActivities { get; }
        IBaseRepo<Notification> Notifications { get; }

        Task<int> SaveAsync();


    }

}
