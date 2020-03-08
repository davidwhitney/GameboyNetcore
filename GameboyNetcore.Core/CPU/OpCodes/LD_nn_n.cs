using System.Diagnostics;

namespace GameboyNetcore.Core.CPU.OpCodes
{
    public class LD_nn_n : OpCodeHandlerBase
    {
        public LD_nn_n()
            : base(0x06, 0x0E, 0x16, 0x1E, 0x26, 0x2E)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            var val = ValueFrom(opCode.Operand1, registers);
            registers[opCode.Operand2] = val;
            Debug.WriteLine($"LD {opCode.Operand1.Value}, {val}");
        }
    }
}