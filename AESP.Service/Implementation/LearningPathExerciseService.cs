using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class LearningPathExerciseService : ILearningPathExerciseService
    {
        private readonly IGenericRepository<LearningPathExercise> _repo;
        private readonly IGenericRepository<Exercise> _exerciseRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LearningPathExerciseService(
            IGenericRepository<LearningPathExercise> repo,
            IGenericRepository<Exercise> exerciseRepo,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _exerciseRepo = exerciseRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> CreateByChapterAsync(Guid learningPathChapterId)
        {
            // placeholder: implement sau
            return new ResponseDTO
            {
                IsSucess = true,
                BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
                Message = "Tạo danh sách bài tập (LearningPathExercise) thành công (demo)."
            };
        }
    }
}
