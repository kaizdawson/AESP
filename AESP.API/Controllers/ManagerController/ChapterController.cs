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

    public class ChapterController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public ChapterController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAllChapters(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? courseId = null,
            [FromQuery] string? keyword = null)
        {
            var response = await _chapterService.GetAllChaptersAsync(pageNumber, pageSize, courseId, keyword);
            return Ok(response);
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChapterById(Guid id)
        {
            var response = await _chapterService.GetChapterByIdAsync(id);
            return Ok(response);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterDTO dto)
        {
            var response = await _chapterService.CreateChapterAsync(dto);
            return Ok(response);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChapter(Guid id, [FromBody] UpdateChapterDTO dto)
        {
            var response = await _chapterService.UpdateChapterAsync(id, dto);
            return Ok(response);
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChapter(Guid id)
        {
            var response = await _chapterService.DeleteChapterAsync(id);
            return Ok(response);
        }
    }
}

