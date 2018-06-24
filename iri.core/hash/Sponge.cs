namespace iri.core.hash
{
    public abstract class Sponge
    {
        public const int HashLength = 243;
        public abstract void Absorb( int[] trits, int offset, int length);
        public abstract void Squeeze( int[] trits, int offset, int length);
        public abstract void Reset();
    }
}
