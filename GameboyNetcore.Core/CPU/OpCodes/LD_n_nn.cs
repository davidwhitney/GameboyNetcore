using System.Diagnostics;

namespace GameboyNetcore.Core.CPU.OpCodes
{
    /// <summary>
    /// Put value nn into n
    /// Use with:
    /// n = BC,DE,HL,SP
    /// nn = 16 bit immediate value
    /// </summary>
    public class LD_n_nn : OpCodeHandlerBase
    {
        public LD_n_nn()
            : base(0x01, 0x11, 0x21, 0x31)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            var val = ValueFrom(opCode.Operand2, registers);
            registers[opCode.Operand1] = val;
            Debug.WriteLine($"LD {opCode.Operand1.Value}, {val}");
        }
    }
}