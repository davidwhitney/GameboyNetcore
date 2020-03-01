using NUnit.Framework;

namespace GameboyNetcore.Core.Test.Unit
{
    [TestFixture]
    public class CartridgeTests
    {
        private const string TestRom = "C:\\dev\\GameboyNetcore\\GameboyNetcore.Core.Test.Unit\\dmg_test_prog_ver1.gb.bin";
        private const string TetrisRom = "C:\\dev\\GameboyNetcore\\GameboyNetcore.Core.Test.Unit\\tetris.gb";
        
        [Test]
        public void Load_ParsesTitleFromMetadata()
        {
            var rom = new Cartridge(TestRom);

            Assert.That(rom.TitleText, Is.EqualTo(""));
        }

        [Test]
        public void Load_ParsesNintendoLogoFromMetadata()
        {
            var rom = new Cartridge(TestRom);

            Assert.That(rom.NintendoLogo.ToHexString(), Is.EqualTo("CE ED 66 66 CC 0D 00 0B 03 73 00 83 00 0C 00 0D 00 08 11 1F 88 89 00 0E DC CC 6E E6 DD DD D9 99 BB BB 67 63 6E 0E EC CC DD DC 99 9F BB B9 33 3E"));
        }

        [Test]
        public void Load_ParsesNintendoLogoFromMetadata_Tetris()
        {
            var rom = new Cartridge(TetrisRom);

            Assert.That(rom.TitleText, Is.EqualTo("TETRIS"));
            Assert.That(rom.IsSuperGameboy, Is.False);
            Assert.That(rom.RomSizeInKb, Is.EqualTo(32));
            Assert.That(rom.RamSizeInKb, Is.EqualTo(0));
            Assert.That(rom.IsColorGameboyOnly, Is.False);
            Assert.That(rom.CartridgeTypeText, Is.EqualTo("ROM ONLY"));
            Assert.That(rom.DestinationCodeText, Is.EqualTo("Japanese"));
            Assert.That(rom.LicenseeText, Is.EqualTo("nintendo"));
            Assert.That(rom.MaskRomVersionNumber, Is.EqualTo(0x00));
        }

        [Test]
        public void Load_UnderstandsRomBanks()
        {
            var rom = new Cartridge(TestRom);

            Assert.That(rom.RomBank0.Length, Is.EqualTo(16384));
            Assert.That(rom.SwitchableRomBank.Length, Is.EqualTo(16384));
        }
    
        [Test]
        public void Load_UnderstandsEntryPoint()
        {
            var rom = new Cartridge(TestRom);

            Assert.That(rom.EntryPoint.Length, Is.EqualTo(3));
        }
    }
}