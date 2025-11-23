using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class ProgressAnalyticsService : IProgressAnalyticsService
    {
        private readonly IGenericRepository<ProgressAnalytics> _progressRepo;
        private readonly IGenericRepository<LearnerAnswer> _learnerAnswerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProgressAnalyticsService(
            IGenericRepository<ProgressAnalytics> progressRepo,
            IGenericRepository<LearnerAnswer> learnerAnswerRepo,
            IUnitOfWork unitOfWork)
        {
            _progressRepo = progressRepo;
            _learnerAnswerRepo = learnerAnswerRepo;
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // LIFETIME PROGRESS
        // ============================================================
        public async Task UpdateLifetimeAsync(Guid learnerProfileId)
        {
            if (learnerProfileId == Guid.Empty)
                return;

            // Lấy toàn bộ answer lifetime
            var answers = await _learnerAnswerRepo.AsQueryable()
                .Where(a => a.LearnerProfileId == learnerProfileId)
                .OrderBy(a => a.SubmittedAt)
                .ToListAsync();

            int sessionsCompleted = answers.Count;
            double avgScore = answers.Any() ? answers.Average(a => a.ScoreForVoice) : 0;

            double speakingSeconds = 0;

            // =====================================================
            // Tính SpeakingTime từ audio thật
            // =====================================================
            foreach (var ans in answers)
            {
                if (!string.IsNullOrWhiteSpace(ans.AudioRecordingUrl))
                {
                    speakingSeconds += await GetCloudinaryDurationAsync(ans.AudioRecordingUrl);
                }
            }

            double speakingMinutes = Math.Round(speakingSeconds / 60, 2);

            // =====================================================
            // Chỉ có 1 record duy nhất cho mỗi learner
            // =====================================================
            var progress = await _progressRepo.AsQueryable()
                .FirstOrDefaultAsync(p => p.LearnerProfileId == learnerProfileId);

            if (progress == null)
            {
                progress = new ProgressAnalytics
                {
                    ProgressAnalyticsId = Guid.NewGuid(),
                    LearnerProfileId = learnerProfileId,
                    DateRecorded = DateTime.UtcNow,
                    SpeakingTime = speakingMinutes,
                    SessionsCompleted = sessionsCompleted,
                    PronunciationScoreAvg = avgScore
                };

                await _progressRepo.Insert(progress);
            }
            else
            {
                progress.DateRecorded = DateTime.UtcNow;
                progress.SpeakingTime = speakingMinutes;
                progress.SessionsCompleted = sessionsCompleted;
                progress.PronunciationScoreAvg = avgScore;

                await _progressRepo.Update(progress);
            }

            await _unitOfWork.SaveChangeAsync();
        }

        // ============================================================
        // HÀM ĐỌC ĐỘ DÀI AUDIO TỪ CLOUDINARY (MemoryStream bắt buộc)
        // ============================================================
        private async Task<double> GetCloudinaryDurationAsync(string audioUrl)
        {
            try
            {
                // Lấy public_id từ URL
                var parts = audioUrl.Split('/');
                var fileName = parts[^1]; // ominous-47658.mp3
                var nameWithoutExt = fileName.Split('.')[0];

                var publicId = $"AESP/audios/{nameWithoutExt}";

                var cloudName = "ddqfq0jut";
                var apiKey = "YOUR_API_KEY";
                var apiSecret = "YOUR_API_SECRET";

                var requestUrl = $"https://api.cloudinary.com/v1_1/{cloudName}/resources/raw/upload/{publicId}";

                var client = new HttpClient();
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{apiKey}:{apiSecret}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var json = await client.GetStringAsync(requestUrl);

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                double duration = result.duration;

                return duration;
            }
            catch
            {
                return 0;
            }
        }



    }
}
