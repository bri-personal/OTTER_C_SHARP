using System.Collections;

namespace Otter
{
    public static class Util
    {
        public static void CopyBits(BitArray dest, BitArray src, Int32 srcStart=0, Int32 len=32, Int32 destStart=0)
        {
            for (int i=srcStart; i < src.Length && i + destStart < dest.Length && i < srcStart+len; i++)
            {
                dest.Set(destStart+i, src[i]);
            }
        }

        public static Int32 BitArrayToInt32(BitArray bits)
        {
            Int32 sum = 0;
            for (int i = 0; i < bits.Length && i<32; i++)
            {
                if (bits[i])
                {
                    sum += 1 << i;
                }
            }
            return sum;
        }

        public static void SetBitArrayToInt32(BitArray bits, Int32 src)
        {
            CopyBits(bits, new BitArray(new Int32[] {src}));
        }

        public static string BitsToString(BitArray bits, Int32 len=32)
        {
            string s = "";
            for (int i = 0; i < len; i++)
            {
                s += (bits[i] ? 1 : 0) + " ";
            }
            return s;
        }
    }
}
