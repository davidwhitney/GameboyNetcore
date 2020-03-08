using System.Diagnostics;

namespace GameboyNetcore.Core.CPU.OpCodes
{
    public class RST : OpCodeHandlerBase
    {
        public RST()
            : base(0xC7, 0xCF, 0xD7, 0xDF, 0xE7, 0xEF, 0xF7, 0xFF)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            Debug.WriteLine($"RST {opCode.Operand1.Value}");
        }
    }
}