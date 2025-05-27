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
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly QuestionService _questionService;

        public QuestionController(QuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet]
        [AllowAnonymous]
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
        [AllowAnonymous]
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

        [HttpGet("count")]
        public async Task<IActionResult> GetQuestionCount()
        {
            try
            {
                var count = await _questionService.GetQuestionCountAsync();
                return Ok(new { count });
            }
            catch (Exception)
            {
                return StatusCode(500, "Hiba történt a kérdések számának lekérdezése során.");
            }
        }

        [HttpGet("random/{count}")]
        public async Task<IActionResult> GetRandomQuestions(int count)
        {
            try
            {
                var questions = await _questionService.GetRandomQuestionsAsync(count);
                return Ok(questions);
            }
            catch (Exception)
            {
                return StatusCode(500, "Hiba történt a véletlenszerű kérdések lekérdezése során.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                var success = await _questionService.DeleteQuestionAsync(id);
                if (!success)
                    return NotFound();
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Hiba történt a kérdés törlése során.");
            }
        }
    }
} 