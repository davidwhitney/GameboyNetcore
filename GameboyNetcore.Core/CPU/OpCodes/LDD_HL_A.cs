using System.Diagnostics;

namespace GameboyNetcore.Core.CPU.OpCodes
{
    public class LDD_HL_A : OpCodeHandlerBase
    {
        public LDD_HL_A()
            : base(0x32)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            Debug.WriteLine($"SBC A,n {opCode.Operand1.Value}");
        }
    }
}