using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AESP.Realtime.Hubs
{
    public class ReviewerHub : Hub
    {
        public async Task JoinReviewerGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Reviewers");
        }

        public async Task LeaveReviewerGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Reviewers");
        }
    }
}
