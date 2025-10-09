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

        public async Task<ResponseDTO> GetAllCourseAsync(int pageNumber, int pageSize, string? level = null, string? keyword = null)
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

        public async Task<ResponseDTO> GetByCourseIdAsync(Guid id)
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


        public async Task<ResponseDTO> CreateCourseAsync(CreateCourseDTO request)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                if (request == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Dữ liệu đầu vào không được để trống.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên khóa học không được để trống.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Type))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Loại khóa học không được để trống.";
                    return dto;
                }

                // ✅ Bỏ kiểm tra string level vì giờ là enum
                if (!Enum.IsDefined(typeof(CourseLevel), request.Level))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Cấp độ (Level) không hợp lệ. Giá trị hợp lệ: A1, A2, B1, B2, C1, C2.";
                    return dto;
                }

                if (request.NumberOfChapter <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Số chương phải lớn hơn 0.";
                    return dto;
                }

                if (request.OrderIndex < 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Thứ tự (OrderIndex) không được âm.";
                    return dto;
                }

                var newCourse = new Course
                {
                    CourseId = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    Type = request.Type.Trim(),
                    NumberOfChapter = request.NumberOfChapter,
                    OrderIndex = request.OrderIndex,
                    // ✅ Lưu enum dạng string để giữ tương thích DB
                    Level = request.Level.ToString()
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

        public async Task<ResponseDTO> UpdateCourseAsync(Guid id, UpdateCourseDTO request)
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

                if (request == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Dữ liệu đầu vào không được để trống.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên khóa học không được để trống.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Type))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Loại khóa học không được để trống.";
                    return dto;
                }

                if (request.Level == null || !Enum.IsDefined(typeof(CourseLevel), request.Level))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Cấp độ (Level) không hợp lệ. Giá trị hợp lệ: A1, A2, B1, B2, C1, C2.";
                    return dto;
                }


                if (request.NumberOfChapter.HasValue && request.NumberOfChapter <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Số chương phải lớn hơn 0.";
                    return dto;
                }

                if (request.OrderIndex.HasValue && request.OrderIndex < 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Thứ tự (OrderIndex) không được âm.";
                    return dto;
                }

                // Cập nhật hợp lệ
                course.Title = request.Title.Trim();
                course.Type = request.Type.Trim(); 
                course.Level = request.Level.ToString();
                course.NumberOfChapter = request.NumberOfChapter ?? course.NumberOfChapter;
                course.OrderIndex = request.OrderIndex ?? course.OrderIndex;

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

        public async Task<ResponseDTO> DeleteCourseAsync(Guid id)
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