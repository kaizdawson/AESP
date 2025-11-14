using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("api/admin/ai-charge")]
    public class AIConversationChargeController : ControllerBase
    {
        private readonly IAIConversationChargeService _service;

        public AIConversationChargeController(IAIConversationChargeService service)
        {
            _service = service;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 20)
            => Ok(await _service.GetAllAsync(pageNumber, pageSize));

        [HttpGet("active")]
        public async Task<IActionResult> GetAllActive()
    => Ok(await _service.GetAllActiveAsync());


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AIConversationChargeCreateOrUpdateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AIConversationChargeCreateOrUpdateDto dto)
            => Ok(await _service.UpdateAsync(id, dto));

        [HttpPatch("status/{id}")]
        public async Task<IActionResult> ToggleStatus(Guid id)=> Ok(await _service.ToggleStatusAsync(id));


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok(await _service.DeleteAsync(id));
    }
}
