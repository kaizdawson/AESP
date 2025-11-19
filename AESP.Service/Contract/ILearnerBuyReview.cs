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
    }
}
