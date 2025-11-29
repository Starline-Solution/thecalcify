namespace thecalcify.Shared
{
    public static class TickConverter
    {
        public static TickBinary ToBinary(MarketDataDtoFast d)
        {
            return new TickBinary
            {
                Symbol = d.i ?? "",
                n = d.n ?? "",

                B = d.b,
                A = d.a,
                Ltp = d.ltp,
                H = d.h,
                L = d.l,
                O = d.o,
                C = d.c,
                D = d.d,
                Atp = d.atp,

                v = d.v,
                T = d.t,

                Bq = d.bq,
                Tbq = d.tbq,
                Sq = d.sq,
                Tsq = d.tsq,

                Vt = d.vt,
                Oi = d.oi,
                Ltq = d.ltq
            };
        }
    }

}
