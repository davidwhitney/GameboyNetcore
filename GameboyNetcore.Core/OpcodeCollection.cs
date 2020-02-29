using System.Collections.Generic;

namespace GameboyNetcore.Core
{
    public class OpcodeCollection
    {
        public int Count => unprefixed.Count + cbprefixed.Count;
        public Dictionary<string, Opcode> unprefixed { get; set; }
        public Dictionary<string, Opcode> cbprefixed { get; set; }
    }
}