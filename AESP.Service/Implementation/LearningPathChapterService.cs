using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class LearningPathChapterService : ILearningPathChapterService
    {
        private readonly IGenericRepository<LearningPathChapter> _repo;
        private readonly IGenericRepository<LearningPathCourse> _lpCourseRepo;
        private readonly IGenericRepository<Chapter> _chapterRepo;
        private readonly IGenericRepository<Exercise> _exerciseRepo;
        private readonly IGenericRepository<LearningPathExercise> _learningPathExerciseRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LearningPathChapterService(
            IGenericRepository<LearningPathChapter> repo,
            IGenericRepository<Chapter> chapterRepo,
            IGenericRepository<LearningPathCourse> lpCourseRepo,
            IGenericRepository<Exercise> exerciseRepo,
            IGenericRepository<LearningPathExercise> learningPathExerciseRepo,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _chapterRepo = chapterRepo;
            _lpCourseRepo = lpCourseRepo;
            _exerciseRepo = exerciseRepo;
            _learningPathExerciseRepo = learningPathExerciseRepo;
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // 🔹 Lấy tất cả chapter trong LearningPathCourse
        // ============================================================
        public async Task<ResponseDTO> GetAllByLearningPathCourseIdAsync(Guid learningPathCourseId)
        {
            var list = await _repo.AsQueryable()
                .Include(x => x.Chapter)
                .Where(x => x.LearningPathCourseId == learningPathCourseId)
                .OrderBy(x => x.OrderIndex)
                .Select(x => new
                {
                    x.LearningPathChapterId,
                    x.ChapterId,
                    x.OrderIndex,
                    x.Status,
                    x.Progress,


                    // 🔹 Thêm thông tin từ Chapter
                    ChapterTitle = x.Chapter.Title,
                    Description = x.Chapter.Description,
                    NumberOfExercise = x.Chapter.NumberOfExercise,

                    // Giữ field cũ
                    x.NumberOfModule



                })
                .ToListAsync();

            if (!list.Any())
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương trong lộ trình.");

            return Success(BusinessCode.GET_DATA_SUCCESSFULLY, "Lấy danh sách chương thành công.", list);
        }

        // ============================================================
        // 🔹 Lấy chi tiết 1 chương trong LearningPathCourse
        // ============================================================
        public async Task<ResponseDTO> GetByIdAsync(Guid learningPathChapterId)
        {
            var entity = await _repo.AsQueryable()
                .Include(x => x.Chapter)
                .FirstOrDefaultAsync(x => x.LearningPathChapterId == learningPathChapterId);

            if (entity == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương trong lộ trình.");

            var dto = new
            {
                entity.LearningPathChapterId,
                entity.ChapterId,
                entity.LearningPathCourseId,
                entity.Status,
                entity.Progress,
                entity.NumberOfModule,
                entity.OrderIndex,
                // 🔹 Dữ liệu từ Chapter
                ChapterTitle = entity.Chapter.Title,
                Description = entity.Chapter.Description,
                NumberOfExercise = entity.Chapter.NumberOfExercise
            };

            return Success(BusinessCode.GET_DATA_SUCCESSFULLY, "Lấy chi tiết chương thành công.", dto);
        }

   
        public async Task<ResponseDTO> CreateByCourseAsync(Guid learningPathCourseId, Guid learnerCourseId)
        {
            if (learningPathCourseId == Guid.Empty || learnerCourseId == Guid.Empty)
                return Fail(BusinessCode.VALIDATION_FAILED, "Thiếu thông tin đầu vào.");

            var lpCourse = await _lpCourseRepo.AsQueryable()
                .Include(x => x.Course)
                .Include(x => x.LearnerCourse)
                    .ThenInclude(lc => lc.LearnerProfile)
                .FirstOrDefaultAsync(x => x.LearningPathCourseId == learningPathCourseId);

            if (lpCourse == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học trong lộ trình.");

            if (lpCourse.LearnerCourse.LearnerCourseId != learnerCourseId)
                return Fail(BusinessCode.ACCESS_DENIED, "Bạn không có quyền tạo chương cho khóa học này.");

            if (lpCourse.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return Fail(BusinessCode.INVALID_ACTION, "Khóa học đã hoàn tất, không thể tạo chương.");

            var course = lpCourse.Course;
            var chapters = await _chapterRepo.AsQueryable()
                .Where(c => c.CourseId == course.CourseId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            if (!chapters.Any())
                return Fail(BusinessCode.DATA_NOT_FOUND, "Khóa học này chưa có chương.");

            var existed = await _repo.AsQueryable()
                .Where(x => x.LearningPathCourseId == learningPathCourseId)
                .ToListAsync();

            if (existed.Any())
                return Fail(BusinessCode.DUPLICATE_DATA, "Danh sách chương đã tồn tại.");

            using var tran = await _unitOfWork.GetDbContext().Database.BeginTransactionAsync();
            try
            {
                // 🔹 1️⃣ Sinh danh sách chương
                var newChapters = chapters.Select((ch, idx) => new LearningPathChapter
                {
                    LearningPathChapterId = Guid.NewGuid(),
                    LearningPathCourseId = learningPathCourseId,
                    ChapterId = ch.ChapterId,
                    OrderIndex = idx + 1,
                    Status = "Enrolled",
                    Progress = 0,
                    NumberOfModule = ch.NumberOfExercise
                }).ToList();

                await _repo.InsertRange(newChapters);
                await _unitOfWork.SaveChangeAsync();

                // 🔹 2️⃣ Sinh luôn bài tập trong mỗi chương
                var exercises = new List<LearningPathExercise>();
                foreach (var lpChapter in newChapters)
                {
                    var chapterExercises = await _exerciseRepo.AsQueryable()
                        .Where(ex => ex.ChapterId == lpChapter.ChapterId)
                        .OrderBy(ex => ex.OrderIndex)
                        .ToListAsync();

                    foreach (var ex in chapterExercises)
                    {
                        exercises.Add(new LearningPathExercise
                        {
                            LearningPathExerciseId = Guid.NewGuid(),
                            LearningPathChapterId = lpChapter.LearningPathChapterId,
                            ExerciseId = ex.ExerciseId,
                            OrderIndex = ex.OrderIndex,
                            Status = "NotStarted",
                            ScoreAchieved = 0,
                            NumberOfQuestion = ex.NumberOfQuestion
                        });
                    }
                }

                if (exercises.Any())
                {
                    await _learningPathExerciseRepo.InsertRange(exercises);
                    await _unitOfWork.SaveChangeAsync();
                }

                await tran.CommitAsync();
                return Success(BusinessCode.INSERT_SUCESSFULLY,
                    $"Tạo {newChapters.Count} chương và {exercises.Count} bài tập thành công.",
                    new { Chapters = newChapters, Exercises = exercises });
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return Fail(BusinessCode.EXCEPTION, $"Lỗi khi tạo danh sách chương & bài tập: {ex.Message}");
            }
        }


        // ============================================================
        // 🔹 Cập nhật tiến độ chương học
        // ============================================================
        public async Task<ResponseDTO> UpdateProgressAsync(Guid learnerProfileId, Guid learningPathChapterId, double progress)
        {
            if (learnerProfileId == Guid.Empty || learningPathChapterId == Guid.Empty)
                return Fail(BusinessCode.VALIDATION_FAILED, "Thiếu thông tin đầu vào.");

            var entity = await _repo.AsQueryable()
                .Include(x => x.LearningPathCourse)
                    .ThenInclude(lpc => lpc.LearnerCourse)
                .FirstOrDefaultAsync(x => x.LearningPathChapterId == learningPathChapterId);

            if (entity == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương học.");


            // 🚫 Không cho cập nhật khi khóa học hoặc chương đã hoàn thành
            if (entity.LearningPathCourse.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return Fail(BusinessCode.INVALID_ACTION, "Khóa học đã hoàn tất, không thể cập nhật tiến độ.");

            if (entity.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return Fail(BusinessCode.INVALID_ACTION, "Chương học này đã hoàn thành.");

            // ✅ Validate giá trị progress
            if (progress < 0 || progress > 100)
                return Fail(BusinessCode.VALIDATION_FAILED, "Progress phải nằm trong khoảng 0 - 100.");

            // ✅ Cập nhật tiến độ và trạng thái
            entity.Progress = progress;

            entity.Status = progress >= 100
                ? "Completed"
                : progress > 0
                    ? "InProgress"
                    : "Enrolled";

            await _repo.Update(entity);
            await _unitOfWork.SaveChangeAsync();

            // ✅ Nếu toàn bộ chương của course đã hoàn thành → cập nhật khóa học
            await AutoUpdateCourseProgressAsync(entity.LearningPathCourseId);

            return Success(BusinessCode.UPDATE_SUCESSFULLY, "Cập nhật tiến độ chương học thành công.", new
            {
                entity.LearningPathChapterId,
                entity.Progress,
                entity.Status
            });
        }

        // ============================================================
        // 🔹 Cập nhật trạng thái và tiến độ Course nếu cần
        // ============================================================
        private async Task AutoUpdateCourseProgressAsync(Guid learningPathCourseId)
        {
            var chapters = await _repo.AsQueryable()
                .Where(x => x.LearningPathCourseId == learningPathCourseId)
                .ToListAsync();

            var lpCourse = await _lpCourseRepo.GetById(learningPathCourseId);
            if (lpCourse == null) return;

            // ✅ Nếu tất cả chương đều hoàn thành
            if (chapters.All(c => c.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
            {
                lpCourse.Status = "Completed";
                lpCourse.Progress = 100;
            }
            // ✅ Nếu có ít nhất 1 chương đang học (InProgress)
            else if (chapters.Any(c => c.Status.Equals("InProgress", StringComparison.OrdinalIgnoreCase)))
            {
                lpCourse.Status = "InProgress";
                lpCourse.Progress = (int)Math.Round(chapters.Average(c => c.Progress));
            }
            // ✅ Còn lại: tất cả đều chưa bắt đầu
            else
            {
                lpCourse.Status = "Enrolled";
                lpCourse.Progress = 0;
            }

            await _lpCourseRepo.Update(lpCourse);
            await _unitOfWork.SaveChangeAsync();
        }


        // ============================================================
        // 🔹 Helper chuẩn hóa
        // ============================================================
        private static ResponseDTO Success(BusinessCode code, string msg, object? data = null)
            => new ResponseDTO { IsSucess = true, BusinessCode = code, Message = msg, Data = data };

        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };
    }
}
