using AESP.API.Helpers;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        private readonly Cloudinary _cloudinary;

        public ProgressAnalyticsService(
            IGenericRepository<ProgressAnalytics> progressRepo,
            IGenericRepository<LearnerAnswer> learnerAnswerRepo,
            IUnitOfWork unitOfWork,
            Cloudinary cloudinary)
        {
            _progressRepo = progressRepo;
            _learnerAnswerRepo = learnerAnswerRepo;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
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
            double avgScore = answers.Any() ? Math.Round(answers.Average(a => a.ScoreForVoice), 1) : 0;

            double speakingSeconds = 0;

            // =====================================================
            // Tính SpeakingTime từ audio thật
            // =====================================================
            foreach (var ans in answers)
            {
                if (!string.IsNullOrWhiteSpace(ans.AudioRecordingUrl))
                {
                    speakingSeconds += await GetAudioDurationAsync(ans.AudioRecordingUrl);
                }
            }

            double totalSeconds = Math.Round(speakingSeconds);

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
                    DateRecorded = DateTimeHelper.NowVN(),
                    SpeakingTime = totalSeconds,
                    SessionsCompleted = sessionsCompleted,
                    PronunciationScoreAvg = avgScore
                };

                await _progressRepo.Insert(progress);
            }
            else
            {
                progress.DateRecorded = DateTimeHelper.NowVN();
                progress.SpeakingTime = totalSeconds;
                progress.SessionsCompleted = sessionsCompleted;
                progress.PronunciationScoreAvg = avgScore;

                await _progressRepo.Update(progress);
            }

            await _unitOfWork.SaveChangeAsync();
        }

        // ============================================================
        // HÀM ĐỌC ĐỘ DÀI AUDIO TỪ CLOUDINARY (MemoryStream bắt buộc)
        // ============================================================
        private async Task<double> GetAudioDurationAsync(string audioUrl)
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v quiet -of csv=p=0 -show_entries format=duration \"{audioUrl}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new System.Diagnostics.Process();
                process.StartInfo = startInfo;
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (double.TryParse(output.Trim(),
                                   System.Globalization.NumberStyles.Any,
                                   System.Globalization.CultureInfo.InvariantCulture,
                                   out double duration))
                {
                    return duration;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }








    }
}
