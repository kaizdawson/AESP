using AESP.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Contract
{
    public interface IAuthQueryRepository
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<LearnerProfile?> GetLearnerProfileAsync(Guid userId);
        Task<ReviewerProfile?> GetReviewerProfileAsync(Guid userId);
    }
}
