using System.Diagnostics;

namespace GameboyNetcore.Core.CPU.OpCodes
{
    public class SBC_A_n : OpCodeHandlerBase
    {
        public SBC_A_n()
            : base(0x9F, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            Debug.WriteLine($"SBC A,n {opCode.Operand1.Value}");
        }
    }
}