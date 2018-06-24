using System;
using iri.core.hash;
using iri.core.utils;

namespace iri.core.model
{
    public class Hash
    {
        public const int SizeInTrits = 243;
        public const int SizeInBytes = 49;

        public static Hash NullHash = new Hash(new int[Sponge.HashLength]);

        private byte[] _bytes;
        private int[] _trits;
        private int _hashCode;

        public Hash(byte[] bytes, int offset, int size)
        {
            FullRead(bytes, offset, size);
        }

        public Hash(string trytes)
        {
            _trits = new int[SizeInTrits];
            Converter.Trits(trytes, _trits, 0);
        }

        public Hash(int[] trits) : this(trits, 0)
        {
        }

        public Hash(int[] trits, int offset)
        {
            _trits = new int[SizeInTrits];
            Array.Copy(trits, offset, _trits, 0, SizeInTrits);
        }

        public int TrailingZeros()
        {
            var index = SizeInTrits;
            var zeros = 0;
            var trits = Trits();
            while (index-- > 0 && trits[index] == 0)
            {
                zeros++;
            }

            return zeros;
        }

        public int[] Trits()
        {
            if (_trits == null)
            {
                _trits = new int[Sponge.HashLength];
                Converter.GetTrits(_bytes, _trits);
            }

            return _trits;
        }

        public static Hash Calculate(int[] tritsToCalculate, int offset, int length, Sponge curl)
        {
            int[] hashTrits = new int[SizeInTrits];
            curl.Reset();
            curl.Absorb(tritsToCalculate, offset, length);
            curl.Squeeze(hashTrits, 0, SizeInTrits);
            return new Hash(hashTrits);
        }

        private void FullRead(byte[] bytes, int offset, int size)
        {
            _bytes = new byte[SizeInBytes];
            Array.Copy(bytes, offset, _bytes, 0, size - offset > bytes.Length ? bytes.Length - offset : size);
            _hashCode = _bytes.GetHashCode();
            // TODO(gjc): may be difference
            //hashCode = Arrays.hashCode(this.bytes);
        }

        public override string ToString()
        {
            return Converter.Trytes(Trits());
        }
    }
}
