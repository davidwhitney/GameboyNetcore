namespace GameboyNetcore.Core.CPU.OpCodes
{
    public class JP : OpCodeHandlerBase
    {
        public JP()
            : base(0xc3)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            if (string.IsNullOrWhiteSpace(opCode.Operand2))
            {
                registers.SP = (ushort)ValueFrom(opCode.Operand1, registers);
            }
        }
    }
}