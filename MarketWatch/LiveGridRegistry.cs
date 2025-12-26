using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thecalcify.MarketWatch
{
    public static class LiveGridRegistry
    {
        private static readonly List<ILiveMarketGrid> _grids = new List<ILiveMarketGrid>();

        public static void Register(ILiveMarketGrid grid)
        {
            if (!_grids.Contains(grid))
                _grids.Add(grid);
        }

        public static void Unregister(ILiveMarketGrid grid)
        {
            _grids.Remove(grid);
        }

        public static IEnumerable<ILiveMarketGrid> All => _grids;
    }

}
