namespace Machi.Core
{
    public interface IRandom
    {
        /// <summary>Integer in [0, maxExclusive).</summary>
        int Next(int maxExclusive);
    }

    public class SystemRandom : IRandom
    {
        readonly System.Random _r;
        public SystemRandom(int? seed = null) { _r = seed.HasValue ? new System.Random(seed.Value) : new System.Random(); }
        public int Next(int maxExclusive) => _r.Next(maxExclusive);
    }
}
