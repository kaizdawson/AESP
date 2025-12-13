using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordChargeController : ControllerBase
    {
        private readonly IRecordChargeService _service;

        public RecordChargeController(IRecordChargeService service)
        {
            _service = service;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("list")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
            => Ok(await _service.GetAllAsync(pageNumber, pageSize));

        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActive()
            => Ok(await _service.GetAllActiveAsync());

        [Authorize(Roles = "ADMIN")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] RecordChargeCreateOrUpdateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [Authorize(Roles = "ADMIN")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RecordChargeCreateOrUpdateDto dto)
            => Ok(await _service.UpdateAsync(id, dto));

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("status/{id}")]
        public async Task<IActionResult> ToggleStatus(Guid id)
            => Ok(await _service.ToggleStatusAsync(id));

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok(await _service.DeleteAsync(id));
        [Authorize(Roles = "ADMIN")]
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetail(
    Guid id,
    int pageNumber = 1,
    int pageSize = 10)
        {
            return Ok(await _service.GetDetailAsync(id, pageNumber, pageSize));
        }

    }
}
