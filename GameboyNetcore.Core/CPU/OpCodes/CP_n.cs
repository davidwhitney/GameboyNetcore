using System.Collections.Generic;
using System.Diagnostics;

namespace GameboyNetcore.Core.CPU.OpCodes
{
    /// <summary>
    /// Description:
    /// Compare A with n.
    /// This is basically an A - n
    /// subtraction instruction but the results are thrown away.
    ///
    /// Use with:
    /// n = A,B,C,D,E,H,L,(HL),#
    /// 
    /// Flags affected:
    /// Z - Set if result is zero. (Set if A = n.)
    /// N - Set.
    /// H - Set if no borrow from bit 4.
    /// C - Set for no borrow. (Set if A<n.)
    /// </summary>
    public class CP_n : OpCodeHandlerBase
    {
        private readonly Dictionary<int, string> _registerMap = new Dictionary<int, string>
        {
            {0xBF, "A"}, {0xB8, "B"}, {0xB9, "C"}, {0xBA, "D"}, 
            {0xBB, "E"}, {0xBC, "H"}, {0xBD, "L"}, {0xBE, "HL"}, 
            {0xFE, "A"},
        };

        public CP_n()
            : base(0xBF, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xFE)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {

            //SetFlags(Registers.A, arg1, 2, 1, 2, 2);
            //Registers.Cycles += 4;
            //Registers.PC++;
            //break;

            var sourceRegister = _registerMap[opCode.addrInt];

            var registerValue = (byte)registers[sourceRegister];
            var valueToCompare = (ushort)ValueFrom(opCode.operand1, registers);

            var result = registerValue - valueToCompare;


            Debug.WriteLine($"CP_n {opCode.Operand1.Value}");
        }
    }
}