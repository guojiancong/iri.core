using System;
using System.Text;

namespace iri.core.utils
{
    public class Converter
    {
        public const int Radix = 3;
        public const int MaxTritValue = (Radix - 1) / 2;
        public const int MinTritValue = -MaxTritValue;

        public const int NumberOfTritsInATryte = 3;
        public const int NumberOfTritsInAByte = 5;

        public const string TryteAlphabet = "9ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public const long HighLongBits = unchecked((long) 0xFFFFFFFFFFFFFFFFL);

        private static readonly int[][] ByteToTritsMappings = new int[243][];
        private static readonly int[][] TryteToTritsMappings = new int[27][];

        static Converter()
        {
            var trits = new int[NumberOfTritsInAByte];

            for (var i = 0; i < 243; i++)
            {
                ByteToTritsMappings[i] = new int[NumberOfTritsInAByte];
                ByteToTritsMappings[i] = new int[NumberOfTritsInAByte];
                Array.Copy(trits, ByteToTritsMappings[i], NumberOfTritsInAByte);
                Increment(trits, NumberOfTritsInAByte);
            }

            for (var i = 0; i < 27; i++)
            {
                TryteToTritsMappings[i] = new int[NumberOfTritsInATryte];
                Array.Copy(trits, TryteToTritsMappings[i], NumberOfTritsInATryte);
                Increment(trits, NumberOfTritsInATryte);
            }
        }

        public static void Trits(string trytes, int[] dest, int destOffset)
        {
            if (dest.Length - destOffset < trytes.Length * NumberOfTritsInATryte)
                throw new ArgumentException("Destination array is not large enough.");

            for (var i = 0; i < trytes.Length; i++)
                Array.Copy(TryteToTritsMappings[TryteAlphabet.IndexOf(trytes[i])], 0, dest,
                    destOffset + i * NumberOfTritsInATryte, NumberOfTritsInATryte);
        }

        public static void GetTrits(byte[] bytes, int[] trits)
        {
            int offset = 0;
            for (int i = 0; i < bytes.Length && offset < trits.Length; i++)
            {
                Array.Copy(ByteToTritsMappings[bytes[i]], 0,
                    trits, offset,
                    trits.Length - offset < NumberOfTritsInAByte
                        ? (trits.Length - offset)
                        : NumberOfTritsInAByte);
                offset += NumberOfTritsInAByte;
            }

            while (offset < trits.Length)
            {
                trits[offset++] = 0;
            }
        }

        public static void Increment(int[] trits, int size)
        {
            for (var i = 0; i < size; i++)
                if (++trits[i] > MaxTritValue)
                    trits[i] = MinTritValue;
                else
                    break;
        }

        public static int[] AllocateTritsForTrytes(int tryteCount)
        {
            return new int[tryteCount * NumberOfTritsInATryte];
        }

        public static byte[] AllocateBytesForTrits(int tritCount)
        {
            int expectedLength = (tritCount + NumberOfTritsInAByte - 1) / NumberOfTritsInAByte;
            return new byte[expectedLength];
        }

        public static string Trytes(int[] trits, int offset, int size)
        {
            StringBuilder trytes = new StringBuilder();
            for (int i = 0; i < (size + NumberOfTritsInATryte - 1) / NumberOfTritsInATryte; i++)
            {
                int j = trits[offset + i * 3] + trits[offset + i * 3 + 1] * 3 + trits[offset + i * 3 + 2] * 9;
                if (j < 0)
                {
                    j += TryteAlphabet.Length;
                }

                trytes.Append(TryteAlphabet[j]);
            }

            return trytes.ToString();
        }

        public static string Trytes(int[] trits)
        {
            return Trytes(trits, 0, trits.Length);
        }

        public static void Bytes(int[] trits, int srcPos, byte[] dest, int destPos, int tritsLength)
        {

            int expectedLength = (tritsLength + NumberOfTritsInAByte - 1) / NumberOfTritsInAByte;

            if ((dest.Length - destPos) < expectedLength)
            {

                throw new ArgumentException("Input array not large enough.");
            }

            for (int i = 0; i < expectedLength; i++)
            {
                int value = 0;
                for (int j = (tritsLength - i * NumberOfTritsInAByte) < 5
                        ? (tritsLength - i * NumberOfTritsInAByte)
                        : NumberOfTritsInAByte;
                    j-- > 0;)
                {
                    value = value * Radix + trits[srcPos + i * NumberOfTritsInAByte + j];
                }

                dest[destPos + i] = (byte) value;
            }
        }

        public static void CopyTrits(long value, int[] destination, int offset, int size)
        {

            long absoluteValue = value < 0 ? -value : value;
            for (int i = 0; i < size; i++)
            {

                int remainder = (int) (absoluteValue % Radix);
                absoluteValue /= Radix;
                if (remainder > MaxTritValue)
                {

                    remainder = MinTritValue;
                    absoluteValue++;
                }

                destination[offset + i] = remainder;
            }

            if (value < 0)
            {
                for (int i = 0; i < size; i++)
                {
                    destination[offset + i] = -destination[offset + i];
                }
            }
        }

        public static long LongValue(int[] trits, int srcPos, int size)
        {
            long value = 0;
            for (int i = size; i-- > 0;)
            {
                value = value * Radix + trits[srcPos + i];
            }

            return value;
        }
    }
}