namespace GameboyNetcore.Core.CPU.OpCodes
{
    public class NOP : OpCodeHandlerBase
    {
        public NOP() : base(0x00)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            // Do Nothing.
        }
    }
}