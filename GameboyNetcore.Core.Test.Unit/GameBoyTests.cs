using System;
using NUnit.Framework;

namespace GameboyNetcore.Core.Test.Unit
{
    public class GameBoyTests
    {
        private GameBoy _sut;
        private const string TestRom = "C:\\dev\\GameboyNetcore\\GameboyNetcore.Core.Test.Unit\\dmg_test_prog_ver1.gb.bin";

        [SetUp]
        public void Setup()
        {
            _sut = new GameBoy();
        }

        [Test]
        public void Ctor_LoadsOpCodes()
        {
            Assert.That(_sut.OpCodes.Count, Is.Not.Zero);
        }

        [Test]
        public void Ctor_DefaultsToRomNothing()
        {
            Assert.That(_sut.Cartridge, Is.EqualTo(Cartridge.Nothing));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase((string) null)]
        [TestCase("blahblahblah.gb")]
        public void Load_InvalidPath_Throws(string badParams)
        {
            Assert.Throws<ArgumentException>(() => _sut.Load(badParams));
        }

        [Test]
        public void Load_ValidPath_LoadedFlagIsTrue()
        {
            _sut.Load(TestRom);

            Assert.That(_sut.Cartridge, Is.Not.EqualTo(Cartridge.Nothing));
            Assert.That(_sut.Cartridge.Loaded, Is.True);
        }
    }

    [TestFixture]
    public class CartridgeTests
    {
        private Cartridge _sut;
        private const string TestRom = "C:\\dev\\GameboyNetcore\\GameboyNetcore.Core.Test.Unit\\dmg_test_prog_ver1.gb.bin";
        
        [Test]
        public void Load_ParsesTitleFromMetadata()
        {
            var rom = new Cartridge(TestRom);

            Assert.That(rom.Title, Is.EqualTo("foo"));
        }

        [Test]
        public void Load_ParsesNintendoLogoFromMetadata()
        {
            var rom = new Cartridge(TestRom);

            Assert.That(rom.NintendoLogo, Is.EqualTo("CE ED 66 66 CC 0D 00 0B 03 73 00 83 00 0C 00 0D 00 08 11 1F 88 89 00 0E DC CC 6E E6 DD DD D9 99 BB BB 67 63 6E 0E EC CC DD DC 99 9F BB B9 33 3E"));
        }

        [Test]
        public void Load_UnderstandsRomBanks()
        {
            var rom = new Cartridge(TestRom);

            Assert.That(rom.RomBank0.Length, Is.EqualTo(16384));
            Assert.That(rom.SwitchableRomBank.Length, Is.EqualTo(16384));
        }
    }
}