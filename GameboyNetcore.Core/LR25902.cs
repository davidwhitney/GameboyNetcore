using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace GameboyNetcore.Core
{
    public class LR25902
    {
        private readonly GameBoy _gameBoy;
        private const double ClockSpeed = 4.295454;
        public OpcodeCollection OpCodes { get; set; }

        public Dictionary<string, ushort> Registers { get; set; }
        public ushort AF { get => Registers["AF"]; set => Registers["AF"] = value; }
        public ushort BC { get => Registers["BC"]; set => Registers["BC"] = value; }
        public ushort DE { get => Registers["DE"]; set => Registers["DE"] = value; }
        public ushort HL { get => Registers["HL"]; set => Registers["HL"] = value; }
        public ushort SP { get => Registers["SP"]; set => Registers["SP"] = value; }
        public ushort PC { get => Registers["PC"]; set => Registers["PC"] = value; }
        
        public ushort StackPointer { get => SP; set => SP = value; }
        public ushort ProgramCounter { get => PC; set => PC = value; }
        
        public byte Flags
        {
            get => AF.LowerBits();
            set => AF = AF.SetLowerBits(value);
        }

/*
Bit  Name  Set Clr  Expl.
 7    zf    Z   NZ   Zero Flag
 6    n     -   -    Add/Sub-Flag (BCD)
 5    h     -   -    Half Carry Flag (BCD)
 4    cy    C   NC   Carry Flag
 3-0  -     -   -    Not used (always zero)
 */


        public LR25902(GameBoy gameBoy)
        {
            _gameBoy = gameBoy;
            var fileContents = File.ReadAllText("C:\\dev\\GameboyNetcore\\GameboyNetcore.Core\\opcodes.json");
            OpCodes = JsonConvert.DeserializeObject<OpcodeCollection>(fileContents);
            Registers = new Dictionary<string, ushort>
            {
                {"AF", ushort.MinValue},
                {"BC", ushort.MinValue},
                {"DE", ushort.MinValue},
                {"HL", ushort.MinValue},
                {"SP", ushort.MinValue},
                {"PC", ushort.MinValue},
            };
        }

        public void StepOnce()
        {
            var value = (int)_gameBoy.Memory.Get(ProgramCounter);
            var opcode = OpCodes.unprefixed.Single(x => x.Value.addrInt == value);
            

        }
    }
}
 