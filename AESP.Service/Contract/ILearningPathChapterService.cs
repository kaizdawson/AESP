using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ILearningPathChapterService
    {
        Task<ResponseDTO> GetAllByLearningPathCourseIdAsync(Guid learningPathCourseId);
        Task<ResponseDTO> GetByIdAsync(Guid learningPathChapterId);
        Task<ResponseDTO> CreateByCourseAsync(Guid learningPathCourseId, Guid learnerCourseId);
        Task<ResponseDTO> UpdateProgressAsync(Guid learnerProfileId, Guid learningPathChapterId, double progress);
    }
}
