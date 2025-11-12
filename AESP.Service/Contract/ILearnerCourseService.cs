using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ILearnerCourseService
    {
        Task<ResponseDTO> EnrollAsync(Guid learnerId, Guid courseId);
        Task<ResponseDTO> UnenrollAsync(Guid learnerId, Guid courseId);
        Task<ResponseDTO> UpdateProgressAsync(Guid learnerId, Guid courseId, double progress);
        Task<ResponseDTO> GetFullCoursesByLevelAsync(string level, string? keyword = null);
        // 🔹 Hàm mới: học lại bài tập để cải thiện điểm
        Task<ResponseDTO> RelearnAndUpdateScoreAsync(Guid learnerProfileId, Guid exerciseId, double? newScore = null);
        Task<ResponseDTO> GetMyLevelsAsync(Guid learnerProfileId);


    }
}
