using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerBuyReviewController : ControllerBase
    {
        private readonly ILearnerBuyReview _service;

        public LearnerBuyReviewController(ILearnerBuyReview service)
        {
            _service = service;
        }


        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu()
        {
            var data = await _service.GetReviewFeeMenuAsync();

            if (data == null || !data.Any())
                return NotFound(new { message = "Không có gói review nào." });

            return Ok(data);
        }


        [HttpPost("buy")]
        public async Task<IActionResult> BuyPackage([FromBody] BuyReviewFeeRequest request)
        {

            var userIdString =
                  User.FindFirst("userId")?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value   
               ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdString))
                return Unauthorized(new { message = "Không tìm thấy userId trong token." });

            var userId = Guid.Parse(userIdString);

            var result = await _service
                .BuyReviewFeeAsync(userId, request.ReviewFeeId, request.LearnerAnswerId);

            if (!result.isSuccess)
                return BadRequest(new { message = result.message });

            return Ok(new { message = result.message });
        }

        [HttpPost("buy-record")]
        public async Task<IActionResult> BuyPackageForRecord([FromBody] BuyRecordReviewFeeRequest request)
        {
            var userIdString =
                  User.FindFirst("userId")?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdString))
                return Unauthorized(new { message = "Không tìm thấy userId trong token." });

            var userId = Guid.Parse(userIdString);

            var result = await _service
                .BuyReviewFeeForRecordAsync(userId, request.ReviewFeeId, request.RecordId);

            if (!result.isSuccess)
                return BadRequest(new { message = result.message });

            return Ok(new { message = result.message });
        }

    }
}
