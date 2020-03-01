using System;
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
            Assert.Throws<ArgumentException>(() => _sut.Load(badParams));
        }

        [Test]
        public void Load_ValidPath_LoadedFlagIsTrue()
        {
            _sut.Load(TestRom);

            Assert.That(_sut.Cartridge, Is.Not.EqualTo(Cartridge.Nothing));
        }
    }
}