using System;
using System.Collections.Generic;
using System.IO;

namespace GameboyNetcore.Core
{
    public class Cartridge
    {
        public ReadOnlyMemory<byte> Bytes { get; }
        public static Cartridge Nothing { get; } = new Cartridge();
        
        public Cartridge(string path)
        {
            if (path == null || !File.Exists(path))
            {
                throw new ArgumentException("Invalid ROM path.", nameof(path));
            }

            Bytes = new ReadOnlyMemory<byte>(File.ReadAllBytes(path));
        }

        private Cartridge()
        {
        }

        public ReadOnlyMemory<byte> RomBank0 => Bytes.Slice(0x0000, 0x4000);
        public ReadOnlyMemory<byte> SwitchableRomBank => Bytes.Range(0x4000, 0x8000);

        public ReadOnlyMemory<byte> EntryPoint => Bytes.Slice(0x0100, 3);
        public ReadOnlyMemory<byte> NintendoLogo => Bytes.Slice(0x0104, 48);
        public ReadOnlyMemory<byte> Title => Bytes.Slice(0x0134, 16);
        public byte ColorGameboyFlag => Bytes.Single(0x0143);
        public byte SuperGameboyFlag => Bytes.Single(0x0146);
        public byte CartridgeType => Bytes.Single(0x0147);
        public byte RomSize => Bytes.Single(0x0148);
        public byte RamSize => Bytes.Single(0x0149);
        public byte DestinationCode => Bytes.Single(0x014A);
        public byte OldLicenseeCode => Bytes.Single(0x014B);
        public ReadOnlyMemory<byte> NewLicenseeCode => Bytes.Slice(0x0144, 2);
        public byte MaskRomVersionNumber => Bytes.Single(0x014C);

        public byte HeaderChecksum => Bytes.Single(0x014D);
        // x=0:FOR i=0134h TO 014Ch:x=x-MEM[i]-1:NEXT
        // The lower 8 bits of the result must be the same than the value in this entry.
        // The GAME WON'T WORK if this checksum is incorrect.

        public ReadOnlyMemory<byte> GlobalChecksum => Bytes.Slice(0x014E, 16);

        // Higher level abstractions

        public bool IsSuperGameboy => SuperGameboyFlag == 0x03;
        public bool IsColorGameboyOnly => ColorGameboyFlag == 0xC0;
        public string TitleText => Title.ToAscii();
        public string CartridgeTypeText => CartridgeTypes[CartridgeType].Trim();
        public string DestinationCodeText => DestinationCodes[DestinationCode].Trim();

        public int RomSizeInKb
        {
            get
            {
                const int bankSize = 16;
                switch (RomSize)
                {
                    case 0x00: return 32;
                    case 0x01: return bankSize * 4;
                    case 0x02: return bankSize * 8;
                    case 0x03: return bankSize * 16;
                    case 0x04: return bankSize * 32;
                    case 0x05: return bankSize * 64;
                    case 0x06: return bankSize * 128;
                    case 0x07: return bankSize * 256;
                    case 0x08: return bankSize * 512;
                    case 0x52: return bankSize * 72;
                    case 0x53: return bankSize * 80;
                    case 0x54: return bankSize * 96;
                }

                throw new Exception("Can't detect RomSize in kbs");
            }
        }

        public int RamSizeInKb
        {
            get
            {
                const int bankSize = 8;
                switch (RamSize)
                {
                    case 0x00: return 0;
                    case 0x01: return 2;
                    case 0x02: return 8;
                    case 0x03: return bankSize * 4;
                    case 0x04: return bankSize * 16;
                    case 0x05: return bankSize * 8;
                }

                throw new Exception("Can't detect RamSize in kbs");
            }
        }

        public string LicenseeText
        {
            get
            {
                if (OldLicenseeCode == 0x33)
                {
                    //var newLicenseeCode = NewLicenseeCode;
                    //var val = Licensees.New[newLicenseeCode];
                }

                return Licensees.Old[OldLicenseeCode];
            }
        }

        private static readonly Dictionary<int, string> CartridgeTypes = new Dictionary<int, string>
        {
            {0x00, "ROM ONLY                                  "},
            {0x01, "MBC1                                      "},
            {0x02, "MBC1+RAM                                  "},
            {0x03, "MBC1+RAM + BATTERY                        "},
            {0x05, "MBC2                                      "},
            {0x06, "MBC2+BATTERY                              "},
            {0x08, "ROM+RAM                                   "},
            {0x09, "ROM+RAM + BATTERY                         "},
            {0x0B, "MMM01                                     "},
            {0x0C, "MMM01+RAM                                 "},
            {0x0D, "MMM01+RAM + BATTERY                       "},
            {0x0F, "MBC3+TIMER + BATTERY                      "},
            {0x10, "MBC3+TIMER + RAM + BATTERY                "},
            {0x11, "MBC3                                      "},
            {0x12, "MBC3+RAM                                  "},
            {0x13, "MBC3+RAM + BATTERY                        "},
            {0x19, "MBC5                                      "},
            {0x1A, "MBC5+RAM                                  "},
            {0x1B, "MBC5+RAM + BATTERY                        "},
            {0x1C, "MBC5+RUMBLE                               "},
            {0x1D, "MBC5+RUMBLE + RAM                         "},
            {0x1E, "MBC5+RUMBLE + RAM + BATTERY               "},
            {0x20, "MBC6                                      "},
            {0x22, "MBC7+SENSOR + RUMBLE + RAM + BATTERY      "},
            {0xFC, "POCKET CAMERA                             "},
            {0xFD, "BANDAI TAMA5                              "},
            {0xFE, "HuC3                                      "},
            {0xFF, "HuC1+RAM + BATTERY                        "}
        };

        private static readonly Dictionary<int, string> DestinationCodes = new Dictionary<int, string>
        {
            { 0x00, "Japanese" },
            { 0x01, "Non-Japanese" }
        };
    }
}