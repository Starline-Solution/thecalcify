using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thecalcify.Charts
{
    public static class GlobalTickDispatcher
    {
        public static event Action<Tick> TickReceived;

        public static void Publish(Tick t)
        {
            TickReceived?.Invoke(t);
        }
    }
}
