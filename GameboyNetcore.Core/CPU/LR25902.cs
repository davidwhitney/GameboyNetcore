using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameboyNetcore.Core.CPU.OpCodes;
using Newtonsoft.Json;

namespace GameboyNetcore.Core.CPU
{
    public class LR25902
    {
        private readonly GameBoy _gameBoy;
        private readonly List<IHandleGameboyAssembly> _handlers;

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

            _handlers = GetType()
                .Assembly
                .GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IHandleGameboyAssembly)))
                .Where(x => !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IHandleGameboyAssembly>()
                .ToList();
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
                throw new Exception($"No handler for opcode: {opCode.addr} - {JsonConvert.SerializeObject(opCode)}");
            }

            handler.Execute(Registers, _gameBoy.Memory, opCode);

            ProgramCounter++;
            return opCode;
        }
    }
}
 