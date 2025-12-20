using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class QuestionMediaService : IQuestionMediaService
    {
        private readonly IGenericRepository<QuestionMedia> _questionMediaRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public QuestionMediaService(
            IGenericRepository<QuestionMedia> questionMediaRepository,
            IGenericRepository<Question> questionRepository,
            IUnitOfWork unitOfWork)
        {
            _questionMediaRepository = questionMediaRepository;
            _questionRepository = questionRepository;
            _unitOfWork = unitOfWork;
        }

        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };

        // ============================================================
        // 🔹 GET ALL (PHÂN TRANG)
        // ============================================================
        public async Task<ResponseDTO> GetAllQuestionMediasAsync(int pageNumber, int pageSize, Guid? questionId = null)
        {
            try
            {
                var result = await _questionMediaRepository.GetAllDataByExpression(
            x => (!questionId.HasValue || x.QuestionId == questionId),
            pageNumber,
            pageSize,
            x => x.QuestionMediaId, 
            true
        );

                var mapped = result.Items.Select(m => new ReadQuestionMediaV2DTO
                {
                    QuestionMediaId = m.QuestionMediaId,
                    QuestionId = m.QuestionId,
                 
                  
                    VideoUrl = m.VideoUrl,
                    ImageUrl = m.ImageUrl,
                  
                }).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy danh sách QuestionMedia thành công.",
                    Data = new PagedResult<ReadQuestionMediaV2DTO>
                    {
                        Items = mapped,
                        TotalPages = result.TotalPages
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Lỗi khi lấy danh sách QuestionMedia: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 GET BY ID
        // ============================================================
        public async Task<ResponseDTO> GetQuestionMediaByIdAsync(Guid id)
        {
            try
            {
                var media = await _questionMediaRepository.GetById(id);
                if (media == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy QuestionMedia.");

                var dto = new ReadQuestionMediaV2DTO
                {
                    QuestionMediaId = media.QuestionMediaId,
                    QuestionId = media.QuestionId,
                  
                    VideoUrl = media.VideoUrl,
                    ImageUrl = media.ImageUrl,
                    
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy QuestionMedia thành công.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Lỗi khi lấy QuestionMedia: {ex.Message}");
            }
        }


        public async Task<ResponseDTO> CreateQuestionMediaAsync(
            Guid questionId,
            CreateQuestionMediaV2DTO request)
        {
            try
            {
                // --- VALIDATION ---
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không hợp lệ.");

                if (questionId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "QuestionId không hợp lệ.");

                // --- ÍT NHẤT 1 TRONG 2 URL PHẢI CÓ ---
                bool hasAnyMedia =
                    !string.IsNullOrWhiteSpace(request.VideoUrl) ||
                    !string.IsNullOrWhiteSpace(request.ImageUrl);

                if (!hasAnyMedia)
                    return Fail(
                        BusinessCode.VALIDATION_FAILED,
                        "Phải có ít nhất một trong VideoUrl hoặc ImageUrl."
                    );

                // --- CHECK QUESTION ---
                var question = await _questionRepository.GetById(questionId);
                if (question == null)
                    return Fail(
                        BusinessCode.DATA_NOT_FOUND,
                        "Không tìm thấy câu hỏi để gắn media."
                    );

                // --- CREATE ---
                var media = new QuestionMedia
                {
                    QuestionMediaId = Guid.NewGuid(),
                    QuestionId = questionId,
                    VideoUrl = request.VideoUrl,
                    ImageUrl = request.ImageUrl
                };

                await _questionMediaRepository.Insert(media);
                await _unitOfWork.SaveChangeAsync();

                // --- RESPONSE ---
                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo QuestionMedia thành công.",
                    Data = new
                    {
                        media.QuestionMediaId,
                        media.QuestionId,
                        media.VideoUrl,
                        media.ImageUrl
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(
                    BusinessCode.EXCEPTION,
                    $"Không thể tạo QuestionMedia: {ex.Message}"
                );
            }
        }


        public async Task<ResponseDTO> UpdateQuestionMediaAsync(Guid id, UpdateQuestionMediaV2DTO request)
        {
            try
            {
                var media = await _questionMediaRepository.GetById(id);
                if (media == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy QuestionMedia để cập nhật.");

             

                // --- ÍT NHẤT 1 TRONG 3 URL PHẢI CÓ ---
                bool hasAnyMedia =
                    !string.IsNullOrWhiteSpace(request.VideoUrl) ||
                    !string.IsNullOrWhiteSpace(request.ImageUrl);

                if (!hasAnyMedia)
                    return Fail(BusinessCode.VALIDATION_FAILED,
                        "Phải có ít nhất một trong các URL ( VideoUrl, ImageUrl).");

                // --- RÀNG BUỘC: KHÔNG TRÙNG ACCENT TRONG CÙNG QUESTION ---
              

                // --- CẬP NHẬT ---
              
                media.VideoUrl = request.VideoUrl;
                media.ImageUrl = request.ImageUrl;

                await _questionMediaRepository.Update(media);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                    Message = "Cập nhật QuestionMedia thành công.",
                    Data = media
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể cập nhật QuestionMedia: {ex.Message}");
            }
        }


        // ============================================================
        // 🔹 DELETE
        // ============================================================
        public async Task<ResponseDTO> DeleteQuestionMediaAsync(Guid id)
        {
            try
            {
                var media = await _questionMediaRepository.GetById(id);
                if (media == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy QuestionMedia để xoá.");

                await _questionMediaRepository.Delete(media);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
                    Message = "Xoá QuestionMedia thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể xoá QuestionMedia: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 GET BY QUESTION ID (FULL LIST)
        // ============================================================
        public async Task<ResponseDTO> GetQuestionMediasByQuestionIdAsync(Guid questionId)
        {
            try
            {
                if (questionId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "QuestionId không hợp lệ.");

                var medias = await _questionMediaRepository.AsQueryable()
                    .Where(x => x.QuestionId == questionId)
                    .ToListAsync();

                if (!medias.Any())
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy QuestionMedia cho câu hỏi này.");

                var mapped = medias.Select(m => new ReadQuestionMediaV2DTO
                {
                    QuestionMediaId = m.QuestionMediaId,
                    QuestionId = m.QuestionId,
                 
                    VideoUrl = m.VideoUrl,
                    ImageUrl = m.ImageUrl,
                   
                }).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy danh sách QuestionMedia theo QuestionId thành công.",
                    Data = mapped
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể lấy danh sách QuestionMedia: {ex.Message}");
            }
        }
    }
}
