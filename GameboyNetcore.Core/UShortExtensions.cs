namespace GameboyNetcore.Core
{
    public static class UShortExtensions
    {
        public static byte High(this ushort src) => (byte) src;
        public static byte LowerBits(this ushort src) => (byte) (src >> 8);

        public static ushort SetHigh(this ushort target, byte value)
        {
            return (ushort)((target << 8) | value);
        }

        public static ushort SetLowerBits(this ushort target, byte value) 
            => (ushort)((value << 8) | target.High());
    }
}