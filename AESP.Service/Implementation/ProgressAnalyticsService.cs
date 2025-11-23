// AESP.Service/Implementation/ProgressAnalyticsService.cs
using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class ProgressAnalyticsService : IProgressAnalyticsService
    {
        private readonly IGenericRepository<ProgressAnalytics> _progressRepo;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;
        private readonly IGenericRepository<LearnerAnswer> _learnerAnswerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProgressAnalyticsService(
            IGenericRepository<ProgressAnalytics> progressRepo,
            IGenericRepository<LearnerProfile> learnerProfileRepo,
            IGenericRepository<LearnerAnswer> learnerAnswerRepo,
            IUnitOfWork unitOfWork)
        {
            _progressRepo = progressRepo;
            _learnerProfileRepo = learnerProfileRepo;
            _learnerAnswerRepo = learnerAnswerRepo;
            _unitOfWork = unitOfWork;
        }

        // ================================
        // Public API
        // ================================
        public async Task<ResponseDTO> GetByLearnerProfileIdAsync(Guid learnerProfileId)
        {
            if (learnerProfileId == Guid.Empty)
                return Fail(BusinessCode.VALIDATION_FAILED, "LearnerProfileId không hợp lệ.");

            // lấy bản ghi mới nhất theo DateRecorded
            var data = await _progressRepo.AsQueryable()
                .Where(x => x.LearnerProfileId == learnerProfileId)
                .OrderByDescending(x => x.DateRecorded)
                .FirstOrDefaultAsync();

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

        // ================================
        // Được gọi từ BackgroundService
        // ================================
        public async Task UpdateTodayAsync(Guid learnerProfileId)
        {
            if (learnerProfileId == Guid.Empty) return;

            var todayUtc = DateTime.UtcNow.Date;

            // Lấy tất cả LearnerAnswers của học viên trong ngày hôm nay
            var answers = await _learnerAnswerRepo.AsQueryable()
                .Where(a => a.LearnerProfileId == learnerProfileId
                            && a.SubmittedAt.Date == todayUtc)       // ✔ SubmittedAt là DateTime
                .OrderBy(a => a.SubmittedAt)
                .ToListAsync();

            double speakingSeconds = 0;
            int sessionsCompleted = answers.Count;
            double avgScore = 0;

            if (answers.Any())
            {
                const int maxGapSeconds = 180; // 3 phút

                for (int i = 1; i < answers.Count; i++)
                {
                    var prev = answers[i - 1].SubmittedAt;   // ✔ DateTime
                    var current = answers[i].SubmittedAt;

                    var delta = (current - prev).TotalSeconds;
                    if (delta > 0 && delta <= maxGapSeconds)
                    {
                        speakingSeconds += delta;
                    }
                }

                avgScore = answers.Average(a => a.ScoreForVoice);

            }

            // convert giây → phút
            double speakingMinutes = Math.Round(speakingSeconds / 60.0, 2);


            // Tìm hoặc tạo record ProgressAnalytics cho hôm nay
            var progress = await _progressRepo.AsQueryable()
                .FirstOrDefaultAsync(p =>
                    p.LearnerProfileId == learnerProfileId &&
                    p.DateRecorded.Date == todayUtc);

            if (progress == null)
            {
                progress = new ProgressAnalytics
                {
                    ProgressAnalyticsId = Guid.NewGuid(),
                    DateRecorded = DateTime.UtcNow,
                    LearnerProfileId = learnerProfileId,
                    SpeakingTime = speakingMinutes,
                    SessionsCompleted = sessionsCompleted,
                    PronunciationScoreAvg = avgScore
                };

                await _progressRepo.Insert(progress);
            }
            else
            {
                progress.SpeakingTime = speakingMinutes;
                progress.SessionsCompleted = sessionsCompleted;
                progress.PronunciationScoreAvg = avgScore;

                await _progressRepo.Update(progress);
            }

            await _unitOfWork.SaveChangeAsync();
        }

        // ================================
        // Helper
        // ================================
        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };

        private static ResponseDTO Success(BusinessCode code, string msg, object? data = null)
            => new() { IsSucess = true, BusinessCode = code, Message = msg, Data = data };
    }
}
