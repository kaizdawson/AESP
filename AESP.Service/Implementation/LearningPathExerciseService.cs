using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Implementation;
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
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IGenericRepository<LearningPathQuestion> _lpQuestionRepo;
        private readonly IGenericRepository<LearningPathChapter> _lpChapterRepo;

        private readonly IUnitOfWork _unitOfWork;


        public LearningPathExerciseService(
            IGenericRepository<LearningPathExercise> repo,
            IGenericRepository<Question> questionRepo,
            IGenericRepository<LearningPathQuestion> lpQuestionRepo,
            IGenericRepository<LearningPathChapter> lpChapterRepo,
            IUnitOfWork unitOfWork)
        {
            _lpChapterRepo = lpChapterRepo;
            _repo = repo;
            _questionRepo = questionRepo;
            _lpQuestionRepo = lpQuestionRepo;
            _unitOfWork = unitOfWork;
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





        public async Task<ResponseDTO> UpdateStatusAsync(Guid learningPathExerciseId, string status)
        {
            if (learningPathExerciseId == Guid.Empty)
                return Fail(BusinessCode.VALIDATION_FAILED, "learningPathExerciseId không hợp lệ.");

            var lpExercise = await _repo.AsQueryable()
                .Include(x => x.LearningPathQuestions)
                .Include(x => x.LearningPathChapter)
                    .ThenInclude(c => c.LearningPathCourse)
                .FirstOrDefaultAsync(x => x.LearningPathExerciseId == learningPathExerciseId);

            if (lpExercise == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy LearningPathExercise.");

            var chapterId = lpExercise.LearningPathChapterId;
            var courseId = lpExercise.LearningPathChapter.LearningPathCourseId;
            var currentChapter = lpExercise.LearningPathChapter;
            var currentChapterOrder = currentChapter.OrderIndex;

            // ✅ LẤY TOÀN BỘ EXERCISE TRONG CÙNG CHAPTER
            var allExerciseInChapter = await _repo.AsQueryable()
                .Where(x => x.LearningPathChapterId == chapterId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();

            // ✅ LẤY TOÀN BỘ CHAPTER TRONG COURSE  (FIX _lpChapterRepo)
            var allChapterInCourse = await _lpChapterRepo.AsQueryable()
                .Where(x => x.LearningPathCourseId == courseId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();

            // =========================================================
            // ✅ KHI SET InProgress
            // =========================================================
            var lpCourse = lpExercise.LearningPathChapter.LearningPathCourse;

            // ❌ Course chưa cho học
            if (!string.Equals(lpCourse.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
            {
                return Fail(
                    BusinessCode.INVALID_ACTION,
                    "Khóa học này chưa được mở để học. Vui lòng hoàn thành khóa học trước đó."
                );
            }



            if (status == "InProgress")
            {
                // ✅ 1. CHẶN nếu CÙNG CHAPTER đã có bài khác InProgress
                bool hasOtherInProgress = allExerciseInChapter.Any(x =>
                    x.LearningPathExerciseId != learningPathExerciseId &&
                    x.Status == "InProgress");

                if (hasOtherInProgress)
                    return Fail(BusinessCode.INVALID_ACTION,
                        "Bài trước chỉ đạt {Math.Round(previousExercise.ScoreAchieved, 2)}%. Cần ≥ 50% để mở bài tiếp.");

                // ✅ 2. KHÔNG CHO HỌC CHAPTER SAU KHI CHAPTER TRƯỚC CHƯA HOÀN THÀNH 100%
                var previousChapter = allChapterInCourse
                    .Where(x => x.OrderIndex < currentChapterOrder)
                    .OrderByDescending(x => x.OrderIndex)
                    .FirstOrDefault();

                if (previousChapter != null)
                {
                    var prevChapterExercises = await _repo.AsQueryable()
                        .Where(x => x.LearningPathChapterId == previousChapter.LearningPathChapterId)
                        .ToListAsync();

                    bool prevChapterCompleted = prevChapterExercises.All(x =>
                        x.Status == "Completed" && x.ScoreAchieved >= 50);

                    if (!prevChapterCompleted)
                        return Fail(BusinessCode.INVALID_ACTION,
                            "Bạn cần hoàn thành bài tập trong Chapter trước với điểm tối thiểu 50% để mở Chapter tiếp theo.");
                }

                // ✅ 3. SINH LPQ NẾU CHƯA CÓ
                var existed = await _lpQuestionRepo.AsQueryable()
                    .AnyAsync(q => q.LearningPathExerciseId == learningPathExerciseId);

                if (!existed)
                {
                    var questions = await _questionRepo.AsQueryable()
                        .Where(q => q.ExerciseId == lpExercise.ExerciseId)
                        .OrderBy(q => q.OrderIndex)
                        .ToListAsync();

                    var newItems = questions.Select(q => new LearningPathQuestion
                    {
                        LearningPathQuestionId = Guid.NewGuid(),
                        LearningPathExerciseId = learningPathExerciseId,
                        QuestionId = q.QuestionId,
                        Status = "NotStarted",
                        Score = 0,
                        NumberOfRetake = 0
                    }).ToList();

                    await _lpQuestionRepo.InsertRange(newItems);
                    await _unitOfWork.SaveChangeAsync();
                }
            }

            // =========================================================
            // ✅ KHI SET Completed
            // =========================================================
            if (status == "Completed")
            {
                bool allDone = lpExercise.LearningPathQuestions
                    .All(q => q.Status == "Completed");

                if (!allDone)
                    return Fail(BusinessCode.INVALID_ACTION,
                        "Bạn chưa hoàn thành hết câu hỏi của bài.");

                var avgScore = lpExercise.LearningPathQuestions.Any()
                    ? lpExercise.LearningPathQuestions.Average(q => q.Score)
                    : 0;

                if (avgScore < 50)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Điểm trung bình {Math.Round(avgScore, 2)}%. Cần tối thiểu 50%.");

                // ✅ UPDATE EXERCISE TRƯỚC
                lpExercise.Status = "Completed";
                await _repo.Update(lpExercise);
                await _unitOfWork.SaveChangeAsync();

                // ✅ RELOAD LẠI EXERCISE (FIX BUG LOGIC)
                var reloadedExercises = await _repo.AsQueryable()
                    .Where(x => x.LearningPathChapterId == chapterId)
                    .ToListAsync();

                var allDoneChapter = reloadedExercises.All(x =>
                    x.Status == "Completed" && x.ScoreAchieved >= 50);

                if (allDoneChapter)
                {
                    // ✅ SET CHAPTER HIỆN TẠI = Completed
                    currentChapter.Status = "Completed";
                    await _lpChapterRepo.Update(currentChapter);

                    // ✅ MỞ CHAPTER TIẾP THEO
                    var nextChapter = allChapterInCourse
                        .Where(x => x.OrderIndex > currentChapterOrder)
                        .OrderBy(x => x.OrderIndex)
                        .FirstOrDefault();

                    if (nextChapter != null)
                    {
                        nextChapter.Status = "InProgress";
                        await _lpChapterRepo.Update(nextChapter);

                        // ✅ MỞ EXERCISE ĐẦU TIÊN CỦA CHAPTER SAU
                        var firstExerciseNextChapter = await _repo.AsQueryable()
                            .Where(x => x.LearningPathChapterId == nextChapter.LearningPathChapterId)
                            .OrderBy(x => x.OrderIndex)
                            .FirstOrDefaultAsync();

                        if (firstExerciseNextChapter != null)
                        {
                            firstExerciseNextChapter.Status = "InProgress";
                            await _repo.Update(firstExerciseNextChapter);
                        }
                    }

                    await _unitOfWork.SaveChangeAsync();
                }

                return Success(BusinessCode.UPDATE_SUCESSFULLY, "Hoàn thành bài & cập nhật chương thành công.");
            }

            // =========================================================
            // ✅ UPDATE TRẠNG THÁI BÌNH THƯỜNG
            // =========================================================
            lpExercise.Status = status;
            await _repo.Update(lpExercise);
            await _unitOfWork.SaveChangeAsync();

            return Success(BusinessCode.UPDATE_SUCESSFULLY, "Cập nhật trạng thái thành công.");
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
