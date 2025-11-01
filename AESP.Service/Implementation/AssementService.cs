using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IGenericRepository<Assessment> _assessmentRepository;
        private readonly IGenericRepository<AssessmentDetail> _assessmentDetailRepository;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssessmentService(
            IGenericRepository<Assessment> assessmentRepository,
            IGenericRepository<AssessmentDetail> assessmentDetailRepository,
            IGenericRepository<LearnerProfile> learnerProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _assessmentRepository = assessmentRepository;
            _assessmentDetailRepository = assessmentDetailRepository;
            _learnerProfileRepository = learnerProfileRepository;
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
        public async Task<ResponseDTO> GetAllAssessmentsAsync(int pageNumber, int pageSize, Guid? learnerId = null, string? keyword = null)
        {
            ResponseDTO dto = new();
            try
            {
                var result = await _assessmentRepository.GetAllDataByExpression(
                    filter: x =>
                        (!learnerId.HasValue || x.LearnerProfileId == learnerId) &&
                        (string.IsNullOrEmpty(keyword) ),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    orderBy: x => x.CreatedAt,
                    isAscending: false,
                    x => x.AssessmentDetails
                );

                var mapped = result.Items.Select(a => new ReadAssessmentDTO
                {
                    AssessmentId = a.AssessmentId,
                    CreatedAt = a.CreatedAt,
                    Score = a.Score,
                    Feedback = a.Feedback,
                    NumberOfQuestion = a.NumberOfQuestion,
                    LearnerProfileId = a.LearnerProfileId,
                    AssessmentDetails = a.AssessmentDetails?.Select(d => new ReadAssessmentDetailInAssessmentDTO
                    {
                        AssessmentDetailId = d.AssessmentDetailId,
                        Score = d.Score,
                        Type = d.Type,
                        AI_Feedback = d.AI_Feedback,
                        AnswerAudio = d.AnswerAudio,
                        QuestionAssessmentId = d.QuestionAssessmentId
                    }).ToList() ?? new List<ReadAssessmentDetailInAssessmentDTO>()
                }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách bài đánh giá thành công.";
                dto.Data = new PagedResult<ReadAssessmentDTO>
                {
                    Items = mapped,
                    TotalPages = result.TotalPages
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy danh sách bài đánh giá: " + ex.Message);
            }

            return dto;
        }

        // ✅ GET BY ID
        public async Task<ResponseDTO> GetAssessmentByIdAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var assessment = await _assessmentRepository.GetFirstByExpression(
                    x => x.AssessmentId == id,
                    x => x.AssessmentDetails
                );

                if (assessment == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài đánh giá.");

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thông tin bài đánh giá thành công.";
                dto.Data = new ReadAssessmentDTO
                {
                    AssessmentId = assessment.AssessmentId,
                    CreatedAt = assessment.CreatedAt,
                    Score = assessment.Score,
                    Feedback = assessment.Feedback,
                    NumberOfQuestion = assessment.NumberOfQuestion,
                    LearnerProfileId = assessment.LearnerProfileId,
                    AssessmentDetails = assessment.AssessmentDetails?.Select(d => new ReadAssessmentDetailInAssessmentDTO
                    {
                        AssessmentDetailId = d.AssessmentDetailId,
                        Score = d.Score,
                        Type = d.Type,
                        AI_Feedback = d.AI_Feedback,
                        AnswerAudio = d.AnswerAudio,
                        QuestionAssessmentId = d.QuestionAssessmentId
                    }).ToList() ?? new List<ReadAssessmentDetailInAssessmentDTO>()
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy bài đánh giá: " + ex.Message);
            }

            return dto;
        }

        // ✅ CREATE
        public async Task<ResponseDTO> CreateAssessmentAsync(CreateAssessmentDTO request)
        {
            ResponseDTO dto = new();
            try
            {
                // --- VALIDATION ---
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");
                if (request.LearnerProfileId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "LearnerProfileId không được để trống.");
              
                if (request.NumberOfQuestion <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số lượng câu hỏi phải lớn hơn 0.");

                var learner = await _learnerProfileRepository.GetById(request.LearnerProfileId);
                if (learner == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên tương ứng.");

                // --- CREATE ENTITY ---
                var newAssessment = new Assessment
                {
                    AssessmentId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    LearnerProfileId = request.LearnerProfileId,
                    Score = request.Score,
                    Feedback = request.Feedback?.Trim() ?? "",
                    NumberOfQuestion = request.NumberOfQuestion
                };

                await _assessmentRepository.Insert(newAssessment);
                await _unitOfWork.SaveChangeAsync();

                // --- CREATE DETAILS ---
                List<AssessmentDetail> createdDetails = new();
                if (request.AssessmentDetails != null && request.AssessmentDetails.Any())
                {
                    foreach (var d in request.AssessmentDetails)
                    {
                        if (string.IsNullOrWhiteSpace(d.Type))
                            return Fail(BusinessCode.VALIDATION_FAILED, "Type trong AssessmentDetail không được để trống.");

                        var detail = new AssessmentDetail
                        {
                            AssessmentDetailId = Guid.NewGuid(),
                            AssessmentId = newAssessment.AssessmentId,
                            Score = d.Score,
                            Type = d.Type.Trim(),
                            AI_Feedback = d.AI_Feedback?.Trim() ?? "",
                            AnswerAudio = d.AnswerAudio?.Trim() ?? "",
                            QuestionAssessmentId = d.QuestionAssessmentId
                        };
                        createdDetails.Add(detail);
                    }

                    await _assessmentDetailRepository.InsertRange(createdDetails);
                    await _unitOfWork.SaveChangeAsync();
                }

                // --- MAP RETURN ---
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Tạo bài đánh giá mới thành công.";
                dto.Data = new ReadAssessmentDTO
                {
                    AssessmentId = newAssessment.AssessmentId,
                    CreatedAt = newAssessment.CreatedAt,
                    Score = newAssessment.Score,
                    Feedback = newAssessment.Feedback,
                    NumberOfQuestion = newAssessment.NumberOfQuestion,
                    LearnerProfileId = newAssessment.LearnerProfileId,
                    AssessmentDetails = createdDetails.Select(d => new ReadAssessmentDetailInAssessmentDTO
                    {
                        AssessmentDetailId = d.AssessmentDetailId,
                        Score = d.Score,
                        Type = d.Type,
                        AI_Feedback = d.AI_Feedback,
                        AnswerAudio = d.AnswerAudio,
                        QuestionAssessmentId = d.QuestionAssessmentId
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Không thể tạo bài đánh giá: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return dto;
        }

        // ✅ UPDATE
        public async Task<ResponseDTO> UpdateAssessmentAsync(Guid id, UpdateAssessmentDTO request)
        {
            ResponseDTO dto = new();
            try
            {
                // --- 1️⃣ VALIDATION: Check request tổng ---
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");

                // --- 2️⃣ VALIDATION: Check tồn tại bài đánh giá ---
                var assessment = await _assessmentRepository.GetFirstByExpression(
                    x => x.AssessmentId == id,
                    x => x.AssessmentDetails
                );

                if (assessment == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài đánh giá cần cập nhật.");

                // --- 3️⃣ VALIDATION: Kiểm tra từng field ---
                // Validate Type
               

                // Validate Feedback
                if (request.Feedback != null && string.IsNullOrWhiteSpace(request.Feedback))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Trường 'Feedback' không được để trống khi gửi vào.");

                // Validate Score
                if (request.Score.HasValue && request.Score.Value < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Điểm (Score) không được âm.");

                // Validate NumberOfQuestion
                if (request.NumberOfQuestion.HasValue && request.NumberOfQuestion.Value <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số lượng câu hỏi (NumberOfQuestion) phải lớn hơn 0.");

                // --- 4️⃣ UPDATE MAIN ENTITY ---
               
                if (!string.IsNullOrWhiteSpace(request.Feedback))
                    assessment.Feedback = request.Feedback.Trim();

                if (request.Score.HasValue)
                    assessment.Score = request.Score.Value;

                if (request.NumberOfQuestion.HasValue)
                    assessment.NumberOfQuestion = request.NumberOfQuestion.Value;

                await _assessmentRepository.Update(assessment);
                await _unitOfWork.SaveChangeAsync();

                // --- 5️⃣ UPDATE DETAILS (nếu có) ---
                if (request.AssessmentDetails != null && request.AssessmentDetails.Any())
                {
                    foreach (var d in request.AssessmentDetails)
                    {
                        // Validate từng detail
                        if (d.AssessmentDetailId == Guid.Empty)
                            return Fail(BusinessCode.VALIDATION_FAILED, "AssessmentDetailId không được để trống.");

                        var existingDetail = assessment.AssessmentDetails
                            .FirstOrDefault(x => x.AssessmentDetailId == d.AssessmentDetailId);

                        if (existingDetail == null)
                            return Fail(BusinessCode.DATA_NOT_FOUND, $"AssessmentDetail ID {d.AssessmentDetailId} không tồn tại.");

                        if (d.Score.HasValue && d.Score.Value < 0)
                            return Fail(BusinessCode.VALIDATION_FAILED, $"Điểm (Score) của AssessmentDetail {d.AssessmentDetailId} không được âm.");

                        if (d.Type != null && string.IsNullOrWhiteSpace(d.Type))
                            return Fail(BusinessCode.VALIDATION_FAILED, $"Trường 'Type' của AssessmentDetail {d.AssessmentDetailId} không được để trống.");

                        if (d.AI_Feedback != null && string.IsNullOrWhiteSpace(d.AI_Feedback))
                            return Fail(BusinessCode.VALIDATION_FAILED, $"Trường 'AI_Feedback' của AssessmentDetail {d.AssessmentDetailId} không được để trống.");

                        if (d.AnswerAudio != null && string.IsNullOrWhiteSpace(d.AnswerAudio))
                            return Fail(BusinessCode.VALIDATION_FAILED, $"Trường 'AnswerAudio' của AssessmentDetail {d.AssessmentDetailId} không được để trống.");

                        // --- Cập nhật chi tiết ---
                        if (d.Score.HasValue)
                            existingDetail.Score = d.Score.Value;

                        if (!string.IsNullOrWhiteSpace(d.Type))
                            existingDetail.Type = d.Type.Trim();

                        if (!string.IsNullOrWhiteSpace(d.AI_Feedback))
                            existingDetail.AI_Feedback = d.AI_Feedback.Trim();

                        if (!string.IsNullOrWhiteSpace(d.AnswerAudio))
                            existingDetail.AnswerAudio = d.AnswerAudio.Trim();

                        await _assessmentDetailRepository.Update(existingDetail);
                    }

                    await _unitOfWork.SaveChangeAsync();

                    // ✅ Sau khi lưu, load lại bản ghi kèm detail
                    var updated = await _assessmentRepository.GetFirstByExpression(
                        x => x.AssessmentId == assessment.AssessmentId,
                        x => x.AssessmentDetails
                    );

                    // ✅ Map sang DTO để trả ra client
                    var responseData = new ReadAssessmentDTO
                    {
                        AssessmentId = updated.AssessmentId,
                        CreatedAt = updated.CreatedAt,
                        Score = updated.Score,
                        Feedback = updated.Feedback,
                        NumberOfQuestion = updated.NumberOfQuestion,
                        LearnerProfileId = updated.LearnerProfileId,
                        AssessmentDetails = updated.AssessmentDetails.Select(d => new ReadAssessmentDetailInAssessmentDTO
                        {
                            AssessmentDetailId = d.AssessmentDetailId,
                            Score = d.Score,
                            Type = d.Type,
                            AI_Feedback = d.AI_Feedback,
                            AnswerAudio = d.AnswerAudio,
                            QuestionAssessmentId = d.QuestionAssessmentId
                        }).ToList()
                    };

                    // ✅ Trả kết quả ra ngoài
                    dto.IsSucess = true;
                    dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                    dto.Message = "Cập nhật bài đánh giá thành công.";
                    dto.Data = responseData;

                }

                // --- 6️⃣ Trả kết quả ---
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật bài đánh giá thành công.";
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Không thể cập nhật bài đánh giá: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return dto;
        }

        // ✅ DELETE
        public async Task<ResponseDTO> DeleteAssessmentAsync(Guid id)
        {
            ResponseDTO dto = new();
            try
            {
                var assessment = await _assessmentRepository.GetById(id);
                if (assessment == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài đánh giá để xóa.");

                await _assessmentRepository.Delete(assessment);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                dto.Message = "Xóa bài đánh giá thành công.";
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Không thể xóa bài đánh giá: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return dto;
        }


        public async Task<ResponseDTO> GetPlacementTestForLearnerAsync(Guid learnerProfileId)
        {
            ResponseDTO dto = new();

            try
            {
                // ✅ Lọc theo đúng learner
                var result = await _assessmentRepository.GetAllDataByExpression(
     filter: x => x.LearnerProfileId == learnerProfileId, // ✅ Lọc theo learner
     pageNumber: 0,
     pageSize: 0,
     includes: new Expression<Func<Assessment, object>>[]
     {
        x => x.AssessmentDetails
     }
 );


                var db = _assessmentRepository.GetDbContext();

                foreach (var assessment in result.Items)
                {
                    foreach (var detail in assessment.AssessmentDetails)
                    {
                        await db.Entry(detail).Reference(d => d.QuestionAssessment).LoadAsync();
                    }
                }

                // ✅ Lọc bài test có QuestionAssessment.Status = true
                var activeAssessments = result.Items
                    .Where(a => a.AssessmentDetails.Any(d => d.QuestionAssessment.Status))
                    .ToList();

                if (!activeAssessments.Any())
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Learner này chưa có bài test nào có câu hỏi active.");

                // ✅ Gom nhóm câu hỏi theo type
                var mapped = activeAssessments.Select(assessment => new
                {
                    assessment.AssessmentId,
                    assessment.CreatedAt,
                    assessment.LearnerProfileId,
                    assessment.Score,
                    assessment.Feedback,
                    assessment.NumberOfQuestion,
                    Sections = assessment.AssessmentDetails
                        .Where(d => d.QuestionAssessment.Status)
                        .GroupBy(d => d.QuestionAssessment.Type)
                        .Select(g => new
                        {
                            Type = g.Key,
                            Questions = g.Select(q => new
                            {
                                q.QuestionAssessment.QuestionAssessmentId,
                                q.QuestionAssessment.Content
                            }).ToList()
                        }).ToList()
                }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy bài test đầu vào thành công.";
                dto.Data = mapped;
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy bài test đầu vào: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return dto;
        }





    }
}
