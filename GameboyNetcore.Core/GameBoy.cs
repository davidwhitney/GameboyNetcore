using System.IO;
using Newtonsoft.Json;

namespace GameboyNetcore.Core
{
    public class GameBoy
    {
        public OpcodeCollection OpCodes { get; }
        
        public Cartridge Cartridge { get; set; }

        public GameBoy()
        {
            var fileContents = File.ReadAllText("C:\\dev\\GameboyNetcore\\GameboyNetcore.Core\\opcodes.json");
            OpCodes = JsonConvert.DeserializeObject<OpcodeCollection>(fileContents);
            Cartridge = Cartridge.Nothing;
        }

        public void Load(string path)
        {
            Cartridge = new Cartridge(path);
        }
    }
}
