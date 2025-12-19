using AESP.API.Helpers;
using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class LearnerProfileService : ILearnerProfileService
    {
        private readonly IGenericRepository<LearnerProfile> _learnerRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LearnerProfileService(
            IGenericRepository<LearnerProfile> learnerRepo,
            IGenericRepository<User> userRepo,
            IUnitOfWork unitOfWork)
        {
            _learnerRepo = learnerRepo;
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> EditLearnerProfileAsync(Guid learnerProfileId, EditLearnerProfileDto dto)
        {
            var res = new ResponseDTO();

            try
            {
                var learner = await _learnerRepo.GetByExpression(
                    x => x.LearnerProfileId == learnerProfileId,
                    x => x.User
                );

                if (learner == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Không tìm thấy learner.";
                    return res;
                }
                if (dto.FullName.All(char.IsDigit))
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.INVALID_INPUT;
                    res.Message = "Họ và tên không hợp lệ.";
                    return res;
                }
                if (string.IsNullOrWhiteSpace(dto.FullName))
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.INVALID_INPUT;
                    res.Message = "Họ và tên không được để trống.";
                    return res;
                }

                // ✅ CHỐNG TRÙNG SỐ ĐIỆN THOẠI
                var existedPhone = await _userRepo.GetFirstByExpression(
                    x => x.PhoneNumber == dto.PhoneNumber && x.UserId != learner.UserId
                );

                if (existedPhone != null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DUPLICATE_DATA;
                    res.Message = "Số điện thoại đã được sử dụng.";
                    return res;
                }

                // ✅ UPDATE USER
                learner.User.FullName = dto.FullName.Trim();
                learner.User.PhoneNumber = dto.PhoneNumber.Trim();
                learner.User.UpdatedAt = DateTimeHelper.NowVN();

                await _userRepo.Update(learner.User);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                res.Message = "Cập nhật thông tin cá nhân thành công.";
                res.Data = learner.User.UserId;
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.Message;
            }

            return res;
        }
    }
}
