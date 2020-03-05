using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GameboyNetcore.Core
{
    public class LR25902
    {
        private readonly GameBoy _gameBoy;
        private List<IHandleGameboyAssembly> _handlers;
        private const double ClockSpeed = 4.295454;
        public OpcodeCollection OpCodes { get; set; }
        public CpuRegisters Registers { get; set; }

        public ushort StackPointer { get => Registers.SP; set => Registers.SP = value; }
        public ushort ProgramCounter { get => Registers.PC; set => Registers.PC = value; }
        public byte Flags
        {
            get => Registers.AF.LowerBits();
            set => Registers.AF = Registers.AF.SetLowerBits(value);
        }

        public LR25902(GameBoy gameBoy)
        {
            _gameBoy = gameBoy;
            var fileContents = File.ReadAllText("C:\\dev\\GameboyNetcore\\GameboyNetcore.Core\\opcodes.json");
            OpCodes = JsonConvert.DeserializeObject<OpcodeCollection>(fileContents);
            Registers = new CpuRegisters();

            _handlers = new List<IHandleGameboyAssembly>
            {
                new Nop(),
                new Jp(),
                new LD_nn_n()
            };
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    StepOnce();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                    throw;
                }

                await Task.Delay(100);
                // some clock speed timer maths goes here
                // rather than a random 100ms delay
            }
        }

        public Opcode StepOnce()
        {
            Debug.WriteLine($"PC: {ProgramCounter}, SP: {StackPointer}");

            var pcValue = _gameBoy.Memory.Get(ProgramCounter);

            var opCodeKvp = OpCodes.unprefixed.FirstOrDefault(x => x.Value.addrInt == pcValue);
            opCodeKvp = opCodeKvp.Value == null ? OpCodes.cbprefixed.First(x => x.Value.addrInt == pcValue) : opCodeKvp;
            var opCode = opCodeKvp.Value;

            var handler = _handlers.SingleOrDefault(x => x.Handles(opCode));

            if (handler == null)
            {
                Debug.WriteLine("Don't know what to do with");
                Debug.WriteLine(JsonConvert.SerializeObject(opCode));
                return opCode;
            }

            handler.Execute(Registers, _gameBoy.Memory, opCode);

            ProgramCounter++;
            return opCode;
        }


    }

    public interface IHandleGameboyAssembly
    {
        bool Handles(Opcode opCode);
        void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode);
    }

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

    public class Nop :  OpCodeHandlerBase
    {
        public Nop() : base(0x00)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
           // Do Nothing.
        }
    }

    public class Jp : OpCodeHandlerBase
    {
        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            if (string.IsNullOrWhiteSpace(opCode.Operand2))
            {
                registers.SP = (ushort)ValueFrom(opCode.Operand1, registers);
            }
        }
    }

    public class LD_nn_n : OpCodeHandlerBase
    {
        public LD_nn_n()
            : base(0x06, 0x0E, 0x16, 0x1E, 0x26, 0x2E)
        {
        }

        public override void Execute(CpuRegisters registers, MemoryMap memory, Opcode opCode)
        {
            var val = ValueFrom(opCode.Operand2, registers);
            registers[opCode.Operand1] = val;
            Debug.WriteLine($"LD {opCode.Operand1.Value}, {val}");
        }
    }
}
 