namespace GameboyNetcore.Core.Actions
{
    public class PowerOnAction : IPerformAnOperation
    {
        private readonly GameBoy _gameboy;
        public PowerOnAction(GameBoy gameboy) => _gameboy = gameboy;

        public void Execute()
        {
            _gameboy.CPU.Registers.AF = 0x01B0;
            _gameboy.CPU.Registers.BC = 0x0013;
            _gameboy.CPU.Registers.DE = 0x00D8;
            _gameboy.CPU.Registers.HL = 0x014D;
            _gameboy.CPU.StackPointer = 0xFFFE;
            
            _gameboy.Memory[0xFF05] = 0x00;
            _gameboy.Memory[0xFF05] = 0x00;
            _gameboy.Memory[0xFF06] = 0x00;
            _gameboy.Memory[0xFF07] = 0x00;
            _gameboy.Memory[0xFF10] = 0x80;
            _gameboy.Memory[0xFF11] = 0xBF;
            _gameboy.Memory[0xFF12] = 0xF3;
            _gameboy.Memory[0xFF14] = 0xBF;
            _gameboy.Memory[0xFF16] = 0x3F;
            _gameboy.Memory[0xFF17] = 0x00;
            _gameboy.Memory[0xFF19] = 0xBF;
            _gameboy.Memory[0xFF1A] = 0x7F;
            _gameboy.Memory[0xFF1B] = 0xFF;
            _gameboy.Memory[0xFF1C] = 0x9F;
            _gameboy.Memory[0xFF1E] = 0xBF;
            _gameboy.Memory[0xFF20] = 0xFF;
            _gameboy.Memory[0xFF21] = 0x00;
            _gameboy.Memory[0xFF22] = 0x00;
            _gameboy.Memory[0xFF23] = 0xBF;
            _gameboy.Memory[0xFF24] = 0x77;
            _gameboy.Memory[0xFF25] = 0xF3;
            _gameboy.Memory[0xFF26] = 0xF1;
            _gameboy.Memory[0xFF40] = 0x91;
            _gameboy.Memory[0xFF42] = 0x00;
            _gameboy.Memory[0xFF43] = 0x00;
            _gameboy.Memory[0xFF45] = 0x00;
            _gameboy.Memory[0xFF47] = 0xFC;
            _gameboy.Memory[0xFF48] = 0xFF;
            _gameboy.Memory[0xFF49] = 0xFF;
            _gameboy.Memory[0xFF4A] = 0x00;
            _gameboy.Memory[0xFF4B] = 0x00;
            _gameboy.Memory[0xFFFF] = 0x00;
            
            _gameboy.CPU.ProgramCounter = 0x100;
        }
    }
}
