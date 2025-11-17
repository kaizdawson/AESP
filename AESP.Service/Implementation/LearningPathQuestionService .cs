using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;

namespace AESP.Service.Implementation
{
    public class LearningPathQuestionService : ILearningPathQuestionService
    {
        private readonly IGenericRepository<LearningPathQuestion> _lpQuestionRepo;

        public LearningPathQuestionService(
            IGenericRepository<LearningPathQuestion> lpQuestionRepo
        )
        {
            _lpQuestionRepo = lpQuestionRepo;
        }

        public async Task<ResponseDTO> GetAllNotStartedAsync()
        {
            try
            {
                var data = await _lpQuestionRepo.AsQueryable()
                    .Where(q => q.Status == "NotStarted")
                    .OrderBy(q => q.LearningPathExerciseId)
                    .ToListAsync();

                if (!data.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucess = false,
                        BusinessCode = BusinessCode.DATA_NOT_FOUND,
                        Message = "Không có câu hỏi nào ở trạng thái NotStarted."
                    };
                }

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy danh sách câu hỏi NotStarted thành công.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = BusinessCode.EXCEPTION,
                    Message = "Lỗi khi lấy dữ liệu: " + ex.Message
                };
            }
        }

    }
}
