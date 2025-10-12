using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class ExerciseService : IExerciseService
    {
        private readonly IGenericRepository<Exercise> _exerciseRepository;
        private readonly IGenericRepository<Chapter> _chapterRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ExerciseService(
            IGenericRepository<Exercise> exerciseRepository,
            IGenericRepository<Chapter> chapterRepository,
            IGenericRepository<Course> courseRepository,
            IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _chapterRepository = chapterRepository;
            _courseRepository = courseRepository;
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

        // ✅ GET ALL (load cả Chapter + Course)
        public async Task<ResponseDTO> GetAllExercisesAsync(int pageNumber, int pageSize, Guid? chapterId = null, string? keyword = null)
        {
            ResponseDTO dto = new();
            try
            {
                var result = await _exerciseRepository.GetAllDataByExpression(
     filter: x =>
         (!chapterId.HasValue || x.ChapterId == chapterId) &&
         (string.IsNullOrEmpty(keyword) || x.Title.Contains(keyword)),
     pageNumber: pageNumber,
     pageSize: pageSize,
     orderBy: x => x.OrderIndex,
     isAscending: true,
     x => x.Chapter, x => x.Chapter.Course   // ✅ include bằng expression thay vì string
 );


                var mapped = result.Items.Select(ex => new ReadExerciseDTO
                {
                    ExerciseId = ex.ExerciseId,
                    Title = ex.Title,
                    Description = ex.Description,
                    OrderIndex = ex.OrderIndex,
                    NumberOfQuestion = ex.NumberOfQuestion,
                    ChapterId = ex.ChapterId,
                    Chapter = ex.Chapter == null ? null : new ReadChapterDTO
                    {
                        ChapterId = ex.Chapter.ChapterId,
                        Title = ex.Chapter.Title,
                        CourseId = ex.Chapter.CourseId,
                        Course = ex.Chapter.Course == null ? null : new ReadCourseDTO
                        {
                            CourseId = ex.Chapter.Course.CourseId,
                            Title = ex.Chapter.Course.Title,
                            Level = ex.Chapter.Course.Level
                        }
                    }
                }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách bài tập thành công.";
                dto.Data = new PagedResult<ReadExerciseDTO>
                {
                    Items = mapped,
                    TotalPages = result.TotalPages
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy danh sách bài tập: " + ex.Message);
            }
            return dto;
        }

        // ✅ GET BY ID
        public async Task<ResponseDTO> GetExerciseByIdAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var exercise = await _exerciseRepository.GetFirstByExpression(
                    x => x.ExerciseId == id,
                    x => x.Chapter,
                    x => x.Chapter.Course
                );

                if (exercise == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy bài tập.");

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thông tin bài tập thành công.";
                dto.Data = new ReadExerciseDTO
                {
                    ExerciseId = exercise.ExerciseId,
                    Title = exercise.Title,
                    Description = exercise.Description,
                    OrderIndex = exercise.OrderIndex,
                    NumberOfQuestion = exercise.NumberOfQuestion,
                    ChapterId = exercise.ChapterId,
                    Chapter = exercise.Chapter == null ? null : new ReadChapterDTO
                    {
                        ChapterId = exercise.Chapter.ChapterId,
                        Title = exercise.Chapter.Title,
                        CourseId = exercise.Chapter.CourseId,
                        Course = exercise.Chapter.Course == null ? null : new ReadCourseDTO
                        {
                            CourseId = exercise.Chapter.Course.CourseId,
                            Title = exercise.Chapter.Course.Title,
                            Level = exercise.Chapter.Course.Level
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy bài tập: " + ex.Message);
            }
            return dto;
        }

        // ✅ CREATE
        public async Task<ResponseDTO> CreateExerciseAsync(CreateExerciseDTO request)
        {
            ResponseDTO dto = new();
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên bài tập không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả bài tập không được để trống.");
                if (request.ChapterId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ChapterId không hợp lệ.");

                var chapter = await _chapterRepository.GetById(request.ChapterId);
                if (chapter == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy chương học tương ứng.");

                if (request.NumberOfQuestion < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số câu hỏi không được âm.");

                var newExercise = new Exercise
                {
                    ExerciseId = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    OrderIndex = request.OrderIndex,
                    NumberOfQuestion = request.NumberOfQuestion,
                    ChapterId = request.ChapterId
                };

                await _exerciseRepository.Insert(newExercise);
                await _unitOfWork.SaveChangeAsync();

                // ✅ Load lại Course/Chapter để trả ra
                var fullExercise = await _exerciseRepository.GetFirstByExpression(
                    x => x.ExerciseId == newExercise.ExerciseId,
                    x => x.Chapter,
                    x => x.Chapter.Course
                );

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Tạo bài tập mới thành công.";
                dto.Data = fullExercise;
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Không thể tạo bài tập: " + (ex.InnerException?.Message ?? ex.Message));
            }
            return dto;
        }

        // ✅ UPDATE
        public async Task<ResponseDTO> UpdateExerciseAsync(Guid id, UpdateExerciseDTO request)
        {
            ResponseDTO dto = new();
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");
                var exercise = await _exerciseRepository.GetById(id);
                if (exercise == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy bài tập cần cập nhật.");

                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên bài tập không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả bài tập không được để trống.");
                if (request.NumberOfQuestion.HasValue && request.NumberOfQuestion < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số câu hỏi không được âm.");

                exercise.Title = request.Title.Trim();
                exercise.Description = request.Description.Trim();
                exercise.OrderIndex = request.OrderIndex ?? exercise.OrderIndex;
                exercise.NumberOfQuestion = request.NumberOfQuestion ?? exercise.NumberOfQuestion;

                await _exerciseRepository.Update(exercise);
                await _unitOfWork.SaveChangeAsync();

                // ✅ Load lại Exercise sau khi cập nhật
                var updatedExercise = await _exerciseRepository.GetFirstByExpression(
                    x => x.ExerciseId == exercise.ExerciseId,
                    x => x.Chapter,
                    x => x.Chapter.Course
                );

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật bài tập thành công.";
                dto.Data = updatedExercise;
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Không thể cập nhật bài tập: " + (ex.InnerException?.Message ?? ex.Message));
            }
            return dto;
        }

        // ✅ DELETE
        public async Task<ResponseDTO> DeleteExerciseAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var exercise = await _exerciseRepository.GetById(id);
                if (exercise == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy bài tập để xóa.");

                await _exerciseRepository.Delete(exercise);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                dto.Message = "Xóa bài tập thành công.";
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Không thể xóa bài tập: " + (ex.InnerException?.Message ?? ex.Message));
            }
            return dto;
        }
    }
}
