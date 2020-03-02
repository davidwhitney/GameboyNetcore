using System.Linq;
using NUnit.Framework;

namespace GameboyNetcore.Core.Test.Unit
{
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
            _cpu.Registers.AF = 0b0010_0110_0000_0011;

            Assert.That(_cpu.Flags, Is.EqualTo(0b0010_0110));
        }

        [Test]
        public void Flags_CanBeUpdated()
        {
            _cpu.Registers.AF = 0b0010_0110_0000_0011;

            _cpu.Flags = 0b0000_0111;

            Assert.That(_cpu.Registers.AF, Is.EqualTo(0b0000_0111_0000_0011));
            Assert.That(_cpu.Flags, Is.EqualTo(0b0000_0111));
        }

        [Test]
        public void Flags_CanBeUpdated_ByTextKey()
        {
            _cpu.Registers.AF = 0b0010_0110_0000_0011;

            _cpu.Registers["Flags"] = (byte)0b0000_0111;

            Assert.That(_cpu.Registers.AF, Is.EqualTo(0b0000_0111_0000_0011));
            Assert.That(_cpu.Flags, Is.EqualTo(0b0000_0111));
        }

        [Test]
        public void CanEnumerate8bitRegisters()
        {
            var registers = _cpu.Registers.ToList();

            Assert.That(registers.Count, Is.EqualTo(8));
        }
    }
}