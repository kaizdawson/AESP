using AESP.Repository.DB;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class LearnerTipController : ControllerBase
    {
        private readonly ILearnerTipService _service;
        private readonly AppDbContext _dbContext;
        public LearnerTipController(ILearnerTipService service, AppDbContext dbContext)
        {
            _service = service;
            _dbContext = dbContext;
        }
        [HttpGet("my-tip-history")]
        public async Task<IActionResult> GetMyTipHistory(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var learnerProfileId = GetLearnerProfileIdFromToken(User);
            if (learnerProfileId == null)
            {
                return Unauthorized(new { message = "Không xác định được learner từ token." });
            }

            var result = await _service.GetMyTipHistoryAsync(
                learnerProfileId.Value, fromDate, toDate, pageNumber, pageSize);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        private Guid? GetLearnerProfileIdFromToken(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return null;

            var learner = _dbContext.LearnerProfiles
                .FirstOrDefault(lp => lp.UserId == userId);

            return learner?.LearnerProfileId;
        }
    }

}
