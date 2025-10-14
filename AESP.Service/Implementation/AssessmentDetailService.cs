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
    public class AssessmentDetailService : IAssessmentDetailService
    {
        private readonly IGenericRepository<AssessmentDetail> _assessmentDetailRepository;
        private readonly IGenericRepository<Assessment> _assessmentRepository;
        private readonly IGenericRepository<QuestionAssessment> _questionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssessmentDetailService(
            IGenericRepository<AssessmentDetail> assessmentDetailRepository,
            IGenericRepository<Assessment> assessmentRepository,
            IGenericRepository<QuestionAssessment> questionRepository,
            IUnitOfWork unitOfWork)
        {
            _assessmentDetailRepository = assessmentDetailRepository;
            _assessmentRepository = assessmentRepository;
            _questionRepository = questionRepository;
            _unitOfWork = unitOfWork;
        }

        private static ResponseDTO Fail(BusinessCode code, string msg) =>
            new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };

        // ✅ GET ALL
        public async Task<ResponseDTO> GetAllAssessmentDetailsAsync(int pageNumber, int pageSize, Guid? assessmentId = null)
        {
            ResponseDTO dto = new();
            try
            {
                var result = await _assessmentDetailRepository.GetAllDataByExpression(
                    filter: x => !assessmentId.HasValue || x.AssessmentId == assessmentId,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    orderBy: x => x.AssessmentDetailId,
                    isAscending: false,
                    x => x.Assessment,
                    x => x.QuestionAssessment
                );

                var mapped = (result.Items ?? new List<AssessmentDetail>())
                    .Select(d => new ReadAssessmentDetailDTO
                    {
                        AssessmentDetailId = d.AssessmentDetailId,
                        AssessmentId = d.AssessmentId,
                        QuestionAssessmentId = d.QuestionAssessmentId,
                        Score = d.Score,
                        Type = d.Type,
                        AI_Feedback = d.AI_Feedback,
                        AnswerAudio = d.AnswerAudio
                    }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách chi tiết bài đánh giá thành công.";
                dto.Data = new PagedResult<ReadAssessmentDetailDTO>
                {
                    Items = mapped,
                    TotalPages = result.TotalPages
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy danh sách chi tiết bài đánh giá: " + ex.Message);
            }

            return dto;
        }

        // ✅ GET BY ID
        public async Task<ResponseDTO> GetAssessmentDetailByIdAsync(Guid id)
        {
            try
            {
                var detail = await _assessmentDetailRepository.GetFirstByExpression(
                    x => x.AssessmentDetailId == id,
                    x => x.Assessment,
                    x => x.QuestionAssessment
                );

                if (detail == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chi tiết bài đánh giá.");

                var dto = new ReadAssessmentDetailDTO
                {
                    AssessmentDetailId = detail.AssessmentDetailId,
                    AssessmentId = detail.AssessmentId,
                    QuestionAssessmentId = detail.QuestionAssessmentId,
                    Score = detail.Score,
                    Type = detail.Type,
                    AI_Feedback = detail.AI_Feedback,
                    AnswerAudio = detail.AnswerAudio
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy chi tiết bài đánh giá thành công.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy chi tiết bài đánh giá: " + ex.Message);
            }
        }

        // ✅ CREATE
        public async Task<ResponseDTO> CreateAssessmentDetailAsync(CreateAssessmentDetailDTO dto)
        {
            try
            {
                // --- 1️⃣ VALIDATION ---
                if (dto == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");
                if (dto.AssessmentId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "AssessmentId không hợp lệ.");
                if (dto.QuestionAssessmentId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "QuestionAssessmentId không hợp lệ.");
                if (string.IsNullOrWhiteSpace(dto.Type))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Trường 'Type' không được để trống.");
                if (dto.Score < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Điểm (Score) không được âm.");
                if (dto.AI_Feedback != null && string.IsNullOrWhiteSpace(dto.AI_Feedback))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Trường 'AI_Feedback' không được để trống khi gửi vào.");
                if (dto.AnswerAudio != null && string.IsNullOrWhiteSpace(dto.AnswerAudio))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Trường 'AnswerAudio' không được để trống khi gửi vào.");
                // --- 2️⃣ CHECK FOREIGN KEYS ---
                var assessment = await _assessmentRepository.GetById(dto.AssessmentId);
                if (assessment == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài đánh giá tương ứng.");

                var question = await _questionRepository.GetById(dto.QuestionAssessmentId);
                if (question == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi tương ứng.");

                // --- 3️⃣ MAP ENTITY ---
                var detail = new AssessmentDetail
                {
                    AssessmentDetailId = Guid.NewGuid(),
                    AssessmentId = dto.AssessmentId,
                    QuestionAssessmentId = dto.QuestionAssessmentId,
                    Score = dto.Score,
                    Type = dto.Type.Trim(),
                    AI_Feedback = dto.AI_Feedback?.Trim() ?? "",
                    AnswerAudio = dto.AnswerAudio?.Trim() ?? ""
                };

                await _assessmentDetailRepository.Insert(detail);
                await _unitOfWork.SaveChangeAsync();

                // --- 4️⃣ LOAD LẠI SAU KHI TẠO ---
                var created = await _assessmentDetailRepository.GetFirstByExpression(
                    x => x.AssessmentDetailId == detail.AssessmentDetailId,
                    x => x.Assessment,
                    x => x.QuestionAssessment
                );

                var readDto = new ReadAssessmentDetailDTO
                {
                    AssessmentDetailId = created.AssessmentDetailId,
                    AssessmentId = created.AssessmentId,
                    QuestionAssessmentId = created.QuestionAssessmentId,
                    Score = created.Score,
                    Type = created.Type,
                    AI_Feedback = created.AI_Feedback,
                    AnswerAudio = created.AnswerAudio
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo chi tiết bài đánh giá thành công.",
                    Data = readDto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể tạo chi tiết bài đánh giá: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // ✅ UPDATE
        public async Task<ResponseDTO> UpdateAssessmentDetailAsync(Guid id, UpdateAssessmentDetailDTO dto)
        {
            try
            {
                // --- 1️⃣ VALIDATION ---
                if (dto == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");

                // --- 2️⃣ CHECK ENTITY ---
                var detail = await _assessmentDetailRepository.GetById(id);
                if (detail == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chi tiết bài đánh giá cần cập nhật.");

                // --- 3️⃣ FIELD VALIDATION ---
                if (dto.Score.HasValue && dto.Score.Value < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Điểm (Score) không được âm.");
                if (dto.Type != null && string.IsNullOrWhiteSpace(dto.Type))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Trường 'Type' không được để trống khi gửi vào.");
                if (dto.AI_Feedback != null && string.IsNullOrWhiteSpace(dto.AI_Feedback))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Trường 'AI_Feedback' không được để trống khi gửi vào.");
                if (dto.AnswerAudio != null && string.IsNullOrWhiteSpace(dto.AnswerAudio))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Trường 'AnswerAudio' không được để trống khi gửi vào.");

                // --- 4️⃣ UPDATE ENTITY ---
                if (dto.Score.HasValue) detail.Score = dto.Score.Value;
                if (!string.IsNullOrWhiteSpace(dto.Type)) detail.Type = dto.Type.Trim();
                if (!string.IsNullOrWhiteSpace(dto.AI_Feedback)) detail.AI_Feedback = dto.AI_Feedback.Trim();
                if (!string.IsNullOrWhiteSpace(dto.AnswerAudio)) detail.AnswerAudio = dto.AnswerAudio.Trim();

                await _assessmentDetailRepository.Update(detail);
                await _unitOfWork.SaveChangeAsync();

                // --- 5️⃣ LOAD LẠI SAU KHI UPDATE ---
                var updated = await _assessmentDetailRepository.GetFirstByExpression(
                    x => x.AssessmentDetailId == detail.AssessmentDetailId,
                    x => x.Assessment,
                    x => x.QuestionAssessment
                );

                var readDto = new ReadAssessmentDetailDTO
                {
                    AssessmentDetailId = updated.AssessmentDetailId,
                    AssessmentId = updated.AssessmentId,
                    QuestionAssessmentId = updated.QuestionAssessmentId,
                    Score = updated.Score,
                    Type = updated.Type,
                    AI_Feedback = updated.AI_Feedback,
                    AnswerAudio = updated.AnswerAudio
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                    Message = "Cập nhật chi tiết bài đánh giá thành công.",
                    Data = readDto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể cập nhật chi tiết bài đánh giá: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // ✅ DELETE
        public async Task<ResponseDTO> DeleteAssessmentDetailAsync(Guid id)
        {
            try
            {
                var detail = await _assessmentDetailRepository.GetById(id);
                if (detail == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chi tiết bài đánh giá để xóa.");

                await _assessmentDetailRepository.Delete(detail);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
                    Message = "Xóa chi tiết bài đánh giá thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể xóa chi tiết bài đánh giá: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
