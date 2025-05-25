using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Beadando.Models;
using Beadando.Dtos;
using Beadando.Data;

namespace Beadando.Services
{
    public class QuestionService
    {
        private readonly ApplicationDbContext _context;

        public QuestionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Question> CreateQuestionAsync(QuestionCreateRequest request)
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

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<QuestionResponse>> GetAllQuestionsAsync()
        {
            var questions = await _context.Questions
                .Include(q => q.Answers)
                .ToListAsync();

            return questions.Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Answers = q.Answers.Select(a => new AnswerResponse
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
            }).ToList();
        }

        public async Task<QuestionResponse?> GetQuestionResponseByIdAsync(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return null;

            return new QuestionResponse
            {
                Id = question.Id,
                Text = question.Text,
                Answers = question.Answers.Select(a => new AnswerResponse
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
            };
        }

        public async Task<int> GetQuestionCountAsync()
        {
            return await _context.Questions.CountAsync();
        }

        public async Task<List<QuestionResponse>> GetRandomQuestionsAsync(int count)
        {
            var questions = await _context.Questions
                .Include(q => q.Answers)
                .OrderBy(r => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            return questions.Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Answers = q.Answers.Select(a => new AnswerResponse
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
            }).ToList();
        }

        public async Task<bool> DeleteQuestionAsync(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
                return false;

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 