using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUnitOfWork _unitOfWork;

        public RecordController(IRecordService recordService, IUnitOfWork unitOfWork)
        {
            _recordService = recordService;
            _unitOfWork = unitOfWork;
        }

        // Lấy LearnerProfileId từ token
        private async Task<Guid> GetLearnerProfileIdAsync()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
            if (claim != null && Guid.TryParse(claim.Value, out var id))
                return id;

            var sub = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Sub ||
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type.EndsWith("/nameidentifier")
            );

            if (sub == null || !Guid.TryParse(sub.Value, out var userId))
                throw new UnauthorizedAccessException("Token không hợp lệ.");

            var learner = await _unitOfWork.GetDbContext()
                .Set<LearnerProfile>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (learner == null)
                throw new UnauthorizedAccessException("Không tìm thấy hồ sơ học viên.");

            return learner.LearnerProfileId;
        }

        [HttpPost("{recordContentId}/submit")]
        public async Task<IActionResult> Submit(Guid recordContentId,[FromBody] SubmitRecordDTO dto)
        {
            var learnerId = await GetLearnerProfileIdAsync();

            return Ok(await _recordService
                .SubmitRecordAsync(learnerId, recordContentId, dto));
        }



        [HttpGet("{folderId}/mine")]
        public async Task<IActionResult> GetMine(Guid folderId)
        {
            var learnerId = await GetLearnerProfileIdAsync();
            return Ok(await _recordService.GetRecordsByCategoryAsync(learnerId, folderId));
        }

        [HttpDelete("{recordId}")]
        public async Task<IActionResult> Delete(Guid recordId)
        {
            var learnerId = await GetLearnerProfileIdAsync();
            return Ok(await _recordService.DeleteRecordAsync(learnerId, recordId));
        }


        [HttpPost("{folderId}/create-content-only")]
        public async Task<IActionResult> CreateContentOnly(Guid folderId, [FromBody] CreateRecordContentOnlyDTO dto)
        {
            var learnerId = await GetLearnerProfileIdAsync();
            return Ok(await _recordService.CreateRecordContentOnlyAsync(learnerId, folderId, dto));
        }


        [HttpPut("{recordId}/update-content")]
        public async Task<IActionResult> UpdateContent(Guid recordId, [FromBody] UpdateRecordContentDTO dto)
        {
            var learnerId = await GetLearnerProfileIdAsync();
            return Ok(await _recordService.UpdateRecordContentAsync(learnerId, recordId, dto));
        }

    }
}
