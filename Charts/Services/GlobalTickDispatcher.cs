using System;
using thecalcify.Charts.Models;

namespace thecalcify.Charts.Services
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
