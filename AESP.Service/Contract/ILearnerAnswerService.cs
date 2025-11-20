using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ILearnerAnswerService
    {
        Task<ResponseDTO> SubmitAnswerAsync(Guid learnerProfileId, Guid learningPathQuestionId, SubmitLearnerAnswerDTO dto);
        Task<ResponseDTO> CheckAndUpgradeLevelAsync(Guid learnerProfileId);

    }
}
