using AESP.Repository.Contract;
using AESP.Repository.DB;
using AESP.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Repositories
{
    public class AuthQueryRepository : IAuthQueryRepository
    {
        private readonly AppDbContext _context;

        public AuthQueryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<LearnerProfile?> GetLearnerProfileAsync(Guid userId)
        {
            return await _context.LearnerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(lp => lp.UserId == userId);
        }

        public async Task<ReviewerProfile?> GetReviewerProfileAsync(Guid userId)
        {
            return await _context.ReviewerProfiles
                .AsNoTracking()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);
        }
    }
}
