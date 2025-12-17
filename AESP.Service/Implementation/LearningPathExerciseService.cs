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
        private readonly IGenericRepository<LearningPathCourse> _lpCourseRepo;

        private readonly IUnitOfWork _unitOfWork;


        public LearningPathExerciseService(
            IGenericRepository<LearningPathExercise> repo,
            IGenericRepository<Question> questionRepo,
            IGenericRepository<LearningPathQuestion> lpQuestionRepo,
            IGenericRepository<LearningPathChapter> lpChapterRepo,
            IGenericRepository<LearningPathCourse> lpCourseRepo,
            IUnitOfWork unitOfWork)
        {
            _lpChapterRepo = lpChapterRepo;
            _repo = repo;
            _questionRepo = questionRepo;
            _lpQuestionRepo = lpQuestionRepo;
            _lpCourseRepo = lpCourseRepo;

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

            // =========================================================
            // ✅ NORMALIZE + VALIDATE STATUS (FIX 400 SAI)
            // =========================================================
            status = status?.Trim();

            if (!new[] { "InProgress", "Completed" }
                .Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                return Fail(
                    BusinessCode.INVALID_ACTION,
                    "Trạng thái không hợp lệ."
                );
            }

            var lpExercise = await _repo.AsQueryable()
                .Include(x => x.LearningPathQuestions)
                .Include(x => x.LearningPathChapter)
                    .ThenInclude(c => c.LearningPathCourse)
                .FirstOrDefaultAsync(x => x.LearningPathExerciseId == learningPathExerciseId);

            if (lpExercise == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy LearningPathExercise.");

            var currentChapter = lpExercise.LearningPathChapter;
            var currentCourse = currentChapter.LearningPathCourse;

            var chapterOrder = currentChapter.OrderIndex;
            var courseOrder = currentCourse.OrderIndex;

            // =========================================================
            // 🔒 RULE 1: CHECK CHAPTER TRƯỚC
            // =========================================================
            if (chapterOrder > 1)
            {
                var previousChapter = await _lpChapterRepo.AsQueryable()
                    .FirstOrDefaultAsync(x =>
                        x.LearningPathCourseId == currentCourse.LearningPathCourseId &&
                        x.OrderIndex == chapterOrder - 1);

                if (previousChapter == null ||
                    !previousChapter.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        "Chapter trước chưa hoàn thành."
                    );
                }

                var prevChapterExercises = await _repo.AsQueryable()
                    .Where(x => x.LearningPathChapterId == previousChapter.LearningPathChapterId)
                    .ToListAsync();

                if (!prevChapterExercises.Any() ||
                    prevChapterExercises.Average(x => x.ScoreAchieved) < 50)
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        "Điểm trung bình Chapter trước chưa đạt 50%."
                    );
                }
            }

            // =========================================================
            // 🔒 RULE 2: CHECK COURSE TRƯỚC
            // =========================================================
            if (courseOrder > 1)
            {
                var previousCourse = await _lpCourseRepo.AsQueryable()
     .FirstOrDefaultAsync(x =>
         x.LearnerCourseId == currentCourse.LearnerCourseId &&
         x.OrderIndex == courseOrder - 1);


                if (previousCourse == null ||
                    !previousCourse.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        "Course trước chưa hoàn thành và điểm trung bình của các bài học phải đạt tối thiểu 50%."
                    );
                }

                var prevCourseExercises = await _repo.AsQueryable()
                    .Where(x =>
                        x.LearningPathChapter.LearningPathCourseId == previousCourse.LearningPathCourseId)
                    .ToListAsync();

                if (!prevCourseExercises.Any() ||
                    prevCourseExercises.Average(x => x.ScoreAchieved) < 50)
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        "Điểm trung bình của các bài học trong Course trước chưa đạt 50%."
                    );
                }
            }

            // =========================================================
            // ✅ STATUS = InProgress
            // =========================================================
            if (status.Equals("InProgress", StringComparison.OrdinalIgnoreCase))
            {
                var hasOtherInProgress = await _repo.AsQueryable()
                    .AnyAsync(x =>
                        x.LearningPathChapterId == currentChapter.LearningPathChapterId &&
                        x.LearningPathExerciseId != learningPathExerciseId &&
                        x.Status == "InProgress");

                if (hasOtherInProgress)
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        "Bạn cần hoàn thành bài hiện tại trước khi mở bài tiếp theo."
                    );
                }

                if (currentChapter.Status == "NotStarted")
                {
                    currentChapter.Status = "InProgress";
                    await _lpChapterRepo.Update(currentChapter);
                }

                bool hasQuestion = await _lpQuestionRepo.AsQueryable()
                    .AnyAsync(x => x.LearningPathExerciseId == learningPathExerciseId);

                if (!hasQuestion)
                {
                    var questions = await _questionRepo.AsQueryable()
                        .Where(q => q.ExerciseId == lpExercise.ExerciseId)
                        .OrderBy(q => q.OrderIndex)
                        .ToListAsync();

                    var lpQuestions = questions.Select(q => new LearningPathQuestion
                    {
                        LearningPathQuestionId = Guid.NewGuid(),
                        LearningPathExerciseId = learningPathExerciseId,
                        QuestionId = q.QuestionId,
                        Status = "NotStarted",
                        Score = 0,
                        NumberOfRetake = 0
                    }).ToList();

                    await _lpQuestionRepo.InsertRange(lpQuestions);
                }

                lpExercise.Status = "InProgress";
                await _repo.Update(lpExercise);

                await _unitOfWork.SaveChangeAsync();

                return Success(
                    BusinessCode.UPDATE_SUCESSFULLY,
                    "Mở bài tập thành công."
                );
            }

            // =========================================================
            // ✅ STATUS = Completed
            // =========================================================
            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                if (!lpExercise.LearningPathQuestions.All(q => q.Status == "Completed"))
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        "Bạn chưa hoàn thành hết câu hỏi của bài."
                    );
                }

                var avgScore = lpExercise.LearningPathQuestions.Any()
                    ? lpExercise.LearningPathQuestions.Average(q => q.Score)
                    : 0;

                if (avgScore < 50)
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        $"Điểm trung bình {Math.Round(avgScore, 2)}%. Cần tối thiểu 50%."
                    );
                }

                lpExercise.Status = "Completed";
                await _repo.Update(lpExercise);

                var chapterExercises = await _repo.AsQueryable()
                    .Where(x => x.LearningPathChapterId == currentChapter.LearningPathChapterId)
                    .ToListAsync();

                if (chapterExercises.All(x => x.Status == "Completed"))
                {
                    currentChapter.Status = "Completed";
                    await _lpChapterRepo.Update(currentChapter);

                    var nextChapter = await _lpChapterRepo.AsQueryable()
                        .FirstOrDefaultAsync(x =>
                            x.LearningPathCourseId == currentCourse.LearningPathCourseId &&
                            x.OrderIndex == chapterOrder + 1);

                    if (nextChapter != null)
                    {
                        nextChapter.Status = "InProgress";
                        await _lpChapterRepo.Update(nextChapter);

                        var firstExercise = await _repo.AsQueryable()
                            .Where(x => x.LearningPathChapterId == nextChapter.LearningPathChapterId)
                            .OrderBy(x => x.OrderIndex)
                            .FirstOrDefaultAsync();

                        if (firstExercise != null)
                        {
                            firstExercise.Status = "InProgress";
                            await _repo.Update(firstExercise);
                        }
                    }
                }

                await _unitOfWork.SaveChangeAsync();

                return Success(
                    BusinessCode.UPDATE_SUCESSFULLY,
                    "Hoàn thành bài tập thành công."
                );
            }

            // Không bao giờ rơi tới đây
            return Fail(
                BusinessCode.INVALID_ACTION,
                "Trạng thái không hợp lệ."
            );
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
