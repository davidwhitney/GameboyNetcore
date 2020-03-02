using System;
using System.Net.Sockets;
using NUnit.Framework;

namespace GameboyNetcore.Core.Test.Unit
{
    public class GameBoyTests
    {
        private GameBoy _sut;
        private const string TestRom = "C:\\dev\\GameboyNetcore\\GameboyNetcore.Core.Test.Unit\\dmg_test_prog_ver1.gb.bin";

        [SetUp]
        public void Setup() => _sut = new GameBoy();

        [Test]
        public void Ctor_DefaultsToRomNothing() 
            => Assert.That(_sut.Cartridge, Is.EqualTo(Cartridge.Nothing));
        
        [Test]
        public void Ctor_CPU_NotNull() => Assert.That(_sut.CPU, Is.Not.Null);

        [Test]
        public void Ctor_Video_NotNull() => Assert.That(_sut.Video, Is.Not.Null);

        [Test]
        public void Ctor_LoadsOpCodes()
            => Assert.That(_sut.CPU.OpCodes.Count, Is.Not.Zero);

        [TestCase("")]
        [TestCase(" ")]
        [TestCase((string) null)]
        [TestCase("blahblahblah.gb")]
        public void Load_InvalidPath_Throws(string badParams)
        {
            Assert.Throws<ArgumentException>(() => _sut.InsertCartridge(badParams));
        }

        [Test]
        public void Load_ValidPath_LoadedFlagIsTrue()
        {
            _sut.InsertCartridge(TestRom);

            Assert.That(_sut.Cartridge, Is.Not.EqualTo(Cartridge.Nothing));
        }

        [Test]
        public void Load_PowerOn_SetMemory()
        {
            _sut.InsertCartridge(TestRom);
            _sut.PowerOn();

            Assert.That(_sut.Cartridge, Is.Not.EqualTo(Cartridge.Nothing));
        }
    }

    [TestFixture]
    public class LR25902Tests
    {
        private LR25902 _cpu;

        [SetUp]
        public void SetUp()
        {
            _cpu = new LR25902(null);
        }

        [Test]
        public void Flags_IsExtractedFromAFRegister()
        {
            _cpu.AF = 0b0010_0110_0000_0011;

            Assert.That(_cpu.Flags, Is.EqualTo(0b0010_0110));
        }

        [Test]
        public void Flags_CanBeUpdated()
        {
            _cpu.AF = 0b0010_0110_0000_0011;

            _cpu.Flags = 0b0000_0111;

            Assert.That(_cpu.AF, Is.EqualTo(0b0000_0111_0000_0011));
            Assert.That(_cpu.Flags, Is.EqualTo(0b0000_0111));
        }
    }
}