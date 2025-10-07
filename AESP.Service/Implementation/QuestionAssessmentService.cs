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

        // ✅ LẤY TẤT CẢ (Phân trang + lọc)
        public async Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize, string? type = null, string? keyword = null)
        {
            ResponseDTO dto = new();
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
                dto.Message = "Lỗi khi lấy danh sách câu hỏi: " + ex.Message;
            }

            return dto;
        }

        // ✅ LẤY THEO ID
        public async Task<ResponseDTO> GetByIdAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var question = await _questionRepo.GetById(id);
                if (question == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy câu hỏi.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết câu hỏi thành công.";
                dto.Data = question;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi truy vấn câu hỏi: " + ex.Message;
            }

            return dto;
        }

        // ✅ TẠO MỚI
        public async Task<ResponseDTO> CreateAsync(CreateQuestionAssessmentDTO dtoRequest)
        {
            ResponseDTO dto = new();
            try
            {
                if (string.IsNullOrWhiteSpace(dtoRequest.Type) || string.IsNullOrWhiteSpace(dtoRequest.Content))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.EXCEPTION;
                    dto.Message = "Trường 'Type' và 'Content' không được để trống.";
                    return dto;
                }

                var newQuestion = new QuestionAssessment
                {
                    QuestionAssessmentId = Guid.NewGuid(),
                    Type = dtoRequest.Type,
                    Content = dtoRequest.Content
                };

                await _questionRepo.Insert(newQuestion);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Tạo câu hỏi đánh giá thành công.";
                dto.Data = newQuestion;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi tạo câu hỏi: " + ex.Message;
            }

            return dto;
        }

        // ✅ CẬP NHẬT
        public async Task<ResponseDTO> UpdateAsync(Guid id, UpdateQuestionAssessmentDTO dtoRequest)
        {
            ResponseDTO dto = new();
            try
            {
                var question = await _questionRepo.GetById(id);
                if (question == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy câu hỏi để cập nhật.";
                    return dto;
                }

                question.Type = dtoRequest.Type ?? question.Type;
                question.Content = dtoRequest.Content ?? question.Content;

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
                dto.Message = "Lỗi khi cập nhật câu hỏi: " + ex.Message;
            }

            return dto;
        }

        // ✅ XÓA (chưa liên kết)
        public async Task<ResponseDTO> DeleteAsync(Guid id)
        {
            ResponseDTO dto = new();
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
                dto.Message = "Lỗi khi xóa câu hỏi: " + ex.Message;
            }

            return dto;
        }
    }
}
