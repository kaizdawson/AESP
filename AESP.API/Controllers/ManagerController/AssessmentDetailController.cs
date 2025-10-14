using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AESP.API.Controllers.ManagerController
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] 
    public class AssessmentDetailController : ControllerBase
    {
        private readonly IAssessmentDetailService _assessmentDetailService;

        public AssessmentDetailController(IAssessmentDetailService assessmentDetailService)
        {
            _assessmentDetailService = assessmentDetailService;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAllAssessmentDetails(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? assessmentId = null)
        {
            var response = await _assessmentDetailService.GetAllAssessmentDetailsAsync(pageNumber, pageSize, assessmentId);
            return Ok(response);
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssessmentDetailById(Guid id)
        {
            var response = await _assessmentDetailService.GetAssessmentDetailByIdAsync(id);
            return Ok(response);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> CreateAssessmentDetail([FromBody] CreateAssessmentDetailDTO dto)
        {
            var response = await _assessmentDetailService.CreateAssessmentDetailAsync(dto);
            return Ok(response);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssessmentDetail(Guid id, [FromBody] UpdateAssessmentDetailDTO dto)
        {
            var response = await _assessmentDetailService.UpdateAssessmentDetailAsync(id, dto);
            return Ok(response);
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessmentDetail(Guid id)
        {
            var response = await _assessmentDetailService.DeleteAssessmentDetailAsync(id);
            return Ok(response);
        }
    }
}
