using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAssessmentService
    {
        Task<ResponseDTO> GetAllAssessmentsAsync(int pageNumber, int pageSize, Guid? learnerId = null, string? keyword = null);
        Task<ResponseDTO> GetAssessmentByIdAsync(Guid id);
        Task<ResponseDTO> CreateAssessmentAsync(CreateAssessmentDTO dto);
        Task<ResponseDTO> UpdateAssessmentAsync(Guid id, UpdateAssessmentDTO dto);
        Task<ResponseDTO> DeleteAssessmentAsync(Guid id);
        Task<ResponseDTO> GetPlacementTestForLearnerAsync(Guid userId);

        Task<ResponseDTO> SubmitPlacementTestCombinedAsync(CreatePlacementTestDTO dto);
        Task<ResponseDTO> GetAllAssessmentsAsync(int pageNumber, int pageSize);

    }
}
