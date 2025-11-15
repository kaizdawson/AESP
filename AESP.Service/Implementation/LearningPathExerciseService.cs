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
    public class LearningPathExerciseService : ILearningPathExerciseService
    {
        private readonly IGenericRepository<LearningPathExercise> _repo;

        public LearningPathExerciseService(IGenericRepository<LearningPathExercise> repo)
        {
            _repo = repo;
        }

        // ============================================================
        // 🔹 Lấy danh sách bài tập theo LearningPathChapterId
        // ============================================================
        public async Task<ResponseDTO> GetByLearningPathChapterIdAsync(Guid learningPathChapterId)
        {
            if (learningPathChapterId == Guid.Empty)
                return Fail(BusinessCode.VALIDATION_FAILED, "LearningPathChapterId không hợp lệ.");

            var list = await _repo.AsQueryable()
                .Include(x => x.Exercise)
                .Where(x => x.LearningPathChapterId == learningPathChapterId)
                .OrderBy(x => x.OrderIndex)
                .Select(x => new
                {
                    x.LearningPathExerciseId,
                    x.LearningPathChapterId,
                    x.ExerciseId,
                    x.OrderIndex,
                    x.Status,
                    x.ScoreAchieved,
                    x.NumberOfQuestion,

                    // 🔹 Từ bảng Exercise
                    ExerciseTitle = x.Exercise.Title,
                    ExerciseDescription = x.Exercise.Description
                })
                .ToListAsync();

            if (!list.Any())
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập trong chương này.");

            return Success(BusinessCode.GET_DATA_SUCCESSFULLY, "Lấy danh sách bài tập thành công.", list);
        }


        // ============================================================
        // 🔹 Helper chuẩn (FAIL / SUCCESS)
        // ============================================================
        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };

        private static ResponseDTO Success(BusinessCode code, string msg, object? data = null)
            => new() { IsSucess = true, BusinessCode = code, Message = msg, Data = data };
    }
}
