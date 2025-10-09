using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class QuestionAssessmentService : IQuestionAssessmentService
    {
        private readonly IGenericRepository<QuestionAssessment> _questionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public QuestionAssessmentService(
            IGenericRepository<QuestionAssessment> questionRepo,
            IUnitOfWork unitOfWork)
        {
            _questionRepo = questionRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetAllQuestionAssessmentAsync(int pageNumber, int pageSize, string? type = null, string? keyword = null)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var result = await _questionRepo.GetAllDataByExpression(
                    filter: x =>
                        (string.IsNullOrEmpty(type) || x.Type == type) &&
                        (string.IsNullOrEmpty(keyword) || x.Content.Contains(keyword)),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    orderBy: x => x.QuestionAssessmentId,
                    isAscending: true
                );

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách câu hỏi đánh giá thành công.";
                dto.Data = result;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Đã xảy ra lỗi khi lấy danh sách câu hỏi: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetByQuestionAssessmentIdAsync(Guid id)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var question = await _questionRepo.GetById(id);
                if (question == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy câu hỏi đánh giá.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết câu hỏi đánh giá thành công.";
                dto.Data = question;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết câu hỏi: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> CreateQuestionAssessmentAsync(CreateQuestionAssessmentDTO request)
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

                if (string.IsNullOrWhiteSpace(request.Type))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Loại câu hỏi (Type) không được để trống.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Nội dung câu hỏi (Content) không được để trống.";
                    return dto;
                }

                var newQuestion = new QuestionAssessment
                {
                    QuestionAssessmentId = Guid.NewGuid(),
                    Type = request.Type.Trim(),
                    Content = request.Content.Trim()
                };

                await _questionRepo.Insert(newQuestion);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Tạo câu hỏi đánh giá mới thành công.";
                dto.Data = newQuestion;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Không thể tạo câu hỏi mới: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }

        public async Task<ResponseDTO> UpdateQuestionAssessmentAsync(Guid id, UpdateQuestionAssessmentDTO request)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var question = await _questionRepo.GetById(id);
                if (question == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy câu hỏi cần cập nhật.";
                    return dto;
                }

                if (request == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Dữ liệu đầu vào không được để trống.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Type))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Loại câu hỏi (Type) không được để trống.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Nội dung câu hỏi (Content) không được để trống.";
                    return dto;
                }

                question.Type = request.Type.Trim();
                question.Content = request.Content.Trim();

                await _questionRepo.Update(question);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật câu hỏi đánh giá thành công.";
                dto.Data = question;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Không thể cập nhật câu hỏi: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }

        public async Task<ResponseDTO> DeleteQuestionAssessmentAsync(Guid id)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var question = await _questionRepo.GetById(id);
                if (question == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy câu hỏi để xóa.";
                    return dto;
                }

                await _questionRepo.Delete(question);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                dto.Message = "Xóa câu hỏi đánh giá thành công.";
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Không thể xóa câu hỏi: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return dto;
        }
    }
}
