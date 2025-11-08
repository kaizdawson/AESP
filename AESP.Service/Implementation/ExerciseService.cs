using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ResponseDTO> CreateExerciseAsync(Guid chapterId, CreateExerciseDTO request)
        {
            try
            {
                // --- VALIDATION CƠ BẢN ---
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không hợp lệ.");
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên bài tập không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả bài tập không được để trống.");
                if (chapterId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ChapterId không hợp lệ.");
                if (request.NumberOfQuestion <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mỗi bài tập phải có ít nhất 1 câu hỏi.");
                if (request.OrderIndex <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Thứ tự bài tập (OrderIndex) phải lớn hơn 0.");

                // --- KIỂM TRA CHAPTER CÓ TỒN TẠI ---
                var chapter = await _chapterRepository.GetById(chapterId);
                if (chapter == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy chương học.");

                // --- RÀNG BUỘC: SỐ LƯỢNG EXERCISE KHÔNG VƯỢT QUÁ QUOTA ---
                var existingCount = await _exerciseRepository.AsQueryable()
                    .CountAsync(x => x.ChapterId == chapterId);

                if (existingCount >= chapter.NumberOfExercise)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Không thể tạo thêm bài tập. Chương '{chapter.Title}' chỉ cho phép {chapter.NumberOfExercise} bài tập.");

                // --- RÀNG BUỘC: TITLE KHÔNG TRÙNG TRONG CÙNG CHƯƠNG ---
                var duplicateTitle = await _exerciseRepository.AsQueryable()
                    .AnyAsync(x => x.ChapterId == chapterId &&
                                   x.Title.ToLower() == request.Title.Trim().ToLower());
                if (duplicateTitle)
                    return Fail(BusinessCode.DUPLICATE_DATA,
                        $"Đã tồn tại bài tập '{request.Title}' trong chương này.");

                // --- TẠO EXERCISE ---
                var exercise = new Exercise
                {
                    ExerciseId = Guid.NewGuid(),
                    ChapterId = chapterId,
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    OrderIndex = request.OrderIndex,
                    NumberOfQuestion = request.NumberOfQuestion
                };

                await _exerciseRepository.Insert(exercise);
                await _unitOfWork.SaveChangeAsync();

                // --- TRẢ KẾT QUẢ ---
                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo bài tập thành công.",
                    Data = new
                    {
                        exercise.ExerciseId,
                        exercise.ChapterId,
                        exercise.Title,
                        exercise.Description,
                        exercise.OrderIndex,
                        exercise.NumberOfQuestion,
                        Questions = new List<object>() // luôn rỗng
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể tạo bài tập: " + ex.Message);
            }
        }


        public async Task<ResponseDTO> UpdateExerciseAsync(Guid id, UpdateExerciseDTO request)
        {
            try
            {
                // --- LẤY DỮ LIỆU ---
                var exercise = await _exerciseRepository.GetById(id);
                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập để cập nhật.");

                // --- VALIDATION ---
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên bài tập không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả bài tập không được để trống.");
                if (request.NumberOfQuestion.HasValue && request.NumberOfQuestion.Value <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số lượng câu hỏi phải lớn hơn 0.");
                if (request.OrderIndex <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Thứ tự bài tập (OrderIndex) phải lớn hơn 0.");

                // --- RÀNG BUỘC: KHÔNG TRÙNG TITLE TRONG CÙNG CHƯƠNG ---
                var duplicateTitle = await _exerciseRepository.AsQueryable()
                    .AnyAsync(x => x.ChapterId == exercise.ChapterId &&
                                   x.ExerciseId != exercise.ExerciseId &&
                                   x.Title.ToLower() == request.Title.Trim().ToLower());
                if (duplicateTitle)
                    return Fail(BusinessCode.DUPLICATE_DATA,
                        $"Đã tồn tại bài tập '{request.Title}' trong chương này.");

                // --- CẬP NHẬT DỮ LIỆU ---
                exercise.Title = request.Title.Trim();
                exercise.Description = request.Description.Trim();
                if (request.OrderIndex.HasValue)
                    exercise.OrderIndex = request.OrderIndex.Value;
                if (request.NumberOfQuestion.HasValue)
                    exercise.NumberOfQuestion = request.NumberOfQuestion.Value;

                await _exerciseRepository.Update(exercise);
                await _unitOfWork.SaveChangeAsync();

                // --- TRẢ KẾT QUẢ ---
                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                    Message = "Cập nhật bài tập thành công.",
                    Data = new
                    {
                        exercise.ExerciseId,
                        exercise.ChapterId,
                        exercise.Title,
                        exercise.Description,
                        exercise.OrderIndex,
                        exercise.NumberOfQuestion,
                        Questions = new List<object>() // luôn rỗng
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể cập nhật bài tập: " + ex.Message);
            }
        }


        public async Task<ResponseDTO> DeleteExerciseAsync(Guid id)
        {
            try
            {
                var exercise = await _exerciseRepository.AsQueryable()
                    .Include(x => x.Questions)
                    .FirstOrDefaultAsync(x => x.ExerciseId == id);

                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập để xoá.");

                // ✅ RÀNG BUỘC: Không cho phép xóa nếu có Question
                if (exercise.Questions != null && exercise.Questions.Any())
                {
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Không thể xoá bài tập '{exercise.Title}' vì vẫn còn {exercise.Questions.Count} câu hỏi.");
                }

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
