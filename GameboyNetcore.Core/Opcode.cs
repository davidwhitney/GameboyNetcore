﻿namespace GameboyNetcore.Core
{
    public class Opcode
    {
        public string mnemonic { get; set; }
        public int length { get; set; }
        public int[] cycles { get; set; }
        public string[] flags { get; set; }
        public string addr { get; set; }
    }
}