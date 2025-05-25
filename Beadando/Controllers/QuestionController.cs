using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Beadando.Services;
using Beadando.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Beadando.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly QuestionService _questionService;
        private readonly SessionService _sessionService;

        public QuestionController(QuestionService questionService, SessionService sessionService)
        {
            _questionService = questionService;
            _sessionService = sessionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuestions()
        {
            try
            {
                var questions = await _questionService.GetAllQuestionsAsync();
                return Ok(questions);
            }
            catch (Exception)
            {
                return StatusCode(500, "Hiba történt a kérdések lekérdezése során.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestion(int id)
        {
            try
            {
                var question = await _questionService.GetQuestionResponseByIdAsync(id);
                if (question == null)
                    return NotFound();

                return Ok(question);
            }
            catch (Exception)
            {
                return StatusCode(500, "Hiba történt a kérdés lekérdezése során.");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateRequest request)
        {
            try
            {
                var question = await _questionService.CreateQuestionAsync(request);
                var response = await _questionService.GetQuestionResponseByIdAsync(question.Id);
                return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest("Hiba történt a kérdés létrehozása során.");
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetQuestionCount()
        {
            try
            {
                await _sessionService.ValidateSessionAsync(Request, Response);
                var count = await _questionService.GetQuestionCountAsync();
                return Ok(new { count });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("random/{count}")]
        public async Task<IActionResult> GetRandomQuestions(int count)
        {
            try
            {
                await _sessionService.ValidateSessionAsync(Request, Response);
                var questions = await _questionService.GetRandomQuestionsAsync(count);
                return Ok(questions);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                await _sessionService.ValidateSessionAsync(Request, Response);
                var success = await _questionService.DeleteQuestionAsync(id);
                if (!success)
                    return NotFound();
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
} 