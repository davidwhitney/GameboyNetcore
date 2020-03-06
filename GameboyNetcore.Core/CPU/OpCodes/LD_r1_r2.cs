﻿namespace GameboyNetcore.Core.CPU.OpCodes
{
    /// <summary>
    /// Put value r2 into r1
    /// r1,r2 = A,B,C,D,E,H,L,(HL)
    /// </summary>
    public class LD_r1_r2 : OpCodeHandlerBase
    {
        public LD_r1_r2()
            : base(0x7F, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x48, 0x49,
                0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x58, 0x59, 0x5A, 0x5B, 0x5C,
                0x5D, 0x5E, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x68, 0x69, 0x6B, 0x6C, 0x6D, 0x6E, 0x70, 0x71,
                0x72, 0x73, 0x74, 0x75, 0x36)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
        }
    }
}