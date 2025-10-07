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
        Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize, string? type = null, string? keyword = null);
        Task<ResponseDTO> GetByIdAsync(Guid id);
        Task<ResponseDTO> CreateAsync(CreateQuestionAssessmentDTO dto);
        Task<ResponseDTO> UpdateAsync(Guid id, UpdateQuestionAssessmentDTO dto);
        Task<ResponseDTO> DeleteAsync(Guid id);
    }
}
