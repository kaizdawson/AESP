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

            // ✅✅✅ LẤY TOÀN BỘ EXERCISE TRONG CÙNG COURSE (KHÔNG THEO CHAPTER)
            var allExercisesInCourse = await _repo.AsQueryable()
                .Where(x => x.LearningPathChapter.LearningPathCourseId
                         == lpExercise.LearningPathChapter.LearningPathCourseId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();

            // ✅ CHỈ CHẶN "InProgress"
            if (status == "InProgress")
            {
                // 1️⃣ CHẶN nếu có bài KHÁC đang InProgress trong CÙNG CHAPTER
                bool hasOtherInProgressSameChapter = allExercisesInCourse.Any(x =>
                    x.LearningPathChapterId == lpExercise.LearningPathChapterId &&
                    x.LearningPathExerciseId != learningPathExerciseId &&
                    x.Status == "InProgress");

                if (hasOtherInProgressSameChapter)
                {
                    return Fail(BusinessCode.INVALID_ACTION,
                        "Bạn phải hoàn thành bài tập đang làm dở trước khi bắt đầu bài mới.");
                }

                // 2️⃣ LẤY BÀI TRƯỚC THEO TOÀN COURSE (KỂ CẢ KHÁC CHAPTER)
                var previousExercise = allExercisesInCourse
                    .Where(x => x.OrderIndex < lpExercise.OrderIndex)
                    .OrderByDescending(x => x.OrderIndex)
                    .FirstOrDefault();

                if (previousExercise != null)
                {
                    bool isCompleted = previousExercise.Status == "Completed";
                    bool isScoreValid = previousExercise.ScoreAchieved >= 50;

                    if (!isCompleted || !isScoreValid)
                    {
                        return Fail(
                            BusinessCode.INVALID_ACTION,
                            $"Bài trước CHƯA ĐẠT yêu cầu.\n" +
                            $"- Trạng thái: {previousExercise.Status}\n" +
                            $"- Điểm: {Math.Round(previousExercise.ScoreAchieved, 2)}%\n" +
                            $"Yêu cầu: Completed + ≥ 50%"
                        );
                    }
                }

                // 3️⃣ SINH LPQ NẾU CHƯA CÓ
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

            // ✅ CHẶN "Completed" NẾU CHƯA ĐẠT 50%
            if (status == "Completed")
            {
                bool allDone = lpExercise.LearningPathQuestions
                    .All(q => q.Status == "Completed");

                if (!allDone)
                    return Fail(BusinessCode.INVALID_ACTION, "Bạn chưa hoàn thành tất cả câu hỏi.");

                var avgScore = lpExercise.LearningPathQuestions.Any()
                    ? lpExercise.LearningPathQuestions.Average(q => q.Score)
                    : 0;

                if (avgScore < 50)
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        $"Điểm trung bình hiện tại là {Math.Round(avgScore, 2)}%. Cần tối thiểu 50%."
                    );
                }
            }

            // ✅ UPDATE STATUS
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
