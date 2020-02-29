using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace GameboyNetcore.Core
{
    public class GameBoy
    {
        public OpcodeCollection OpCodes { get; }
        
        public Cartridge Cartridge { get; set; }

        public GameBoy()
        {
            var fileContents = File.ReadAllText("C:\\dev\\GameboyNetcore\\GameboyNetcore.Core\\opcodes.json");
            OpCodes = JsonConvert.DeserializeObject<OpcodeCollection>(fileContents);
            Cartridge = Cartridge.Nothing;
        }

        public void Load(string path)
        {
            Cartridge = new Cartridge(path);
        }
    }

    public class Cartridge
    {
        public ReadOnlyMemory<byte> Bytes { get; }

        public Cartridge(string path)
        {
            if (path == null || !File.Exists(path))
            {
                throw new ArgumentException("Invalid ROM path.", nameof(path));
            }

            Bytes = new ReadOnlyMemory<byte>(File.ReadAllBytes(path));
        }

        public ReadOnlyMemory<byte> RomBank0 => Bytes.Slice(0x0000, 0x4000);
        public ReadOnlyMemory<byte> SwitchableRomBank => Bytes.Range(0x4000, 0x8000);

        public ReadOnlyMemory<byte> EntryPoint => Bytes.Slice(0x0100, 3);
        public ReadOnlyMemory<byte> NintendoLogo => Bytes.Slice(0x0104, 48);
        public string Title => Bytes.Slice(0x0134, 15).ToAscii();
        public byte SuperGameboyFlag => Bytes.Single(0x0146);
        public byte CartridgeType => Bytes.Single(0x0147);
        public byte RomSize => Bytes.Single(0x0148);
        public byte RamSize => Bytes.Single(0x0149);
        public byte DestinationCode => Bytes.Single(0x014A);
        public byte OldLicenseeCode => Bytes.Single(0x014B);
        public byte MaskRomVersionNumber => Bytes.Single(0x014C);

        public byte HeaderChecksum => Bytes.Single(0x014D);
        // x=0:FOR i=0134h TO 014Ch:x=x-MEM[i]-1:NEXT
        // The lower 8 bits of the result must be the same than the value in this entry.
        // The GAME WON'T WORK if this checksum is incorrect.

        public static Cartridge Nothing { get; } = new Cartridge();
        private Cartridge() { }
    }

    // Higher level abstractions
    public static class CartridgeExtensions
    {
        public static bool IsSuperGameboy(this Cartridge src)
            => src.SuperGameboyFlag.ToHexString() == "03";
    }

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
