namespace GameboyNetcore.Core.CPU.OpCodes
{
    public interface IHandleGameboyAssembly
    {
        bool Handles(Opcode opCode);
        void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode);
    }
}