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
                    query = query.Where(x => x.Status.Contains(keyword));

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
                        Duration = c.Duration,
                        Status = c.Status,
                        Description = c.Description,

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
                                                    .ThenInclude(q => q.QuestionMedias) 

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
                    Duration = course.Duration,
                    Status = course.Status,
                    Description = course.Description,
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
                                PhonemeJson = q.PhonemeJson,
                                QuestionMedia = q.QuestionMedias?.Select(m => new ReadQuestionMediaForCourseDTO
                                {
                                    QuestionMediaId = m.QuestionMediaId,
                                    Accent = m.Accent,
                                    AudioUrl = m.AudioUrl,
                                    VideoUrl = m.VideoUrl,
                                    ImageUrl = m.ImageUrl,
                                    Source = m.Source
                                }).ToList() ?? new List<ReadQuestionMediaForCourseDTO>()
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
                // ===== VALIDATION CƠ BẢN =====
                if (request == null)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Dữ liệu không hợp lệ.");

                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên khóa học không được để trống.");


                if (string.IsNullOrWhiteSpace(request.Description))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Mô tả khóa học (Description) không được để trống.");


                if (request.NumberOfChapter <= 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số lượng chương phải lớn hơn 0.");

               

                if (request.Price < 0)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Giá khóa học không thể âm.");

                // ✅ Thêm validate Duration (1–365 ngày)
                if (request.Duration <= 0 || request.Duration > 365)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Thời lượng học phải từ 1 đến 365 ngày.");


                // ✅ Thêm validate: nếu OrderIndex = 1 thì Price phải = 0
               

                // ===== CHECK TRÙNG TITLE TRONG CÙNG LEVEL =====
                var duplicateTitle = await _courseRepository.AsQueryable()
                    .AnyAsync(x => x.Title.ToLower() == request.Title.Trim().ToLower()
                                && x.Level == request.Level.ToString());

                if (duplicateTitle)
                    return Fail(BusinessCode.DUPLICATE_DATA,
                        $"Đã tồn tại khóa học '{request.Title}' ở cấp độ {request.Level}.");

                var maxOrder = await _courseRepository.AsQueryable()
      .Where(x => x.Level == request.Level.ToString())
      .Select(x => (int?)x.OrderIndex)
      .MaxAsync() ?? 0;

                int newOrderIndex = maxOrder + 1;

                // ✅ Rule: Course đầu tiên FREE
                if (newOrderIndex == 1 && request.Price > 0)
                    return Fail(BusinessCode.VALIDATION_FAILED,
                        "Khóa học đầu tiên trong level phải miễn phí.");

                // ===== CREATE COURSE =====
                var course = new Course
                {
                    CourseId = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    NumberOfChapter = request.NumberOfChapter,
                    OrderIndex = newOrderIndex,
                    Level = request.Level.ToString(),
                    Price = newOrderIndex == 1 ? 0 : request.Price,
                    Duration = request.Duration,
                    Status = string.IsNullOrEmpty(request.Status) ? "Active" : request.Status.Trim(),
                    // 🆕 thêm mới
                    Description = request.Description
                };

                await _courseRepository.Insert(course);
                await _unitOfWork.SaveChangeAsync();

                // ✅ Trả đủ dữ liệu (thêm Duration + Status)
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
                        course.Duration,
                        course.Status,
                        course.Description,

                        Chapters = new List<object>()
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể tạo khóa học: " + ex.Message);
            }
        }



        public async Task<ResponseDTO> UpdateCourseAsync(Guid id, UpdateSimpleCourseDTO request)
        {
            try
            {
                var course = await _courseRepository.GetById(id);
                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học để cập nhật.");

                // ===== VALIDATION CƠ BẢN =====
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Tên khóa học không được để trống.");
                if (!request.NumberOfChapter.HasValue)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Số lượng chương không được để trống.");
              
                if (!request.Level.HasValue)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Trình độ (Level) không được để trống.");

                // ✅ Validate Duration nếu có truyền
                if (request.Duration.HasValue && (request.Duration.Value <= 0 || request.Duration.Value > 365))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Thời lượng học phải từ 1 đến 365 ngày.");

                if (request.Description != null) // FE có truyền Description
                {
                    if (string.IsNullOrWhiteSpace(request.Description))
                        return Fail(BusinessCode.VALIDATION_FAILED, "Description không được để trống.");

                    course.Description = request.Description.Trim();
                }

                // ===== CẬP NHẬT =====
                course.Title = request.Title.Trim();
                course.NumberOfChapter = request.NumberOfChapter.Value;
                course.Level = request.Level.ToString();

                if (request.Price.HasValue)
                    course.Price = request.Price.Value;

                if (request.Duration.HasValue)
                    course.Duration = request.Duration.Value;

                if (!string.IsNullOrWhiteSpace(request.Status))
                    course.Status = request.Status.Trim();

                await _courseRepository.Update(course);
                await _unitOfWork.SaveChangeAsync();

                // ✅ Trả đủ dữ liệu (thêm Duration + Status)
                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                    Message = "Cập nhật khóa học thành công.",
                    Data = new
                    {
                        course.CourseId,
                        course.Title,
                        course.NumberOfChapter,
                        course.OrderIndex,
                        course.Level,
                        course.Price,
                        course.Duration,
                        course.Status,
                        course.Description,

                        Chapters = new List<object>()
                    }
                };
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
                var course = await _courseRepository.AsQueryable()
                    .Include(c => c.Chapters)
                        .ThenInclude(ch => ch.Exercises)
                            .ThenInclude(ex => ex.Questions)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học để xoá.");

                // ✅ Check learner đang học
                // ✅ Check học viên đang học dựa theo LearningPathCourse (khóa học còn active)
                var hasLearner = await _unitOfWork
                    .GetDbContext()
                    .Set<LearningPathCourse>()
                    .AnyAsync(lp =>
                        lp.CourseId == course.CourseId &&
                        lp.Status.ToLower() == "enrolled");


                if (hasLearner)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Không thể xoá khóa học '{course.Title}' vì đang có học viên đang học.");

                // ✅ Delete all children
                if (course.Chapters != null && course.Chapters.Any())
                {
                    foreach (var ch in course.Chapters)
                    {
                        if (ch.Exercises != null && ch.Exercises.Any())
                        {
                            foreach (var ex in ch.Exercises)
                            {
                                if (ex.Questions != null && ex.Questions.Any())
                                    await _questionRepository.DeleteRange(ex.Questions);
                            }

                            await _exerciseRepository.DeleteRange(ch.Exercises);
                        }
                    }

                    await _chapterRepository.DeleteRange(course.Chapters);
                }

                // ✅ Delete course
                await _courseRepository.Delete(course);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.DELETE_SUCESSFULLY,
                    Message = $"Đã xoá khóa học '{course.Title}' và toàn bộ dữ liệu liên quan thành công."
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Không thể xoá khóa học: " + ex.Message);
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
                    c.Price,
                    c.Duration,
                    c.Status,
                    c.Description

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
