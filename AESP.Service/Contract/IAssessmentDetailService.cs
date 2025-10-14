using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAssessmentDetailService
    {
        Task<ResponseDTO> GetAllAssessmentDetailsAsync(int pageNumber, int pageSize, Guid? assessmentId = null);
        Task<ResponseDTO> GetAssessmentDetailByIdAsync(Guid id);
        Task<ResponseDTO> CreateAssessmentDetailAsync(CreateAssessmentDetailDTO dto);
        Task<ResponseDTO> UpdateAssessmentDetailAsync(Guid id, UpdateAssessmentDetailDTO dto);
        Task<ResponseDTO> DeleteAssessmentDetailAsync(Guid id);
    }
}
