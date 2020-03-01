namespace GameboyNetcore.Core
{
    public class GameBoy
    {
        public LR25902 CPU { get; set; }
        public Display Video { get; set; }
        public Cartridge Cartridge { get; set; }

        public GameBoy()
        {
            CPU = new LR25902();
            Video = new Display(160, 144);
            Cartridge = Cartridge.Nothing;
        }

        public void Load(string path)
        {
            Cartridge = new Cartridge(path);
        }
    }
}
