using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ILearningPathCourseService
    {
        Task<ResponseDTO> GetAllAsync(Guid? learnerCourseId = null);
        Task<ResponseDTO> GetByIdAsync(Guid id);
        Task<ResponseDTO> CreateAsync(CreateLearningPathCourseDTO dto);
        Task<ResponseDTO> UpdateAsync(Guid id, UpdateLearningPathCourseDTO dto);
    }
}
