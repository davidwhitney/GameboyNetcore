using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GameboyNetcore.Core.Test.Unit
{
    public class GameBoyTests
    {
        private GameBoy _sut;
        private CancellationToken _cancellationToken;
        private const string TestRom = "C:\\dev\\GameboyNetcore\\GameboyNetcore.Core.Test.Unit\\dmg_test_prog_ver1.gb.bin";
        
        [SetUp]
        public void Setup()
        {
            _sut = new GameBoy();
            _cancellationToken = new CancellationTokenSource(1000).Token;
        }

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
        public async Task Load_PowerOn_SetMemory()
        {
            _sut.InsertCartridge(TestRom);
            await _sut.PowerOn(_cancellationToken, false);

            Assert.That(_sut.CPU.Registers.AF, Is.EqualTo(0x01B0));
        }

        [Test]
        public async Task PowerOn_WithCancellationToken_DoesNotLoopForever()
        {
            _sut.InsertCartridge(TestRom);
            await _sut.PowerOn(new CancellationTokenSource(1).Token);
            
            Assert.Pass();
        }

        [Test]
        public async Task Stepping_StartsWithANopAndAJp()
        {
            _sut.InsertCartridge(TestRom);
            await _sut.PowerOn(new CancellationTokenSource(1).Token, false);

            Assert.That(_sut.CPU.StepOnce().mnemonic, Is.EqualTo("NOP"));
            Assert.That(_sut.CPU.StepOnce().mnemonic, Is.EqualTo("JP"));
            Assert.That(_sut.CPU.StepOnce().mnemonic, Is.EqualTo("LD"));
        }

        [Test]
        //[Ignore("To be run manually")]
        public async Task PowerOn_ExploratoryTestForDebugOutput()
        {
            _sut.InsertCartridge(TestRom);
            await _sut.PowerOn(new CancellationTokenSource(1000 * 10).Token);
        }
    }
}