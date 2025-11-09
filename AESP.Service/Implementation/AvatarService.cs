using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AvatarService : IAvatarService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly ICloudinaryService _cloudinary;
        private readonly IUnitOfWork _uow;

        public AvatarService(IGenericRepository<User> userRepo,
                             ICloudinaryService cloudinary,
                             IUnitOfWork uow)
        {
            _userRepo = userRepo;
            _cloudinary = cloudinary;
            _uow = uow;
        }

        public async Task<ResponseDTO> UpdateAvatarAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.INVALID_INPUT, Message = "File không hợp lệ." };

            var user = await _userRepo.GetById(userId);
            if (user == null)
                return new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.DATA_NOT_FOUND, Message = "Không tìm thấy người dùng." };

            //  Nếu user đã có avatar, ta có thể ghi đè hoặc xóa cũ nếu muốn
            var upload = await _cloudinary.UploadFileAsync(file, "avatars");
            if (!upload.IsSuccess)
                return new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.EXCEPTION, Message = upload.Message };

            user.AvatarUrl = upload.Url;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.Update(user);
            await _uow.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucess = true,
                BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                Message = "Cập nhật ảnh đại diện thành công.",
                Data = new
                {
                    user.UserId,
                    user.FullName,
                    user.AvatarUrl
                }
            };
        }

        public async Task<ResponseDTO> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.INVALID_INPUT, Message = "File không hợp lệ." };

            var user = await _userRepo.GetById(userId);
            if (user == null)
                return new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.DATA_NOT_FOUND, Message = "Không tìm thấy người dùng." };

            // upload lên Cloudinary, folder: avatars
            var up = await _cloudinary.UploadFileAsync(file, "avatars");
            if (!up.IsSuccess)
                return new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.EXCEPTION, Message = up.Message };

            user.AvatarUrl = up.Url;
            await _userRepo.Update(user);
            await _uow.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucess = true,
                BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                Message = "Tải lên ảnh đại diện thành công.",
                Data = new { user.UserId, user.AvatarUrl }
            };
        }
    }
}
