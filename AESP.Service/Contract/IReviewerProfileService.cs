using AESP.Common.DTOs;
using AESP.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IReviewerProfileService
    {
        Task<ResponseDTO> GetProfileResponseByUserIdAsync(Guid userId);
        Task<ResponseDTO> UpdateProfileAsync(Guid userId, ReviewerProfileUpdateDto request);
        Task<ReviewerProfile?> GetByUserIdAsync(Guid userId);

    }
}
