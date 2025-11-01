using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AuthQueryService : IAuthQueryService
    {
        private readonly IAuthQueryRepository _authQueryRepository;

        public AuthQueryService(IAuthQueryRepository authQueryRepository)
        {
            _authQueryRepository = authQueryRepository;
        }

        public async Task<ResponseDTO> GetUserInfoAsync(Guid userId)
        {
            var dto = new ResponseDTO();
            var user = await _authQueryRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                dto.IsSucess = false;
                dto.Message = "Người dùng không tồn tại.";
                return dto;
            }

            object? profile = null;

            if (user.Role.Equals("LEARNER", StringComparison.OrdinalIgnoreCase))
            {
                profile = await _authQueryRepository.GetLearnerProfileAsync(userId);
            }
            else if (user.Role.Equals("REVIEWER", StringComparison.OrdinalIgnoreCase))
            {
                var reviewerProfile = await _authQueryRepository.GetReviewerProfileAsync(userId);

                if (reviewerProfile != null)
                {
                    // 🔥 Bỏ phần Wallet, thay bằng coin_balance của user
                    profile = new
                    {
                        reviewerProfile.ReviewerProfileId,
                        reviewerProfile.UserId,
                        reviewerProfile.Experience,
                        reviewerProfile.Rating,
                        reviewerProfile.Status,
                        reviewerProfile.Levels,
                        Balance = user.CoinBalance, // lấy coin từ User
                        reviewerProfile.CreatedAt,
                        reviewerProfile.UpdatedAt,
                        reviewerProfile.IsDeleted
                    };
                }
            }

            dto.IsSucess = true;
            dto.Message = "Lấy thông tin người dùng thành công.";
            dto.Data = new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.AvatarUrl,
                user.Status,
                CoinBalance = user.CoinBalance,
                LearnerProfile = user.Role == "LEARNER" ? profile : null,
                ReviewerProfile = user.Role == "REVIEWER" ? profile : null
            };

            return dto;
        }

    }
}
