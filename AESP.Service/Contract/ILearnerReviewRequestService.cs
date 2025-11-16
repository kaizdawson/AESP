using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ILearnerReviewRequestService
    {
        Task<ResponseDTO> UpdateReviewFlagAsync(
    Guid learnerProfileId,
    Guid answerId,
    bool isNeededReview,
    int numberOfReview);

        Task<ResponseDTO> GetMyReviewRequestsAsync(Guid learnerProfileId);
        Task<ResponseDTO> ClearReviewRequestAsync(Guid learnerProfileId, Guid answerId);
    }
}
