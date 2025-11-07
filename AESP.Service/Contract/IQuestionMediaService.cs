using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IQuestionMediaService
    {
        Task<ResponseDTO> GetAllQuestionMediasAsync(int pageNumber, int pageSize, Guid? questionId = null);
        Task<ResponseDTO> GetQuestionMediaByIdAsync(Guid id);
        Task<ResponseDTO> CreateQuestionMediaAsync(Guid questionId, CreateQuestionMediaV2DTO request);
        Task<ResponseDTO> UpdateQuestionMediaAsync(Guid id, UpdateQuestionMediaV2DTO request);
        Task<ResponseDTO> DeleteQuestionMediaAsync(Guid id);
        Task<ResponseDTO> GetQuestionMediasByQuestionIdAsync(Guid questionId);
    }
}
