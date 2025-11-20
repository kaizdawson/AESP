using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IReviewerReviewService
    {
        Task<ResponseDTO> SubmitReviewAsync(Guid reviewerProfileId, Guid? learnerAnswerId, Guid? recordId, double score, string comment);
        Task<ResponseDTO> GetReviewHistoryAsync(Guid reviewerProfileId, int pageNumber = 1, int pageSize = 10);
        Task<ResponseDTO> GetPendingReviewsAsync(Guid reviewerProfileId, int pageNumber = 1, int pageSize = 10);
        Task<ResponseDTO> GetReviewerStatisticsAsync(Guid reviewerProfileId);
        Task<ResponseDTO> GetReviewerWalletAsync(Guid reviewerProfileId, int pageNumber, int pageSize);
        Task<ResponseDTO> TipAfterReviewAsync(Guid reviewerProfileId, ReviewerTipAfterReviewDTO dto);
    }
}
