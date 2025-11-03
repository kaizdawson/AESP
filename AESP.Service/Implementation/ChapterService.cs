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

        // ============================================================
        // 🔹 CREATE (CHAPTER + OPTIONAL EXERCISES + QUESTIONS)
        // ============================================================
        public async Task<ResponseDTO> CreateChapterAsync(CreateChapterDTO request)
        {
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên chương không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả chương không được để trống.");
                if (request.CourseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Khóa học không hợp lệ.");

                var course = await _courseRepository.GetById(request.CourseId);
                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

                var chapter = new Chapter
                {
                    ChapterId = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    CourseId = request.CourseId,
                    NumberOfExercise = request.NumberOfExercise,
                    CreatedAt = DateTime.UtcNow
                };
                await _chapterRepository.Insert(chapter);

                if (request.Exercises != null && request.Exercises.Any())
                {
                    // insert exercises
                    var exercises = request.Exercises.Select(ex => new Exercise
                    {
                        ExerciseId = Guid.NewGuid(),
                        Title = ex.Title.Trim(),
                        Description = ex.Description.Trim(),
                        OrderIndex = ex.OrderIndex,
                        NumberOfQuestion = ex.NumberOfQuestion,
                        ChapterId = chapter.ChapterId
                    }).ToList();

                    await _exerciseRepository.InsertRange(exercises);

                    // insert questions
                    // --- Auto generate default questions cho mỗi exercise mới ---
                    var newQuestions = new List<Question>();
                    foreach (var ex in exercises)
                    {
                        newQuestions.AddRange(new[]
                        {
        new Question
        {
            QuestionId = Guid.NewGuid(),
            ExerciseId = ex.ExerciseId,
            Text = "Sample question 1",
            Type = "text",
            OrderIndex = 1,
            PhonemeJson = ""
        },
        new Question
        {
            QuestionId = Guid.NewGuid(),
            ExerciseId = ex.ExerciseId,
            Text = "Sample question 2",
            Type = "text",
            OrderIndex = 2,
            PhonemeJson = ""
        }
    });
                    }
                    await _questionRepository.InsertRange(newQuestions);

                }

                await _unitOfWork.SaveChangeAsync();

                // 🔁 Reload 3 tầng trước khi trả về
                return await GetChapterByIdAsync(chapter.ChapterId);
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể tạo chương: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 UPDATE (CHAPTER + OPTIONAL EXERCISES + QUESTIONS)
        // ============================================================
        public async Task<ResponseDTO> UpdateChapterAsync(Guid id, UpdateChapterDTO request)
        {
            try
            {
                var chapter = await _chapterRepository.AsQueryable()
                    .Include(ch => ch.Exercises)
                        .ThenInclude(ex => ex.Questions)
                    .FirstOrDefaultAsync(x => x.ChapterId == id);

                if (chapter == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương để cập nhật.");

                // --- VALIDATION cơ bản ---
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên chương không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả chương không được để trống.");

                // --- CẬP NHẬT FIELD CHÍNH ---
                chapter.Title = request.Title.Trim();
                chapter.Description = request.Description.Trim();

                if (request.NumberOfExercise.HasValue)
                {
                    int newExerciseCount = request.NumberOfExercise.Value;
                    int currentExerciseCount = chapter.Exercises.Count;

                    // --- Nếu chapter có ít hơn số lượng mới => thêm Exercise mới ---
                    if (newExerciseCount > currentExerciseCount)
                    {
                        var toAdd = Enumerable.Range(currentExerciseCount + 1, newExerciseCount - currentExerciseCount)
                            .Select(i => new Exercise
                            {
                                ExerciseId = Guid.NewGuid(),
                                ChapterId = chapter.ChapterId,
                                Title = $"Exercise {i}",
                                Description = "Auto generated exercise (update)",
                                OrderIndex = i,
                                NumberOfQuestion = 2
                            }).ToList();

                        await _exerciseRepository.InsertRange(toAdd);

                        // ✅ Thêm question mặc định cho exercise mới
                        var newQuestions = new List<Question>();
                        foreach (var ex in toAdd)
                        {
                            newQuestions.AddRange(new[]
                            {
                        new Question
                        {
                            QuestionId = Guid.NewGuid(),
                            ExerciseId = ex.ExerciseId,
                            Text = "Sample question 1",
                            Type = "text",
                            OrderIndex = 1,
                            PhonemeJson = ""
                        },
                        new Question
                        {
                            QuestionId = Guid.NewGuid(),
                            ExerciseId = ex.ExerciseId,
                            Text = "Sample question 2",
                            Type = "text",
                            OrderIndex = 2,
                            PhonemeJson = ""
                        }
                    });
                        }
                        await _questionRepository.InsertRange(newQuestions);
                    }
                    // --- Nếu chapter có nhiều hơn số lượng mới => xoá bớt ---
                    else if (newExerciseCount < currentExerciseCount)
                    {
                        var toRemove = chapter.Exercises
                            .OrderByDescending(e => e.OrderIndex)
                            .Take(currentExerciseCount - newExerciseCount)
                            .ToList();

                        var removeQuestions = toRemove.SelectMany(e => e.Questions).ToList();

                        await _questionRepository.DeleteRange(removeQuestions);
                        await _exerciseRepository.DeleteRange(toRemove);
                    }

                    chapter.NumberOfExercise = newExerciseCount;
                }

                // --- Nếu có danh sách exercise truyền vào -> cập nhật chi tiết ---
                if (request.Exercises != null && request.Exercises.Any())
                {
                    foreach (var exDto in request.Exercises)
                    {
                        var exEntity = chapter.Exercises.FirstOrDefault(e => e.ExerciseId == exDto.ExerciseId);
                        if (exEntity == null)
                            return Fail(BusinessCode.DATA_NOT_FOUND, $"Không tìm thấy bài tập (ID: {exDto.ExerciseId}) để cập nhật.");

                        if (!string.IsNullOrWhiteSpace(exDto.Title)) exEntity.Title = exDto.Title.Trim();
                        if (!string.IsNullOrWhiteSpace(exDto.Description)) exEntity.Description = exDto.Description.Trim();
                        if (exDto.OrderIndex.HasValue) exEntity.OrderIndex = exDto.OrderIndex.Value;
                        if (exDto.NumberOfQuestion.HasValue) exEntity.NumberOfQuestion = exDto.NumberOfQuestion.Value;

                        await _exerciseRepository.Update(exEntity);
                    }
                }

                await _chapterRepository.Update(chapter);
                await _unitOfWork.SaveChangeAsync();

                // --- RELOAD FULL SAU UPDATE ---
                var result = await GetChapterByIdAsync(chapter.ChapterId);
                result.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                result.Message = "Cập nhật chương đầy đủ thành công.";
                return result;
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

                var questions = chapter.Exercises.SelectMany(e => e.Questions).ToList();
                if (questions.Any())
                    await _questionRepository.DeleteRange(questions);

                if (chapter.Exercises.Any())
                    await _exerciseRepository.DeleteRange(chapter.Exercises.ToList());

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
