using thecalcify.Helper;
using thecalcify.Shared;

namespace thecalcify.RTDWorker
{
    public static class TickUIConverter
    {
        public static MarketDataDto ToUiDto(TickBinary t)
        {
            return new MarketDataDto
            {
                i = t.Symbol,
                b = t.B.ToString(),
                a = t.A.ToString(),
                ltp = t.Ltp.ToString(),
                h = t.H.ToString(),
                l = t.L.ToString(),
                o = t.O.ToString(),
                c = t.C.ToString(),
                d = t.D.ToString(),

                v = t.v.ToString(),
                t = t.T.ToString(),

                bq = t.Bq.ToString(),
                tbq = t.Tbq.ToString(),
                sq = t.Sq.ToString(),
                tsq = t.Tsq.ToString(),

                vt = t.Vt.ToString(),
                oi = t.Oi.ToString(),
                ltq = t.Ltq.ToString(),

                n = t.Symbol  // or real name if you want to store it
            };
        }
    }

}
