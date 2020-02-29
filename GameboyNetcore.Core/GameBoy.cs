using System;
using System.IO;
using System.Linq;
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
        public Cartridge(string path)
        {
            if (path == null || !File.Exists(path))
            {
                throw new ArgumentException("Invalid ROM path.", nameof(path));
            }

            Bytes = new ReadOnlyMemory<byte>(File.ReadAllBytes(path));
        }

        public ReadOnlyMemory<byte> Bytes { get; }

        public ReadOnlyMemory<byte> RomBank0 => Bytes.Range(0x0000, 0x4000);
        public ReadOnlyMemory<byte> SwitchableRomBank => Bytes.Range(0x4000, 0x8000);

        public ReadOnlyMemory<byte> EntryPoint => Bytes.Range(0x0100, 0x0103);
        public string NintendoLogo => Bytes.Range(0x0104, 0x0133).ToHexString();
        public string Title => Bytes.Range(0x0134, 0x0143).ToAscii();
        public byte SuperGameboyFlag => Bytes.Single(0x0146);
        public byte CartridgeType => Bytes.Single(0x0147);

        public static Cartridge Nothing { get; } = new Cartridge();
        private Cartridge() { }
    }

    public static class ReadOnlyMemoryExtensions
    {
        public static ReadOnlyMemory<byte> Range(this ReadOnlyMemory<byte> src, int start, int end)
        {
            var len = end - start;
            return src.Slice(start, len);
        }

        public static string ToHexString(this ReadOnlyMemory<byte> src)
        {
            return BitConverter.ToString(src.ToArray()).Replace("-", " ");
        }

        public static string ToAscii(this ReadOnlyMemory<byte> src)
        {
            return BitConverter.ToString(src.ToArray());
        }

        public static byte Single(this ReadOnlyMemory<byte> src, int offset)
        {
            return src.Slice(offset, 1).Single();
        }

        public static byte Single(this ReadOnlyMemory<byte> src)
        {
            if (src.Length > 1) throw new Exception("Not a single byte");
            return src.ToArray()[0];
        }
    }
}
