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
    public class AdminUserService : IAdminUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<ReviewerProfile> _reviewerProfileRepository;

        public AdminUserService(IGenericRepository<User> userRepository, IGenericRepository<ReviewerProfile> reviewerProfileRepository)
        {
            _userRepository = userRepository;
            _reviewerProfileRepository = reviewerProfileRepository;
        }

        //  GET USER DETAIL
        public async Task<ResponseDTO> GetUserDetailAsync(Guid userId)
        {
            var dto = new ResponseDTO();

            try
            {
                // 🧠 Không Include property scalar (Role)
                var user = await _userRepository.GetFirstByExpression(u => u.UserId == userId);

                if (user == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy người dùng.";
                    return dto;
                }

                object? profileData = null;

                // ✅ Reviewer: lấy thêm ReviewerProfile + Certificates
                if (user.Role?.ToUpper() == "REVIEWER")
                {
                    var reviewerProfile = await _reviewerProfileRepository.GetFirstByExpression(
                        r => r.UserId == user.UserId,
                        r => r.Certificates
                    );

                    if (reviewerProfile != null)
                    {
                        profileData = new
                        {
                            reviewerProfile.Experience,
                            reviewerProfile.Rating,
                            reviewerProfile.Status,
                            Certificates = reviewerProfile.Certificates?.Select(c => new
                            {
                                c.CertificateId,
                                c.Name,
                                c.Url
                            }).ToList()
                        };
                    }
                }

                // ✅ Chuẩn hóa Response
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết người dùng thành công.";
                dto.Data = new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.Status,
                    user.Role,
                    user.AvatarUrl,
                    Profile = profileData
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = $"Lỗi khi lấy thông tin người dùng: {ex.Message}";
            }

            return dto;
        }
        

        public async Task<ResponseDTO> GetUsersByRoleAsync(string roleName)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var db = _userRepository.GetDbContext();

                var users = await db.Users
                    .Where(u => !string.IsNullOrEmpty(u.Role) && u.Role.ToLower() == roleName.ToLower())
                    .Select(u => new
                    {
                        u.UserId,
                        u.FullName,
                        u.Email,
                        u.PhoneNumber,
                        u.Status,
                        Role = u.Role,
                        Avatar = u.AvatarUrl
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = $"Lấy danh sách {roleName} thành công.";
                dto.Data = users;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách người dùng: " + ex.Message;
            }
            return dto;
        }
    }
}
