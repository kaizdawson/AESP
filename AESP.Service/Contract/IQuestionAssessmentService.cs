using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IQuestionAssessmentService
    {
        Task<ResponseDTO> GetAllQuestionAssessmentAsync(int pageNumber, int pageSize, string? type = null, string? keyword = null);
        Task<ResponseDTO> GetByQuestionAssessmentIdAsync(Guid id);
        Task<ResponseDTO> CreateQuestionAssessmentAsync(CreateQuestionAssessmentDTO dto);
        Task<ResponseDTO> UpdateQuestionAssessmentAsync(Guid id, UpdateQuestionAssessmentDTO dto);
        Task<ResponseDTO> DeleteQuestionAssessmentAsync(Guid id);
        Task<ResponseDTO> GetQuestionsByTypeAsync(string type);
        Task<ResponseDTO> ResetStatusByTypeAsync(string type);
        Task<ResponseDTO> GetActiveQuestionByIdAsync(Guid id);
        Task<ResponseDTO> GetActiveQuestionsByTypeAsync(string type);

        Task<ResponseDTO> SetQuestionStatusAsync(Guid questionId, bool status);

    }
}
