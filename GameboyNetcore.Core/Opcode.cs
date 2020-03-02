using System;

namespace GameboyNetcore.Core
{
    public class Opcode
    {
        public string mnemonic { get; set; }
        public int length { get; set; }
        public int[] cycles { get; set; }
        public string[] flags { get; set; }
        public string addr { get; set; }

        public int addrInt => Convert.ToInt32(addr, 16);
        
        public Operand Operand1 => new Operand(operand1);
        public Operand Operand2 => new Operand(operand2);

        // For json.net
        public string operand1 { get; set; }
        public string operand2 { get; set; }
    }
}