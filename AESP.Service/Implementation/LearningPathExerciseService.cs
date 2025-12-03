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
                .FirstOrDefaultAsync(x => x.LearningPathExerciseId == learningPathExerciseId);

            if (lpExercise == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy LearningPathExercise.");

            // ❗ Lấy toàn bộ exercise trong cùng chapter
            var allExercises = await _repo.AsQueryable()
                .Where(x => x.LearningPathChapterId == lpExercise.LearningPathChapterId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();

            // ❗ Nếu muốn chuyển sang InProgress
            if (status == "InProgress")
            {
                // 1️⃣ Chặn nếu có bài khác đang InProgress
                bool hasOtherInProgress = allExercises
                    .Any(x => x.LearningPathExerciseId != learningPathExerciseId &&
                              x.Status == "InProgress");

                if (hasOtherInProgress)
                {
                    return Fail(BusinessCode.INVALID_ACTION,
                        "Bạn phải hoàn thành bài tập đang làm dở trước khi bắt đầu bài mới.");
                }

                // 2️⃣ Chặn nếu có bài nằm trước nó mà chưa Completed
                bool previousNotCompleted = allExercises
                    .Any(x => x.OrderIndex < lpExercise.OrderIndex &&
                              x.Status != "Completed");

                if (previousNotCompleted)
                {
                    return Fail(BusinessCode.INVALID_ACTION,
                        "Bạn phải hoàn thành các bài trước trong chương trước khi làm bài tiếp theo.");
                }

                // 🚀 Đến đây là hợp lệ → ĐƯỢC SINH LPQ
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

            // ❗ Nếu muốn Completed → phải hoàn tất hết câu hỏi + điểm TB >= 50%
            if (status == "Completed")
            {
                // 1️⃣ Check đã hoàn thành hết câu hỏi chưa
                bool allDone = lpExercise.LearningPathQuestions
                    .All(q => q.Status == "Completed");

                if (!allDone)
                {
                    return Fail(BusinessCode.INVALID_ACTION,
                        "Bạn chưa hoàn thành tất cả câu hỏi.");
                }

                // 2️⃣ Tính điểm trung bình của exercise
                var avgScore = lpExercise.LearningPathQuestions.Any()
                    ? lpExercise.LearningPathQuestions.Average(q => q.Score)
                    : 0;

                // 3️⃣ Nếu < 50% → KHÔNG cho Completed
                if (avgScore < 50)
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        $"Điểm trung bình hiện tại là {Math.Round(avgScore, 2)}%. Cần tối thiểu 50% để hoàn thành bài tập."
                    );
                }
            }

            // ============================
            // ✅ UPDATE EXERCISE STATUS
            // ============================
            lpExercise.Status = status;
            await _repo.Update(lpExercise);
            await _unitOfWork.SaveChangeAsync();

            // ============================
            // ✅ AUTO UPDATE CHAPTER STATUS
            // ============================
            var chapter = await _lpChapterRepo.AsQueryable()
                .FirstOrDefaultAsync(x => x.LearningPathChapterId == lpExercise.LearningPathChapterId);

            if (chapter != null)
            {
                // Nếu vừa vào bài đầu tiên → Chapter = InProgress
                if (chapter.Status == "NotStarted" && status == "InProgress")
                {
                    chapter.Status = "InProgress";
                    chapter.Progress = 1;
                    await _lpChapterRepo.Update(chapter);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Nếu tất cả exercise trong chapter đều Completed → Chapter = Completed
                var allExercisesOfChapter = await _repo.AsQueryable()
                    .Where(x => x.LearningPathChapterId == lpExercise.LearningPathChapterId)
                    .ToListAsync();

                if (allExercisesOfChapter.All(x => x.Status == "Completed"))
                {
                    chapter.Status = "Completed";
                    chapter.Progress = 100;
                    await _lpChapterRepo.Update(chapter);
                    await _unitOfWork.SaveChangeAsync();
                }
            }

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
