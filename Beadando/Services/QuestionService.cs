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
                Image = q.Image,
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
                Image = question.Image,
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
                Image = q.Image,
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