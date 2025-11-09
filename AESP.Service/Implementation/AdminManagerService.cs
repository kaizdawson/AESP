using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Common.Helpers;
using AESP.Repository.Contract;
using AESP.Repository.Implementation;
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
        private readonly IUnitOfWork _unitOfWork;

        public AdminManagerService(IGenericRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
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

                string? decryptedPassword = null;
                if (!string.IsNullOrEmpty(manager.EncryptedPassword))
                {
                    try
                    {
                        decryptedPassword = AesEncryptionHelper.Decrypt(manager.EncryptedPassword);
                    }
                    catch
                    {
                        decryptedPassword = "Lỗi giải mã mật khẩu.";
                    }
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
                    Password = decryptedPassword, // ✅ hiển thị cho admin
                    manager.Status,
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

        public async Task<ResponseDTO> UpdateManagerAsync(Guid userId, UpdateManagerDto dto)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _userRepository.GetDbContext();

                var manager = await db.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.Role == "MANAGER");

                if (manager == null)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    response.Message = "Không tìm thấy người quản lý cần cập nhật.";
                    return response;
                }

                manager.FullName = dto.FullName;
                manager.Email = dto.Email;
                manager.PhoneNumber = dto.PhoneNumber;
                if (!string.IsNullOrEmpty(dto.Status))
                    manager.Status = dto.Status;

                if (!string.IsNullOrEmpty(dto.NewPassword))
                {
                    using var sha = System.Security.Cryptography.SHA256.Create();
                    manager.PasswordHash = Convert.ToBase64String(
                        sha.ComputeHash(Encoding.UTF8.GetBytes(dto.NewPassword)));
                    manager.EncryptedPassword = AesEncryptionHelper.Encrypt(dto.NewPassword);
                }

                await _userRepository.Update(manager);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                response.Message = "Cập nhật người quản lý thành công.";
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.EXCEPTION;
                response.Message = "Lỗi khi cập nhật người quản lý: " + ex.Message;
            }

            return response;
        }
    }
}
