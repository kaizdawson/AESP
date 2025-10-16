using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IQuestionService
    {
        Task<ResponseDTO> GetAllQuestionsAsync(int pageNumber, int pageSize, Guid? exerciseId = null);
        Task<ResponseDTO> GetQuestionByIdAsync(Guid id);
        Task<ResponseDTO> CreateQuestionAsync(CreateQuestionDTO request);
        Task<ResponseDTO> UpdateQuestionAsync(Guid id, UpdateQuestionDTO request);
        Task<ResponseDTO> DeleteQuestionAsync(Guid id);
    }
}
