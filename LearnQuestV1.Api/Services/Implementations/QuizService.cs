using AutoMapper;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.DTOs.Quiz;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.Quiz;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuizService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Quiz Management

        public async Task<QuizResponseDto> CreateQuizAsync(CreateQuizDto createQuizDto, int instructorId)
        {
            var quiz = _mapper.Map<Quiz>(createQuizDto);
            quiz.InstructorId = instructorId;
            quiz.CreatedAt = DateTime.UtcNow;
            quiz.IsActive = true;

            await _unitOfWork.Quizzes.AddAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            var createdQuiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quiz.QuizId);
            return _mapper.Map<QuizResponseDto>(createdQuiz);
        }

        public async Task<QuizResponseDto> CreateQuizWithQuestionsAsync(CreateQuizWithQuestionsDto createDto, int instructorId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create quiz
                var quiz = _mapper.Map<Quiz>(createDto.Quiz);
                quiz.InstructorId = instructorId;
                quiz.CreatedAt = DateTime.UtcNow;
                quiz.IsActive = true;

                await _unitOfWork.Quizzes.AddAsync(quiz);
                await _unitOfWork.SaveChangesAsync();

                // Create new questions
                var questionIds = new List<int>();
                if (createDto.NewQuestions?.Any() == true)
                {
                    foreach (var newQuestionDto in createDto.NewQuestions)
                    {
                        var question = _mapper.Map<Question>(newQuestionDto);
                        question.InstructorId = instructorId;
                        question.CreatedAt = DateTime.UtcNow;
                        question.IsActive = true;

                        await _unitOfWork.Questions.AddAsync(question);
                        await _unitOfWork.SaveChangesAsync();

                        questionIds.Add(question.QuestionId);
                    }
                }

                // Add existing questions
                if (createDto.ExistingQuestionIds?.Any() == true)
                {
                    questionIds.AddRange(createDto.ExistingQuestionIds);
                }

                // Link questions to quiz
                var orderIndex = 1;
                foreach (var questionId in questionIds)
                {
                    var quizQuestion = new QuizQuestion
                    {
                        QuizId = quiz.QuizId,
                        QuestionId = questionId,
                        OrderIndex = orderIndex++
                    };

                    await _unitOfWork.QuizQuestions.AddAsync(quizQuestion);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                var createdQuiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quiz.QuizId);
                return _mapper.Map<QuizResponseDto>(createdQuiz);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<QuizResponseDto?> UpdateQuizAsync(UpdateQuizDto updateQuizDto, int instructorId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(updateQuizDto.QuizId);
            if (quiz == null || quiz.InstructorId != instructorId || quiz.IsDeleted)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateQuizDto.Title))
                quiz.Title = updateQuizDto.Title;

            if (updateQuizDto.Description != null)
                quiz.Description = updateQuizDto.Description;

            if (updateQuizDto.MaxAttempts.HasValue)
                quiz.MaxAttempts = updateQuizDto.MaxAttempts.Value;

            if (updateQuizDto.PassingScore.HasValue)
                quiz.PassingScore = updateQuizDto.PassingScore.Value;

            if (updateQuizDto.IsRequired.HasValue)
                quiz.IsRequired = updateQuizDto.IsRequired.Value;

            if (updateQuizDto.TimeLimitInMinutes.HasValue)
                quiz.TimeLimitInMinutes = updateQuizDto.TimeLimitInMinutes.Value;

            if (updateQuizDto.IsActive.HasValue)
                quiz.IsActive = updateQuizDto.IsActive.Value;

            quiz.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Quizzes.Update(quiz);
            await _unitOfWork.SaveChangesAsync();

            var updatedQuiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quiz.QuizId);
            return _mapper.Map<QuizResponseDto>(updatedQuiz);
        }

        public async Task<bool> DeleteQuizAsync(int quizId, int instructorId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null || quiz.InstructorId != instructorId || quiz.IsDeleted)
                return false;

            quiz.IsDeleted = true;
            quiz.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Quizzes.Update(quiz);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleQuizStatusAsync(int quizId, int instructorId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null || quiz.InstructorId != instructorId || quiz.IsDeleted)
                return false;

            quiz.IsActive = !quiz.IsActive;
            quiz.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Quizzes.Update(quiz);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Quiz Retrieval

        public async Task<QuizResponseDto?> GetQuizByIdAsync(int quizId, int? userId = null)
        {
            var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null) return null;

            var dto = _mapper.Map<QuizResponseDto>(quiz);

            if (userId.HasValue)
            {
                dto.UserAttempts = await _unitOfWork.QuizAttempts.GetAttemptCountForUserAsync(quizId, userId.Value);
                dto.HasPassed = await _unitOfWork.QuizAttempts.HasUserPassedQuizAsync(quizId, userId.Value);
                dto.CanAttempt = await _unitOfWork.QuizAttempts.CanUserStartNewAttemptAsync(quizId, userId.Value);

                var bestAttempt = await _unitOfWork.QuizAttempts.GetBestAttemptForUserAsync(quizId, userId.Value);
                dto.BestScore = bestAttempt?.Score;
            }

            return dto;
        }

        public async Task<IEnumerable<QuizSummaryDto>> GetQuizzesByCourseAsync(int courseId, int? userId = null)
        {
            var quizzes = await _unitOfWork.Quizzes.GetQuizzesByCourseIdAsync(courseId);
            var dtos = _mapper.Map<IEnumerable<QuizSummaryDto>>(quizzes);

            if (userId.HasValue)
            {
                foreach (var dto in dtos)
                {
                    dto.UserAttempts = await _unitOfWork.QuizAttempts.GetAttemptCountForUserAsync(dto.QuizId, userId.Value);
                    dto.HasPassed = await _unitOfWork.QuizAttempts.HasUserPassedQuizAsync(dto.QuizId, userId.Value);
                    dto.CanAttempt = await _unitOfWork.QuizAttempts.CanUserStartNewAttemptAsync(dto.QuizId, userId.Value);
                }
            }

            return dtos;
        }

        public async Task<IEnumerable<QuizSummaryDto>> GetQuizzesByInstructorAsync(int instructorId)
        {
            var quizzes = await _unitOfWork.Quizzes.GetQuizzesByInstructorIdAsync(instructorId);
            return _mapper.Map<IEnumerable<QuizSummaryDto>>(quizzes);
        }

        public async Task<IEnumerable<QuizResponseDto>> GetQuizzesByTypeAsync(QuizType quizType, int? entityId = null, int? userId = null)
        {
            var quizzes = await _unitOfWork.Quizzes.GetQuizzesByTypeAsync(quizType, entityId);
            var dtos = _mapper.Map<IEnumerable<QuizResponseDto>>(quizzes);

            if (userId.HasValue)
            {
                foreach (var dto in dtos)
                {
                    dto.UserAttempts = await _unitOfWork.QuizAttempts.GetAttemptCountForUserAsync(dto.QuizId, userId.Value);
                    dto.HasPassed = await _unitOfWork.QuizAttempts.HasUserPassedQuizAsync(dto.QuizId, userId.Value);
                    dto.CanAttempt = await _unitOfWork.QuizAttempts.CanUserStartNewAttemptAsync(dto.QuizId, userId.Value);

                    var bestAttempt = await _unitOfWork.QuizAttempts.GetBestAttemptForUserAsync(dto.QuizId, userId.Value);
                    dto.BestScore = bestAttempt?.Score;
                }
            }

            return dtos;
        }

        #endregion

        #region Question Management

        public async Task<QuestionResponseDto> CreateQuestionAsync(CreateQuestionDto createQuestionDto, int instructorId)
        {
            var question = _mapper.Map<Question>(createQuestionDto);
            question.InstructorId = instructorId;
            question.CreatedAt = DateTime.UtcNow;
            question.IsActive = true;

            await _unitOfWork.Questions.AddAsync(question);
            await _unitOfWork.SaveChangesAsync();

            var createdQuestion = await _unitOfWork.Questions.GetQuestionWithOptionsAsync(question.QuestionId);
            return _mapper.Map<QuestionResponseDto>(createdQuestion);
        }

        public async Task<QuestionResponseDto?> UpdateQuestionAsync(UpdateQuestionDto updateQuestionDto, int instructorId)
        {
            var question = await _unitOfWork.Questions.GetQuestionWithOptionsAsync(updateQuestionDto.QuestionId);
            if (question == null || question.InstructorId != instructorId)
                return null;

            // Check if question is used in active quizzes
            if (await _unitOfWork.Questions.IsQuestionUsedInActiveQuizAsync(question.QuestionId))
            {
                throw new InvalidOperationException("Cannot update question that is used in active quizzes");
            }

            // Update question properties
            if (!string.IsNullOrEmpty(updateQuestionDto.QuestionText))
                question.QuestionText = updateQuestionDto.QuestionText;

            if (updateQuestionDto.CodeSnippet != null)
                question.CodeSnippet = updateQuestionDto.CodeSnippet;

            if (updateQuestionDto.ProgrammingLanguage != null)
                question.ProgrammingLanguage = updateQuestionDto.ProgrammingLanguage;

            if (updateQuestionDto.Points.HasValue)
                question.Points = updateQuestionDto.Points.Value;

            if (updateQuestionDto.Explanation != null)
                question.Explanation = updateQuestionDto.Explanation;

            if (updateQuestionDto.IsActive.HasValue)
                question.IsActive = updateQuestionDto.IsActive.Value;

            question.UpdatedAt = DateTime.UtcNow;

            // Update options if provided
            if (updateQuestionDto.Options?.Any() == true)
            {
                // Handle option updates
                foreach (var optionDto in updateQuestionDto.Options)
                {
                    if (optionDto.OptionId.HasValue)
                    {
                        // Update existing option
                        var existingOption = question.QuestionOptions.FirstOrDefault(o => o.OptionId == optionDto.OptionId.Value);
                        if (existingOption != null)
                        {
                            if (optionDto.IsDeleted)
                            {
                                question.QuestionOptions.Remove(existingOption);
                            }
                            else
                            {
                                existingOption.OptionText = optionDto.OptionText;
                                existingOption.IsCorrect = optionDto.IsCorrect;
                                existingOption.OrderIndex = optionDto.OrderIndex;
                            }
                        }
                    }
                    else if (!optionDto.IsDeleted)
                    {
                        // Add new option
                        var newOption = new QuestionOption
                        {
                            QuestionId = question.QuestionId,
                            OptionText = optionDto.OptionText,
                            IsCorrect = optionDto.IsCorrect,
                            OrderIndex = optionDto.OrderIndex
                        };
                        question.QuestionOptions.Add(newOption);
                    }
                }
            }

            _unitOfWork.Questions.Update(question);
            await _unitOfWork.SaveChangesAsync();

            var updatedQuestion = await _unitOfWork.Questions.GetQuestionWithOptionsAsync(question.QuestionId);
            return _mapper.Map<QuestionResponseDto>(updatedQuestion);
        }

        public async Task<bool> DeleteQuestionAsync(int questionId, int instructorId)
        {
            var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
            if (question == null || question.InstructorId != instructorId)
                return false;

            // Check if question is used in active quizzes
            if (await _unitOfWork.Questions.IsQuestionUsedInActiveQuizAsync(questionId))
            {
                throw new InvalidOperationException("Cannot delete question that is used in active quizzes");
            }

            question.IsActive = false;
            question.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Questions.Update(question);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddQuestionsToQuizAsync(int quizId, List<int> questionIds, int instructorId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null || quiz.InstructorId != instructorId || quiz.IsDeleted)
                return false;

            // Get max order using the extended interface methods
            var maxOrder = 0;
            var hasExistingQuestions = await _unitOfWork.QuizQuestions.AnyAsync(qq => qq.QuizId == quizId);
            if (hasExistingQuestions)
            {
                maxOrder = await _unitOfWork.QuizQuestions.Where(qq => qq.QuizId == quizId).MaxAsync(qq => qq.OrderIndex);
            }

            foreach (var questionId in questionIds)
            {
                // Check if question belongs to instructor
                if (!await _unitOfWork.Questions.CanInstructorAccessQuestionAsync(questionId, instructorId))
                    continue;

                // Check if question is already in quiz
                var exists = await _unitOfWork.QuizQuestions
                    .AnyAsync(qq => qq.QuizId == quizId && qq.QuestionId == questionId);

                if (!exists)
                {
                    var quizQuestion = new QuizQuestion
                    {
                        QuizId = quizId,
                        QuestionId = questionId,
                        OrderIndex = ++maxOrder
                    };

                    await _unitOfWork.QuizQuestions.AddAsync(quizQuestion);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId, int instructorId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null || quiz.InstructorId != instructorId || quiz.IsDeleted)
                return false;

            var quizQuestion = await _unitOfWork.QuizQuestions
                .FirstOrDefaultAsync(qq => qq.QuizId == quizId && qq.QuestionId == questionId);

            if (quizQuestion == null)
                return false;

            _unitOfWork.QuizQuestions.Remove(quizQuestion);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReorderQuizQuestionsAsync(int quizId, Dictionary<int, int> questionOrders, int instructorId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null || quiz.InstructorId != instructorId || quiz.IsDeleted)
                return false;

            var quizQuestions = await _unitOfWork.QuizQuestions
                .Where(qq => qq.QuizId == quizId)
                .ToListAsync();

            foreach (var quizQuestion in quizQuestions)
            {
                if (questionOrders.TryGetValue(quizQuestion.QuestionId, out var newOrder))
                {
                    quizQuestion.OrderIndex = newOrder;
                    _unitOfWork.QuizQuestions.Update(quizQuestion);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Question Retrieval

        public async Task<QuestionResponseDto?> GetQuestionByIdAsync(int questionId, int instructorId)
        {
            var question = await _unitOfWork.Questions.GetQuestionWithOptionsAsync(questionId);
            if (question == null || question.InstructorId != instructorId)
                return null;

            return _mapper.Map<QuestionResponseDto>(question);
        }

        public async Task<IEnumerable<QuestionSummaryDto>> GetQuestionsByCourseAsync(int courseId, int instructorId)
        {
            var questions = await _unitOfWork.Questions.GetQuestionsByCourseIdAsync(courseId);
            var instructorQuestions = questions.Where(q => q.InstructorId == instructorId);

            var dtos = _mapper.Map<IEnumerable<QuestionSummaryDto>>(instructorQuestions);

            foreach (var dto in dtos)
            {
                dto.UsageCount = await _unitOfWork.Questions.GetQuestionUsageCountAsync(dto.QuestionId);
            }

            return dtos;
        }

        public async Task<IEnumerable<QuestionSummaryDto>> GetAvailableQuestionsForQuizAsync(int courseId, int instructorId)
        {
            var questions = await _unitOfWork.Questions.GetAvailableQuestionsForQuizAsync(courseId, instructorId);
            var dtos = _mapper.Map<IEnumerable<QuestionSummaryDto>>(questions);

            foreach (var dto in dtos)
            {
                dto.UsageCount = await _unitOfWork.Questions.GetQuestionUsageCountAsync(dto.QuestionId);
            }

            return dtos;
        }

        public async Task<IEnumerable<QuestionSummaryDto>> SearchQuestionsAsync(string searchTerm, int courseId, int instructorId)
        {
            var questions = await _unitOfWork.Questions.SearchQuestionsAsync(searchTerm, courseId, instructorId);
            var dtos = _mapper.Map<IEnumerable<QuestionSummaryDto>>(questions);

            foreach (var dto in dtos)
            {
                dto.UsageCount = await _unitOfWork.Questions.GetQuestionUsageCountAsync(dto.QuestionId);
            }

            return dtos;
        }

        #endregion

        #region Quiz Taking

        public async Task<QuizAttemptResponseDto> StartQuizAttemptAsync(int quizId, int userId)
        {
            // Validate access
            if (!await CanUserAttemptQuizAsync(quizId, userId))
                throw new InvalidOperationException("User cannot attempt this quiz");

            var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null)
                throw new ArgumentException("Quiz not found");

            var attemptCount = await _unitOfWork.QuizAttempts.GetAttemptCountForUserAsync(quizId, userId);

            var attempt = new QuizAttempt
            {
                QuizId = quizId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                AttemptNumber = attemptCount + 1,
                Score = 0,
                TotalPoints = quiz.QuizQuestions.Sum(qq => qq.CustomPoints ?? qq.Question.Points),
                Passed = false
            };

            await _unitOfWork.QuizAttempts.AddAsync(attempt);
            await _unitOfWork.SaveChangesAsync();

            var createdAttempt = await _unitOfWork.QuizAttempts.GetAttemptWithAnswersAsync(attempt.AttemptId);
            return _mapper.Map<QuizAttemptResponseDto>(createdAttempt);
        }

        public async Task<QuizAttemptResponseDto> SubmitQuizAsync(SubmitQuizDto submitDto, int userId)
        {
            var activeAttempt = await _unitOfWork.QuizAttempts.GetActiveAttemptAsync(submitDto.QuizId, userId);
            if (activeAttempt == null)
                throw new InvalidOperationException("No active attempt found");

            var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(submitDto.QuizId);
            if (quiz == null)
                throw new ArgumentException("Quiz not found");

            var completedAt = DateTime.UtcNow;
            var timeTaken = (int)(completedAt - activeAttempt.StartedAt).TotalMinutes;

            // Check time limit
            if (quiz.TimeLimitInMinutes.HasValue && timeTaken > quiz.TimeLimitInMinutes.Value)
                throw new InvalidOperationException("Time limit exceeded");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var totalScore = 0;
                var userAnswers = new List<UserAnswer>();

                // Process each answer
                foreach (var answerDto in submitDto.Answers)
                {
                    var quizQuestion = quiz.QuizQuestions.FirstOrDefault(qq => qq.Question.QuestionId == answerDto.QuestionId);
                    if (quizQuestion == null) continue;

                    var question = quizQuestion.Question;
                    var pointsForQuestion = quizQuestion.CustomPoints ?? question.Points;
                    var isCorrect = false;
                    var pointsEarned = 0;

                    if (question.QuestionType == QuestionType.MultipleChoice && answerDto.SelectedOptionId.HasValue)
                    {
                        var selectedOption = question.QuestionOptions.FirstOrDefault(o => o.OptionId == answerDto.SelectedOptionId.Value);
                        if (selectedOption?.IsCorrect == true)
                        {
                            isCorrect = true;
                            pointsEarned = pointsForQuestion;
                        }
                    }
                    else if (question.QuestionType == QuestionType.TrueFalse && answerDto.BooleanAnswer.HasValue)
                    {
                        var correctOption = question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);
                        if (correctOption != null)
                        {
                            var correctAnswer = correctOption.OptionText.ToLower() == "true";
                            if (answerDto.BooleanAnswer.Value == correctAnswer)
                            {
                                isCorrect = true;
                                pointsEarned = pointsForQuestion;
                            }
                        }
                    }

                    var userAnswer = new UserAnswer
                    {
                        AttemptId = activeAttempt.AttemptId,
                        QuestionId = answerDto.QuestionId,
                        SelectedOptionId = answerDto.SelectedOptionId,
                        BooleanAnswer = answerDto.BooleanAnswer,
                        IsCorrect = isCorrect,
                        PointsEarned = pointsEarned,
                        AnsweredAt = DateTime.UtcNow
                    };

                    userAnswers.Add(userAnswer);
                    totalScore += pointsEarned;
                }

                // Add all user answers
                foreach (var userAnswer in userAnswers)
                {
                    await _unitOfWork.UserAnswers.AddAsync(userAnswer);
                }

                // Update attempt
                activeAttempt.CompletedAt = completedAt;
                activeAttempt.TimeTakenInMinutes = timeTaken;
                activeAttempt.Score = totalScore;
                activeAttempt.Passed = activeAttempt.ScorePercentage >= quiz.PassingScore;

                _unitOfWork.QuizAttempts.Update(activeAttempt);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                var completedAttempt = await _unitOfWork.QuizAttempts.GetAttemptWithAnswersAsync(activeAttempt.AttemptId);
                return _mapper.Map<QuizAttemptResponseDto>(completedAttempt);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<QuizAttemptResponseDto?> GetQuizAttemptAsync(int attemptId, int userId)
        {
            var attempt = await _unitOfWork.QuizAttempts.GetAttemptWithAnswersAsync(attemptId);
            if (attempt == null || attempt.UserId != userId)
                return null;

            return _mapper.Map<QuizAttemptResponseDto>(attempt);
        }

        public async Task<IEnumerable<QuizAttemptResponseDto>> GetUserQuizAttemptsAsync(int quizId, int userId)
        {
            var attempts = await _unitOfWork.QuizAttempts.GetUserAttemptsForQuizAsync(quizId, userId);
            return _mapper.Map<IEnumerable<QuizAttemptResponseDto>>(attempts);
        }

        #endregion

        #region Quiz Analytics & Reports

        public async Task<IEnumerable<QuizAttemptResponseDto>> GetQuizAttemptsAsync(int quizId, int instructorId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null || quiz.InstructorId != instructorId)
                return Enumerable.Empty<QuizAttemptResponseDto>();

            var attempts = await _unitOfWork.QuizAttempts.GetAttemptsByQuizIdAsync(quizId);
            return _mapper.Map<IEnumerable<QuizAttemptResponseDto>>(attempts);
        }

        public async Task<QuizAttemptResponseDto?> GetAttemptDetailsAsync(int attemptId, int instructorId)
        {
            var attempt = await _unitOfWork.QuizAttempts.GetAttemptWithAnswersAsync(attemptId);
            if (attempt?.Quiz.InstructorId != instructorId)
                return null;

            return _mapper.Map<QuizAttemptResponseDto>(attempt);
        }

        public async Task<IEnumerable<QuizAttemptResponseDto>> GetRecentAttemptsAsync(int instructorId, int count = 10)
        {
            var attempts = await _unitOfWork.QuizAttempts.GetRecentAttemptsAsync(instructorId, count);
            return _mapper.Map<IEnumerable<QuizAttemptResponseDto>>(attempts);
        }

        #endregion

        #region Validation & Access Control

        public async Task<bool> CanUserAccessQuizAsync(int quizId, int userId)
        {
            return await _unitOfWork.Quizzes.IsQuizAccessibleToUserAsync(quizId, userId);
        }

        public async Task<bool> CanUserAttemptQuizAsync(int quizId, int userId)
        {
            return await _unitOfWork.QuizAttempts.CanUserStartNewAttemptAsync(quizId, userId);
        }

        public async Task<bool> HasUserPassedQuizAsync(int quizId, int userId)
        {
            return await _unitOfWork.QuizAttempts.HasUserPassedQuizAsync(quizId, userId);
        }

        public async Task<bool> CanInstructorAccessQuizAsync(int quizId, int instructorId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            return quiz != null && quiz.InstructorId == instructorId && !quiz.IsDeleted;
        }

        public async Task<bool> CanInstructorAccessQuestionAsync(int questionId, int instructorId)
        {
            return await _unitOfWork.Questions.CanInstructorAccessQuestionAsync(questionId, instructorId);
        }

        #endregion

        #region Progress & Requirements

        public async Task<IEnumerable<QuizSummaryDto>> GetRequiredQuizzesForProgressAsync(int contentId, int? sectionId, int? levelId, int? courseId, int userId)
        {
            var allRequiredQuizzes = new List<Quiz>();

            // Get content quizzes
            var contentQuizzes = await _unitOfWork.Quizzes.GetRequiredQuizzesForContentAsync(contentId);
            allRequiredQuizzes.AddRange(contentQuizzes);

            // Get section quizzes
            if (sectionId.HasValue)
            {
                var sectionQuizzes = await _unitOfWork.Quizzes.GetRequiredQuizzesForSectionAsync(sectionId.Value);
                allRequiredQuizzes.AddRange(sectionQuizzes);
            }

            // Get level quizzes
            if (levelId.HasValue)
            {
                var levelQuizzes = await _unitOfWork.Quizzes.GetRequiredQuizzesForLevelAsync(levelId.Value);
                allRequiredQuizzes.AddRange(levelQuizzes);
            }

            // Get course quizzes
            if (courseId.HasValue)
            {
                var courseQuizzes = await _unitOfWork.Quizzes.GetRequiredQuizzesForCourseAsync(courseId.Value);
                allRequiredQuizzes.AddRange(courseQuizzes);
            }

            var dtos = _mapper.Map<IEnumerable<QuizSummaryDto>>(allRequiredQuizzes.Distinct());

            // Add user progress info
            foreach (var dto in dtos)
            {
                dto.UserAttempts = await _unitOfWork.QuizAttempts.GetAttemptCountForUserAsync(dto.QuizId, userId);
                dto.HasPassed = await _unitOfWork.QuizAttempts.HasUserPassedQuizAsync(dto.QuizId, userId);
                dto.CanAttempt = await _unitOfWork.QuizAttempts.CanUserStartNewAttemptAsync(dto.QuizId, userId);
            }

            return dtos;
        }

        public async Task<bool> AreRequiredQuizzesCompletedAsync(int contentId, int? sectionId, int? levelId, int? courseId, int userId)
        {
            var requiredQuizzes = await GetRequiredQuizzesForProgressAsync(contentId, sectionId, levelId, courseId, userId);

            foreach (var quiz in requiredQuizzes)
            {
                if (!quiz.HasPassed.GetValueOrDefault())
                    return false;
            }

            return true;
        }

        #endregion
    }
}