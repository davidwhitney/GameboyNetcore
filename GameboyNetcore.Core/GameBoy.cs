using System.Threading;
using System.Threading.Tasks;
using GameboyNetcore.Core.Actions;
using GameboyNetcore.Core.CPU;

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

        public async Task PowerOn(CancellationToken cancellationToken, bool start = true)
        {
            new PowerOnAction(this).Execute();

            if (start)
            {
                await CPU.Run(cancellationToken);
            }
        }
    }
}
