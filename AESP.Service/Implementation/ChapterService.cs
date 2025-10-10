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

namespace AESP.Service.Implementation
{
    public class ChapterService : IChapterService
    {
        private readonly IGenericRepository<Chapter> _chapterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChapterService(
            IGenericRepository<Chapter> chapterRepository,
            IUnitOfWork unitOfWork)
        {
            _chapterRepository = chapterRepository;
            _unitOfWork = unitOfWork;
        }

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
                    isAscending: false
                );

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách chương thành công.";
                dto.Data = result;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách chương: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetChapterByIdAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var chapter = await _chapterRepository.GetById(id);
                if (chapter == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy chương.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết chương thành công.";
                dto.Data = chapter;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi truy vấn chương: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> CreateChapterAsync(CreateChapterDTO dtoRequest)
        {
            ResponseDTO dto = new();
            try
            {
                if (dtoRequest == null ||
                    dtoRequest.CourseId == Guid.Empty ||
                    string.IsNullOrWhiteSpace(dtoRequest.Title))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                    return dto;
                }

                var newChapter = new Chapter
                {
                    ChapterId = Guid.NewGuid(),
                    Title = dtoRequest.Title.Trim(),
                    Description = dtoRequest.Description?.Trim() ?? "",
                    CourseId = dtoRequest.CourseId,
                    NumberOfExercise = dtoRequest.NumberOfExercise,
                    CreatedAt = DateTime.UtcNow
                };

                await _chapterRepository.Insert(newChapter);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Tạo chương mới thành công.";
                dto.Data = newChapter;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi tạo chương: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }

        public async Task<ResponseDTO> UpdateChapterAsync(Guid id, UpdateChapterDTO dtoRequest)
        {
            ResponseDTO dto = new();
            try
            {
                var chapter = await _chapterRepository.GetById(id);
                if (chapter == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy chương cần cập nhật.";
                    return dto;
                }

                if (dtoRequest == null || string.IsNullOrWhiteSpace(dtoRequest.Title))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên chương không được để trống.";
                    return dto;
                }

                chapter.Title = dtoRequest.Title.Trim();
                chapter.Description = dtoRequest.Description?.Trim() ?? chapter.Description;
                chapter.NumberOfExercise = dtoRequest.NumberOfExercise ?? chapter.NumberOfExercise;

                await _chapterRepository.Update(chapter);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật chương thành công.";
                dto.Data = chapter;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi cập nhật chương: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }

        public async Task<ResponseDTO> DeleteChapterAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var chapter = await _chapterRepository.GetById(id);
                if (chapter == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy chương để xóa.";
                    return dto;
                }

                await _chapterRepository.Delete(chapter);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                dto.Message = "Xóa chương thành công.";
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi xóa chương: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }
    }
}
