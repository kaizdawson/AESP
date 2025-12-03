using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminServicePackageController : ControllerBase
    {
        private readonly IServicePackageService _service;

        public AdminServicePackageController(IServicePackageService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActive()
        {
            var result = await _service.GetAllActiveAsync();
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll(
    [FromQuery] string? search,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? filter = "All")
        {
            var result = await _service.GetAllAsync(search, pageNumber, pageSize, filter);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServicePackageDto dto)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .OrderBy(x => GetFieldOrder(x.Key))
                    .Select(x => new
                    {
                        Field = x.Key,
                        Message = x.Value.Errors.First().ErrorMessage
                    })
                    .FirstOrDefault();

                return BadRequest(new
                {
                    isSucess = false,
                    businessCode = 4001,
                    message = firstError?.Message ?? "Dữ liệu không hợp lệ.",
                });
            }

            var result = await _service.CreateAsync(dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServicePackageDto dto)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .OrderBy(x => GetFieldOrder(x.Key))
                    .Select(x => new
                    {
                        Field = x.Key,
                        Message = x.Value.Errors.First().ErrorMessage
                    })
                    .FirstOrDefault();

                return BadRequest(new
                {
                    isSucess = false,
                    businessCode = 4001,
                    message = firstError?.Message ?? "Dữ liệu không hợp lệ.",
                    field = firstError?.Field
                });
            }

            var result = await _service.UpdateAsync(id, dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var result = await _service.ToggleStatusAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("statistic")]
        public async Task<IActionResult> GetServicePackageStatistic()
        {
            var result = await _service.GetServicePackageStatisticAsync();
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("{id}/buyers")]
        public async Task<IActionResult> GetBuyersOfServicePackage(Guid id)
        {
            var result = await _service.GetBuyersOfServicePackageAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        //[HttpPatch("{id}/bonus")]
        //public async Task<IActionResult> UpdateBonus(Guid id, [FromBody] UpdateBonusPercentDto dto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var firstError = ModelState
        //            .Where(x => x.Value.Errors.Count > 0)
        //            .Select(x => x.Value.Errors.First().ErrorMessage)
        //            .FirstOrDefault();

        //        return BadRequest(new
        //        {
        //            isSucess = false,
        //            businessCode = 4001,
        //            message = firstError ?? "Dữ liệu không hợp lệ."
        //        });
        //    }

        //    var result = await _service.UpdateBonusPercentAsync(id, dto);
        //    return StatusCode(result.IsSucess ? 200 : 400, result);
        //}





        private static int GetFieldOrder(string fieldName)
        {
            var order = new System.Collections.Generic.List<string>
            {
                "Name",
                "Description",
                "Price",
                "NumberOfCoin",
                "BonusPercent",
                "Status"
            };

            var key = order.FindIndex(x => fieldName.Contains(x, StringComparison.OrdinalIgnoreCase));
            return key == -1 ? int.MaxValue : key;
        }
    }
}
