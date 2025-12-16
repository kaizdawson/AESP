using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [ApiController]
    [Route("api/admin/ai-charge")]
    public class AIConversationChargeController : ControllerBase
    {
        private readonly IAIConversationChargeService _service;

        public AIConversationChargeController(IAIConversationChargeService service)
        {
            _service = service;
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet("list")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 20, string? status = null)
            => Ok(await _service.GetAllAsync(pageNumber, pageSize, status));
        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActive()
    => Ok(await _service.GetAllActiveAsync());

        [Authorize(Roles = "ADMIN")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AIConversationChargeCreateOrUpdateDto dto)
            => Ok(await _service.CreateAsync(dto));
        [Authorize(Roles = "ADMIN")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AIConversationChargeCreateOrUpdateDto dto)
            => Ok(await _service.UpdateAsync(id, dto));
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("status/{id}")]
        public async Task<IActionResult> ToggleStatus(Guid id)=> Ok(await _service.ToggleStatusAsync(id));

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok(await _service.DeleteAsync(id));
    }
}
