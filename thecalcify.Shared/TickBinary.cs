using System;
using System.IO;
using System.Text;

namespace thecalcify.Shared
{
    public class TickBinary
    {
        public string Symbol, v, n;
        public double B, A, Ltp, H, L;
        public double? O, C, D, Atp;
        public long T, Bq, Tbq, Sq, Tsq, Vt, Oi, Ltq;

        const int SYMBOL_SIZE = 32;
        const int NAME_SIZE = 32;
        const int V_SIZE = 16;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[256];

            using (var ms = new MemoryStream(buffer))
            using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
            {
                WriteFixedString(bw, Symbol, SYMBOL_SIZE);
                WriteFixedString(bw, n, NAME_SIZE);

                bw.Write(B);
                bw.Write(A);
                bw.Write(Ltp);
                bw.Write(H);
                bw.Write(L);
                bw.Write(O ?? Double.NaN);
                bw.Write(C ?? Double.NaN);
                bw.Write(D ?? Double.NaN);
                bw.Write(Atp ?? Double.NaN);

                WriteFixedString(bw, v, V_SIZE);
                bw.Write(T);

                bw.Write(Bq);
                bw.Write(Tbq);
                bw.Write(Sq);
                bw.Write(Tsq);

                bw.Write(Vt);
                bw.Write(Oi);
                bw.Write(Ltq);
            }

            return buffer;
        }

        public static TickBinary FromBytes(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms, Encoding.UTF8))
            {
                var tick = new TickBinary();

                tick.Symbol = ReadFixedString(br, SYMBOL_SIZE);
                tick.n = ReadFixedString(br, NAME_SIZE);

                tick.B = br.ReadDouble();
                tick.A = br.ReadDouble();
                tick.Ltp = br.ReadDouble();
                tick.H = br.ReadDouble();
                tick.L = br.ReadDouble();
                tick.O = ReadNullableDouble(br);
                tick.C = ReadNullableDouble(br);
                tick.D = ReadNullableDouble(br);
                tick.Atp = br.ReadDouble();

                tick.v = ReadFixedString(br, V_SIZE);
                tick.T = br.ReadInt64();

                tick.Bq = br.ReadInt64();
                tick.Tbq = br.ReadInt64();
                tick.Sq = br.ReadInt64();
                tick.Tsq = br.ReadInt64();

                tick.Vt = br.ReadInt64();
                tick.Oi = br.ReadInt64();
                tick.Ltq = br.ReadInt64();

                return tick;
            }
        }

        private static void WriteFixedString(BinaryWriter bw, string value, int size)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? "");
            Array.Resize(ref bytes, size);
            bw.Write(bytes);
        }

        private static string ReadFixedString(BinaryReader br, int size)
        {
            var bytes = br.ReadBytes(size);
            int len = Array.IndexOf(bytes, (byte)0);
            if (len < 0) len = bytes.Length;
            return Encoding.UTF8.GetString(bytes, 0, len);
        }

        static double? ReadNullableDouble(BinaryReader br)
        {
            var value = br.ReadDouble();
            return double.IsNaN(value) ? (double?)null : value;
        }

    }
}
