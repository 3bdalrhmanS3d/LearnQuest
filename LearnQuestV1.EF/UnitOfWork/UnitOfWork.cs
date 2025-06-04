using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.EF.Application;
using LearnQuestV1.EF.Repositories;

namespace LearnQuestV1.EF.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new BaseRepo<User>(context);
            UserDetails = new BaseRepo<UserDetail>(context);
            AccountVerifications = new BaseRepo<AccountVerification>(context);
            RefreshTokens = new BaseRepo<RefreshToken>(context);
            UserVisitHistories = new BaseRepo<UserVisitHistory>(context);
            BlacklistTokens = new BaseRepo<BlacklistToken>(context);

            Courses = new BaseRepo<Course>(context);
            AboutCourses = new BaseRepo<AboutCourse>(context);
            CourseSkills = new BaseRepo<CourseSkill>(context);
            Levels = new BaseRepo<Level>(context);
            Sections = new BaseRepo<Section>(context);
            Contents = new BaseRepo<Content>(context);
            CourseEnrollments = new BaseRepo<CourseEnrollment>(context);
            CourseFeedbacks = new BaseRepo<CourseFeedback>(context);
            CourseReviews = new BaseRepo<CourseReview>(context);
            CourseTracks = new BaseRepo<CourseTrack>(context);
            CourseTrackCourses = new BaseRepo<CourseTrackCourse>(context);

            FavoriteCourses = new BaseRepo<FavoriteCourse>(context);
            Payments = new BaseRepo<Payment>(context);
            UserCoursePoints = new BaseRepo<UserCoursePoint>(context);
            UserProgresses = new BaseRepo<UserProgress>(context);
            //UserContentActivities = new BaseRepo<UserContentActivity>(context);
            //Notifications = new BaseRepo<Notification>(context);
        }

        public IBaseRepo<User> Users { get; }
        public IBaseRepo<UserDetail> UserDetails { get; }
        public IBaseRepo<AccountVerification> AccountVerifications { get; }
        public IBaseRepo<RefreshToken> RefreshTokens { get; }
        public IBaseRepo<UserVisitHistory> UserVisitHistories { get; }
        public IBaseRepo<BlacklistToken> BlacklistTokens { get; }

        public IBaseRepo<Course> Courses { get; }
        public IBaseRepo<AboutCourse> AboutCourses { get; }
        public IBaseRepo<CourseSkill> CourseSkills { get; }
        public IBaseRepo<Level> Levels { get; }
        public IBaseRepo<Section> Sections { get; }
        public IBaseRepo<Content> Contents { get; }
        public IBaseRepo<CourseEnrollment> CourseEnrollments { get; }
        public IBaseRepo<CourseFeedback> CourseFeedbacks { get; }
        public IBaseRepo<CourseReview> CourseReviews { get; }
        public IBaseRepo<CourseTrack> CourseTracks { get; }
        public IBaseRepo<CourseTrackCourse> CourseTrackCourses { get; }

        public IBaseRepo<FavoriteCourse> FavoriteCourses { get; }
        public IBaseRepo<Payment> Payments { get; }
        public IBaseRepo<UserCoursePoint> UserCoursePoints { get; }
        public IBaseRepo<UserProgress> UserProgresses { get; }
        public IBaseRepo<UserContentActivity> UserContentActivities { get; }
        public IBaseRepo<Notification> Notifications { get; }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
