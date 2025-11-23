// AESP.Service/Implementation/ProgressAnalyticsQueryService.cs
using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace AESP.Service.Implementation
{
    public class ProgressAnalyticsQueryService : IProgressAnalyticsQueryService
    {
        private readonly IGenericRepository<ProgressAnalytics> _progressRepo;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;

        public ProgressAnalyticsQueryService(
            IGenericRepository<ProgressAnalytics> progressRepo,
            IGenericRepository<LearnerProfile> learnerProfileRepo)
        {
            _progressRepo = progressRepo;
            _learnerProfileRepo = learnerProfileRepo;
        }

        // GET theo LearnerProfileId
        public async Task<ResponseDTO> GetByLearnerProfileIdAsync(Guid learnerProfileId)
        {
            if (learnerProfileId == Guid.Empty)
                return Fail(BusinessCode.VALIDATION_FAILED, "LearnerProfileId không hợp lệ.");

            var data = await _progressRepo.AsQueryable()
                .FirstOrDefaultAsync(x => x.LearnerProfileId == learnerProfileId);

            if (data == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Chưa có thống kê nào cho học viên này.");

            var dto = new ReadProgressAnalyticsDTO
            {
                ProgressAnalyticsId = data.ProgressAnalyticsId,
                DateRecorded = data.DateRecorded,
                SpeakingTime = data.SpeakingTime,
                SessionsCompleted = data.SessionsCompleted,
                PronunciationScoreAvg = data.PronunciationScoreAvg,
                LearnerProfileId = data.LearnerProfileId
            };

            return Success(BusinessCode.GET_DATA_SUCCESSFULLY,
                "Lấy ProgressAnalytics thành công.",
                dto);
        }

        // GET theo UserId (đọc từ token)
        public async Task<ResponseDTO> GetMyProgressAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return Fail(BusinessCode.VALIDATION_FAILED, "UserId không hợp lệ.");

            var learner = await _learnerProfileRepo.AsQueryable()
                .FirstOrDefaultAsync(lp => lp.UserId == userId);

            if (learner == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy LearnerProfile tương ứng UserId.");

            return await GetByLearnerProfileIdAsync(learner.LearnerProfileId);
        }

        // Helper
        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };

        private static ResponseDTO Success(BusinessCode code, string msg, object? data = null)
            => new() { IsSucess = true, BusinessCode = code, Message = msg, Data = data };
    }
}
