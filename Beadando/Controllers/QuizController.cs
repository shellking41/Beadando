using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Beadando.Dtos;
using Beadando.Models;
using Beadando.Services;
using Microsoft.EntityFrameworkCore;
using Beadando.Data;

namespace Beadando.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly QuizService _quizService;
        private readonly SessionService _sessionService;
        private readonly ApplicationDbContext _context;

        public QuizController(QuizService quizService, SessionService sessionService, ApplicationDbContext context)
        {
            _quizService = quizService;
            _sessionService = sessionService;
            _context = context;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateQuiz([FromForm] QuizCreateRequest request)
        {
            try
            {
                var user = await _sessionService.ValidateSessionAsync(Request, Response);
                var quiz = await _quizService.CreateQuizAsync(request, user.Id);
                return Ok(new { message = "Quiz created successfully", quizId = quiz.Id });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("diagnostics")]
        public async Task<IActionResult> GetDiagnostics()
        {
            var questionCount = await _context.Questions.CountAsync();
            var answerCount = await _context.Answers.CountAsync();
            return Ok(new { 
                questionCount, 
                answerCount,
                message = questionCount == 0 ? "No questions found in database" : $"Found {questionCount} questions and {answerCount} answers"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuizzes()
        {
            try
            {
                await _sessionService.ValidateSessionAsync(Request, Response);
                var quizzes = await _quizService.GetAllQuizzesAsync();
                return Ok(quizzes);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            try
            {
                await _sessionService.ValidateSessionAsync(Request, Response);
                var quiz = await _quizService.GetQuizByIdAsync(id);
                if (quiz == null)
                    return NotFound("Quiz not found");
                return Ok(quiz);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("Question")]
        public async Task<IActionResult> AddQuestion([FromBody] QuestionCreateRequest request)
        {
            try
            {
                await _sessionService.ValidateSessionAsync(Request, Response);
                if (request.Answers == null || request.Answers.Count == 0)
                {
                    return BadRequest("At least one answer is required");
                }
                var question = await _quizService.AddQuestionAsync(request);
                return Ok(new { message = "Question added successfully", questionId = question.Id });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartQuiz()
        {
            try
            {
                var session = await _sessionService.ValidateSessionAsync(Request, Response);
                var quiz = await _quizService.StartNewQuizAsync(session.UserId);
                return Ok(quiz);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] List<UserAnswerSubmissionDto> answers)
        {
            try
            {
                var session = await _sessionService.ValidateSessionAsync(Request, Response);
                var result = await _quizService.SubmitQuizAsync(session.UserId, answers);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetUserResults()
        {
            try
            {
                var session = await _sessionService.ValidateSessionAsync(Request, Response);
                var results = await _context.UserQuizResults
                    .Where(r => r.UserId == session.UserId)
                    .OrderByDescending(r => r.CompletedAt)
                    .Select(r => new {
                        r.Id,
                        r.Score,
                        r.TotalQuestions,
                        r.CompletedAt,
                        Passed = (double)r.Score / r.TotalQuestions >= 0.75
                    })
                    .ToListAsync();
                return Ok(results);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}