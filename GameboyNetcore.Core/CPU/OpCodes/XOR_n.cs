using System.Diagnostics;

namespace GameboyNetcore.Core.CPU.OpCodes
{
    public class XOR_n : OpCodeHandlerBase
    {
        public XOR_n()
            : base(0xAF, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xEE)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            Debug.WriteLine($"XOR n {opCode.Operand1.Value}");
        }
    }
}