using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Realtime.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task NotifyReviewCompletedAsync(Guid learnerAnswerId, int remaining);
    }
}
