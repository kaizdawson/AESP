using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class RecordController : ControllerBase
    {
        private readonly IRecordService _recordService;

        public RecordController(IRecordService recordService)
        {
            _recordService = recordService;
        }

        private Guid GetLearnerProfileId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
            if (claim != null && Guid.TryParse(claim.Value, out var g))
                return g;

            throw new UnauthorizedAccessException("Không tìm thấy LearnerProfileId trong token");
        }

        // =========================
        // Create
        // =========================
        [HttpPost("create")]
        public async Task<IActionResult> CreateRecord([FromBody] CreateRecordDTO dto)
        {
            var learnerId = GetLearnerProfileId();
            var result = await _recordService.CreateRecordAsync(learnerId, dto);
            return Ok(result);
        }

        // =========================
        // Submit
        // =========================
        [HttpPost("{recordId}/submit")]
        public async Task<IActionResult> Submit(Guid recordId, [FromBody] SubmitRecordDTO dto)
        {
            var learnerId = GetLearnerProfileId();
            var result = await _recordService.SubmitRecordAsync(learnerId, recordId, dto);
            return Ok(result);
        }

        // =========================
        // Delete
        // =========================
        [HttpDelete("{recordId}")]
        public async Task<IActionResult> Delete(Guid recordId)
        {
            var learnerId = GetLearnerProfileId();
            var result = await _recordService.DeleteRecordAsync(learnerId, recordId);
            return Ok(result);
        }

        // =========================
        // GetAll
        // =========================
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var learnerId = GetLearnerProfileId();
            var result = await _recordService.GetAllRecordsAsync(learnerId);
            return Ok(result);
        }
    }
}
