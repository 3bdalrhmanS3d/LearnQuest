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
            CreateQuizMappings();
            CreateQuestionMappings();
            CreateQuizAttemptMappings();
            CreateQuizQuestionMappings();
        }

        private void CreateQuizMappings()
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
        }

        private void CreateQuestionMappings()
        {
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
        }

        private void CreateQuizAttemptMappings()
        {
            // Quiz Attempt Mappings
            CreateMap<QuizAttempt, QuizAttemptResponseDto>()
                .ForMember(dest => dest.QuizTitle, opt => opt.MapFrom(src => src.Quiz.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.UserAnswers))
                .ForMember(dest => dest.Questions, opt => opt.Ignore()); // Will be mapped separately

            // User Answer Mappings
            CreateMap<UserAnswer, UserAnswerResponseDto>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
                .ForMember(dest => dest.SelectedOptionText, opt => opt.MapFrom(src => src.SelectedOption != null ? src.SelectedOption.OptionText : null))
                .ForMember(dest => dest.CorrectAnswerText, opt => opt.MapFrom(src =>
                    src.Question.QuestionOptions.FirstOrDefault(o => o.IsCorrect) != null ?
                    src.Question.QuestionOptions.FirstOrDefault(o => o.IsCorrect).OptionText : null))
                .ForMember(dest => dest.Explanation, opt => opt.MapFrom(src => src.Question.Explanation));
        }

        private void CreateQuizQuestionMappings()
        {
            // QuizQuestion -> QuizQuestionResponseDto (for quiz attempts)
            CreateMap<QuizQuestion, QuizQuestionResponseDto>()
                .ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => src.Question.QuestionId))
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.Question.QuestionType))
                .ForMember(dest => dest.HasCode, opt => opt.MapFrom(src => src.Question.HasCode))
                .ForMember(dest => dest.CodeSnippet, opt => opt.MapFrom(src => src.Question.CodeSnippet))
                .ForMember(dest => dest.ProgrammingLanguage, opt => opt.MapFrom(src => src.Question.ProgrammingLanguage))
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.CustomPoints ?? src.Question.Points))
                .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.OrderIndex))
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Question.QuestionOptions.OrderBy(o => o.OrderIndex)))
                .ForMember(dest => dest.Explanation, opt => opt.Ignore()) // Not shown during attempt
                .ForMember(dest => dest.TimeStarted, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.IsAnswered, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.IsMarkedForReview, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.DifficultyLevel, opt => opt.Ignore()) // Optional
                .ForMember(dest => dest.Topic, opt => opt.Ignore()) // Optional
                .ForMember(dest => dest.RecommendedTime, opt => opt.Ignore()); // Optional

            // Question -> QuizQuestionResponseDto (direct mapping)
            CreateMap<Question, QuizQuestionResponseDto>()
                .ForMember(dest => dest.OrderIndex, opt => opt.Ignore()) // Will be set from QuizQuestion
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.QuestionOptions.OrderBy(o => o.OrderIndex)))
                .ForMember(dest => dest.TimeStarted, opt => opt.Ignore())
                .ForMember(dest => dest.IsAnswered, opt => opt.Ignore())
                .ForMember(dest => dest.IsMarkedForReview, opt => opt.Ignore())
                .ForMember(dest => dest.DifficultyLevel, opt => opt.Ignore())
                .ForMember(dest => dest.Topic, opt => opt.Ignore())
                .ForMember(dest => dest.RecommendedTime, opt => opt.Ignore());

            // QuestionOption -> QuizQuestionOptionDto
            CreateMap<QuestionOption, QuizQuestionOptionDto>()
                .ForMember(dest => dest.IsCorrect, opt => opt.Ignore()) // Hidden during quiz attempt
                .ForMember(dest => dest.SelectionCount, opt => opt.Ignore()) // For analytics
                .ForMember(dest => dest.SelectionPercentage, opt => opt.Ignore()); // For analytics

            // Question -> QuizQuestionStatsDto (for analytics)
            CreateMap<Question, QuizQuestionStatsDto>()
                .ForMember(dest => dest.TotalAttempts, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.CorrectAttempts, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.AccuracyRate, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.AverageTimeSpent, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.DifficultyLevel, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.DifficultyIndex, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.OptionStats, opt => opt.Ignore()) // Mapped separately
                .ForMember(dest => dest.Recommendations, opt => opt.Ignore()); // Generated in service

            // QuestionImportDto -> Question (for import functionality)
            CreateMap<QuestionImportDto, Question>()
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.InstructorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
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

            // Question -> QuestionImportDto (for export functionality)
            CreateMap<Question, QuestionImportDto>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.QuestionOptions.OrderBy(o => o.OrderIndex)));

            // QuestionOptionImportDto -> QuestionOption
            CreateMap<QuestionOptionImportDto, QuestionOption>()
                .ForMember(dest => dest.OptionId, opt => opt.Ignore())
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.UserAnswers, opt => opt.Ignore());

            // QuestionOption -> QuestionOptionImportDto
            CreateMap<QuestionOption, QuestionOptionImportDto>();
        }
    }
}