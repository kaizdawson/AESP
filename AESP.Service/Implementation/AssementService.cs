using AESP.API.Helpers;
using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
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
        private readonly IGenericRepository<QuestionAssessment> _questionAssessmentRepository;

        private readonly IUnitOfWork _unitOfWork;

        public AssessmentService(
            IGenericRepository<Assessment> assessmentRepository,
            IGenericRepository<AssessmentDetail> assessmentDetailRepository,
            IGenericRepository<LearnerProfile> learnerProfileRepository,
            IGenericRepository<QuestionAssessment> questionAssessmentRepository,
        IUnitOfWork unitOfWork)
        {
            _assessmentRepository = assessmentRepository;
            _assessmentDetailRepository = assessmentDetailRepository;
            _learnerProfileRepository = learnerProfileRepository;
            _questionAssessmentRepository = questionAssessmentRepository;

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

        private static ResponseDTO Success(BusinessCode code, string msg, object? data = null)
        {
            return new ResponseDTO
            {
                IsSucess = true,
                BusinessCode = code,
                Message = msg,
                Data = data
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
                    CreatedAt = DateTimeHelper.NowVN(),
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


        public async Task<ResponseDTO> GetPlacementTestForLearnerAsync(Guid userId)
        {
            ResponseDTO dto = new();
            try
            {
                // --- 1️⃣ Lấy hồ sơ learner ---
                var learnerProfile = await _learnerProfileRepository.GetByExpression(lp => lp.UserId == userId);
                if (learnerProfile == null)
                {
                    learnerProfile = new LearnerProfile
                    {
                        LearnerProfileId = Guid.NewGuid(),
                        UserId = userId,
                        CreatedAt = DateTimeHelper.NowVN()
                    };
                    await _learnerProfileRepository.Insert(learnerProfile);
                    await _unitOfWork.SaveChangeAsync();
                }

                Guid learnerProfileId = learnerProfile.LearnerProfileId;

                // --- 2️⃣ Kiểm tra xem học viên đã có bài test nào chưa ---
                var existingAssessment = await _assessmentRepository.AsQueryable()
                    .Include(a => a.AssessmentDetails)
                    .ThenInclude(d => d.QuestionAssessment)
                    .Where(a => a.LearnerProfileId == learnerProfileId)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (existingAssessment == null)
                {
                    // ❗️Chưa có bài test nào → tạo mới 1 bài test với câu hỏi mặc định
                    var dbQuestion = _questionAssessmentRepository.GetDbContext();
                    var activeQuestions = await dbQuestion.Set<QuestionAssessment>()
                        .Where(q => q.Status == true)
                        .OrderBy(q => q.Type)
                        .ToListAsync();

                    if (!activeQuestions.Any())
                        return Fail(BusinessCode.DATA_NOT_FOUND, "Không có câu hỏi nào active trong hệ thống.");

                    var newAssessment = new Assessment
                    {
                        AssessmentId = Guid.NewGuid(),
                        LearnerProfileId = learnerProfileId,
                        CreatedAt = DateTimeHelper.NowVN(),
                        Feedback = string.Empty,
                        Score = null,

                        NumberOfQuestion = activeQuestions.Count
                    };

                    await _assessmentRepository.Insert(newAssessment);

                    var assessmentDetails = activeQuestions.Select(q => new AssessmentDetail
                    {
                        AssessmentDetailId = Guid.NewGuid(),
                        AssessmentId = newAssessment.AssessmentId,
                        QuestionAssessmentId = q.QuestionAssessmentId,
                        Type = q.Type,
                        Score = null,
                        AI_Feedback = string.Empty,
                        AnswerAudio = string.Empty
                    }).ToList();

                    await _assessmentDetailRepository.InsertRange(assessmentDetails);
                    await _unitOfWork.SaveChangeAsync();

                    existingAssessment = newAssessment;
                }

                // --- 3️⃣ Không tạo thêm mới nữa nếu học viên đã có test ---
                var dbContext = _assessmentRepository.GetDbContext();
                var assessmentWithDetails = await dbContext.Assessments
                    .Include(a => a.AssessmentDetails)
                    .ThenInclude(d => d.QuestionAssessment)
                    .Where(a => a.LearnerProfileId == learnerProfileId)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (assessmentWithDetails == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài test của học viên này.");

                // --- 4️⃣ Trả dữ liệu ra (chỉ là câu hỏi để học viên làm) ---
                var mapped = new
                {
                    assessmentWithDetails.AssessmentId,
                    assessmentWithDetails.CreatedAt,
                    assessmentWithDetails.NumberOfQuestion,
                    Sections = assessmentWithDetails.AssessmentDetails
                        .Where(d => d.QuestionAssessment != null && d.QuestionAssessment.Status)
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
                };

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách câu hỏi bài test đầu vào thành công.";
                dto.Data = mapped;
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy bài test đầu vào: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return dto;
        }

        public async Task<ResponseDTO> SubmitPlacementTestCombinedAsync(CreatePlacementTestDTO dto)
        {
            try
            {
                // 🧩 1️⃣ VALIDATION CƠ BẢN
                if (dto == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");

                if (dto.LearnerProfileId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "LearnerProfileId không hợp lệ.");

                if (dto.NumberOfQuestion <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "NumberOfQuestion phải lớn hơn 0.");

                if (dto.Tests == null || !dto.Tests.Any())
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tests không được để trống.");

                foreach (var test in dto.Tests)
                {
                    if (test.AssessmentDetails == null || !test.AssessmentDetails.Any())
                        return Fail(BusinessCode.VALIDATION_FAILED, "Mỗi phần test phải có ít nhất 1 câu hỏi.");

                    foreach (var detail in test.AssessmentDetails)
                    {
                        if (detail.QuestionAssessmentId == Guid.Empty)
                            return Fail(BusinessCode.VALIDATION_FAILED, "Một câu hỏi có QuestionAssessmentId không hợp lệ.");

                        if (detail.Score < 0 || detail.Score > 100)
                            return Fail(BusinessCode.VALIDATION_FAILED, "Điểm Score phải nằm trong khoảng 0 - 100.");

                    }
                }

               

                // 🧩 3️⃣ TÍNH TOÁN TRUNG BÌNH
                double totalScore = 0;
                int totalCount = 0;

                foreach (var test in dto.Tests)
                {
                    foreach (var detail in test.AssessmentDetails)
                    {
                        totalScore += detail.Score;
                        totalCount++;
                    }
                }

                double averageScore = totalCount > 0 ? totalScore / totalCount : 0;

                // 🧩 2️⃣ KIỂM TRA HỌC VIÊN ĐÃ LÀM TEST CHƯA
                var existed = await _assessmentRepository.AsQueryable()
                    .FirstOrDefaultAsync(x => x.LearnerProfileId == dto.LearnerProfileId);

                // Nếu đã có và Score > 0 → không cho nộp lại
                if (existed != null && existed.Score > 0 && averageScore > 0)
                    return Fail(BusinessCode.INVALID_ACTION, "Bạn đã hoàn thành bài test đầu vào. Không thể làm lại.");

                Assessment assessment;
                if (existed != null)
                {
                    // ✅ Đã có bài test → chỉ update dòng cũ
                    assessment = existed;
                    assessment.Score = averageScore;
                    assessment.Feedback = string.Empty;
                    assessment.NumberOfQuestion = dto.NumberOfQuestion;

                    await _assessmentRepository.Update(assessment);
                }
                else
                {
                    // ✅ Chưa có bài nào → tạo mới
                    assessment = new Assessment
                    {
                        AssessmentId = Guid.NewGuid(),
                        LearnerProfileId = dto.LearnerProfileId,
                        CreatedAt = DateTimeHelper.NowVN(),
                        Score = averageScore,
                        Feedback = string.Empty,
                        NumberOfQuestion = dto.NumberOfQuestion
                    };

                    await _assessmentRepository.Insert(assessment);
                }

                // Xóa hết detail cũ (nếu có) để ghi lại chi tiết mới
                var oldDetails = await _assessmentDetailRepository.AsQueryable()
                    .Where(d => d.AssessmentId == assessment.AssessmentId)
                    .ToListAsync();
                if (oldDetails.Any())
                    await _assessmentDetailRepository.DeleteRange(oldDetails);

                // 🧩 4️⃣ GHI CHI TIẾT CÂU HỎI
                var details = dto.Tests
                    .SelectMany(t => t.AssessmentDetails.Select(d => new AssessmentDetail
                    {
                        AssessmentDetailId = Guid.NewGuid(),
                        AssessmentId = assessment.AssessmentId,
                        QuestionAssessmentId = d.QuestionAssessmentId,
                        Type = string.Empty,
                        Score = d.Score,
                        // ✅ Gán mặc định hệ thống
                        AI_Feedback = string.Empty,
                        AnswerAudio = string.Empty

                    }))
                    .ToList();

                await _assessmentDetailRepository.InsertRange(details);
                await _unitOfWork.SaveChangeAsync();

                // 🧩 5️⃣ GÁN LEVEL MỚI THEO ĐIỂM (FIX CHUẨN)
                string assignedLevel = "A1";
                if (averageScore >= 90) assignedLevel = "C1";
                else if (averageScore >= 75) assignedLevel = "B2";
                else if (averageScore >= 60) assignedLevel = "B1";
                else if (averageScore >= 45) assignedLevel = "A2";

                // ✅ BẮT BUỘC PHẢI TỒN TẠI LEARNER
                var learner = await _learnerProfileRepository.GetById(dto.LearnerProfileId);
                if (learner == null)
                {
                    return Fail(BusinessCode.AUTH_NOT_FOUND, "Không tìm thấy LearnerProfile hợp lệ.");
                }

                // ✅ LUÔN UPDATE LEVEL — KỂ CẢ KHI = A1 (0 ĐIỂM)
                learner.Level = assignedLevel;
                learner.PronunciationScore = averageScore;
                learner.UpdatedAt = DateTimeHelper.NowVN();

                await _learnerProfileRepository.Update(learner);
                await _unitOfWork.SaveChangeAsync();


                // 🧩 6️⃣ TRẢ KẾT QUẢ
                return Success(BusinessCode.UPDATE_SUCESSFULLY, "Đã ghi kết quả bài test đầu vào thành công.", new
                {
                    assessment.AssessmentId,
                    dto.LearnerProfileId,
                    averageScore,
                    assignedLevel
                });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi ghi bài test đầu vào: " + inner);
            }
        }

        public async Task<ResponseDTO> GetAllAssessmentsAsync(int pageNumber, int pageSize)
        {
            ResponseDTO dto = new();

            try
            {
                var db = _assessmentRepository.GetDbContext();

                // 1️ Load full depth
                var query = db.Assessments
                .Include(a => a.LearnerProfile)
                .ThenInclude(lp => lp.User)
                .Include(a => a.AssessmentDetails)
                .ThenInclude(d => d.QuestionAssessment)
                .AsQueryable();

                // 2️ Pagination
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var data = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 3️ Map format (ĐÃ BỎ questionAssessmentId)
                var mapped = data.Select(a => new
                {
                    assessmentId = a.AssessmentId,
                    createdAt = a.CreatedAt,
                    score = a.Score,
                    feedback = a.Feedback,
                    numberOfQuestion = a.NumberOfQuestion,
                    learnerProfileId = a.LearnerProfileId,
                    learnerName = a.LearnerProfile != null
                    ? a.LearnerProfile.User.FullName
                    : null,

                    assessmentDetails = a.AssessmentDetails.Select(d => new
                    {
                        assessmentDetailId = d.AssessmentDetailId,
                        score = d.Score,
                        type = d.Type,
                        ai_Feedback = d.AI_Feedback,
                        answerAudio = d.AnswerAudio,

                        // ❌ Không lấy QuestionAssessmentId nữa
                        // ✅ Chỉ lấy Content
                        questionAssessment = d.QuestionAssessment == null
                            ? null
                            : new
                            {
                                content = d.QuestionAssessment.Content
                            }
                    }).ToList()
                }).ToList();

                // 4️⃣ Response
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách bài đánh giá thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = mapped
                };
            }
            catch (Exception ex)
            {
                dto = new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = BusinessCode.EXCEPTION,
                    Message = "Lỗi khi lấy danh sách bài đánh giá: " + ex.Message
                };
            }

            return dto;
        }
    }
}
