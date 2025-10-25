using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AdminManagerService : IAdminManagerService
    {
        private readonly IGenericRepository<User> _userRepository;

        public AdminManagerService(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseDTO> GetManagerDetailAsync(Guid userId)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _userRepository.GetDbContext();

                var manager = await db.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.Role == "MANAGER");

                if (manager == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy người quản lý.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết người quản lý thành công.";
                dto.Data = new
                {
                    manager.UserId,
                    manager.FullName,
                    manager.Email,
                    manager.PhoneNumber,
                    manager.Role,
                    manager.CreatedAt
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết người quản lý: " + ex.Message;
            }

            return dto;
        }
    
        

        public async Task<ResponseDTO> GetManagersAsync(string? search, int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _userRepository.GetDbContext();

                var query = db.Users
                    .Where(u => u.Role == "MANAGER")
                    .AsQueryable();

                //  Tìm kiếm theo tên hoặc email
                if (!string.IsNullOrEmpty(search))
                {
                    string keyword = search.Trim().ToLower();
                    query = query.Where(u =>
                        u.FullName.ToLower().Contains(keyword));
                        
                }

                var totalItems = await query.CountAsync();

                var managers = await query
                    .OrderBy(u => u.FullName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.UserId,
                        u.FullName,
                        u.Email,
                        u.PhoneNumber,
                        u.Role,
                        u.CreatedAt
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách người quản lý thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = managers
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách người quản lý: " + ex.Message;
            }

            return dto;
        }
    }
}
