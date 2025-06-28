// إنشاء ملف جديد: ExamMappingProfile.cs في مجلد Profiles

using AutoMapper;
using LearnQuestV1.Api.DTOs.Exam;
using LearnQuestV1.Core.DTOs.Quiz;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.Quiz;

namespace LearnQuestV1.Api.Profiles
{
    public class ExamMappingProfile : Profile
    {
        public ExamMappingProfile()
        {
            CreateExamMappings();
        }

        private void CreateExamMappings()
        {
            // CreateExamDto -> CreateQuizDto
            CreateMap<CreateExamDto, CreateQuizDto>()
                .ForMember(dest => dest.QuizType, opt => opt.MapFrom(src => QuizType.ExamQuiz));

            // UpdateExamDto -> UpdateQuizDto
            CreateMap<UpdateExamDto, UpdateQuizDto>()
                .ForMember(dest => dest.QuizId, opt => opt.MapFrom(src => src.ExamId));

            // QuizResponseDto -> ExamResponseDto
            CreateMap<QuizResponseDto, ExamResponseDto>()
                .ForMember(dest => dest.ExamId, opt => opt.MapFrom(src => src.QuizId))
                .ForMember(dest => dest.ExamType, opt => opt.MapFrom(src =>
                    src.LevelId.HasValue ? ExamType.LevelExam : ExamType.FinalExam))
                .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.TotalQuestions))
                .ForMember(dest => dest.TotalPoints, opt => opt.MapFrom(src => src.TotalPoints))
                .ForMember(dest => dest.RemainingAttempts, opt => opt.MapFrom(src =>
                    src.UserAttempts.HasValue ? Math.Max(0, src.MaxAttempts - src.UserAttempts.Value) : src.MaxAttempts))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.CanAttempt))
                .ForMember(dest => dest.IsScheduled, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.RequireProctoring, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.ShuffleQuestions, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.ShowResultsImmediately, opt => opt.MapFrom(src => false));

            // QuizSummaryDto -> ExamSummaryDto  
            CreateMap<QuizSummaryDto, ExamSummaryDto>()
                .ForMember(dest => dest.ExamId, opt => opt.MapFrom(src => src.QuizId))
                .ForMember(dest => dest.ExamType, opt => opt.MapFrom(src =>
                    src.QuizType == QuizType.LevelQuiz ? ExamType.LevelExam : ExamType.FinalExam))
                .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.TotalQuestions))
                .ForMember(dest => dest.TotalPoints, opt => opt.MapFrom(src => src.TotalPoints))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.CanAttempt))
                .ForMember(dest => dest.IsScheduled, opt => opt.MapFrom(src => false));

            // QuizAttemptResponseDto -> ExamAttemptResponseDto
            CreateMap<QuizAttemptResponseDto, ExamAttemptResponseDto>()
                .ForMember(dest => dest.ExamId, opt => opt.MapFrom(src => src.QuizId))
                .ForMember(dest => dest.ExamTitle, opt => opt.MapFrom(src => src.QuizTitle))
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            // QuizQuestionResponseDto -> ExamQuestionDto
            CreateMap<QuizQuestionResponseDto, ExamQuestionDto>()
                .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.QuestionId));

            // QuestionOptionResponseDto -> ExamQuestionOptionDto
            CreateMap<QuestionOptionResponseDto, ExamQuestionOptionDto>();

            // SubmitExamDto -> SubmitQuizDto
            CreateMap<SubmitExamDto, SubmitQuizDto>()
                .ForMember(dest => dest.QuizId, opt => opt.MapFrom(src => src.ExamId));

            // SubmitExamAnswerDto -> SubmitAnswerDto
            CreateMap<SubmitExamAnswerDto, SubmitAnswerDto>();

            // QuizAttempt -> ExamAttemptSummaryDto
            CreateMap<QuizAttempt, ExamAttemptSummaryDto>()
                .ForMember(dest => dest.ExamId, opt => opt.MapFrom(src => src.QuizId))
                .ForMember(dest => dest.ExamTitle, opt => opt.MapFrom(src => src.Quiz.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));

            // QuizAttempt -> ExamAttemptDetailDto
            CreateMap<QuizAttempt, ExamAttemptDetailDto>()
                .ForMember(dest => dest.ExamId, opt => opt.MapFrom(src => src.QuizId))
                .ForMember(dest => dest.ExamTitle, opt => opt.MapFrom(src => src.Quiz.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.UserAnswers));

            // UserAnswer -> ExamAnswerResultDto
            CreateMap<UserAnswer, ExamAnswerResultDto>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
                .ForMember(dest => dest.SelectedOptionText, opt => opt.MapFrom(src =>
                    src.SelectedOption != null ? src.SelectedOption.OptionText : null))
                .ForMember(dest => dest.CorrectAnswerText, opt => opt.MapFrom(src =>
                    src.Question.QuestionOptions.FirstOrDefault(o => o.IsCorrect).OptionText))
                .ForMember(dest => dest.Explanation, opt => opt.MapFrom(src => src.Question.Explanation));
        }
    }
}