using System.Collections;

namespace Otter
{
    public static class Util
    {
        public static void CopyBits(BitArray dest, BitArray src)
        {
            for (int i = 0; i < dest.Length; i++)
            {
                dest.Set(i, src[i]);
            }
        }

        public static Int32 BitArrayToInt32(BitArray bits)
        {
            Int32 sum = 0;
            for (int i = 0; i < bits.Length; i++)
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
    }
}
