using AutoMapper;
using LearnQuestV1.Core.DTOs.Quiz;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.Quiz;

namespace LearnQuestV1.Api.Profiles
{
    public class QuizProfiles : Profile
    {
        public QuizProfiles()
        {
            // Quiz Mappings
            CreateMap<CreateQuizDto, Quiz>()
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForMember(dest => dest.InstructorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore())
                .ForMember(dest => dest.Section, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore())
                .ForMember(dest => dest.QuizQuestions, opt => opt.Ignore())
                .ForMember(dest => dest.QuizAttempts, opt => opt.Ignore());

            CreateMap<Quiz, QuizResponseDto>()
                .ForMember(dest => dest.ContentTitle, opt => opt.MapFrom(src => src.Content != null ? src.Content.Title : null))
                .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.Section != null ? src.Section.SectionName : null))
                .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => src.Level != null ? src.Level.LevelName : null))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.FullName))
                .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.QuizQuestions.Count))
                .ForMember(dest => dest.TotalPoints, opt => opt.MapFrom(src => src.QuizQuestions.Sum(qq => qq.CustomPoints ?? qq.Question.Points)))
                .ForMember(dest => dest.UserAttempts, opt => opt.Ignore())
                .ForMember(dest => dest.BestScore, opt => opt.Ignore())
                .ForMember(dest => dest.HasPassed, opt => opt.Ignore())
                .ForMember(dest => dest.CanAttempt, opt => opt.Ignore());

            CreateMap<Quiz, QuizSummaryDto>()
                .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.QuizQuestions.Count))
                .ForMember(dest => dest.TotalPoints, opt => opt.MapFrom(src => src.QuizQuestions.Sum(qq => qq.CustomPoints ?? qq.Question.Points)))
                .ForMember(dest => dest.UserAttempts, opt => opt.Ignore())
                .ForMember(dest => dest.HasPassed, opt => opt.Ignore())
                .ForMember(dest => dest.CanAttempt, opt => opt.Ignore());

            // Question Mappings
            CreateMap<CreateQuestionDto, Question>()
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.InstructorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.QuizQuestions, opt => opt.Ignore())
                .ForMember(dest => dest.UserAnswers, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src.Options != null)
                    {
                        dest.QuestionOptions = src.Options.Select(o => new QuestionOption
                        {
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect,
                            OrderIndex = o.OrderIndex
                        }).ToList();
                    }
                });

            CreateMap<Question, QuestionResponseDto>()
                .ForMember(dest => dest.ContentTitle, opt => opt.MapFrom(src => src.Content != null ? src.Content.Title : null))
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.QuestionOptions.OrderBy(o => o.OrderIndex)));

            CreateMap<Question, QuestionSummaryDto>()
                .ForMember(dest => dest.UsageCount, opt => opt.Ignore());

            // Question Option Mappings
            CreateMap<CreateQuestionOptionDto, QuestionOption>()
                .ForMember(dest => dest.OptionId, opt => opt.Ignore())
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.UserAnswers, opt => opt.Ignore());

            CreateMap<QuestionOption, QuestionOptionResponseDto>()
                .ForMember(dest => dest.IsCorrect, opt => opt.Ignore()); // Will be set conditionally in controller

            // Quiz Attempt Mappings
            CreateMap<QuizAttempt, QuizAttemptResponseDto>()
                .ForMember(dest => dest.QuizTitle, opt => opt.MapFrom(src => src.Quiz.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.UserAnswers));

            // User Answer Mappings
            CreateMap<UserAnswer, UserAnswerResponseDto>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
                .ForMember(dest => dest.SelectedOptionText, opt => opt.MapFrom(src => src.SelectedOption != null ? src.SelectedOption.OptionText : null))
                .ForMember(dest => dest.CorrectAnswerText, opt => opt.MapFrom(src =>
                    src.Question.QuestionOptions.FirstOrDefault(o => o.IsCorrect) != null ?
                    src.Question.QuestionOptions.FirstOrDefault(o => o.IsCorrect).OptionText : null))
                .ForMember(dest => dest.Explanation, opt => opt.MapFrom(src => src.Question.Explanation));
        }
    }
}