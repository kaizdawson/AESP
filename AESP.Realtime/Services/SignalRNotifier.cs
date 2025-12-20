using AESP.Realtime.Hubs;
using AESP.Realtime.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Realtime.Services
{
    public class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<ReviewerHub> _hubContext;

        public SignalRNotifier(IHubContext<ReviewerHub> hubContext)
        {
            _hubContext = hubContext;
        }

        

        public async Task NotifyReviewItemUpdatedAsync(string itemType, Guid itemId, int remainingReviews)
        {
            await _hubContext.Clients.Group("Reviewers").SendAsync("reviewItemUpdated", new
            {
                itemType,
                itemId,
                remainingReviews
            });
        }
    }
}
