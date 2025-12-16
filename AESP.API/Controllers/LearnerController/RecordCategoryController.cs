using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "LEARNER")]
public class RecordCategoryController : ControllerBase
{
    private readonly IRecordCategoryService _categoryService;
    private readonly IUnitOfWork _unitOfWork;

    public RecordCategoryController(
        IRecordCategoryService categoryService,
        IUnitOfWork unitOfWork)
    {
        _categoryService = categoryService;
        _unitOfWork = unitOfWork;
    }


    private async Task<Guid> GetLearnerProfileIdAsync()
    {
 
        var learnerProfileClaim = User.Claims
            .FirstOrDefault(c => c.Type == "LearnerProfileId");

        if (learnerProfileClaim != null &&
            Guid.TryParse(learnerProfileClaim.Value, out var learnerProfileId))
        {
            return learnerProfileId;
        }


        var userIdClaim = User.Claims.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Sub ||
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type.EndsWith("/nameidentifier")
        );

        if (userIdClaim == null)
            throw new UnauthorizedAccessException("Token không hợp lệ.");

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("UserId trong token không hợp lệ.");


        var learnerProfile = await _unitOfWork
            .GetDbContext()
            .Set<LearnerProfile>()
            .FirstOrDefaultAsync(lp => lp.UserId == userId);

        if (learnerProfile == null)
            throw new UnauthorizedAccessException("Không tìm thấy hồ sơ học viên.");

        return learnerProfile.LearnerProfileId;
    }


    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateRecordCategoryDTO dto)
    {
        var id = await GetLearnerProfileIdAsync();
        return Ok(await _categoryService.CreateCategoryAsync(id, dto));
    }


    [HttpPut("{categoryId}/rename")]
    public async Task<IActionResult> Rename(Guid categoryId, [FromBody] RenameRecordCategoryDTO dto)
    {
        var id = await GetLearnerProfileIdAsync();
        return Ok(await _categoryService.RenameCategoryAsync(id, categoryId, dto.NewName));
    }


    [HttpDelete("{categoryId}")]
    public async Task<IActionResult> Delete(Guid categoryId)
    {
        var id = await GetLearnerProfileIdAsync();
        return Ok(await _categoryService.DeleteCategoryAsync(id, categoryId));
    }


    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var id = await GetLearnerProfileIdAsync();
        return Ok(await _categoryService.GetAllCategoriesAsync(id));
    }

    [HttpPost("{folderId}/purchase-record")]
    public async Task<IActionResult> PurchaseRecord(
    Guid folderId,
    [FromBody] PurchaseRecordChargeDTO dto)
    {
        var learnerProfileId = await GetLearnerProfileIdAsync();

        var userIdClaim = User.Claims.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Sub ||
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type.EndsWith("/nameidentifier")
        );

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        return Ok(await _categoryService
            .PurchaseRecordChargeAsync(learnerProfileId, userId, folderId, dto));
    }

}
