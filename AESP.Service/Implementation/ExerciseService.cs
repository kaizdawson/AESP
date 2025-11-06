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
    public class ExerciseService : IExerciseService
    {
        private readonly IGenericRepository<Exercise> _exerciseRepository;
        private readonly IGenericRepository<Chapter> _chapterRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ExerciseService(
            IGenericRepository<Exercise> exerciseRepository,
            IGenericRepository<Chapter> chapterRepository,
            IGenericRepository<Question> questionRepository,
            IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _chapterRepository = chapterRepository;
            _questionRepository = questionRepository;
            _unitOfWork = unitOfWork;
        }

        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };

        // ============================================================
        // 🔹 GET ALL
        // ============================================================
        public async Task<ResponseDTO> GetAllExercisesAsync(int pageNumber, int pageSize, Guid? chapterId = null, string? keyword = null)
        {
            try
            {
                var result = await _exerciseRepository.GetAllDataByExpression(
                    x => (!chapterId.HasValue || x.ChapterId == chapterId)
                      && (string.IsNullOrEmpty(keyword) || x.Title.Contains(keyword)),
                    pageNumber,
                    pageSize,
                    x => x.OrderIndex,
                    true,
                    x => x.Questions
                );

                var mapped = result.Items.Select(ex => new ReadExerciseDTO
                {
                    ExerciseId = ex.ExerciseId,
                    Title = ex.Title,
                    Description = ex.Description,
                    OrderIndex = ex.OrderIndex,
                    NumberOfQuestion = ex.NumberOfQuestion,
                    ChapterId = ex.ChapterId,
                    Questions = ex.Questions?.Select(q => new ReadExerciseQuestionDTO
                    {
                        QuestionId = q.QuestionId,
                        Text = q.Text,
                        Type = q.Type,
                        OrderIndex = q.OrderIndex,
                        PhonemeJson = q.PhonemeJson
                    }).ToList()
                }).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy danh sách bài tập thành công.",
                    Data = new PagedResult<ReadExerciseDTO>
                    {
                        Items = mapped,
                        TotalPages = result.TotalPages
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Lỗi khi lấy danh sách bài tập: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 GET BY ID
        // ============================================================
        public async Task<ResponseDTO> GetExerciseByIdAsync(Guid id)
        {
            try
            {
                var exercise = await _exerciseRepository.GetFirstByExpression(
                    x => x.ExerciseId == id,
                    x => x.Questions
                );

                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập.");

                var dto = new ReadExerciseDTO
                {
                    ExerciseId = exercise.ExerciseId,
                    Title = exercise.Title,
                    Description = exercise.Description,
                    OrderIndex = exercise.OrderIndex,
                    NumberOfQuestion = exercise.NumberOfQuestion,
                    ChapterId = exercise.ChapterId,
                    Questions = exercise.Questions?.Select(q => new ReadExerciseQuestionDTO
                    {
                        QuestionId = q.QuestionId,
                        Text = q.Text,
                        Type = q.Type,
                        OrderIndex = q.OrderIndex,
                        PhonemeJson = q.PhonemeJson
                    }).ToList()
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy bài tập thành công.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Lỗi khi lấy bài tập: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 CREATE
        // ============================================================
        public async Task<ResponseDTO> CreateExerciseAsync(CreateExerciseDTO request)
        {
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không hợp lệ.");
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên bài tập không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả bài tập không được để trống.");
                if (request.ChapterId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ChapterId không hợp lệ.");

                var chapter = await _chapterRepository.GetById(request.ChapterId);
                if (chapter == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương học.");

                var exercise = new Exercise
                {
                    ExerciseId = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    OrderIndex = request.OrderIndex,
                    NumberOfQuestion = request.NumberOfQuestion,
                    ChapterId = request.ChapterId
                };
                await _exerciseRepository.Insert(exercise);
                await _unitOfWork.SaveChangeAsync();

                // ✅ Thêm question nếu có
                if (request.Questions != null && request.Questions.Any())
                {
                    foreach (var q in request.Questions)
                    {
                        var question = new Question
                        {
                            QuestionId = Guid.NewGuid(),
                            Text = q.Text.Trim(),
                            Type = q.Type.Trim(),
                            OrderIndex = q.OrderIndex,
                            PhonemeJson = q.PhonemeJson.Trim(),
                            ExerciseId = exercise.ExerciseId
                        };
                        await _questionRepository.Insert(question);
                    }
                    await _unitOfWork.SaveChangeAsync();
                }

                // ✅ Load lại Exercise sau khi thêm
                var loaded = await _exerciseRepository.GetFirstByExpression(
                    x => x.ExerciseId == exercise.ExerciseId,
                    x => x.Questions
                );

                var dto = new ReadExerciseDTO
                {
                    ExerciseId = loaded.ExerciseId,
                    Title = loaded.Title,
                    Description = loaded.Description,
                    OrderIndex = loaded.OrderIndex,
                    NumberOfQuestion = loaded.NumberOfQuestion,
                    ChapterId = loaded.ChapterId,
                    Questions = loaded.Questions?.Select(q => new ReadExerciseQuestionDTO
                    {
                        QuestionId = q.QuestionId,
                        Text = q.Text,
                        Type = q.Type,
                        OrderIndex = q.OrderIndex,
                        PhonemeJson = q.PhonemeJson
                    }).ToList()
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo bài tập mới thành công.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể tạo bài tập: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 UPDATE
        // ============================================================
        public async Task<ResponseDTO> UpdateExerciseAsync(Guid id, UpdateExerciseDTO request)
        {
            try
            {
                var exercise = await _exerciseRepository.GetFirstByExpression(
                    x => x.ExerciseId == id,
                    x => x.Questions
                );
                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập.");

                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên bài tập không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả bài tập không được để trống.");

                exercise.Title = request.Title.Trim();
                exercise.Description = request.Description.Trim();
                exercise.OrderIndex = request.OrderIndex ?? exercise.OrderIndex;
                exercise.NumberOfQuestion = request.NumberOfQuestion ?? exercise.NumberOfQuestion;
                await _exerciseRepository.Update(exercise);
                await _unitOfWork.SaveChangeAsync();

                // ✅ Update question nếu có
                if (request.Questions != null && request.Questions.Any())
                {
                    foreach (var q in request.Questions)
                    {
                        var existing = exercise.Questions.FirstOrDefault(x => x.QuestionId == q.QuestionId);
                        if (existing == null) continue;

                        existing.Text = q.Text ?? existing.Text;
                        existing.Type = q.Type ?? existing.Type;
                        existing.OrderIndex = q.OrderIndex ?? existing.OrderIndex;
                        existing.PhonemeJson = q.PhonemeJson ?? existing.PhonemeJson;

                        await _questionRepository.Update(existing);
                    }
                    await _unitOfWork.SaveChangeAsync();
                }

                // ✅ Load lại sau khi update
                return await GetExerciseByIdAsync(id);
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể cập nhật bài tập: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 DELETE
        // ============================================================
        public async Task<ResponseDTO> DeleteExerciseAsync(Guid id)
        {
            try
            {
                var exercise = await _exerciseRepository.GetFirstByExpression(
                    x => x.ExerciseId == id,
                    x => x.Questions
                );

                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập để xoá.");

                // xoá question → exercise
                await _questionRepository.DeleteRange(exercise.Questions);
                await _exerciseRepository.Delete(exercise);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
                    Message = "Xoá bài tập thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể xoá bài tập: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 GET LIST BY CHAPTER ID
        // ============================================================
        public async Task<ResponseDTO> GetExercisesByChapterIdAsync(Guid chapterId)
        {
            try
            {
                // 🔹 1. Kiểm tra đầu vào
                if (chapterId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ChapterId không hợp lệ.");

                // 🔹 2. Kiểm tra chương có tồn tại không
                var chapter = await _chapterRepository.GetById(chapterId);
                if (chapter == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương học trong hệ thống.");

                // 🔹 3. Lấy toàn bộ bài tập thuộc chương (không phân trang)
                var query = _exerciseRepository.AsQueryable()
                    .Where(x => x.ChapterId == chapterId)
                    .Select(ex => new ReadExerciseDTO
                    {
                        ExerciseId = ex.ExerciseId,
                        Title = ex.Title,
                        Description = ex.Description,
                        OrderIndex = ex.OrderIndex,
                        NumberOfQuestion = ex.NumberOfQuestion,
                        ChapterId = ex.ChapterId,
                        Questions = ex.Questions.Select(q => new ReadExerciseQuestionDTO
                        {
                            QuestionId = q.QuestionId,
                            Text = q.Text,
                            Type = q.Type,
                            OrderIndex = q.OrderIndex,
                            PhonemeJson = q.PhonemeJson
                        }).ToList()
                    });

                var list = query.ToList();

                // 🔹 4. Trả về kết quả (kể cả khi rỗng)
                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy danh sách bài tập theo chương thành công.",
                    Data = list
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể lấy danh sách bài tập: {ex.Message}");
            }
        }


    }
}
