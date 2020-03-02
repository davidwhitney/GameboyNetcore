namespace GameboyNetcore.Core
{
    public class GameBoy
    {
        public LR25902 CPU { get; set; }
        public Display Video { get; set; }
        public Cartridge Cartridge { get; set; }
        public MemoryMap Memory { get; set; }

        public GameBoy()
        {
            CPU = new LR25902(this);
            Video = new Display(160, 144);
            Cartridge = Cartridge.Nothing;
            Memory = new MemoryMap();
        }

        public void InsertCartridge(string path)
        {
            Cartridge = new Cartridge(path);
            Memory.Clear();
            Memory.Load(Cartridge);
        }

        public void PowerOn()
        {
            CPU.AF = 0x01B0;
            CPU.BC = 0x0013;
            CPU.DE = 0x00D8;
            CPU.HL = 0x014D;
            CPU.StackPointer = 0xFFFE;

            Memory.Set(0xFF05, 0x00);
            Memory.Set(0xFF05, 0x00);
            Memory.Set(0xFF06, 0x00);
            Memory.Set(0xFF07, 0x00);
            Memory.Set(0xFF10, 0x80);
            Memory.Set(0xFF11, 0xBF);
            Memory.Set(0xFF12, 0xF3);
            Memory.Set(0xFF14, 0xBF);
            Memory.Set(0xFF16, 0x3F);
            Memory.Set(0xFF17, 0x00);
            Memory.Set(0xFF19, 0xBF);
            Memory.Set(0xFF1A, 0x7F);
            Memory.Set(0xFF1B, 0xFF);
            Memory.Set(0xFF1C, 0x9F);
            Memory.Set(0xFF1E, 0xBF);
            Memory.Set(0xFF20, 0xFF);
            Memory.Set(0xFF21, 0x00);
            Memory.Set(0xFF22, 0x00);
            Memory.Set(0xFF23, 0xBF);
            Memory.Set(0xFF24, 0x77);
            Memory.Set(0xFF25, 0xF3);
            Memory.Set(0xFF26, 0xF1);
            Memory.Set(0xFF40, 0x91);
            Memory.Set(0xFF42, 0x00);
            Memory.Set(0xFF43, 0x00);
            Memory.Set(0xFF45, 0x00);
            Memory.Set(0xFF47, 0xFC);
            Memory.Set(0xFF48, 0xFF);
            Memory.Set(0xFF49, 0xFF);
            Memory.Set(0xFF4A, 0x00);
            Memory.Set(0xFF4B, 0x00);
            Memory.Set(0xFFFF, 0x00);

            CPU.StackPointer = 0x100;
            CPU.StepOnce();
        }
    }
}
