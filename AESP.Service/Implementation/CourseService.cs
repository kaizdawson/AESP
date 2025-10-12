using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AESP.Service.Implementation
{
    public class CourseService : ICourseService
    {
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Chapter> _chapterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(
            IGenericRepository<Course> courseRepository,
            IGenericRepository<Chapter> chapterRepository,
            IUnitOfWork unitOfWork)
        {
            _courseRepository = courseRepository;
            _chapterRepository = chapterRepository;
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

        // ✅ GET ALL (Load luôn chapters)
        public async Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize, string? level = null, string? keyword = null)
        {
            ResponseDTO dto = new();
            try
            {
                var result = await _courseRepository.GetAllDataByExpression(
                    filter: x =>
                        (string.IsNullOrEmpty(level) || x.Level == level) &&
                        (string.IsNullOrEmpty(keyword) || x.Title.Contains(keyword)),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    orderBy: x => x.CourseId,
                    isAscending: true,
                    x => x.Chapters
                );

                var mapped = result.Items.Select(c => new ReadCourseDTO
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Type = c.Type,
                    NumberOfChapter = c.NumberOfChapter,
                    OrderIndex = c.OrderIndex,
                    Level = c.Level,
                    Chapters = c.Chapters?.Select(ch => new ReadCourseChapterDTO
                    {
                        ChapterId = ch.ChapterId,
                        Title = ch.Title,
                        Description = ch.Description,
                        NumberOfExercise = ch.NumberOfExercise
                    }).ToList()
                }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách khóa học thành công.";
                dto.Data = new PagedResult<ReadCourseDTO>
                {
                    Items = mapped,
                    TotalPages = result.TotalPages
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy danh sách khóa học: " + ex.Message);
            }
            return dto;
        }

        public async Task<ResponseDTO> GetByCourseIdAsync(Guid id)
        {
            try
            {
                var course = await _courseRepository.GetFirstByExpression(
                    x => x.CourseId == id,
                    x => x.Chapters
                );

                if (course == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy khóa học.");

                var readDto = new ReadCourseDTO
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Type = course.Type,
                    NumberOfChapter = course.NumberOfChapter,
                    OrderIndex = course.OrderIndex,
                    Level = course.Level,
                    Chapters = course.Chapters?.Select(ch => new ReadCourseChapterDTO
                    {
                        ChapterId = ch.ChapterId,
                        Title = ch.Title,
                        Description = ch.Description,
                        NumberOfExercise = ch.NumberOfExercise
                    }).ToList()
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy khóa học thành công.",
                    Data = readDto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy khóa học: " + ex.Message);
            }
        }

        // ✅ CREATE (valid toàn bộ field)
        public async Task<ResponseDTO> CreateAsync(CreateCourseDTO request)
        {
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không được để trống.");

                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên khóa học không được để trống.");

                if (string.IsNullOrWhiteSpace(request.Type))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Loại khóa học không được để trống.");

                if (request.NumberOfChapter <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số chương phải lớn hơn 0.");

                if (request.OrderIndex < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Thứ tự (OrderIndex) không hợp lệ.");

                if (request.Chapters == null || !request.Chapters.Any())
                    return Fail(BusinessCode.VALIDATION_FAILED, "Phải có ít nhất 1 chương trong khóa học.");

                var newCourse = new Course
                {
                    CourseId = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    Type = request.Type.Trim(),
                    NumberOfChapter = request.NumberOfChapter,
                    OrderIndex = request.OrderIndex,
                    Level = request.Level.ToString()
                };

                await _courseRepository.Insert(newCourse);
                await _unitOfWork.SaveChangeAsync();

                // Tạo chapters
                var chapters = request.Chapters.Select(ch => new Chapter
                {
                    ChapterId = Guid.NewGuid(),
                    Title = ch.Title.Trim(),
                    Description = ch.Description.Trim(),
                    NumberOfExercise = ch.NumberOfExercise,
                    CourseId = newCourse.CourseId
                }).ToList();

                await _chapterRepository.InsertRange(chapters);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo khóa học mới thành công.",
                    Data = new ReadCourseDTO
                    {
                        CourseId = newCourse.CourseId,
                        Title = newCourse.Title,
                        Type = newCourse.Type,
                        NumberOfChapter = newCourse.NumberOfChapter,
                        OrderIndex = newCourse.OrderIndex,
                        Level = newCourse.Level,
                        Chapters = chapters.Select(ch => new ReadCourseChapterDTO
                        {
                            ChapterId = ch.ChapterId,
                            Title = ch.Title,
                            Description = ch.Description,
                            NumberOfExercise = ch.NumberOfExercise
                        }).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể tạo khóa học: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        public async Task<ResponseDTO> UpdateCourseAsync(Guid id, UpdateCourseDTO request)
        {
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không được để trống.");

                var course = await _courseRepository.GetFirstByExpression(
                    x => x.CourseId == id,
                    x => x.Chapters
                );
                if (course == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy khóa học.");

                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên khóa học không được để trống.");

                if (string.IsNullOrWhiteSpace(request.Type))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Loại khóa học không được để trống.");

                if (request.NumberOfChapter <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số chương phải lớn hơn 0.");

                if (request.Chapters == null || !request.Chapters.Any())
                    return Fail(BusinessCode.VALIDATION_FAILED, "Phải có ít nhất 1 chương trong khóa học.");

                course.Title = request.Title.Trim();
                course.Type = request.Type.Trim();
                course.NumberOfChapter = request.NumberOfChapter ?? course.NumberOfChapter;
                course.OrderIndex = request.OrderIndex ?? course.OrderIndex;
                course.Level = request.Level?.ToString() ?? course.Level;

                await _courseRepository.Update(course);
                await _unitOfWork.SaveChangeAsync();

                foreach (var ch in request.Chapters)
                {
                    if (ch.ChapterId == null)
                        return Fail(BusinessCode.VALIDATION_FAILED, "Thiếu ChapterId khi cập nhật.");

                    var existing = await _chapterRepository.GetById(ch.ChapterId);
                    if (existing == null)
                        return Fail(BusinessCode.AUTH_NOT_FOUND, $"Không tìm thấy chương có ID {ch.ChapterId}");

                    if (string.IsNullOrWhiteSpace(ch.Title))
                        return Fail(BusinessCode.VALIDATION_FAILED, "Tên chương không được để trống.");

                    if (string.IsNullOrWhiteSpace(ch.Description))
                        return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả chương không được để trống.");

                    existing.Title = ch.Title.Trim();
                    existing.Description = ch.Description.Trim();
                    existing.NumberOfExercise = ch.NumberOfExercise ?? existing.NumberOfExercise;

                    await _chapterRepository.Update(existing);
                }
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                    Message = "Cập nhật khóa học thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể cập nhật khóa học: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        public async Task<ResponseDTO> DeleteCourseAsync(Guid id)
        {
            try
            {
                var course = await _courseRepository.GetById(id);
                if (course == null)
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy khóa học để xóa.");

                await _courseRepository.Delete(course);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
                    Message = "Xóa khóa học thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể xóa khóa học: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}