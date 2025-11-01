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
                    IPA = q.IPA,
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
                        IPA = question.IPA,
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
        public async Task<ResponseDTO> CreateQuestionAsync(CreateQuestionDTO request)
        {
            try
            {
                // --- VALIDATION ---
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu đầu vào không được để trống.");

                if (request.ExerciseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ExerciseId không được để trống.");

                if (string.IsNullOrWhiteSpace(request.Text))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Text không được để trống.");

                if (string.IsNullOrWhiteSpace(request.Type))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Type không được để trống.");

                if (string.IsNullOrWhiteSpace(request.IPA))
                    return Fail(BusinessCode.VALIDATION_FAILED, "IPA không được để trống.");

                if (string.IsNullOrWhiteSpace(request.PhonemeJson))
                    return Fail(BusinessCode.VALIDATION_FAILED, "PhonemeJson không được để trống.");

                if (request.OrderIndex < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "OrderIndex không hợp lệ.");

                // Check exercise tồn tại
                var exercise = await _exerciseRepository.GetById(request.ExerciseId);
                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập tương ứng.");

                // --- CREATE ENTITY ---
                var newQuestion = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    ExerciseId = request.ExerciseId,
                    Text = request.Text.Trim(),
                    Type = request.Type.Trim(),
                    OrderIndex = request.OrderIndex,
                    IPA = request.IPA?.Trim() ?? "",
                    PhonemeJson = request.PhonemeJson?.Trim() ?? ""
                };

                await _questionRepository.Insert(newQuestion);
                await _unitOfWork.SaveChangeAsync();

                // --- CREATE MEDIA (nếu có) ---
                if (request.Media != null && request.Media.Any())
                {
                    var medias = request.Media.Select(m => new QuestionMedia
                    {
                        QuestionMediaId = Guid.NewGuid(),
                        QuestionId = newQuestion.QuestionId,
                        Accent = m.Accent.Trim(),
                        AudioUrl = m.AudioURL,
                        VideoUrl = m.VideoURL,
                        ImageUrl = m.ImageURL,
                        Source = m.Source
                    }).ToList();

                    await _mediaRepository.InsertRange(medias);
                    await _unitOfWork.SaveChangeAsync();
                }

                // --- LOAD LẠI FULL QUESTION + MEDIA ---
                var fullQuestion = await _questionRepository.GetFirstByExpression(
                    x => x.QuestionId == newQuestion.QuestionId,
                    x => x.QuestionMedias
                );

                if (fullQuestion == null)
                    return Fail(BusinessCode.EXCEPTION, "Không thể load lại dữ liệu sau khi tạo.");

                // --- MAP RA DTO ---
                var dto = new ReadQuestionDTO
                {
                    QuestionId = fullQuestion.QuestionId,
                    ExerciseId = fullQuestion.ExerciseId,
                    Text = fullQuestion.Text,
                    Type = fullQuestion.Type,
                    OrderIndex = fullQuestion.OrderIndex,
                    IPA = fullQuestion.IPA,
                    PhonemeJson = fullQuestion.PhonemeJson,
                    Media = fullQuestion.QuestionMedias?.Select(m => new ReadQuestionMediaDTO
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
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo câu hỏi thành công.",
                    Data = dto // ✅ trả full DTO bạn đã map ở trên
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể tạo câu hỏi: " + ex.Message);
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

                // --- VALIDATION FIELD THEO THỨ TỰ ---
                if (request.Text != null && string.IsNullOrWhiteSpace(request.Text))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Text không được để trống.");

                if (request.Type != null && string.IsNullOrWhiteSpace(request.Type))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Type không được để trống.");

                if (string.IsNullOrWhiteSpace(request.IPA))
                    return Fail(BusinessCode.VALIDATION_FAILED, "IPA không được để trống.");

                if (string.IsNullOrWhiteSpace(request.PhonemeJson))
                    return Fail(BusinessCode.VALIDATION_FAILED, "PhonemeJson không được để trống.");


                if (request.OrderIndex.HasValue && request.OrderIndex.Value < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "OrderIndex không hợp lệ.");

                // --- UPDATE MAIN FIELDS ---
                if (!string.IsNullOrWhiteSpace(request.Text))
                    question.Text = request.Text.Trim();
                if (!string.IsNullOrWhiteSpace(request.Type))
                    question.Type = request.Type.Trim();
                if (request.OrderIndex.HasValue)
                    question.OrderIndex = request.OrderIndex.Value;
                if (!string.IsNullOrWhiteSpace(request.IPA))
                    question.IPA = request.IPA.Trim();
                if (!string.IsNullOrWhiteSpace(request.PhonemeJson))
                    question.PhonemeJson = request.PhonemeJson.Trim();

                await _questionRepository.Update(question);
                await _unitOfWork.SaveChangeAsync();

                // --- UPDATE MEDIA ---
                if (request.Media != null && request.Media.Any())
                {
                    foreach (var m in request.Media)
                    {
                        if (m.QuestionMediaId == Guid.Empty)
                            return Fail(BusinessCode.VALIDATION_FAILED, "QuestionMediaId không được để trống.");

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

                // --- LOAD LẠI FULL DATA SAU UPDATE ---
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
                    IPA = updated.IPA,
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
                    Message = "Tạo câu hỏi thành công.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể cập nhật câu hỏi: " + ex.Message);
            }
        }

        // ---------------- DELETE ----------------
        //public async Task<ResponseDTO> DeleteQuestionAsync(Guid id)
        //{
        //    try
        //    {
        //        var question = await _questionRepository.GetById(id);
        //        if (question == null)
        //            return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi để xoá.");

        //        await _questionRepository.Delete(question);
        //        await _unitOfWork.SaveChangeAsync();

        //        return new ResponseDTO
        //        {
        //            IsSucess = true,
        //            BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
        //            Message = "Xoá câu hỏi thành công."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail(BusinessCode.EXCEPTION, "Không thể xoá câu hỏi: " + ex.Message);
        //    }
        //}


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

    }
}
