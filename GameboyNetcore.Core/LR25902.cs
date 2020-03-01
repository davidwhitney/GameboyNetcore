using System.IO;
using Newtonsoft.Json;

namespace GameboyNetcore.Core
{
    public class LR25902
    {
        private const double ClockSpeed = 4.295454;
        public OpcodeCollection OpCodes { get; set; }

        public LR25902()
        {
            var fileContents = File.ReadAllText("C:\\dev\\GameboyNetcore\\GameboyNetcore.Core\\opcodes.json");
            OpCodes = JsonConvert.DeserializeObject<OpcodeCollection>(fileContents);
        }
    }
}