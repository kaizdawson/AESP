using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;

namespace AESP.Service.Implementation
{
    public class LearnerReviewRequestService : ILearnerReviewRequestService
    {
        private readonly IGenericRepository<LearnerAnswer> _answerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LearnerReviewRequestService(
            IGenericRepository<LearnerAnswer> answerRepo,
            IUnitOfWork unitOfWork)
        {
            _answerRepo = answerRepo;
            _unitOfWork = unitOfWork;
        }

        // =======================================================
        // 1) Learner bật / tắt IsNeededReviewed
        // =======================================================
        public async Task<ResponseDTO> UpdateReviewFlagAsync(
      Guid learnerProfileId,
      Guid answerId,
      bool isNeededReview,
      int numberOfReview)
        {
            var answer = await _answerRepo.AsQueryable()
                .FirstOrDefaultAsync(a => a.LearnerAnswerId == answerId);

            if (answer == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu trả lời.");

            if (answer.LearnerProfileId != learnerProfileId)
                return Fail(BusinessCode.ACCESS_DENIED, "Bạn không thể cập nhật câu trả lời của người khác.");

            answer.IsNeededReviewed = isNeededReview;

            if (isNeededReview)
            {
                answer.NumberofReview = numberOfReview;   // 👈 SET THEO REQUEST
            }
            else
            {
                answer.NumberofReview = 0;
            }

            await _answerRepo.Update(answer);
            await _unitOfWork.SaveChangeAsync();

            return Success("Cập nhật trạng thái review thành công.", new
            {
                answer.LearnerAnswerId,
                answer.IsNeededReviewed,
                answer.NumberofReview      // 👈 TRẢ VỀ SỐ REVIEW
            });
        }


        // =======================================================
        // 2) Lấy danh sách câu trả lời cần review
        // =======================================================
        public async Task<ResponseDTO> GetMyReviewRequestsAsync(Guid learnerProfileId)
        {
            var list = await _answerRepo.AsQueryable()
                .Where(a => a.LearnerProfileId == learnerProfileId && a.IsNeededReviewed == true)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return Success("Lấy danh sách review thành công.", list);
        }

        // =======================================================
        // 3) Xóa yêu cầu review cho 1 câu trả lời
        // =======================================================
        public async Task<ResponseDTO> ClearReviewRequestAsync(Guid learnerProfileId, Guid answerId)
        {
            var answer = await _answerRepo.AsQueryable()
                .FirstOrDefaultAsync(a => a.LearnerAnswerId == answerId);

            if (answer == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu trả lời.");

            if (answer.LearnerProfileId != learnerProfileId)
                return Fail(BusinessCode.ACCESS_DENIED, "Bạn không có quyền.");

            answer.IsNeededReviewed = false;
            answer.NumberofReview = 0;

            await _answerRepo.Update(answer);
            await _unitOfWork.SaveChangeAsync();

            return Success("Đã xóa yêu cầu review.", new { answer.LearnerAnswerId });
        }

        private ResponseDTO Fail(BusinessCode code, string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };

        private ResponseDTO Success(string msg, object data)
            => new ResponseDTO { IsSucess = true, Message = msg, Data = data, BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY };
    }
}
