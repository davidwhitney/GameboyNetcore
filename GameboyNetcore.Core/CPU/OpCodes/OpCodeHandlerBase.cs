using System;
using System.Diagnostics;
using System.Linq;

namespace GameboyNetcore.Core.CPU.OpCodes
{
    public abstract class OpCodeHandlerBase : IHandleGameboyAssembly
    {
        private readonly int[] _opCodes;
        protected OpCodeHandlerBase(params int[] opCodes) => _opCodes = opCodes;
        public bool Handles(Opcode opCode) => _opCodes.Contains(opCode.addrInt);
        public abstract void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode);

        protected object ValueFrom(Operand operand, CpuRegisters registers)
        {
            try
            {
                if (operand.Type == OperandType.Register8bit || operand.Type == OperandType.Register16bit)
                {
                    return registers[operand.Value];
                }

                if (operand.Type == OperandType.ValueFrom16bitRegister)
                {
                    var identity = operand.Value.Substring(1, operand.Value.Length - 2);
                    return registers[identity];
                }

                return Convert.ToUInt16(operand.Value, 16);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.WriteLine($"Could not convert {operand.Value}");
                throw;
            }
        }
    }
}
