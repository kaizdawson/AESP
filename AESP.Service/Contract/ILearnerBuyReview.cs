using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ILearnerBuyReview
    {
        Task<List<ReviewFeeMenuDto>> GetReviewFeeMenuAsync();

        Task<(bool isSuccess, string message)> BuyReviewFeeAsync(Guid userId, Guid reviewFeeId, Guid learnerAnswerId);

        Task<(bool isSuccess, string message)> BuyReviewFeeForRecordAsync(Guid userId, Guid reviewFeeId, Guid recordId);
        Task<ResponseDTO> GetLearnerReviewHistoryAsync(Guid learnerProfileId, int pageNumber = 1, int pageSize = 10, string? status = null, string? keyword = null);
    }
}
