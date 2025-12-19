using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
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
    public class LearnerProfileController : ControllerBase
    {
        private readonly ILearnerProfileService _service;

        public LearnerProfileController(ILearnerProfileService service)
        {
            _service = service;
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditProfile([FromBody] EditLearnerProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    businessCode = BusinessCode.INVALID_INPUT,
                    errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                        })
                });
            }
            var learnerProfileId = GetLearnerProfileIdFromToken(User);

            if (learnerProfileId == null)
                return Unauthorized(new { message = "Không xác định được learner từ token." });

            var result = await _service.EditLearnerProfileAsync(
                learnerProfileId.Value, dto);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        private Guid? GetLearnerProfileIdFromToken(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              user.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return null;

            var db = HttpContext.RequestServices.GetService<AESP.Repository.DB.AppDbContext>();
            var learner = db.Set<AESP.Repository.Models.LearnerProfile>()
                            .FirstOrDefault(x => x.UserId == userId);

            return learner?.LearnerProfileId;
        }
    }
}

