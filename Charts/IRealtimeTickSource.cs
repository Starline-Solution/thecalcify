using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thecalcify.Charts
{
    public interface IRealtimeTickSource
    {
        event Action<Tick> TickReceived;
        void Subscribe(string symbol);
        void Unsubscribe(string symbol);
    }

}
