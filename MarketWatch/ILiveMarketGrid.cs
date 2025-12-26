using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using thecalcify.Helper;

namespace thecalcify.MarketWatch
{
    public interface ILiveMarketGrid
    {
        bool IsReady { get; }
        bool TryApplyDto(MarketDataDto dto);
    }

}
