using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AESP.Service.Implementation
{
    public class ChapterService : IChapterService
    {
        private readonly IGenericRepository<Chapter> _chapterRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Exercise> _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChapterService(
            IGenericRepository<Chapter> chapterRepository,
            IGenericRepository<Course> courseRepository,
            IGenericRepository<Exercise> exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _chapterRepository = chapterRepository;
            _courseRepository = courseRepository;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        private static ResponseDTO Fail(BusinessCode code, string msg)
        {
            return new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = code,
                Message = msg
            };
        }

        // ✅ GET ALL
        public async Task<ResponseDTO> GetAllChaptersAsync(int pageNumber, int pageSize, Guid? courseId = null, string? keyword = null)
        {
            ResponseDTO dto = new();
            try
            {
                var result = await _chapterRepository.GetAllDataByExpression(
      filter: x =>
          (!courseId.HasValue || x.CourseId == courseId) &&
          (string.IsNullOrEmpty(keyword) || x.Title.Contains(keyword)),
      pageNumber: pageNumber,
      pageSize: pageSize,
      orderBy: x => x.CreatedAt,
      isAscending: false,
      x => x.Course, x => x.Exercises   // ✅ thay includeProperties bằng includes
  );


                var mapped = result.Items.Select(ch => new ReadChapterDTO
                {
                    ChapterId = ch.ChapterId,
                    Title = ch.Title,
                    Description = ch.Description,
                    NumberOfExercise = ch.NumberOfExercise,
                    CreatedAt = ch.CreatedAt,
                    CourseId = ch.CourseId,
                    Course = ch.Course == null ? null : new ReadCourseDTO
                    {
                        CourseId = ch.Course.CourseId,
                        Title = ch.Course.Title,
                        Level = ch.Course.Level
                    },
                    Exercises = ch.Exercises?.Select(ex => new ReadChapterExerciseDTO
                    {
                        ExerciseId = ex.ExerciseId,
                        Title = ex.Title,
                        Description = ex.Description,
                        OrderIndex = ex.OrderIndex,
                        NumberOfQuestion = ex.NumberOfQuestion,
                        ChapterId = ex.ChapterId
                    }).ToList() ?? new List<ReadChapterExerciseDTO>()
                }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách chương thành công.";
                dto.Data = new PagedResult<ReadChapterDTO>
                {
                    Items = mapped,
                    TotalPages = result.TotalPages
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy danh sách chương: " + ex.Message);
            }

            return dto;
        }

        // ✅ GET BY ID
        public async Task<ResponseDTO> GetChapterByIdAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var chapter = await _chapterRepository.GetFirstByExpression(
                    x => x.ChapterId == id,
                    x => x.Course,
                    x => x.Exercises
                );

                if (chapter == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy chương.");

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thông tin chương thành công.";
                dto.Data = new ReadChapterDTO
                {
                    ChapterId = chapter.ChapterId,
                    Title = chapter.Title,
                    Description = chapter.Description,
                    NumberOfExercise = chapter.NumberOfExercise,
                    CreatedAt = chapter.CreatedAt,
                    CourseId = chapter.CourseId,
                    Course = chapter.Course == null ? null : new ReadCourseDTO
                    {
                        CourseId = chapter.Course.CourseId,
                        Title = chapter.Course.Title,
                        Level = chapter.Course.Level
                    },
                    Exercises = chapter.Exercises?.Select(ex => new ReadChapterExerciseDTO
                    {
                        ExerciseId = ex.ExerciseId,
                        Title = ex.Title,
                        Description = ex.Description,
                        OrderIndex = ex.OrderIndex,
                        NumberOfQuestion = ex.NumberOfQuestion,
                        ChapterId = ex.ChapterId
                    }).ToList() ?? new List<ReadChapterExerciseDTO>()
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy chương: " + ex.Message);
            }

            return dto;
        }

        // ✅ CREATE
        public async Task<ResponseDTO> CreateChapterAsync(CreateChapterDTO request)
        {
            ResponseDTO dto = new();
            try
            {
                // Validate cơ bản
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên chương không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả (Description) không được để trống.");
                if (request.CourseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Khóa học (CourseId) không hợp lệ.");

                var course = await _courseRepository.GetById(request.CourseId);
                if (course == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy khóa học tương ứng.");

                var newChapter = new Chapter
                {
                    ChapterId = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    CourseId = request.CourseId,
                    NumberOfExercise = request.NumberOfExercise,
                    CreatedAt = DateTime.UtcNow
                };

                await _chapterRepository.Insert(newChapter);
                await _unitOfWork.SaveChangeAsync();

                // Nếu có bài tập kèm theo
                List<Exercise> createdExercises = new();
                if (request.Exercises != null && request.Exercises.Any())
                {
                    createdExercises = request.Exercises.Select(ex => new Exercise
                    {
                        ExerciseId = Guid.NewGuid(),
                        Title = ex.Title.Trim(),
                        Description = ex.Description.Trim(),
                        OrderIndex = ex.OrderIndex,
                        NumberOfQuestion = ex.NumberOfQuestion,
                        ChapterId = newChapter.ChapterId
                    }).ToList();

                    await _exerciseRepository.InsertRange(createdExercises);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Map trả ra
                var exerciseDtos = createdExercises.Select(ex => new ReadChapterExerciseDTO
                {
                    ExerciseId = ex.ExerciseId,
                    Title = ex.Title,
                    Description = ex.Description,
                    OrderIndex = ex.OrderIndex,
                    NumberOfQuestion = ex.NumberOfQuestion,
                    ChapterId = ex.ChapterId
                }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Tạo chương mới thành công.";
                dto.Data = new ReadChapterDTO
                {
                    ChapterId = newChapter.ChapterId,
                    Title = newChapter.Title,
                    Description = newChapter.Description,
                    NumberOfExercise = newChapter.NumberOfExercise,
                    CreatedAt = newChapter.CreatedAt,
                    CourseId = newChapter.CourseId,
                    Course = new ReadCourseDTO
                    {
                        CourseId = course.CourseId,
                        Title = course.Title,
                        Level = course.Level
                    },
                    Exercises = exerciseDtos
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Không thể tạo chương: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return dto;
        }

        // ✅ UPDATE
        public async Task<ResponseDTO> UpdateChapterAsync(Guid id, UpdateChapterDTO request)
        {
            ResponseDTO dto = new();
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");

                var chapter = await _chapterRepository.GetById(id);
                if (chapter == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy chương cần cập nhật.");

                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên chương không được để trống.");

                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả (Description) không được để trống.");

                if (request.CourseId != Guid.Empty)
                {
                    var course = await _courseRepository.GetById(request.CourseId);
                    if (course == null)
                        return Fail(BusinessCode.AUTH_NOT_FOUND, "Khóa học được chọn không tồn tại.");
                }

                if (request.NumberOfExercise.HasValue && request.NumberOfExercise < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số bài tập không được âm.");

                chapter.Title = request.Title.Trim();
                chapter.Description = request.Description.Trim();
                chapter.NumberOfExercise = request.NumberOfExercise ?? chapter.NumberOfExercise;

                await _chapterRepository.Update(chapter);
                await _unitOfWork.SaveChangeAsync();

                // Update Exercises nếu có
                if (request.Exercises != null && request.Exercises.Any())
                {
                    foreach (var ex in request.Exercises)
                    {
                        var existingExercise = await _exerciseRepository.GetById(ex.ExerciseId);
                        if (existingExercise == null)
                            return Fail(BusinessCode.AUTH_NOT_FOUND, $"Bài tập có ID {ex.ExerciseId} không tồn tại.");

                        if (string.IsNullOrWhiteSpace(ex.Title))
                            return Fail(BusinessCode.VALIDATION_FAILED, "Tên bài tập không được để trống.");
                        if (string.IsNullOrWhiteSpace(ex.Description))
                            return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả bài tập không được để trống.");

                        existingExercise.Title = ex.Title.Trim();
                        existingExercise.Description = ex.Description.Trim();
                        existingExercise.OrderIndex = ex.OrderIndex ?? existingExercise.OrderIndex;
                        existingExercise.NumberOfQuestion = ex.NumberOfQuestion ?? existingExercise.NumberOfQuestion;

                        await _exerciseRepository.Update(existingExercise);
                    }
                    await _unitOfWork.SaveChangeAsync();
                }

                // Load lại Chapter + Exercises
                var updatedChapter = await _chapterRepository.GetFirstByExpression(
                    x => x.ChapterId == chapter.ChapterId,
                    x => x.Course,
                    x => x.Exercises
                );

                if (updatedChapter == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không thể load lại chương sau khi cập nhật.");

                var exerciseDtos = updatedChapter.Exercises?.Select(ex => new ReadChapterExerciseDTO
                {
                    ExerciseId = ex.ExerciseId,
                    Title = ex.Title,
                    Description = ex.Description,
                    OrderIndex = ex.OrderIndex,
                    NumberOfQuestion = ex.NumberOfQuestion,
                    ChapterId = ex.ChapterId
                }).ToList() ?? new List<ReadChapterExerciseDTO>();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật chương thành công.";
                dto.Data = new ReadChapterDTO
                {
                    ChapterId = updatedChapter.ChapterId,
                    Title = updatedChapter.Title,
                    Description = updatedChapter.Description,
                    NumberOfExercise = updatedChapter.NumberOfExercise,
                    CreatedAt = updatedChapter.CreatedAt,
                    CourseId = updatedChapter.CourseId,
                    Course = updatedChapter.Course == null ? null : new ReadCourseDTO
                    {
                        CourseId = updatedChapter.Course.CourseId,
                        Title = updatedChapter.Course.Title,
                        Level = updatedChapter.Course.Level
                    },
                    Exercises = exerciseDtos
                };

                return dto;
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể cập nhật chương: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return dto;
        }

        // ✅ DELETE
        public async Task<ResponseDTO> DeleteChapterAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var chapter = await _chapterRepository.GetById(id);
                if (chapter == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy chương để xóa.");

                await _chapterRepository.Delete(chapter);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                dto.Message = "Xóa chương thành công.";
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Không thể xóa chương: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return dto;
        }
    }
}
