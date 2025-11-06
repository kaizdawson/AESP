using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AESP.Service.Implementation
{
    public class CourseService : ICourseService
    {
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Chapter> _chapterRepository;
        private readonly IGenericRepository<Exercise> _exerciseRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(
            IGenericRepository<Course> courseRepository,
            IGenericRepository<Chapter> chapterRepository,
            IGenericRepository<Exercise> exerciseRepository,
            IGenericRepository<Question> questionRepository,
            IUnitOfWork unitOfWork)
        {
            _courseRepository = courseRepository;
            _chapterRepository = chapterRepository;
            _exerciseRepository = exerciseRepository;
            _questionRepository = questionRepository;
            _unitOfWork = unitOfWork;
        }

        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };

        // ============================================================
        // 🔹 GET ALL COURSE
        // ============================================================
        public async Task<ResponseDTO> GetAllCourseAsync(int pageNumber, int pageSize, string? level = null, string? keyword = null)
        {
            try
            {
                var query = _courseRepository.AsQueryable();

                if (!string.IsNullOrEmpty(level))
                    query = query.Where(x => x.Level == level);
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(x => x.Title.Contains(keyword));

                query = query
                    .Include(x => x.Chapters)
                        .ThenInclude(ch => ch.Exercises)
                            .ThenInclude(ex => ex.Questions)
                    .OrderBy(x => x.OrderIndex);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var courses = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var allCourses = await _courseRepository.AsQueryable().ToListAsync();

                var mapped = courses.Select(c =>
                {
                    bool isFree = IsCourseFree(c, allCourses);

                    return new ReadCourseFullDTO
                    {
                        CourseId = c.CourseId,
                        Title = c.Title,
                        NumberOfChapter = c.NumberOfChapter,
                        OrderIndex = c.OrderIndex,
                        Level = c.Level,
                        Price = c.Price,
                        IsFree = isFree,
                        Chapters = c.Chapters?.Select(ch => new ReadCourseChapterForCourseDTO
                        {
                            ChapterId = ch.ChapterId,
                            Title = ch.Title,
                            Description = ch.Description,
                            CreatedAt = ch.CreatedAt,
                            NumberOfExercise = ch.NumberOfExercise,
                            Exercises = ch.Exercises?.Select(ex => new ReadCourseExerciseForCourseDTO
                            {
                                ExerciseId = ex.ExerciseId,
                                Title = ex.Title,
                                Description = ex.Description,
                                OrderIndex = ex.OrderIndex,
                                NumberOfQuestion = ex.NumberOfQuestion,
                                IsFree = isFree, // dùng cùng logic với course
                                Questions = ex.Questions?.Select(q => new ReadCourseQuestionForCourseDTO
                                {
                                    QuestionId = q.QuestionId,
                                    Text = q.Text,
                                    Type = q.Type,
                                    OrderIndex = q.OrderIndex,
                                    PhonemeJson = q.PhonemeJson
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    };
                }).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy danh sách khóa học đầy đủ thành công.",
                    Data = new PagedResult<ReadCourseFullDTO>
                    {
                        Items = mapped,
                        TotalPages = totalPages
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy danh sách khóa học đầy đủ: " + ex.Message);
            }
        }

        // ============================================================
        // 🔹 GET FULL COURSE BY ID (LOAD 3 TẦNG)
        // ============================================================
        public async Task<ResponseDTO> GetFullCourseByIdAsync(Guid id)
        {
            try
            {
                var allCourses = await _courseRepository.AsQueryable().ToListAsync();

                var course = await _courseRepository.AsQueryable()
                     .AsNoTracking()
                    .Include(c => c.Chapters)
                        .ThenInclude(ch => ch.Exercises)
                            .ThenInclude(ex => ex.Questions)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

                bool isFree = IsCourseFree(course, allCourses);

                var result = new ReadCourseFullDTO
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    NumberOfChapter = course.NumberOfChapter,
                    OrderIndex = course.OrderIndex,
                    Level = course.Level,
                    Price = course.Price,
                    IsFree = isFree,
                    Chapters = course.Chapters.Select(ch => new ReadCourseChapterForCourseDTO
                    {
                        ChapterId = ch.ChapterId,
                        Title = ch.Title,
                        Description = ch.Description,
                        CreatedAt = ch.CreatedAt,
                        NumberOfExercise = ch.NumberOfExercise,
                        Exercises = ch.Exercises?.Select(ex => new ReadCourseExerciseForCourseDTO
                        {
                            ExerciseId = ex.ExerciseId,
                            Title = ex.Title,
                            Description = ex.Description,
                            OrderIndex = ex.OrderIndex,
                            NumberOfQuestion = ex.NumberOfQuestion,
                            IsFree = isFree,
                            Questions = ex.Questions?.Select(q => new ReadCourseQuestionForCourseDTO
                            {
                                QuestionId = q.QuestionId,
                                Text = q.Text,
                                Type = q.Type,
                                OrderIndex = q.OrderIndex,
                                PhonemeJson = q.PhonemeJson
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy khóa học đầy đủ thành công.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy khóa học đầy đủ: " + ex.Message);
            }
        }

        // ============================================================
        // 🔹 CREATE (COURSE + CHAPTER + EXERCISE + QUESTION)
        // ============================================================
        //public async Task<ResponseDTO> CreateFullCourseAsync(CreateCourseFullDTO request)
        //{
        //    try
        //    {
        //        if (request == null)
        //            return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không hợp lệ.");
        //        if (string.IsNullOrWhiteSpace(request.Title))
        //            return Fail(BusinessCode.VALIDATION_FAILED, "Tên khóa học không được trống.");
        //        if (string.IsNullOrWhiteSpace(request.Type))
        //            return Fail(BusinessCode.VALIDATION_FAILED, "Loại khóa học không được trống.");
        //        if (request.Chapters == null || !request.Chapters.Any())
        //            return Fail(BusinessCode.VALIDATION_FAILED, "Phải có ít nhất 1 chương.");

        //        // 🔹 1. Tạo Course
        //        var course = new Course
        //        {
        //            CourseId = Guid.NewGuid(),
        //            Title = request.Title.Trim(),
        //            Type = request.Type.Trim(),
        //            NumberOfChapter = request.NumberOfChapter,
        //            OrderIndex = request.OrderIndex,
        //            Level = request.Level.ToString()
        //        };
        //        await _courseRepository.Insert(course);

        //        // 🔹 2. Tạo Chapter
        //        var chapters = request.Chapters.Select(ch => new Chapter
        //        {
        //            ChapterId = Guid.NewGuid(),
        //            CourseId = course.CourseId,
        //            Title = ch.Title.Trim(),
        //            Description = ch.Description.Trim(),
        //            NumberOfExercise = ch.NumberOfExercise,
        //            CreatedAt = DateTime.UtcNow
        //        }).ToList();

        //        await _chapterRepository.InsertRange(chapters);
        //        await _unitOfWork.SaveChangeAsync();

        //        // 🔹 3. Tự tạo Exercise & Question trống theo mỗi Chapter
        //        var exercisesToInsert = new List<Exercise>();
        //        var questionsToInsert = new List<Question>();

        //        foreach (var chapter in chapters)
        //        {
        //            // tạo exercise rỗng dựa theo NumberOfExercise
        //            var exercises = Enumerable.Range(1, chapter.NumberOfExercise).Select(i => new Exercise
        //            {
        //                ExerciseId = Guid.NewGuid(),
        //                ChapterId = chapter.ChapterId,
        //                Title = $"Exercise {i}",
        //                Description = "Auto generated exercise",
        //                OrderIndex = i,
        //                NumberOfQuestion = 2 // ví dụ mỗi exercise có sẵn 2 câu hỏi trống
        //            }).ToList();

        //            exercisesToInsert.AddRange(exercises);

        //            // tạo question trống cho mỗi exercise
        //            foreach (var ex in exercises)
        //            {
        //                var questions = new List<Question>
        //        {
        //            new Question
        //            {
        //                QuestionId = Guid.NewGuid(),
        //                ExerciseId = ex.ExerciseId,
        //                Text = "Sample question 1",
        //                Type = "text",
        //                OrderIndex = 1,
        //                PhonemeJson = ""
        //            },
        //            new Question
        //            {
        //                QuestionId = Guid.NewGuid(),
        //                ExerciseId = ex.ExerciseId,
        //                Text = "Sample question 2",
        //                Type = "text",
        //                OrderIndex = 2,
        //                PhonemeJson = ""
        //            }
        //        };
        //                questionsToInsert.AddRange(questions);
        //            }
        //        }

        //        if (exercisesToInsert.Any())
        //            await _exerciseRepository.InsertRange(exercisesToInsert);
        //        if (questionsToInsert.Any())
        //            await _questionRepository.InsertRange(questionsToInsert);

        //        await _unitOfWork.SaveChangeAsync();

        //        // 🔹 4. Load lại full data 3 tầng
        //        var result = await GetFullCourseByIdAsync(course.CourseId);
        //        result.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
        //        result.Message = "Tạo khóa học đầy đủ thành công.";
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail(BusinessCode.EXCEPTION, "Không thể tạo khóa học: " + ex.Message);
        //    }
        //}




        public async Task<ResponseDTO> CreateFullCourseAsync(CreateCourseFullDTO request)
        {
            try
            {
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không hợp lệ.");
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên khóa học không được trống.");
               

                // 🔹 1. Tạo Course (chưa có Chapter nào)
                var course = new Course
                {
                    CourseId = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    NumberOfChapter = request.NumberOfChapter,
                    OrderIndex = request.OrderIndex,
                    Level = request.Level.ToString(),
                    Price = request.Price
                };

                await _courseRepository.Insert(course);
                await _unitOfWork.SaveChangeAsync();

                // ✅ Không auto tạo chapter / exercise / question

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                    Message = "Tạo khóa học thành công (chưa có chương).",
                    Data = new
                    {
                        course.CourseId,
                        course.Title,
                        course.NumberOfChapter,
                        course.OrderIndex,
                        course.Level,
                        course.Price,
                        Chapters = new List<object>() // ✅ luôn trả về mảng trống []
                    }
                };

            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể tạo khóa học: " + ex.Message);
            }
        }



        // ============================================================
        // 🔹 UPDATE (COURSE + CHAPTER + AUTO SYNC EXERCISE + QUESTION)
        // ============================================================
        public async Task<ResponseDTO> UpdateFullCourseAsync(Guid id, UpdateCourseFullDTO request)
        {
            try
            {
                var course = await _courseRepository.AsQueryable()
                    .Include(c => c.Chapters)
                        .ThenInclude(ch => ch.Exercises)
                            .ThenInclude(ex => ex.Questions)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học để cập nhật.");

                // --- CẬP NHẬT COURSE CƠ BẢN ---
                if (!string.IsNullOrWhiteSpace(request.Title)) course.Title = request.Title.Trim();
                if (request.NumberOfChapter.HasValue) course.NumberOfChapter = request.NumberOfChapter.Value;
                if (request.OrderIndex.HasValue) course.OrderIndex = request.OrderIndex.Value;
                if (request.Level.HasValue) course.Level = request.Level.ToString();

                await _courseRepository.Update(course);

                // --- CẬP NHẬT CHAPTER + EXERCISE + QUESTION ---
                if (request.Chapters != null && request.Chapters.Any())
                {
                    foreach (var chDto in request.Chapters)
                    {
                        var chapter = course.Chapters.FirstOrDefault(x => x.ChapterId == chDto.ChapterId);

                        // ❌ Nếu không có chapterId hợp lệ -> fail luôn
                        if (chapter == null)
                            return Fail(BusinessCode.DATA_NOT_FOUND, $"Không tìm thấy chương (ID: {chDto.ChapterId}) để cập nhật.");


                        if (!string.IsNullOrWhiteSpace(chDto.Title)) chapter.Title = chDto.Title.Trim();
                        if (!string.IsNullOrWhiteSpace(chDto.Description)) chapter.Description = chDto.Description.Trim();
                        if (chDto.NumberOfExercise.HasValue)
                        {
                            int newExerciseCount = chDto.NumberOfExercise.Value;
                            int currentExerciseCount = chapter.Exercises.Count;

                            // --- Nếu chapter có ít hơn số lượng mới => thêm Exercise mới ---
                            if (newExerciseCount > currentExerciseCount)
                            {
                                var toAdd = Enumerable.Range(currentExerciseCount + 1, newExerciseCount - currentExerciseCount)
                                    .Select(i => new Exercise
                                    {
                                        ExerciseId = Guid.NewGuid(),
                                        ChapterId = chapter.ChapterId,
                                        Title = $"Exercise {i}",
                                        Description = "Auto generated exercise (update)",
                                        OrderIndex = i,
                                        NumberOfQuestion = 2
                                    }).ToList();

                                await _exerciseRepository.InsertRange(toAdd);

                                // Thêm question mặc định cho exercise mới
                                var newQuestions = new List<Question>();
                                foreach (var ex in toAdd)
                                {
                                    newQuestions.AddRange(new[]
                                    {
                                new Question
                                {
                                    QuestionId = Guid.NewGuid(),
                                    ExerciseId = ex.ExerciseId,
                                    Text = "Sample question 1",
                                    Type = "text",
                                    OrderIndex = 1,
                                    PhonemeJson = ""
                                },
                                new Question
                                {
                                    QuestionId = Guid.NewGuid(),
                                    ExerciseId = ex.ExerciseId,
                                    Text = "Sample question 2",
                                    Type = "text",
                                    OrderIndex = 2,
                                    PhonemeJson = ""
                                }
                            });
                                }
                                await _questionRepository.InsertRange(newQuestions);
                            }
                            // --- Nếu chapter có nhiều hơn số lượng mới => xoá bớt ---
                            else if (newExerciseCount < currentExerciseCount)
                            {
                                var toRemove = chapter.Exercises
                                    .OrderByDescending(e => e.OrderIndex)
                                    .Take(currentExerciseCount - newExerciseCount)
                                    .ToList();

                                var removeQuestions = toRemove.SelectMany(e => e.Questions).ToList();

                                await _questionRepository.DeleteRange(removeQuestions);
                                await _exerciseRepository.DeleteRange(toRemove);
                            }

                            chapter.NumberOfExercise = newExerciseCount;
                            await _chapterRepository.Update(chapter);
                        }
                    }
                }

                await _unitOfWork.SaveChangeAsync();

                // --- LOAD LẠI FULL DỮ LIỆU ---
                var result = await GetFullCourseByIdAsync(course.CourseId);
                result.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                result.Message = "Cập nhật khóa học đầy đủ thành công.";
                return result;
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể cập nhật khóa học: " + ex.Message);
            }
        }



        // ============================================================
        // 🔹 DELETE FULL COURSE
        // ============================================================
        public async Task<ResponseDTO> DeleteFullCourseAsync(Guid id)
        {
            try
            {
                // ✅ Load đủ 3 tầng bằng Include / ThenInclude
                var course = await _courseRepository.AsQueryable()
                    .Include(c => c.Chapters)
                        .ThenInclude(ch => ch.Exercises)
                            .ThenInclude(ex => ex.Questions)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học để xoá.");

                // ✅ Lấy danh sách tất cả question
                var questions = course.Chapters
                    .SelectMany(c => c.Exercises)
                    .SelectMany(e => e.Questions)
                    .ToList();

                if (questions.Any())
                    await _questionRepository.DeleteRange(questions);

                // ✅ Lấy danh sách exercise
                var exercises = course.Chapters
                    .SelectMany(c => c.Exercises)
                    .ToList();

                if (exercises.Any())
                    await _exerciseRepository.DeleteRange(exercises);

                // ✅ Lấy danh sách chapter
                var chapters = course.Chapters.ToList();

                if (chapters.Any())
                    await _chapterRepository.DeleteRange(chapters);

                // ✅ Cuối cùng xóa course
                await _courseRepository.Delete(course);

                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
                    Message = "Xoá khóa học đầy đủ thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể xoá khóa học đầy đủ: " + ex.Message);
            }
        }



        // ============================================================
        // 🔹 GET COURSE BY LEVEL (CHỈ TRẢ COURSE, KHÔNG TRẢ 3 TẦNG)
        // ============================================================
        public async Task<ResponseDTO> GetCoursesByLevelAsync(string level)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(level))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Level không được để trống.");

                var query = _courseRepository.AsQueryable()
                    .Where(c => c.Level == level)
                    .OrderBy(c => c.OrderIndex);

                var courses = await query.ToListAsync();

                if (!courses.Any())
                    return Fail(BusinessCode.DATA_NOT_FOUND, $"Không tìm thấy khóa học thuộc level {level}.");

                var mapped = courses.Select(c => new
                {
                    c.CourseId,
                    c.Title,
                    c.NumberOfChapter,
                    c.OrderIndex,
                    c.Level,
                    c.Price
                }).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = $"Lấy danh sách khóa học level {level} thành công.",
                    Data = mapped
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy khóa học theo level: " + ex.Message);
            }
        }


        private bool IsCourseFree(Course course, IEnumerable<Course> allCourses)
        {
            // Lấy danh sách tất cả course cùng level
            var levelCourses = allCourses
                .Where(c => c.Level == course.Level)
                .OrderBy(c => c.OrderIndex)
                .ToList();

            // ✅ Khóa đầu tiên (OrderIndex = 1) → free
            return course.OrderIndex == 1;
        }



    }
}
