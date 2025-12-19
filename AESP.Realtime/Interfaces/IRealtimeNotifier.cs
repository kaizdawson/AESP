using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Realtime.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task NotifyReviewItemUpdatedAsync(string itemType, Guid itemId,int remainingReviews);
    }
}
