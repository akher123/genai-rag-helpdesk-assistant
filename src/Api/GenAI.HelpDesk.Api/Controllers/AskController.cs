using GenAI.HelpDesk.Api.Models;
using GenAI.HelpDesk.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GenAI.HelpDesk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AskController : ControllerBase
    {
        private readonly IRagService _rag;

        public AskController(IRagService rag)
        {
            _rag = rag;
        }

        /// <summary>
        /// Ask a question about the DRM help documents.
        /// </summary>
        /// <param name="request">User's question.</param>
        /// <returns>Answer and source list.</returns>
        [HttpPost]
        [Route("ask")]
        public async Task<ActionResult<AskQuestionResponse>> AskQuestion([FromBody] AskQuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.QuestionText))
                return BadRequest("QuestionText cannot be empty.");

            try
            {
                var result = await _rag.AskQuestionAsync(request.QuestionText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error here if desired
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
