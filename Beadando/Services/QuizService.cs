using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Beadando.Models;
using Beadando.Dtos;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Beadando.Data;

namespace Beadando.Services
{
    public class QuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();
        private const int QUESTIONS_PER_QUIZ = 10;
        private const double PASS_THRESHOLD = 0.75;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Quiz> CreateQuizAsync(QuizCreateRequest request, int userId)
        {
            var quiz = new Quiz
            {
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return quiz;
        }

        public async Task<List<Quiz>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes.ToListAsync();
        }

        public async Task<Quiz> GetQuizByIdAsync(int id)
        {
            return await _context.Quizzes.FindAsync(id);
        }

        public async Task<Question> AddQuestionAsync(QuestionCreateRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var question = new Question
                {
                    Text = request.Text,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                if (request.Answers == null || request.Answers.Count < 2 || request.Answers.Count > 4)
                    throw new ArgumentException("A question must have between 2 and 4 answers");

                if (!request.Answers.Any(a => a.IsCorrect))
                    throw new ArgumentException("At least one answer must be correct");

                foreach (var answerRequest in request.Answers)
                {
                    var answer = new Answer
                    {
                        Text = answerRequest.Text,
                        IsCorrect = answerRequest.IsCorrect,
                        QuestionId = question.Id,
                        Question = question
                    };

                    _context.Answers.Add(answer);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return question;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<QuizStartResponse> StartNewQuizAsync(int userId)
        {
            // Delete any unfinished quiz results for this user
            var unfinishedQuizzes = await _context.UserQuizResults
                .Where(q => q.UserId == userId && !q.CompletedAt.HasValue)
                .ToListAsync();
            
            if (unfinishedQuizzes.Any())
            {
                _context.UserQuizResults.RemoveRange(unfinishedQuizzes);
                await _context.SaveChangesAsync();
            }

            var questions = await _context.Questions
                .Include(q => q.Answers)
                .ToListAsync();

            if (questions.Count < QUESTIONS_PER_QUIZ)
            {
                throw new InvalidOperationException("Nincs elég kérdés az adatbázisban.");
            }

            var selectedQuestions = questions
                .OrderBy(x => _random.Next())
                .Take(QUESTIONS_PER_QUIZ)
                .ToList();

            var quizResult = new UserQuizResult
            {
                UserId = userId,
                User = await _context.Users.FindAsync(userId),
                StartedAt = DateTime.UtcNow,
                TotalQuestions = QUESTIONS_PER_QUIZ,
                Score = 0
            };

            await _context.UserQuizResults.AddAsync(quizResult);
            await _context.SaveChangesAsync();

            var questionDtos = selectedQuestions.Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Image = q.Image,
                Answers = q.Answers.Select(a => new AnswerResponse
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).OrderBy(x => _random.Next()).ToList()
            }).ToList();

            return new QuizStartResponse
            {
                QuizTitle = "KRESZ Teszt",
                UserQuizResultId = quizResult.Id,
                Questions = questionDtos
            };
        }

        public async Task<QuizResultDto> SubmitQuizAsync(int userId, List<UserAnswerSubmissionDto> answers)
        {
            var latestQuiz = await _context.UserQuizResults
                .Where(q => q.UserId == userId && !q.CompletedAt.HasValue)
                .OrderByDescending(q => q.StartedAt)
                .FirstOrDefaultAsync();

            if (latestQuiz == null)
            {
                throw new InvalidOperationException("Nincs aktív teszt.");
            }

            var correctAnswers = 0;

            foreach (var answer in answers)
            {
                var question = await _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == answer.QuestionId);

                if (question == null)
                {
                    throw new InvalidOperationException($"A kérdés nem található: {answer.QuestionId}");
                }

                var selectedAnswer = await _context.Answers
                    .FirstOrDefaultAsync(a => a.Id == answer.AnswerId);

                if (selectedAnswer == null)
                {
                    throw new InvalidOperationException($"A válasz nem található: {answer.AnswerId}");
                }

                var userAnswer = new UserAnswer
                {
                    UserQuizResultId = latestQuiz.Id,
                    UserQuizResult = latestQuiz,
                    QuestionId = question.Id,
                    Question = question,
                    AnswerId = selectedAnswer.Id,
                    Answer = selectedAnswer
                };

                await _context.UserAnswers.AddAsync(userAnswer);

                if (selectedAnswer.IsCorrect)
                {
                    correctAnswers++;
                }
            }

            latestQuiz.Score = correctAnswers;
            latestQuiz.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var passed = (double)correctAnswers / QUESTIONS_PER_QUIZ >= PASS_THRESHOLD;

            return new QuizResultDto
            {
                QuizTitle = "KRESZ Teszt",
                Score = correctAnswers,
                TotalQuestions = QUESTIONS_PER_QUIZ,
                Passed = passed,
                CompletedAt = latestQuiz.CompletedAt.Value
            };
        }
    }
}