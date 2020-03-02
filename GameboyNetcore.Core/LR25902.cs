using System;
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

            var opcodeKvp = OpCodes.unprefixed.FirstOrDefault(x => x.Value.addrInt == pcValue);
            opcodeKvp = opcodeKvp.Value == null ? OpCodes.cbprefixed.First(x => x.Value.addrInt == pcValue) : opcodeKvp;
            var opcode = opcodeKvp.Value;

            switch (opcode.mnemonic)
            {
                case "NOP":
                    break;
                case "JP":
                {
                    if (string.IsNullOrWhiteSpace(opcode.Operand2))
                    {
                        StackPointer = (ushort)ValueFrom(opcode.Operand1);
                    }

                    break;
                }
                case "LD":
                {
                    var val = ValueFrom(opcode.Operand2);
                    Registers[opcode.Operand1] = val;
                    Debug.WriteLine($"LD {opcode.Operand1.Value}, {val}");

                    break;
                }
                default:
                {
                    Debug.WriteLine("Don't know what to do with");
                    Debug.WriteLine(JsonConvert.SerializeObject(opcode));
                    break;
                }
            }
            
            ProgramCounter++;
            return opcode;
        }


        public object ValueFrom(Operand operand)
        {
            try
            {
                if (operand.Type == OperandType.Register8bit || operand.Type == OperandType.Register16bit)
                {
                    return Registers[operand.Value];
                }

                if(operand.Type == OperandType.ValueFrom16bitRegister)
                {
                    var identity = operand.Value.Substring(1, operand.Value.Length - 2);
                    return Registers[identity];
                }

                return Convert.ToUInt16(operand.Value, 16);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.WriteLine($"Could not convert {operand.Value}");
                throw;
            }
        }
    }
}
 