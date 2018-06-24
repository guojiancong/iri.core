namespace iri.core.hash
{
    public abstract class SpongeFactory
    {
        public enum Mode
        {
            CurlP81,
            CurlP27,
            Kerl,
            //BCURLT
        }
        public static Sponge Create(Mode mode)
        {
            switch (mode)
            {
                case Mode.CurlP81: return new Curl(mode);
                case Mode.CurlP27: return new Curl(mode);
                case Mode.Kerl: return new Kerl();
                //case BCURLT: return new Curl(true, mode);
                default: return null;
            }
        }
    }
}
