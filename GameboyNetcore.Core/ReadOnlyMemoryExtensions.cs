using System;
using System.Text;

namespace GameboyNetcore.Core
{
    public static class ReadOnlyMemoryExtensions
    {
        public static ReadOnlyMemory<byte> Range(this ReadOnlyMemory<byte> src, int start, int end)
            => src.Slice(start, end - start);

        public static string ToHexString(this byte src)
            => new[] {src}.ToHexString();

        public static string ToHexString(this ReadOnlyMemory<byte> src)
            => src.ToArray().ToHexString();

        public static string ToHexString(this byte[] src) 
            => BitConverter.ToString(src).Replace("-", " ");

        public static string ToAscii(this ReadOnlyMemory<byte> src)
            => Encoding.ASCII.GetString(src.ToArray()).Replace("\0", "");

        public static byte Single(this ReadOnlyMemory<byte> src, int offset)
            => src.Slice(offset, 1).Single();

        public static byte Single(this ReadOnlyMemory<byte> src)
        {
            if (src.Length > 1) throw new Exception("Not a single byte");
            return src.ToArray()[0];
        }
    }
}