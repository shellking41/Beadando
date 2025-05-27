using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Beadando.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Beadando.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public class QuestionJson
        {
            public required string question { get; set; }
            public required string image { get; set; }
            public required List<string> answers { get; set; }
        }

        public class AnswersJson
        {
            public required List<string> correct_answers { get; set; }
        }

        private int GetAnswerIndex(string letter)
        {
            return letter.ToLower() switch
            {
                "a" => 0,
                "b" => 1,
                "c" => 2,
                _ => throw new ArgumentException($"Invalid answer letter: {letter}")
            };
        }

        public async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting database cleanup...");
                
                // Töröljük az összes választ
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Answers");
                _logger.LogInformation("Deleted all existing answers");

                // Töröljük az összes kérdést
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Questions");
                _logger.LogInformation("Deleted all existing questions");

                // Reset the auto-increment counters
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name='Questions' OR name='Answers'");
                _logger.LogInformation("Reset auto-increment counters");

                _logger.LogInformation("Starting to import questions and answers from JSON files...");

                string questionsPath = Path.Combine(Directory.GetCurrentDirectory(), "questions.json");
                string answersPath = Path.Combine(Directory.GetCurrentDirectory(), "asnwers.json");

                _logger.LogInformation($"Reading questions from: {questionsPath}");
                _logger.LogInformation($"Reading answers from: {answersPath}");

                if (!File.Exists(questionsPath) || !File.Exists(answersPath))
                {
                    throw new FileNotFoundException($"JSON files not found. Questions exists: {File.Exists(questionsPath)}, Answers exists: {File.Exists(answersPath)}");
                }

                var questionsJson = await File.ReadAllTextAsync(questionsPath);
                var answersJson = await File.ReadAllTextAsync(answersPath);

                _logger.LogInformation($"Questions JSON length: {questionsJson.Length}");
                _logger.LogInformation($"Answers JSON length: {answersJson.Length}");

                var questions = JsonSerializer.Deserialize<List<QuestionJson>>(questionsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Failed to deserialize questions.json");
                
                var answers = JsonSerializer.Deserialize<AnswersJson>(answersJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Failed to deserialize asnwers.json");

                _logger.LogInformation($"Deserialized {questions.Count} questions and {answers.correct_answers.Count} answers");

                // Ellenőrizzük, hogy a kérdések és válaszok száma megegyezik-e
                if (questions.Count != answers.correct_answers.Count)
                {
                    throw new InvalidOperationException($"Mismatch between questions ({questions.Count}) and answers ({answers.correct_answers.Count})");
                }

                // Létrehozzuk az összes kérdést először
                var questionEntities = new List<Question>();
                for (int i = 0; i < questions.Count; i++)
                {
                    var questionData = questions[i];
                    var correctAnswerLetter = answers.correct_answers[i];

                    _logger.LogInformation($"Processing question {i + 1}: {questionData.question.Substring(0, Math.Min(50, questionData.question.Length))}...");
                    _logger.LogInformation($"Correct answer is: {correctAnswerLetter}");

                    var question = new Question
                    {
                        Text = questionData.question,
                        Image = string.IsNullOrWhiteSpace(questionData.image) ? null : questionData.image,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Questions.AddAsync(question);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Added question with ID: {question.Id}");

                    // Most hozzáadjuk a válaszokat ehhez a kérdéshez
                    var correctAnswerIndex = GetAnswerIndex(correctAnswerLetter);
                    for (int j = 0; j < questionData.answers.Count; j++)
                    {
                        var answer = new Answer
                        {
                            Text = questionData.answers[j],
                            IsCorrect = j == correctAnswerIndex,
                            QuestionId = question.Id,
                            Question = question
                        };

                        await _context.Answers.AddAsync(answer);
                        _logger.LogInformation($"Added answer: {answer.Text} (IsCorrect: {answer.IsCorrect})");
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Saved answers for question {i + 1}");
                }

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private class QuestionData
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public string Image { get; set; }
        }

        private class AnswerData
        {
            public int Id { get; set; }
            public int QuestionId { get; set; }
            public string Text { get; set; }
            public bool IsCorrect { get; set; }
        }
    }
} 