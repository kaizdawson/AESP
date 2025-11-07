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
    public class ChapterService : IChapterService
    {
        private readonly IGenericRepository<Chapter> _chapterRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Exercise> _exerciseRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChapterService(
            IGenericRepository<Chapter> chapterRepository,
            IGenericRepository<Course> courseRepository,
            IGenericRepository<Exercise> exerciseRepository,
            IGenericRepository<Question> questionRepository,
            IUnitOfWork unitOfWork)
        {
            _chapterRepository = chapterRepository;
            _courseRepository = courseRepository;
            _exerciseRepository = exerciseRepository;
            _questionRepository = questionRepository;
            _unitOfWork = unitOfWork;
        }

        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };

        // ============================================================
        // 🔹 GET ALL
        // ============================================================
        public async Task<ResponseDTO> GetAllChaptersAsync(int pageNumber, int pageSize, Guid? courseId = null, string? keyword = null)
        {
            try
            {
                var query = _chapterRepository.AsQueryable()
                    .AsNoTracking()
                    .Include(ch => ch.Exercises)
                        .ThenInclude(ex => ex.Questions)
                    .Where(x => (!courseId.HasValue || x.CourseId == courseId)
                             && (string.IsNullOrEmpty(keyword) || x.Title.Contains(keyword)))
                    .OrderByDescending(x => x.CreatedAt);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var chapters = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var mapped = chapters.Select(MapChapterToReadDto).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy danh sách chương thành công.",
                    Data = new PagedResult<ReadChapterDTO>
                    {
                        Items = mapped,
                        TotalPages = totalPages
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Lỗi khi lấy danh sách chương: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 GET BY ID (LOAD 3 TẦNG)
        // ============================================================
        public async Task<ResponseDTO> GetChapterByIdAsync(Guid id)
        {
            try
            {
                var chapter = await _chapterRepository.AsQueryable()
                    .AsNoTracking()
                    .Include(ch => ch.Exercises)
                        .ThenInclude(ex => ex.Questions)
                    .FirstOrDefaultAsync(x => x.ChapterId == id);

                if (chapter == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương.");

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy chương thành công.",
                    Data = MapChapterToReadDto(chapter)
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Lỗi khi lấy chương: {ex.Message}");
            }
        }


        public async Task<ResponseDTO> CreateChapterAsync(Guid courseId, CreateChapterDTO request)
        {
            try
            {
                // ===== VALIDATION CƠ BẢN =====
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không hợp lệ.");
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên chương không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả chương không được để trống.");
                if (courseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "CourseId không hợp lệ.");
                if (request.NumberOfExercise <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mỗi chương phải có ít nhất 1 bài tập.");

                // ===== KIỂM TRA KHÓA HỌC TỒN TẠI =====
                var course = await _courseRepository.GetById(courseId);
                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

                // ===== RÀNG BUỘC SỐ LƯỢNG CHAPTER =====
                var existingCount = await _chapterRepository.AsQueryable()
                    .CountAsync(x => x.CourseId == courseId);

                if (existingCount >= course.NumberOfChapter)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Không thể tạo thêm chương. Khóa học '{course.Title}' chỉ cho phép {course.NumberOfChapter} chương.");

                // ===== TẠO CHAPTER =====
                var chapter = new Chapter
                {
                    ChapterId = Guid.NewGuid(),
                    CourseId = courseId,
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    NumberOfExercise = request.NumberOfExercise,
                    CreatedAt = DateTime.UtcNow
                };

                await _chapterRepository.Insert(chapter);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo chương thành công.",
                    Data = new
                    {
                        chapter.ChapterId,
                        chapter.CourseId,
                        chapter.Title,
                        chapter.Description,
                        chapter.CreatedAt,
                        chapter.NumberOfExercise,
                        Exercises = new List<object>()
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể tạo chương: " + ex.Message);
            }
        }




        public async Task<ResponseDTO> UpdateChapterAsync(Guid id, UpdateChapterDTO request)
        {
            try
            {
                var chapter = await _chapterRepository.GetById(id);
                if (chapter == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương để cập nhật.");

                // --- Validate ---
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên chương không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả chương không được để trống.");
                if (request.CourseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "CourseId không hợp lệ.");

                if (request.NumberOfExercise.HasValue && request.NumberOfExercise.Value <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số lượng bài tập phải lớn hơn 0.");

                // --- Kiểm tra khóa học có tồn tại ---
                var course = await _courseRepository.GetById(request.CourseId);
                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học để gán.");

                // --- Cập nhật ---
                chapter.Title = request.Title.Trim();
                chapter.Description = request.Description.Trim();
                chapter.NumberOfExercise = request.NumberOfExercise ?? chapter.NumberOfExercise;
                chapter.CourseId = request.CourseId;

                await _chapterRepository.Update(chapter);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                    Message = "Cập nhật chương thành công.",
                    Data = new
                    {
                        chapter.ChapterId,
                        chapter.CourseId,
                        chapter.Title,
                        chapter.Description,
                        chapter.NumberOfExercise,
                        chapter.CreatedAt,
                        Exercises = new List<object>()
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể cập nhật chương: " + ex.Message);
            }
        }


        // ============================================================
        // 🔹 DELETE (XOÁ 3 TẦNG: QUESTION → EXERCISE → CHAPTER)
        // ============================================================
        public async Task<ResponseDTO> DeleteChapterAsync(Guid id)
        {
            try
            {
                var chapter = await _chapterRepository.AsQueryable()
                    .Include(ch => ch.Exercises)
                    .ThenInclude(ex => ex.Questions)
                    .FirstOrDefaultAsync(x => x.ChapterId == id);

                if (chapter == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương để xoá.");

                // ✅ RÀNG BUỘC: Không cho phép xóa nếu có Exercise
                if (chapter.Exercises != null && chapter.Exercises.Any())
                {
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Không thể xoá chương '{chapter.Title}' vì vẫn còn {chapter.Exercises.Count} bài tập.");
                }

                await _chapterRepository.Delete(chapter);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
                    Message = "Xoá chương thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể xoá chương: {ex.Message}");
            }
        }





        // ============================================================
        // 🔹 GET CHAPTERS BY COURSE ID (chuẩn 3-layer)
        // ============================================================
        public async Task<ResponseDTO> GetChaptersByCourseIdAsync(Guid courseId)
        {
            try
            {
                // 1️⃣ Validate đầu vào
                if (courseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "CourseId không hợp lệ.");

                // 2️⃣ Kiểm tra tồn tại khóa học
                var course = await _courseRepository.GetById(courseId);
                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học trong hệ thống.");

                // 3️⃣ Lấy dữ liệu 4 tầng (Chapter → Exercise → Question → Media)
                var db = _chapterRepository.GetDbContext();
                var chapters = await db.Chapters
                    .Where(ch => ch.CourseId == courseId)
                    .Include(ch => ch.Exercises)
                        .ThenInclude(ex => ex.Questions)
                            .ThenInclude(q => q.QuestionMedias)
                    .ToListAsync();

                // 4️⃣ Map DTO
                var mapped = chapters.Select(ch => new ReadChapterDTO
                {
                    ChapterId = ch.ChapterId,
                    Title = ch.Title,
                    Description = ch.Description,
                    NumberOfExercise = ch.NumberOfExercise,
                    CreatedAt = ch.CreatedAt,
                    Exercises = ch.Exercises?.Select(ex => new ReadChapterExerciseDTO
                    {
                        ExerciseId = ex.ExerciseId,
                        Title = ex.Title,
                        Description = ex.Description,
                        OrderIndex = ex.OrderIndex,
                        NumberOfQuestion = ex.NumberOfQuestion,
                        Questions = ex.Questions?.Select(q => new ReadChapterQuestionDTO
                        {
                            QuestionId = q.QuestionId,
                            Text = q.Text,
                            Type = q.Type,
                            OrderIndex = q.OrderIndex,
                            IPA = "", // Nếu bạn muốn parse từ JSON thì thay bằng q.PhonemeJson
                            PhonemeJson = q.PhonemeJson
                        }).ToList() ?? new List<ReadChapterQuestionDTO>()
                    }).ToList() ?? new List<ReadChapterExerciseDTO>()
                }).ToList();

              

                // 5️⃣ Kết quả trả về
                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = mapped.Any()
                        ? "Lấy danh sách chương theo khóa học thành công."
                        : "Khóa học này chưa có chương nào.",
                    Data = mapped
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể lấy danh sách chương: {ex.Message}");
            }
        }





        // ============================================================
        // 🔹 MAPPING HELPER
        // ============================================================
        private static ReadChapterDTO MapChapterToReadDto(Chapter ch)
        {
            return new ReadChapterDTO
            {
                ChapterId = ch.ChapterId,
                Title = ch.Title,
                Description = ch.Description,
                NumberOfExercise = ch.NumberOfExercise,
                CreatedAt = ch.CreatedAt,
                Exercises = ch.Exercises?.Select(ex => new ReadChapterExerciseDTO
                {
                    ExerciseId = ex.ExerciseId,
                    Title = ex.Title,
                    Description = ex.Description,
                    OrderIndex = ex.OrderIndex,
                    NumberOfQuestion = ex.NumberOfQuestion,
                    Questions = ex.Questions?.Select(q => new ReadChapterQuestionDTO
                    {
                        QuestionId = q.QuestionId,
                        Text = q.Text,
                        Type = q.Type,
                        OrderIndex = q.OrderIndex,
                        PhonemeJson = q.PhonemeJson
                    }).ToList()
                }).ToList()
            };
        }
    }
}
