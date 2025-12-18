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
    public class QuestionService : IQuestionService
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<QuestionMedia> _mediaRepository;
        private readonly IGenericRepository<Exercise> _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public QuestionService(
            IGenericRepository<Question> questionRepository,
            IGenericRepository<QuestionMedia> mediaRepository,
            IGenericRepository<Exercise> exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _questionRepository = questionRepository;
            _mediaRepository = mediaRepository;
            _exerciseRepository = exerciseRepository;
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

        // ---------------- GET ALL ----------------
        public async Task<ResponseDTO> GetAllQuestionsAsync(int pageNumber, int pageSize, Guid? exerciseId = null)
        {
            ResponseDTO dto = new();
            try
            {
                var result = await _questionRepository.GetAllDataByExpression(
                    x => !exerciseId.HasValue || x.ExerciseId == exerciseId,
                    pageNumber,
                    pageSize,
                    orderBy: x => x.OrderIndex,
                    isAscending: true,
                    x => x.QuestionMedias
                );

                var mapped = result.Items.Select(q => new ReadQuestionDTO
                {
                    QuestionId = q.QuestionId,
                    ExerciseId = q.ExerciseId,
                    Text = q.Text,
                    Type = q.Type,
                    OrderIndex = q.OrderIndex,
                    Media = q.QuestionMedias?.Select(m => new ReadQuestionMediaDTO
                    {
                        QuestionMediaId = m.QuestionMediaId,
                        Accent = m.Accent,
                        AudioURL = m.AudioUrl,
                        VideoURL = m.VideoUrl,
                        ImageURL = m.ImageUrl,
                        Source = m.Source
                    }).ToList() ?? new List<ReadQuestionMediaDTO>()
                }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách câu hỏi thành công.";
                dto.Data = new PagedResult<ReadQuestionDTO> { Items = mapped, TotalPages = result.TotalPages };
            }
            catch (Exception ex)
            {
                dto = Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy danh sách câu hỏi: " + ex.Message);
            }

            return dto;
        }

        // ---------------- GET BY ID ----------------
        public async Task<ResponseDTO> GetQuestionByIdAsync(Guid id)
        {
            try
            {
                var question = await _questionRepository.GetFirstByExpression(
                    x => x.QuestionId == id,
                    x => x.QuestionMedias
                );

                if (question == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi.");

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy thông tin câu hỏi thành công.",
                    Data = new ReadQuestionDTO
                    {
                        QuestionId = question.QuestionId,
                        ExerciseId = question.ExerciseId,
                        Text = question.Text,
                        Type = question.Type,
                        OrderIndex = question.OrderIndex,
                        Media = question.QuestionMedias?.Select(m => new ReadQuestionMediaDTO
                        {
                            QuestionMediaId = m.QuestionMediaId,
                            Accent = m.Accent,
                            AudioURL = m.AudioUrl,
                            VideoURL = m.VideoUrl,
                            ImageURL = m.ImageUrl,
                            Source = m.Source
                        }).ToList() ?? new List<ReadQuestionMediaDTO>()
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy câu hỏi: " + ex.Message);
            }
        }
        public async Task<ResponseDTO> CreateQuestionsByExerciseIdAsync(Guid exerciseId, List<CreateQuestionDTO> requests)
        {
            try
            {
                // ===== VALIDATION CƠ BẢN =====
                if (exerciseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ExerciseId không hợp lệ.");

                var exercise = await _exerciseRepository.GetById(exerciseId);
                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập tương ứng.");

                if (requests == null || !requests.Any())
                    return Fail(BusinessCode.VALIDATION_FAILED, "Danh sách câu hỏi không được để trống.");

                // ===== RÀNG BUỘC SỐ LƯỢNG CÂU HỎI =====
                var existingCount = await _questionRepository.AsQueryable()
                    .CountAsync(q => q.ExerciseId == exerciseId);

                var availableSlots = exercise.NumberOfQuestion - existingCount;
                if (availableSlots <= 0)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Bài tập '{exercise.Title}' đã đạt tối đa {exercise.NumberOfQuestion} câu hỏi.");

                if (requests.Count > availableSlots)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Chỉ có thể thêm tối đa {availableSlots} câu hỏi nữa cho bài tập '{exercise.Title}'.");

                // ===== RÀNG BUỘC: ORDERINDEX TRÙNG TRONG DANH SÁCH GỬI LÊN =====
               
                


                var currentMax = await _questionRepository.AsQueryable()
                 .Where(q => q.ExerciseId == exerciseId)
                 .Select(q => (int?)q.OrderIndex)
                 .MaxAsync() ?? 0;
                int nextOrder = currentMax + 1;
                // ===== TẠO DANH SÁCH CÂU HỎI =====
                var questions = new List<Question>();
                int index = 1;

                foreach (var req in requests)
                {
                    if (req == null)
                        return Fail(BusinessCode.VALIDATION_FAILED, $"Câu hỏi thứ {index} bị null.");
                    if (string.IsNullOrWhiteSpace(req.Text))
                        return Fail(BusinessCode.VALIDATION_FAILED, $"Câu hỏi thứ {index}: Text không được để trống.");
                    if (!Enum.IsDefined(typeof(QuestionType), req.Type))
                        return Fail(BusinessCode.VALIDATION_FAILED, $"Câu hỏi thứ {index}: Type không hợp lệ.");

                    questions.Add(new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        ExerciseId = exerciseId,
                        Text = req.Text.Trim(),
                        Type = req.Type.ToString(),
                        OrderIndex = nextOrder++,
                    });

                    index++;
                }

                await _questionRepository.InsertRange(questions);
                await _unitOfWork.SaveChangeAsync();

                // ===== TRẢ KẾT QUẢ =====
                var created = await _questionRepository.AsQueryable()
                    .Where(q => q.ExerciseId == exerciseId)
                    .OrderBy(q => q.OrderIndex)
                    .ToListAsync();

                var dto = created.Select(q => new ReadQuestionDTO
                {
                    QuestionId = q.QuestionId,
                    ExerciseId = q.ExerciseId,
                    Text = q.Text,
                    Type = q.Type,
                    OrderIndex = q.OrderIndex,
                    Media = new List<ReadQuestionMediaDTO>()
                }).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo danh sách câu hỏi thành công.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể tạo câu hỏi: " + ex.Message);
            }
        }


        public async Task<ResponseDTO> UpdateQuestionAsync(Guid id, UpdateQuestionDTO request)
        {
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không hợp lệ.");

                var question = await _questionRepository.GetById(id);
                if (question == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi cần cập nhật.");

                // 🔹 Validation cơ bản
                if (string.IsNullOrWhiteSpace(request.Text))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Text không được để trống.");
                if (request.Type.HasValue && !Enum.IsDefined(typeof(QuestionType), request.Type.Value))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Type không hợp lệ.");
             


                //// 🔹 Check trùng OrderIndex trong cùng Exercise
                //if (request.OrderIndex.HasValue)
                //{
                //    var isDuplicateIndex = await _questionRepository.AsQueryable()
                //        .AnyAsync(q => q.ExerciseId == question.ExerciseId
                //                    && q.QuestionId != question.QuestionId
                //                    && q.OrderIndex == request.OrderIndex.Value);
                //    if (isDuplicateIndex)
                //        return Fail(BusinessCode.DUPLICATE_DATA,
                //            $"OrderIndex {request.OrderIndex.Value} đã tồn tại trong bài tập.");
                //}

                // 🔹 Cập nhật các field
                question.Text = request.Text.Trim();
                if (request.Type.HasValue)
                    question.Type = request.Type.Value.ToString();
             

                await _questionRepository.Update(question);
                await _unitOfWork.SaveChangeAsync();

                var dto = new ReadQuestionDTO
                {
                    QuestionId = question.QuestionId,
                    ExerciseId = question.ExerciseId,
                    Text = question.Text,
                    Type = question.Type,
                    OrderIndex = question.OrderIndex,
                    Media = new List<ReadQuestionMediaDTO>()
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                    Message = "Cập nhật câu hỏi thành công.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể cập nhật câu hỏi: " + ex.Message);
            }
        }





        public async Task<ResponseDTO> DeleteQuestionAsync(Guid id)
        {
            try
            {
                // --- Lấy Question kèm các navigation cần kiểm tra ---
                var question = await _questionRepository.GetFirstByExpression(
                    x => x.QuestionId == id,
                    x => x.QuestionMedias,
                    x => x.AssessmentDetails,
                   // x => x.LearnerAnswers,
                    x => x.PhonemeResults
                );

                if (question == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi để xoá.");

                // --- RÀNG BUỘC NGHIỆP VỤ: KHÔNG XOÁ NẾU CÒN MEDIA ---
                if (question.QuestionMedias?.Any() == true)
                    return Fail(BusinessCode.INVALID_ACTION,
                        "Không thể xoá câu hỏi vì vẫn còn QuestionMedia. Hãy xoá hoặc di chuyển các media trước.");

                // --- Xoá các entity con khác (có thể xoá an toàn) ---
                var db = _questionRepository.GetDbContext();

                if (question.AssessmentDetails?.Any() == true)
                    db.AssessmentDetails.RemoveRange(question.AssessmentDetails);

                //if (question.LearnerAnswers?.Any() == true)
                //    db.LearnerAnswers.RemoveRange(question.LearnerAnswers);

                if (question.PhonemeResults?.Any() == true)
                    db.PhonemeResults.RemoveRange(question.PhonemeResults);

                // --- Xoá Question chính ---
                db.Questions.Remove(question);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
                    Message = "Xoá câu hỏi thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể xoá câu hỏi: " + ex.Message);
            }
        }




        // ============================================================
        // 🔹 GET QUESTIONS BY EXERCISE ID (chuẩn 3-layer)
        // ============================================================
        public async Task<ResponseDTO> GetQuestionsByExerciseIdAsync(Guid exerciseId)
        {
            try
            {
                if (exerciseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ExerciseId không hợp lệ.");

                var exercise = await _exerciseRepository.GetById(exerciseId);
                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập.");

                var result = await _questionRepository.GetAllDataByExpression(
                    x => x.ExerciseId == exerciseId,
                    1,
                    int.MaxValue,
                    x => x.OrderIndex,
                    true,
                    x => x.QuestionMedias
                );

                var questions = result.Items.Select(q => new ReadQuestionDTO
                {
                    QuestionId = q.QuestionId,
                    ExerciseId = q.ExerciseId,
                    Text = q.Text,
                    Type = q.Type,
                    OrderIndex = q.OrderIndex,
                    Media = q.QuestionMedias?.Select(m => new ReadQuestionMediaDTO
                    {
                        QuestionMediaId = m.QuestionMediaId,
                        Accent = m.Accent,
                        AudioURL = m.AudioUrl,
                        VideoURL = m.VideoUrl,
                        ImageURL = m.ImageUrl,
                        Source = m.Source
                    }).ToList() ?? new List<ReadQuestionMediaDTO>()
                }).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = questions.Any()
                        ? "Lấy danh sách câu hỏi thành công."
                        : "Bài tập này chưa có câu hỏi nào.",
                    Data = questions
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể lấy danh sách câu hỏi: {ex.Message}");
            }
        }


    }
}
