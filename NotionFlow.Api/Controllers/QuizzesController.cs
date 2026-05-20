using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotionFlow.Api.Services;

namespace NotionFlow.Api.Controllers
{
    [ApiController]
    [Route("api/quizzes")]
    [Authorize]
    public class QuizzesController : ControllerBase
    {
        private readonly AnthropicService _anthropic;
        private readonly ILogger<QuizzesController> _logger;

        public QuizzesController(AnthropicService anthropic, ILogger<QuizzesController> logger)
        {
            _anthropic = anthropic;
            _logger = logger;
        }

        [HttpPost("generate-from-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> GenerateFromImage([FromForm] IFormFile? image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { error = "Imagen no proporcionada o vacía." });

            try
            {
                var quiz = await _anthropic.GenerateQuizFromImageAsync(image);
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar cuestionario desde imagen");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
