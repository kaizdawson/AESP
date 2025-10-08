using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IReviewerProfileService
    {
        Task<ResponseDTO> GetByUserIdAsync(Guid userId);
        Task<ResponseDTO> UpdateProfileAsync(Guid userId, UpdateReviewerProfileDTO request);

    }
}
