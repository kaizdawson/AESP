using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ILearnerQuestionService
    {
        Task<ResponseDTO> GetQuestionsByExerciseIdForLearnerAsync(Guid learnerProfileId, Guid exerciseId);
    }
}
