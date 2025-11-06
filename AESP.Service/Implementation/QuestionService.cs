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
                    PhonemeJson = q.PhonemeJson,
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
                        PhonemeJson = question.PhonemeJson,
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

        // ---------------- CREATE ----------------
        public async Task<ResponseDTO> CreateQuestionsByExerciseIdAsync(Guid exerciseId, List<CreateQuestionDTO> requests)
        {
            try
            {
                // --- VALIDATION ---
                if (exerciseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ExerciseId không được để trống.");

                var exercise = await _exerciseRepository.GetById(exerciseId);
                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập tương ứng.");

                if (requests == null || !requests.Any())
                    return Fail(BusinessCode.VALIDATION_FAILED, "Danh sách câu hỏi không được để trống.");

                int index = 1;
                foreach (var req in requests)
                {
                    if (req == null)
                        return Fail(BusinessCode.VALIDATION_FAILED, $"Câu hỏi thứ {index} bị null.");

                    if (string.IsNullOrWhiteSpace(req.Text))
                        return Fail(BusinessCode.VALIDATION_FAILED, $"Câu hỏi thứ {index}: 'Text' không được để trống.");

                    if (!Enum.IsDefined(typeof(QuestionType), req.Type))
                        return Fail(BusinessCode.VALIDATION_FAILED, $"Câu hỏi thứ {index}: 'Type' không hợp lệ.");

                    if (req.OrderIndex < 0)
                        return Fail(BusinessCode.VALIDATION_FAILED, $"Câu hỏi thứ {index}: 'OrderIndex' phải ≥ 0.");

                    if (string.IsNullOrWhiteSpace(req.PhonemeJson))
                        return Fail(BusinessCode.VALIDATION_FAILED, $"Câu hỏi thứ {index}: 'PhonemeJson' không được để trống.");

                    if (req.Media != null && req.Media.Any())
                    {
                        int mediaIndex = 1;
                        foreach (var m in req.Media)
                        {
                            if (string.IsNullOrWhiteSpace(m.Accent))
                                return Fail(BusinessCode.VALIDATION_FAILED,
                                    $"Câu hỏi {index}, Media {mediaIndex}: 'Accent' không được để trống.");

                            if (string.IsNullOrWhiteSpace(m.AudioURL) &&
                                string.IsNullOrWhiteSpace(m.VideoURL) &&
                                string.IsNullOrWhiteSpace(m.ImageURL))
                                return Fail(BusinessCode.VALIDATION_FAILED,
                                    $"Câu hỏi {index}, Media {mediaIndex}: ít nhất 1 trong 3 URL (Audio, Video, Image) phải có giá trị.");

                            mediaIndex++;
                        }
                    }

                    index++;
                }

                // --- TẠO DANH SÁCH QUESTION + MEDIA ---
                var questions = new List<Question>();
                var medias = new List<QuestionMedia>();

                foreach (var req in requests)
                {
                    var question = new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        ExerciseId = exerciseId,
                        Text = req.Text.Trim(),
                        Type = req.Type.ToString(), // enum → string
                        OrderIndex = req.OrderIndex,
                        PhonemeJson = req.PhonemeJson.Trim()
                    };

                    questions.Add(question);

                    if (req.Media != null && req.Media.Any())
                    {
                        medias.AddRange(req.Media.Select(m => new QuestionMedia
                        {
                            QuestionMediaId = Guid.NewGuid(),
                            QuestionId = question.QuestionId,
                            Accent = m.Accent.Trim(),
                            AudioUrl = m.AudioURL ?? "",
                            VideoUrl = m.VideoURL ?? "",
                            ImageUrl = m.ImageURL ?? "",
                            Source = m.Source ?? ""
                        }));
                    }
                }

                await _questionRepository.InsertRange(questions);
                if (medias.Any())
                    await _mediaRepository.InsertRange(medias);
                await _unitOfWork.SaveChangeAsync();

                // --- LOAD LẠI QUESTIONS ---
                var context = _questionRepository.GetDbContext();
                var createdQuestions = await context.Questions
                    .Include(q => q.QuestionMedias)
                    .Where(q => q.ExerciseId == exerciseId)
                    .OrderBy(q => q.OrderIndex)
                    .ToListAsync();

                var dto = createdQuestions.Select(q => new ReadQuestionDTO
                {
                    QuestionId = q.QuestionId,
                    ExerciseId = q.ExerciseId,
                    Text = q.Text,
                    Type = q.Type,
                    OrderIndex = q.OrderIndex,
                    PhonemeJson = q.PhonemeJson,
                    Media = q.QuestionMedias?.Select(m => new ReadQuestionMediaDTO
                    {
                        QuestionMediaId = m.QuestionMediaId,
                        Accent = m.Accent,
                        AudioURL = m.AudioUrl,
                        VideoURL = m.VideoUrl,
                        ImageURL = m.ImageUrl,
                        Source = m.Source
                    }).ToList()
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
                var inner = ex.InnerException?.Message ?? "";
                return Fail(BusinessCode.EXCEPTION, $"Không thể tạo câu hỏi: {ex.Message} | {inner}");
            }
        }
        // ---------------- UPDATE ----------------
        public async Task<ResponseDTO> UpdateQuestionAsync(Guid id, UpdateQuestionDTO request)
        {
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");

                var question = await _questionRepository.GetFirstByExpression(
                    x => x.QuestionId == id,
                    x => x.QuestionMedias
                );

                if (question == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi cần cập nhật.");

                // --- VALIDATION FIELD ---
                if (request.Text != null && string.IsNullOrWhiteSpace(request.Text))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Text không được để trống.");

                if (request.Type.HasValue && !Enum.IsDefined(typeof(QuestionType), request.Type.Value))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Type không hợp lệ.");

                if (request.OrderIndex.HasValue && request.OrderIndex.Value < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "OrderIndex không hợp lệ.");

                // --- UPDATE MAIN ---
                if (!string.IsNullOrWhiteSpace(request.Text))
                    question.Text = request.Text.Trim();

                if (request.Type.HasValue)
                    question.Type = request.Type.Value.ToString();

                if (request.OrderIndex.HasValue)
                    question.OrderIndex = request.OrderIndex.Value;

                if (!string.IsNullOrWhiteSpace(request.PhonemeJson))
                    question.PhonemeJson = request.PhonemeJson.Trim();

                await _questionRepository.Update(question);
                await _unitOfWork.SaveChangeAsync();

                // --- UPDATE MEDIA ---
                if (request.Media != null && request.Media.Any())
                {
                    foreach (var m in request.Media)
                    {
                        var existingMedia = question.QuestionMedias.FirstOrDefault(x => x.QuestionMediaId == m.QuestionMediaId);
                        if (existingMedia == null)
                            return Fail(BusinessCode.DATA_NOT_FOUND, $"Không tìm thấy media ID {m.QuestionMediaId}");

                        existingMedia.Accent = m.Accent.Trim();
                        existingMedia.AudioUrl = m.AudioURL;
                        existingMedia.VideoUrl = m.VideoURL;
                        existingMedia.ImageUrl = m.ImageURL;
                        existingMedia.Source = m.Source;

                        await _mediaRepository.Update(existingMedia);
                    }
                    await _unitOfWork.SaveChangeAsync();
                }

                // --- LOAD LẠI FULL DATA ---
                var updated = await _questionRepository.GetFirstByExpression(
                    x => x.QuestionId == question.QuestionId,
                    x => x.QuestionMedias
                );

                var dto = new ReadQuestionDTO
                {
                    QuestionId = updated.QuestionId,
                    ExerciseId = updated.ExerciseId,
                    Text = updated.Text,
                    Type = updated.Type,
                    OrderIndex = updated.OrderIndex,
                    PhonemeJson = updated.PhonemeJson,
                    Media = updated.QuestionMedias?.Select(m => new ReadQuestionMediaDTO
                    {
                        QuestionMediaId = m.QuestionMediaId,
                        Accent = m.Accent,
                        AudioURL = m.AudioUrl,
                        VideoURL = m.VideoUrl,
                        ImageURL = m.ImageUrl,
                        Source = m.Source
                    }).ToList()
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
                var question = await _questionRepository.GetFirstByExpression(
                    x => x.QuestionId == id,
                    x => x.QuestionMedias,
                    x => x.AssessmentDetails,
                    x => x.LearnerAnswers,
                    x => x.PhonemeResults
                );

                if (question == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi để xoá.");

                // --- Xoá các entity con nếu có ---
                var db = _questionRepository.GetDbContext();

                if (question.QuestionMedias?.Any() == true)
                    db.QuestionMedias.RemoveRange(question.QuestionMedias);

                if (question.AssessmentDetails?.Any() == true)
                    db.AssessmentDetails.RemoveRange(question.AssessmentDetails);

                if (question.LearnerAnswers?.Any() == true)
                    db.LearnerAnswers.RemoveRange(question.LearnerAnswers);

                if (question.PhonemeResults?.Any() == true)
                    db.PhonemeResults.RemoveRange(question.PhonemeResults);

              

                // --- Xoá question chính ---
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
                    PhonemeJson = q.PhonemeJson,
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
