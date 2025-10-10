using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.ManagerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MANAGER")]

    public class ManagerChapterController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public ManagerChapterController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        [HttpGet("chapters")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
                                                [FromQuery] Guid? courseId = null, [FromQuery] string? keyword = null)
        {
            var result = await _chapterService.GetAllChaptersAsync(pageNumber, pageSize, courseId, keyword);
            return Ok(result);
        }

        [HttpGet("chapters/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _chapterService.GetChapterByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("chapters")]
        public async Task<IActionResult> Create([FromBody] CreateChapterDTO dto)
        {
            var result = await _chapterService.CreateChapterAsync(dto);
            return Ok(result);
        }

        [HttpPut("chapters/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChapterDTO dto)
        {
            var result = await _chapterService.UpdateChapterAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("chapters/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _chapterService.DeleteChapterAsync(id);
            return Ok(result);
        }
    }
}
