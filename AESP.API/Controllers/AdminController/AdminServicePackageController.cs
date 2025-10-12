using AESP.Common.DTOs;
using AESP.Repository.Contract;
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        // POST: /api/AdminServicePackage
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServicePackageDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Lấy lỗi đầu tiên theo thứ tự field khai báo trong DTO
                var firstError = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .OrderBy(x => GetFieldOrder(x.Key)) // 🧠 Giữ thứ tự đúng như DTO
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
            return Ok(result);
        }

        // PUT: /api/AdminServicePackage/{id}
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
            return Ok(result);
        }

        // DELETE: /api/AdminServicePackage/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var result = await _service.GetListAsync();
            return Ok(result);
        }
        private static int GetFieldOrder(string fieldName)
        {
            var order = new List<string>
    {
        "Name",
        "Description",
        "Level",
        "Price",
        "Duration",
        "NumberOfReview",
        "Status"
    };

            var key = order.FindIndex(x => fieldName.Contains(x, StringComparison.OrdinalIgnoreCase));
            return key == -1 ? int.MaxValue : key;
        }
    }
}
