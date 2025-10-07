using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class CourseService : ICourseService
    {
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(
            IGenericRepository<Course> courseRepository,
            IUnitOfWork unitOfWork)
        {
            _courseRepository = courseRepository;
            _unitOfWork = unitOfWork;
        }

        // ✅ GET ALL
        public async Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize, string? level = null, string? keyword = null)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var result = await _courseRepository.GetAllDataByExpression(
                    filter: x =>
                        (string.IsNullOrEmpty(level) || x.Level == level) &&
                        (string.IsNullOrEmpty(keyword) || x.Title.Contains(keyword)),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    orderBy: x => x.CourseId,
                    isAscending: true
                );

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách khóa học thành công.";

                dto.Data = result;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Đã xảy ra lỗi trong quá trình lấy dữ liệu: " + ex.Message;
            }

            return dto;
        }

        // ✅ GET BY ID
        public async Task<ResponseDTO> GetByIdAsync(Guid id)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var course = await _courseRepository.GetById(id);
                if (course == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy khóa học.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thông tin khóa học thành công.";
                dto.Data = course;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = ex.Message;
            }

            return dto;
        }

        // ✅ CREATE
        public async Task<ResponseDTO> CreateAsync(CreateCourseDTO request)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var newCourse = new Course
                {
                    CourseId = Guid.NewGuid(),
                    Title = request.Title,
                    Type = request.Type,
                    NumberOfChapter = request.NumberOfChapter,
                    OrderIndex = request.OrderIndex,
                    Level = request.Level
                };

                await _courseRepository.Insert(newCourse);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Tạo khóa học mới thành công.";
                dto.Data = newCourse;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Không thể tạo khóa học mới: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }

        // ✅ UPDATE
        public async Task<ResponseDTO> UpdateAsync(Guid id, UpdateCourseDTO request)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var course = await _courseRepository.GetById(id);
                if (course == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy khóa học cần cập nhật.";
                    return dto;
                }

                course.Title = request.Title ?? course.Title;
                course.Type = request.Type ?? course.Type;
                course.NumberOfChapter = request.NumberOfChapter ?? course.NumberOfChapter;
                course.OrderIndex = request.OrderIndex ?? course.OrderIndex;
                course.Level = request.Level ?? course.Level;

                await _courseRepository.Update(course);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật khóa học thành công.";
                dto.Data = course;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Không thể cập nhật khóa học: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }

        // ✅ DELETE
        public async Task<ResponseDTO> DeleteAsync(Guid id)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var course = await _courseRepository.GetById(id);
                if (course == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy khóa học để xóa.";
                    return dto;
                }

                await _courseRepository.Delete(course);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                dto.Message = "Xóa khóa học thành công.";
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Không thể xóa khóa học: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }
    }
}
