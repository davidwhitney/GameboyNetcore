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
            var instruction = _gameBoy.Memory.Get(ProgramCounter);

            var opCode = StepOnce(instruction);

            ProgramCounter++;
            return opCode;
        }

        public Opcode StepOnce(byte instruction)
        {
            var opCodeKvp = OpCodes.unprefixed.FirstOrDefault(x => x.Value.addrInt == instruction);
            opCodeKvp = opCodeKvp.Value == null ? OpCodes.cbprefixed.First(x => x.Value.addrInt == instruction) : opCodeKvp;
            var opCode = opCodeKvp.Value;

            var handler = _handlers.SingleOrDefault(x => x.Handles(opCode));

            if (handler == null)
            {
                throw new Exception($"No handler for opcode: {opCode.addr} - {JsonConvert.SerializeObject(opCode)}");
            }

            handler.Execute(Registers, _gameBoy.Memory, opCode);
            return opCode;
        }


        /*
        
        private MemoryMap memory => _gameBoy.Memory;
        private void ProcessOpCode()
        {
           var opCode = memory[Registers.PC];
           var arg1 = memory[Registers.PC + 1];
           var arg2 = memory[Registers.PC + 2];

            Registers.PC++;

            byte value = 0, value2 = 0;
            int addr, value16;
            Tuple<byte, byte> bytePair;
            bool trueOrFa;

            switch (opCode)
            {
                case 0x00: break;
                case 0x01:
                    bytePair = SeperateBytes(arg2 * 256 + arg1);
                    Registers.B = bytePair.Item1;
                    Registers.C = bytePair.Item2;
                    Registers.PC += 2;
                    Registers.Cycles += 12;
                    break;
                case 0x02:
                    addr = Registers.B * 256 + Registers.C;
                    if (!IsRom(addr)) memory[addr] = Registers.A;
                    Registers.Cycles += 8;
                    break;
                case 0x03:
                    Registers.C++;
                    if (Registers.C == 0) Registers.B++;
                    Registers.Cycles += 8;
                    break;
                case 0x04:
                    SetFlags(Registers.B, 1, 3, 0, 3, -1);
                    Registers.B++;
                    Registers.Cycles += 4;
                    break;
                case 0x05:
                    SetFlags(Registers.B, 1, 2, 1, 2, -1);
                    Registers.B--;
                    Registers.Cycles += 4;
                    break;
                case 0x06:
                    Registers.B = arg1;
                    Registers.PC++;
                    Registers.Cycles += 8;
                    break;
                case 0x07: //RLC A
                    trueOrFa = Registers.A > 127;
                    Registers.A = SeperateBytes(Registers.A * 2).Item2;
                    if (trueOrFa) Registers.A++;
                    SetFlags(Registers.A, 0, 0, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x08:
                    Registers.PC += 2;
                    addr = arg2 * 256 + arg1;
                    bytePair = SeperateBytes(Registers.SP);
                    if (!IsRom(addr)) memory[addr] = bytePair.Item2;
                    if (!IsRom(addr + 1)) memory[addr + 1] = bytePair.Item1;
                    Registers.Cycles += 20;
                    break;
                case 0x09:
                    Registers.HL = Registers.H * 256 + Registers.L;
                    Registers.BC = Registers.B * 256 + Registers.C;
                    SetFlags(Registers.HL, Registers.BC, -1, 0, 4, 5);
                    Registers.HL += Registers.BC;
                    bytePair = SeperateBytes(Registers.HL);
                    Registers.H = bytePair.Item1;
                    Registers.L = bytePair.Item2;
                    Registers.Cycles += 8;
                    break;
                case 0x0a:
                    addr = Registers.B * 256 + Registers.C;
                    Registers.A = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x0b:
                    Registers.C--;
                    if (Registers.C == 255) Registers.B--;
                    Registers.Cycles += 8;
                    break;
                case 0x0c:
                    SetFlags(Registers.C, 1, 3, 0, 3, -1);
                    Registers.C++;
                    Registers.Cycles += 4;
                    break;
                case 0x0d:
                    SetFlags(Registers.C, 1, 2, 1, 2, -1);
                    Registers.C--;
                    Registers.Cycles += 4;
                    break;
                case 0x0e:
                    Registers.C = arg1;
                    Registers.PC++;
                    Registers.Cycles += 8;
                    break;
                case 0x0f:
                    trueOrFa = Registers.A % 2 != 0;
                    SetFlags(Registers.A, 0, 0, 0, 0, 6);
                    Registers.A = Math.Floor(Registers.A / 2);
                    if (trueOrFa) Registers.A += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x10: return false; // STOP
                case 0x11:
                    bytePair = SeperateBytes(arg2 * 256 + arg1);
                    Registers.D = bytePair.Item1;
                    Registers.E = bytePair.Item2;
                    Registers.PC += 2;
                    Registers.Cycles += 12;
                    break;
                case 0x12:
                    addr = Registers.D * 256 + Registers.E;
                    if (!IsRom(addr)) memory[addr] = Registers.A;
                    Registers.Cycles += 8;
                    break;
                case 0x13:
                    Registers.E++;
                    if (Registers.E == 0) Registers.D++;
                    Registers.Cycles += 8;
                    break;
                case 0x14:
                    SetFlags(Registers.D, 1, 3, 0, 3, -1);
                    Registers.D++;
                    Registers.Cycles += 4;
                    break;
                case 0x15:
                    SetFlags(Registers.D, 1, 2, 1, 2, -1);
                    Registers.D--;
                    Registers.Cycles += 4;
                    break;
                case 0x16:
                    Registers.D = arg1;
                    Registers.PC++;
                    Registers.Cycles += 8;
                    break;
                case 0x17:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, 0, 0, 0, 0, 3);
                    value16 = Registers.A * 2;
                    Registers.A = SeperateBytes(value16).Item2;
                    if (trueOrFa) Registers.A++;
                    Registers.Cycles += 8;
                    break;
                case 0x18:
                    Registers.PC++;
                    Registers.PC += arg1;
                    Registers.Cycles += 8;
                    break;
                case 0x19: // Registers.HL += DE
                    Registers.HL = Registers.H * 256 + Registers.L;
                    Registers.DE = Registers.D * 256 + Registers.E;
                    SetFlags(Registers.HL, Registers.DE, -1, 0, 4, 5);
                    Registers.HL += Registers.DE;
                    bytePair = SeperateBytes(Registers.HL);
                    Registers.H = bytePair.Item1;
                    Registers.L = bytePair.Item2;
                    Registers.Cycles += 8;
                    break;
                case 0x1a:
                    addr = Registers.D * 256 + Registers.E;
                    Registers.A = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x1b:
                    Registers.E--;
                    if (Registers.E == 255) Registers.D--;
                    Registers.Cycles += 8;
                    break;
                case 0x1c:
                    SetFlags(Registers.E, 1, 3, 0, 3, -1);
                    Registers.E++;
                    Registers.Cycles += 4;
                    break;
                case 0x1d:
                    SetFlags(Registers.E, 1, 2, 1, 2, -1);
                    Registers.E--;
                    Registers.Cycles += 4;
                    break;
                case 0x1e:
                    Registers.E = arg1;
                    Registers.PC++;
                    Registers.Cycles += 8;
                    break;
                case 0x1f: //RR A
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, 0, 0, 0, 0, 6);
                    Registers.A = Math.Floor(Registers.A / 2);
                    if (trueOrFa) Registers.A += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x20:
                    Registers.PC++;
                    if (!Registers.Flags.z) Registers.PC += (byte) arg1;
                    Registers.Cycles += 4;
                    break;
                case 0x21:
                    bytePair = SeperateBytes(arg2 * 256 + arg1);
                    Registers.H = bytePair.Item1;
                    Registers.L = bytePair.Item2;
                    Registers.PC += 2;
                    Registers.Cycles += 12;
                    break;
                case 0x22:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.A;
                    Registers.L++;
                    if (Registers.L == 0) Registers.H++;
                    Registers.Cycles += 8;
                    break;
                case 0x23:
                    Registers.L++;
                    if (Registers.L == 0) Registers.H++;
                    Registers.Cycles += 8;
                    break;
                case 0x24:
                    SetFlags(Registers.H, 1, 3, 0, 3, -1);
                    Registers.H++;
                    Registers.Cycles += 4;
                    break;
                case 0x25:
                    SetFlags(Registers.H, 1, 2, 1, 2, -1);
                    Registers.H--;
                    Registers.Cycles += 4;
                    break;
                case 0x26:
                    Registers.H = arg1;
                    Registers.PC++;
                    Registers.Cycles += 8;
                    break;
                case 0x27: //DAA Decimal Adjust A
                    trueOrFa = Registers.Flags.cy || !Registers.Flags.n && Registers.A > 0x99;
                    if (!Registers.Flags.n)
                    {
                        // Addition
                        if (Registers.Flags.h || (Registers.A & 0x0f) > 9) value += 0x06; /// Add 6 to lower_nibble
                        if (Registers.Flags.cy || Registers.A > 0x99) value += 0x60; // Add 6 to upper_nibble
                        Registers.A += value;
                    }
                    else
                    {
                        // Subtraction
                        if (Registers.Flags.h) Registers.A -= 0x06; /// Add 6 to lower_nibble
                        if (Registers.Flags.cy) Registers.A -= 0x60; // Add 6 to upper_nibble
                    }

                    SetFlags(Registers.A, 0, 2, -1, 0, trueOrFa);
                    Registers.Cycles += 16;
                    break;
                case 0x28:
                    Registers.PC++;
                    if (Registers.Flags.z) Registers.PC += (int8_t) arg1;
                    Registers.Cycles += 8;
                    break;
                case 0x29:
                    Registers.HL = Registers.H * 256 + Registers.L;
                    SetFlags(Registers.HL, Registers.HL, -1, 0, 4, 5);
                    Registers.HL += Registers.HL;
                    bytePair = SeperateBytes(Registers.HL);
                    Registers.H = bytePair.Item1;
                    Registers.L = bytePair.Item2;
                    Registers.Cycles += 8;
                    break;
                case 0x2a:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.A = memory[addr];
                    Registers.L++;
                    if (Registers.L == 0) Registers.H++;
                    Registers.Cycles += 8;
                    break;
                case 0x2b:
                    Registers.L--;
                    if (Registers.L == 255) Registers.H--;
                    Registers.Cycles += 8;
                    break;
                case 0x2c:
                    SetFlags(Registers.L, 1, 3, 0, 3, -1);
                    Registers.L++;
                    Registers.Cycles += 4;
                    break;
                case 0x2d:
                    SetFlags(Registers.L, 1, 2, 1, 2, -1);
                    Registers.L--;
                    Registers.Cycles += 4;
                    break;
                case 0x2e:
                    Registers.L = arg1;
                    Registers.PC++;
                    Registers.Cycles += 8;
                    break;
                case 0x2f:
                    SetFlags(0, 0, -1, 1, 1, -1);
                    Registers.A = (uint8_t) ~Registers.A;
                    Registers.Cycles += 4;
                    break;
                case 0x30:
                    Registers.PC++;
                    if (!Registers.Flags.cy) Registers.PC += (int8_t) arg1;
                    Registers.Cycles += 8;
                    break;
                case 0x31:
                    Registers.SP = arg2 * 256 + arg1;
                    Registers.PC += 2;
                    Registers.Cycles += 12;
                    break;
                case 0x32:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.A;
                    Registers.L--;
                    if (Registers.L == 255) Registers.H--;
                    Registers.Cycles += 8;
                    break;
                case 0x33:
                    Registers.SP++;
                    Registers.Cycles += 8;
                    break;
                case 0x34:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1, 3, 0, 3, -1);
                    memory[addr]++;
                    Registers.Cycles += 12;
                    break;
                case 0x35:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1, 2, 1, 2, -1);
                    memory[addr]--;
                    Registers.Cycles += 12;
                    break;
                case 0x36:
                    addr = Registers.H * 256 + Registers.L;
                    if (addr >= 0x2000 && addr <= 0x3FFF && Registers.CartIsMRegisters.BCType)
                    {
                        ///Setting ROM Mode
                        if (arg1 == 1)
                        {
                            Registers.romBankMode = 0;
                        }
                        else
                        {
                            Registers.romBankMode = arg1 & 0x7F;
                        }
                    }
                    else if (!IsRom(addr)) memory[addr] = arg1;

                    Registers.PC++;
                    Registers.Cycles += 12;
                    break;
                case 0x37:
                    SetFlags(0, 0, -1, 0, 0, 1);
                    Registers.Cycles += 4;
                    break;
                case 0x38:
                    Registers.PC++;
                    if (Registers.Flags.cy) Registers.PC += (int8_t) arg1;
                    Registers.Cycles += 8;
                    break;
                case 0x39:
                    Registers.HL = Registers.H * 256 + Registers.L;
                    SetFlags(Registers.HL, Registers.SP, -1, 0, 4, 5);
                    Registers.HL += Registers.SP;
                    bytePair = SeperateBytes(Registers.HL);
                    Registers.H = bytePair.Item1;
                    Registers.L = bytePair.Item2;
                    Registers.Cycles += 8;
                    break;
                case 0x3a:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.A = memory[addr];
                    Registers.L--;
                    if (Registers.L == 255) Registers.H--;
                    Registers.Cycles += 8;
                    break;
                case 0x3b:
                    Registers.SP--;
                    Registers.Cycles += 8;
                    break;
                case 0x3c:
                    SetFlags(Registers.A, 1, 3, 0, 3, -1);
                    Registers.A++;
                    Registers.Cycles += 4;
                    break;
                case 0x3d:
                    SetFlags(Registers.A, 1, 2, 1, 2, -1);
                    Registers.A--;
                    Registers.Cycles += 4;
                    break;
                case 0x3e:
                    Registers.A = arg1;
                    Registers.PC++;
                    Registers.Cycles += 8;
                    break;
                case 0x3f:
                    SetFlags(!Registers.Flags.cy, 0, -1, 0, 0, 6);
                    Registers.Cycles += 4;
                    break;

                case 0x40:
                    Registers.B = Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x41:
                    Registers.B = Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x42:
                    Registers.B = Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x43:
                    Registers.B = Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x44:
                    Registers.B = Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x45:
                    Registers.B = Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x46:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.B = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x47:
                    Registers.B = Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0x48:
                    Registers.C = Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x49:
                    Registers.C = Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x4a:
                    Registers.C = Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x4b:
                    Registers.C = Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x4c:
                    Registers.C = Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x4d:
                    Registers.C = Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x4e:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.C = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x4f:
                    Registers.C = Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0x50:
                    Registers.D = Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x51:
                    Registers.D = Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x52:
                    Registers.D = Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x53:
                    Registers.D = Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x54:
                    Registers.D = Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x55:
                    Registers.D = Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x56:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.D = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x57:
                    Registers.D = Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0x58:
                    Registers.E = Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x59:
                    Registers.E = Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x5a:
                    Registers.E = Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x5b:
                    Registers.E = Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x5c:
                    Registers.E = Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x5d:
                    Registers.E = Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x5e:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.E = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x5f:
                    Registers.E = Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0x60:
                    Registers.H = Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x61:
                    Registers.H = Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x62:
                    Registers.H = Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x63:
                    Registers.H = Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x64:
                    Registers.H = Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x65:
                    Registers.H = Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x66:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.H = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x67:
                    Registers.H = Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0x68:
                    Registers.L = Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x69:
                    Registers.L = Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x6a:
                    Registers.L = Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x6b:
                    Registers.L = Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x6c:
                    Registers.L = Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x6d:
                    Registers.L = Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x6e:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.L = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x6f:
                    Registers.L = Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0x70:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.B;
                    Registers.Cycles += 8;
                    break;
                case 0x71:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.C;
                    Registers.Cycles += 8;
                    break;
                case 0x72:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.D;
                    Registers.Cycles += 8;
                    break;
                case 0x73:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.E;
                    Registers.Cycles += 8;
                    break;
                case 0x74:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.H;
                    Registers.Cycles += 8;
                    break;
                case 0x75:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.L;
                    Registers.Cycles += 8;
                    break;

                case 0x76: /// HALT
                    Registers.Halted = true;
                    Registers.Cycles += 4;
                    break;
                case 0x77:
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = Registers.A;
                    Registers.Cycles += 8;
                    break;

                case 0x78:
                    Registers.A = Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x79:
                    Registers.A = Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x7a:
                    Registers.A = Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x7b:
                    Registers.A = Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x7c:
                    Registers.A = Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x7d:
                    Registers.A = Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x7e:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.A = memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x7f:
                    Registers.A = Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0x80:
                    SetFlags(Registers.A, Registers.B, 3, 0, 3, 4);
                    Registers.A += Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x81:
                    SetFlags(Registers.A, Registers.C, 3, 0, 3, 4);
                    Registers.A += Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x82:
                    SetFlags(Registers.A, Registers.D, 3, 0, 3, 4);
                    Registers.A += Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x83:
                    SetFlags(Registers.A, Registers.E, 3, 0, 3, 4);
                    Registers.A += Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x84:
                    SetFlags(Registers.A, Registers.H, 3, 0, 3, 4);
                    Registers.A += Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x85:
                    SetFlags(Registers.A, Registers.L, 3, 0, 3, 4);
                    Registers.A += Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x86:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(Registers.A, memory[addr], 3, 0, 3, 4);
                    Registers.A += memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x87:
                    SetFlags(Registers.A, Registers.A, 3, 0, 3, 4);
                    Registers.A += Registers.A;
                    Registers.Cycles += 4;
                    break;


                case 0x88:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.B, -1, -1, 5, 7);
                    Registers.A = Registers.A + Registers.B + trueOrFa;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x89:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.C, -1, -1, 5, 7);
                    Registers.A = Registers.A + Registers.C + trueOrFa;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x8a:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.D, -1, -1, 5, 7);
                    Registers.A = Registers.A + Registers.D + trueOrFa;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x8b:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.E, -1, -1, 5, 7);
                    Registers.A = Registers.A + Registers.E + trueOrFa;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x8c:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.H, -1, -1, 5, 7);
                    Registers.A = Registers.A + Registers.H + trueOrFa;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x8d:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.L, -1, -1, 5, 7);
                    Registers.A = Registers.A + Registers.L + trueOrFa;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x8e:
                    addr = Registers.H * 256 + Registers.L;
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, memory[addr], -1, -1, 5, 7);
                    Registers.A = Registers.A + memory[addr] + trueOrFa;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x8f:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.A, -1, -1, 5, 7);
                    Registers.A = Registers.A + Registers.A + trueOrFa;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 4;
                    break;

                case 0x90:
                    SetFlags(Registers.A, Registers.B, 2, 1, 2, 2);
                    Registers.A -= Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0x91:
                    SetFlags(Registers.A, Registers.C, 2, 1, 2, 2);
                    Registers.A -= Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0x92:
                    SetFlags(Registers.A, Registers.D, 2, 1, 2, 2);
                    Registers.A -= Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0x93:
                    SetFlags(Registers.A, Registers.E, 2, 1, 2, 2);
                    Registers.A -= Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0x94:
                    SetFlags(Registers.A, Registers.H, 2, 1, 2, 2);
                    Registers.A -= Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0x95:
                    SetFlags(Registers.A, Registers.L, 2, 1, 2, 2);
                    Registers.A -= Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0x96:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(Registers.A, memory[addr], 2, 1, 2, 2);
                    Registers.A -= memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0x97:
                    SetFlags(Registers.A, Registers.A, 2, 1, 2, 2);
                    Registers.A -= Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0x98:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.B, -1, -1, 6, 8);
                    Registers.A = Registers.A - Registers.B - trueOrFa;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x99:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.C, -1, -1, 6, 8);
                    Registers.A = Registers.A - Registers.C - trueOrFa;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x9a:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.D, -1, -1, 6, 8);
                    Registers.A = Registers.A - Registers.D - trueOrFa;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x9b:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.E, -1, -1, 6, 8);
                    Registers.A = Registers.A - Registers.E - trueOrFa;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x9c:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.H, -1, -1, 6, 8);
                    Registers.A = Registers.A - Registers.H - trueOrFa;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x9d:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.L, -1, -1, 6, 8);
                    Registers.A = Registers.A - Registers.L - trueOrFa;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 4;
                    break;
                case 0x9e:
                    addr = Registers.H * 256 + Registers.L;
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, memory[addr], -1, -1, 6, 8);
                    Registers.A = Registers.A - memory[addr] - trueOrFa;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x9f:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, Registers.A, -1, -1, 6, 8);
                    Registers.A = Registers.A - Registers.A - trueOrFa;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 4;
                    break;

                case 0xa0:
                    SetFlags(Registers.A, Registers.B, 5, 0, 1, 0);
                    Registers.A = Registers.A & Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0xa1:
                    SetFlags(Registers.A, Registers.C, 5, 0, 1, 0);
                    Registers.A = Registers.A & Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0xa2:
                    SetFlags(Registers.A, Registers.D, 5, 0, 1, 0);
                    Registers.A = Registers.A & Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0xa3:
                    SetFlags(Registers.A, Registers.E, 5, 0, 1, 0);
                    Registers.A = Registers.A & Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0xa4:
                    SetFlags(Registers.A, Registers.H, 5, 0, 1, 0);
                    Registers.A = Registers.A & Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0xa5:
                    SetFlags(Registers.A, Registers.L, 5, 0, 1, 0);
                    Registers.A = Registers.A & Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0xa6:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(Registers.A, memory[addr], 5, 0, 1, 0);
                    Registers.A = Registers.A & memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0xa7:
                    SetFlags(Registers.A, Registers.A, 5, 0, 1, 0);
                    Registers.A = Registers.A & Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0xa8:
                    SetFlags(Registers.A, Registers.B, 4, 0, 0, 0);
                    Registers.A = Registers.A ^ Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0xa9:
                    SetFlags(Registers.A, Registers.C, 4, 0, 0, 0);
                    Registers.A = Registers.A ^ Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0xaa:
                    SetFlags(Registers.A, Registers.D, 4, 0, 0, 0);
                    Registers.A = Registers.A ^ Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0xab:
                    SetFlags(Registers.A, Registers.E, 4, 0, 0, 0);
                    Registers.A = Registers.A ^ Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0xac:
                    SetFlags(Registers.A, Registers.H, 4, 0, 0, 0);
                    Registers.A = Registers.A ^ Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0xad:
                    SetFlags(Registers.A, Registers.L, 4, 0, 0, 0);
                    Registers.A = Registers.A ^ Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0xae:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(Registers.A, memory[addr], 4, 0, 0, 0);
                    Registers.A = Registers.A ^ memory[addr];
                    Registers.Cycles += 4;
                    break;
                case 0xaf:
                    SetFlags(Registers.A, Registers.A, 4, 0, 0, 0);
                    Registers.A = Registers.A ^ Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0xb0:
                    SetFlags(Registers.A, Registers.B, 7, 0, 0, 0);
                    Registers.A = Registers.A | Registers.B;
                    Registers.Cycles += 4;
                    break;
                case 0xb1:
                    SetFlags(Registers.A, Registers.C, 7, 0, 0, 0);
                    Registers.A = Registers.A | Registers.C;
                    Registers.Cycles += 4;
                    break;
                case 0xb2:
                    SetFlags(Registers.A, Registers.D, 7, 0, 0, 0);
                    Registers.A = Registers.A | Registers.D;
                    Registers.Cycles += 4;
                    break;
                case 0xb3:
                    SetFlags(Registers.A, Registers.E, 7, 0, 0, 0);
                    Registers.A = Registers.A | Registers.E;
                    Registers.Cycles += 4;
                    break;
                case 0xb4:
                    SetFlags(Registers.A, Registers.H, 7, 0, 0, 0);
                    Registers.A = Registers.A | Registers.H;
                    Registers.Cycles += 4;
                    break;
                case 0xb5:
                    SetFlags(Registers.A, Registers.L, 7, 0, 0, 0);
                    Registers.A = Registers.A | Registers.L;
                    Registers.Cycles += 4;
                    break;
                case 0xb6:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(Registers.A, memory[addr], 7, 0, 0, 0);
                    Registers.A = Registers.A | memory[addr];
                    Registers.Cycles += 8;
                    break;
                case 0xb7:
                    SetFlags(Registers.A, Registers.A, 7, 0, 0, 0);
                    Registers.A = Registers.A | Registers.A;
                    Registers.Cycles += 4;
                    break;

                case 0xb8:
                    SetFlags(Registers.A, Registers.B, 2, 1, 2, 2);
                    Registers.Cycles += 8;
                    break;
                case 0xb9:
                    SetFlags(Registers.A, Registers.C, 2, 1, 2, 2);
                    Registers.Cycles += 8;
                    break;
                case 0xba:
                    SetFlags(Registers.A, Registers.D, 2, 1, 2, 2);
                    Registers.Cycles += 8;
                    break;
                case 0xbb:
                    SetFlags(Registers.A, Registers.E, 2, 1, 2, 2);
                    Registers.Cycles += 8;
                    break;
                case 0xRegisters.BC:
                    SetFlags(Registers.A, Registers.H, 2, 1, 2, 2);
                    Registers.Cycles += 8;
                    break;
                case 0xbd:
                    SetFlags(Registers.A, Registers.L, 2, 1, 2, 2);
                    Registers.Cycles += 8;
                    break;
                case 0xbe:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(Registers.A, memory[addr], 2, 1, 2, 2);
                    Registers.Cycles += 8;
                    break;
                case 0xbf:
                    SetFlags(Registers.A, Registers.A, 2, 1, 2, 2);
                    Registers.Cycles += 8;
                    break;

                case 0xc0:
                    if (!Registers.Flags.z)
                    {
                        Registers.PC = memory[Registers.SP + 1] * 256 + memory[Registers.SP];
                        Registers.SP += 2;
                        Registers.Cycles += 8;
                        break;
                    }

                    Registers.Cycles += 4;
                    break;
                case 0xc1:
                    Registers.B = memory[Registers.SP + 1];
                    Registers.C = memory[Registers.SP];
                    Registers.SP += 2;
                    Registers.Cycles += 12;
                    break;
                case 0xc2:
                    !Registers.Flags.z ? Registers.PC = arg2 * 256 + arg1 : Registers.PC += 2;
                    Registers.Cycles += 12;
                    break;
                case 0xc3:
                    Registers.PC = arg2 * 256 + arg1;
                    Registers.Cycles += 12;
                    break;
                case 0xc4:
                    if (!Registers.Flags.z)
                    {
                        if (!IsRom(Registers.SP - 1))
                            memory[Registers.SP - 1] = SeperateBytes(Registers.PC + 2).Item1;
                        if (!IsRom(Registers.SP - 2))
                            memory[Registers.SP - 2] = SeperateBytes(Registers.PC + 2).Item2;
                        Registers.SP -= 2;
                        Registers.PC = arg2 * 256 + arg1;
                    }
                    else
                    {
                        Registers.PC += 2;
                    }

                    Registers.Cycles += 12;
                    break;
                case 0xc5:
                    if (!IsRom(Registers.SP - 2)) memory[Registers.SP - 2] = Registers.C;
                    if (!IsRom(Registers.SP - 1)) memory[Registers.SP - 1] = Registers.B;
                    Registers.SP -= 2;
                    Registers.Cycles += 16;
                    break;
                case 0xc6:
                    Registers.PC++;
                    SetFlags(Registers.A, arg1, 3, 0, 3, 4);
                    Registers.A += arg1;
                    Registers.Cycles += 8;
                    break;
                case 0xc7:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC).Item2;
                    Registers.SP -= 2;
                    Registers.PC = 0x00;
                    Registers.Cycles += 32;
                    break;
                case 0xc8:
                    if (Registers.Flags.z)
                    {
                        Registers.PC = memory[Registers.SP + 1] * 256 +
                                    memory[Registers.SP];
                        Registers.SP += 2;
                        Registers.Cycles += 8;
                        break;
                    }

                    Registers.Cycles += 4;
                    break;
                case 0xc9:
                    Registers.PC = memory[Registers.SP + 1] * 256 + memory[Registers.SP];
                    Registers.SP += 2;
                    Registers.Cycles += 8;
                    break;
                case 0xca:
                    Registers.Flags.z ? Registers.PC = arg2 * 256 + arg1 : Registers.PC += 2;
                    Registers.Cycles += 12;
                    break;
                case 0xcb: break; /// more opCodes
                case 0xcc:
                    if (Registers.Flags.z)
                    {
                        if (!IsRom(Registers.SP - 1))
                            memory[Registers.SP - 1] = SeperateBytes(Registers.PC + 2).Item1;
                        if (!IsRom(Registers.SP - 2))
                            memory[Registers.SP - 2] = SeperateBytes(Registers.PC + 2).Item2;
                        Registers.SP -= 2;
                        Registers.PC = arg2 * 256 + arg1;
                    }
                    else
                    {
                        Registers.PC += 2;
                    }

                    Registers.Cycles += 12;
                    break;
                case 0xcd:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC + 2).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC + 2).Item2;
                    Registers.SP -= 2;
                    Registers.PC = arg2 * 256 + arg1;
                    Registers.Cycles += 12;
                    break;
                case 0xce:
                    Registers.PC++;
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, arg1, -1, -1, 5, 7);
                    Registers.A = Registers.A + trueOrFa + arg1;
                    SetFlags(Registers.A, 0, 3, 0, -1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0xcf:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC).Item2;
                    Registers.SP -= 2;
                    Registers.PC = 0x08;
                    Registers.Cycles += 32;
                    break;
                case 0xd0:
                    if (!Registers.Flags.cy)
                    {
                        Registers.PC = memory[Registers.SP + 1] * 256 +
                                    memory[Registers.SP];
                        Registers.SP += 2;
                        Registers.Cycles += 8;
                        break;
                    }

                    Registers.Cycles += 4;
                    break;
                case 0xd1:
                    Registers.D = memory[Registers.SP + 1];
                    Registers.E = memory[Registers.SP];
                    Registers.SP += 2;
                    Registers.Cycles += 12;
                    break;
                case 0xd2:
                    !Registers.Flags.cy ? Registers.PC = arg2 * 256 + arg1 : Registers.PC += 2;
                    Registers.Cycles += 12;
                    break;
                case 0xd4:
                    if (!Registers.Flags.cy)
                    {
                        if (!IsRom(Registers.SP - 1))
                            memory[Registers.SP - 1] = SeperateBytes(Registers.PC + 2).Item1;
                        if (!IsRom(Registers.SP - 2))
                            memory[Registers.SP - 2] = SeperateBytes(Registers.PC + 2).Item2;
                        Registers.SP -= 2;
                        Registers.PC = arg2 * 256 + arg1;
                    }
                    else
                    {
                        Registers.PC += 2;
                    }

                    Registers.Cycles += 12;
                    break;
                case 0xd5:
                    if (!IsRom(Registers.SP - 2)) memory[Registers.SP - 2] = Registers.E;
                    if (!IsRom(Registers.SP - 1)) memory[Registers.SP - 1] = Registers.D;
                    Registers.SP -= 2;
                    Registers.Cycles += 16;
                    break;
                case 0xd6:
                    Registers.PC++;
                    SetFlags(Registers.A, arg1, 2, 1, 2, 2);
                    Registers.A -= arg1;
                    Registers.Cycles += 8;
                    break;
                case 0xd7:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC).Item2;
                    Registers.SP -= 2;
                    Registers.PC = 0x10;
                    Registers.Cycles += 32;
                    break;
                case 0xd8:
                    if (Registers.Flags.cy)
                    {
                        Registers.PC = memory[Registers.SP + 1] * 256 +
                                    memory[Registers.SP];
                        Registers.SP += 2;
                        Registers.Cycles += 8;
                        break;
                    }

                    Registers.Cycles += 4;
                    break;
                case 0xd9: //RETI return interrupt
                    Registers.PC = memory[Registers.SP + 1] * 256 + memory[Registers.SP];
                    Registers.SP += 2;
                    Registers.interrupt_enable = true;
                    Registers.Cycles += 8;
                    break;
                case 0xda:
                    Registers.Flags.cy ? Registers.PC = arg2 * 256 + arg1 : Registers.PC += 2;
                    Registers.Cycles += 12;
                    break;
                case 0xdc:
                    if (Registers.Flags.cy)
                    {
                        if (!IsRom(Registers.SP - 1))
                            memory[Registers.SP - 1] = SeperateBytes(Registers.PC + 2).Item1;
                        if (!IsRom(Registers.SP - 2))
                            memory[Registers.SP - 2] = SeperateBytes(Registers.PC + 2).Item2;
                        Registers.SP -= 2;
                        Registers.PC = arg2 * 256 + arg1;
                    }
                    else
                    {
                        Registers.PC += 2;
                    }

                    Registers.Cycles += 12;
                    break;
                case 0xde:
                    Registers.PC++;
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, arg1, -1, -1, 6, 8);
                    Registers.A = Registers.A - trueOrFa - arg1;
                    SetFlags(Registers.A, 0, 3, 1, -1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0xdf:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC).Item2;
                    Registers.SP -= 2;
                    Registers.PC = 0x18;
                    Registers.Cycles += 32;
                    break;
                case 0xe0: // Write to special Registers
                    if (arg1 == 0) memory[0xff00] = Registers.A + 15; //Writing to special inputByte
                    else if (arg1 == 0x46) initiateDMATransfer(Registers.A * 0x100);
                    else if (arg1 == 0x50) overwriteBootRom();
                    else memory[0xff00 + arg1] = Registers.A;
                    Registers.PC++;
                    Registers.Cycles += 12;
                    break;
                case 0xe1:
                    Registers.H = memory[Registers.SP + 1];
                    Registers.L = memory[Registers.SP];
                    Registers.SP += 2;
                    Registers.Cycles += 12;
                    break;
                case 0xe2:
                    memory[0xff00 + Registers.C] = Registers.A;
                    Registers.Cycles += 8;
                    break;
                case 0xe5:
                    if (!IsRom(Registers.SP - 2)) memory[Registers.SP - 2] = Registers.L;
                    if (!IsRom(Registers.SP - 1)) memory[Registers.SP - 1] = Registers.H;
                    Registers.SP -= 2;
                    Registers.Cycles += 16;
                    break;
                case 0xe6:
                    Registers.PC++;
                    SetFlags(Registers.A, arg1, 5, 0, 1, 0);
                    Registers.A = Registers.A & arg1;
                    Registers.Cycles += 8;
                    break;
                case 0xe7:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC).Item2;
                    Registers.SP -= 2;
                    Registers.PC = 0x20;
                    Registers.Cycles += 32;
                    break;
                case 0xe8:
                    Registers.PC++;
                    value16 = Registers.SP ^ (int8_t) arg1;
                    Registers.SP += (int8_t) arg1;
                    value = ((value16 ^ (Registers.SP & 0xFFFF)) & 0x10) == 0x10;
                    value2 = ((value16 ^ (Registers.SP & 0xFFFF)) & 0x100) == 0x100;
                    SetFlags(0, 0, 0, 0, value, value2);
                    Registers.Cycles += 16;
                    break;
                case 0xe9:
                    addr = Registers.H * 256 + Registers.L;
                    Registers.PC = addr;
                    Registers.Cycles += 4;
                    break;
                case 0xea: //Includes writing to Rom Mode
                    addr = arg2 * 256 + arg1;
                    if (addr >= 0x8000)
                        if (!IsRom(addr))
                            memory[addr] = Registers.A;
                    if (addr >= 0x2000 && addr <= 0x3FFF && Registers.CartIsMRegisters.BCType)
                    {
                        ///Setting ROM Mode
                        if (Registers.A == 1)
                        {
                            Registers.romBankMode = 0;
                        }
                        else
                        {
                            Registers.romBankMode = Registers.A & 0x7F;
                        }
                    }

                    if (addr >= 0x4000 && addr <= 0x5FFF && Registers.CartIsMRegisters.BCType)
                    {
                        if (Registers.A >= 0 && Registers.A <= 3) Registers.ramBankMode = Registers.A;
                    }

                    Registers.PC += 2;
                    Registers.Cycles += 16;
                    break;
                case 0xee:
                    Registers.PC++;
                    SetFlags(Registers.A, arg1, 4, 0, 0, 0);
                    Registers.A = Registers.A ^ arg1;
                    Registers.Cycles += 8;
                    break;
                case 0xef:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC).Item2;
                    Registers.SP -= 2;
                    Registers.PC = 0x28;
                    Registers.Cycles += 32;
                    break;

                case 0xf0: //Read from Special Registers / Read from input if arg1 == 0
                    if (arg1 == 0 && memory[0xff00] == 31) Registers.A = 16 + Registers.ButtonInputBits;
                    else if (arg1 == 0 && memory[0xff00] == 47) Registers.A = 32 + Registers.DirectionInputBits;
                    else Registers.A = memory[0xff00 + arg1];
                    Registers.PC++;
                    Registers.Cycles += 4;
                    break;

                case 0xf1:
                    Registers.A = memory[Registers.SP + 1];
                    value = memory[Registers.SP];
                    restoreFlagsFromByte(value);
                    Registers.SP += 2;
                    Registers.Cycles += 12;
                    break;
                case 0xf2:
                    Registers.A = memory[0xff00 + Registers.C];
                    Registers.Cycles += 8;
                    break;
                case 0xf3:
                    Registers.interrupt_enable = false;
                    Registers.Cycles += 4;
                    break;
                case 0xf5:
                    if (!IsRom(Registers.SP - 2)) memory[Registers.SP - 2] = convertFlagsToByte();
                    if (!IsRom(Registers.SP - 1)) memory[Registers.SP - 1] = Registers.A;
                    Registers.SP -= 2;
                    Registers.Cycles += 16;
                    break;
                case 0xf6:
                    Registers.PC++;
                    SetFlags(Registers.A, arg1, 7, 0, 0, 0);
                    Registers.A = Registers.A | arg1;
                    Registers.Cycles += 8;
                    break;
                case 0xf7:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC).Item2;
                    Registers.SP -= 2;
                    Registers.PC = 0x30;
                    Registers.Cycles += 32;
                    break;
                case 0xf8:
                    Registers.PC++;
                    value16 = Registers.SP ^ (int8_t) arg1;
                    Registers.HL = Registers.SP + (int8_t) arg1;
                    value = ((value16 ^ (Registers.HL & 0xFFFF)) & 0x10) == 0x10;
                    value2 = ((value16 ^ (Registers.HL & 0xFFFF)) & 0x100) == 0x100;
                    bytePair = SeperateBytes(Registers.HL);
                    Registers.H = bytePair.Item1;
                    Registers.L = bytePair.Item2;
                    SetFlags(0, 0, 0, 0, value, value2);
                    Registers.Cycles += 16;
                    break;
                case 0xf9:
                    Registers.SP = Registers.H * 256 + Registers.L;
                    Registers.Cycles += 8;
                    break;
                case 0xfa:
                    Registers.PC += 2;
                    addr = arg2 * 256 + arg1;
                    Registers.A = memory[addr];
                    Registers.Cycles += 16;
                    break;
                case 0xfb:
                    Registers.interrupt_enable = true;
                    Registers.Cycles += 4;
                    break;
                case 0xfe:
                    SetFlags(Registers.A, arg1, 2, 1, 2, 2);
                    Registers.Cycles += 4;
                    Registers.PC++;
                    break;
                case 0xff:
                    if (!IsRom(Registers.SP - 1))
                        memory[Registers.SP - 1] = SeperateBytes(Registers.PC).Item1;
                    if (!IsRom(Registers.SP - 2))
                        memory[Registers.SP - 2] = SeperateBytes(Registers.PC).Item2;
                    Registers.SP -= 2;
                    Registers.PC = 0x38;
                    Registers.Cycles += 32;
                    break;
                default: return unimplementedopCode(opCode);
            }

            if (opCode != 0xcb) return true; /// Normal endpoint

            byte cb_opCode = memory[Registers.PC];
            Registers.PC++;

            switch (cb_opCode)
            {
                case 0x00: //RLC B
                    trueOrFa = Registers.B > 127;
                    Registers.B = SeperateBytes(Registers.B * 2).Item2;
                    if (trueOrFa) Registers.B++;
                    SetFlags(Registers.B, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x01: //RLC C
                    trueOrFa = Registers.C > 127;
                    Registers.C = SeperateBytes(Registers.C * 2).Item2;
                    if (trueOrFa) Registers.C++;
                    SetFlags(Registers.C, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x02: //RLC D
                    trueOrFa = Registers.D > 127;
                    Registers.D = SeperateBytes(Registers.D * 2).Item2;
                    if (trueOrFa) Registers.D++;
                    SetFlags(Registers.D, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x03: //RLC E
                    trueOrFa = Registers.E > 127;
                    Registers.E = SeperateBytes(Registers.E * 2).Item2;
                    if (trueOrFa) Registers.E++;
                    SetFlags(Registers.E, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x04: //RLC H
                    trueOrFa = Registers.H > 127;
                    Registers.H = SeperateBytes(Registers.H * 2).Item2;
                    if (trueOrFa) Registers.H++;
                    SetFlags(Registers.H, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x05: //RLC L
                    trueOrFa = Registers.L > 127;
                    Registers.L = SeperateBytes(Registers.L * 2).Item2;
                    if (trueOrFa) Registers.L++;
                    SetFlags(Registers.L, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x06: //RLC (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    trueOrFa = memory[addr] > 127;
                    if (!IsRom(addr))
                        memory[addr] = SeperateBytes(memory[addr] * 2).Item2;
                    if (trueOrFa) memory[addr]++;
                    SetFlags(memory[addr], 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 16;
                    break;
                case 0x07: //RLC A
                    trueOrFa = Registers.A > 127;
                    Registers.A = SeperateBytes(Registers.A * 2).Item2;
                    if (trueOrFa) Registers.A++;
                    SetFlags(Registers.A, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;

                case 0x08: //RRC B
                    trueOrFa = Registers.B % 2 != 0;
                    Registers.B = Math.Floor(Registers.B / 2);
                    if (trueOrFa) Registers.B += 128;
                    SetFlags(Registers.B, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x09: //RRC C
                    trueOrFa = Registers.C % 2 != 0;
                    Registers.C = Math.Floor(Registers.C / 2);
                    if (trueOrFa) Registers.C += 128;
                    SetFlags(Registers.C, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x0a: //RRC D
                    trueOrFa = Registers.D % 2 != 0;
                    Registers.D = Math.Floor(Registers.D / 2);
                    if (trueOrFa) Registers.D += 128;
                    SetFlags(Registers.D, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x0b: //RRC E
                    trueOrFa = Registers.E % 2 != 0;
                    Registers.E = Math.Floor(Registers.E / 2);
                    if (trueOrFa) Registers.E += 128;
                    SetFlags(Registers.E, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x0c: //RRC H
                    trueOrFa = Registers.H % 2 != 0;
                    Registers.H = Math.Floor(Registers.H / 2);
                    if (trueOrFa) Registers.H += 128;
                    SetFlags(Registers.H, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x0d: //RRC L
                    trueOrFa = Registers.L % 2 != 0;
                    Registers.L = Math.Floor(Registers.L / 2);
                    if (trueOrFa) Registers.L += 128;
                    SetFlags(Registers.L, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x0e: //RRC (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    trueOrFa = memory[addr] % 2 != 0;
                    if (!IsRom(addr)) memory[addr] = Math.Floor(memory[addr] / 2);
                    if (trueOrFa) memory[addr] += 128;
                    SetFlags(memory[addr], 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 16;
                    break;
                case 0x0f: //RRC A
                    trueOrFa = Registers.A % 2 != 0;
                    Registers.A = Math.Floor(Registers.A / 2);
                    if (trueOrFa) Registers.A += 128;
                    SetFlags(Registers.A, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;

                case 0x10:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.B, 0, 6, 0, 0, 3);
                    value16 = Registers.B * 2;
                    Registers.B = SeperateBytes(value16).Item2;
                    if (trueOrFa) Registers.B++;
                    Registers.Cycles += 8;
                    break;
                case 0x11:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.C, 0, 6, 0, 0, 3);
                    value16 = Registers.C * 2;
                    Registers.C = SeperateBytes(value16).Item2;
                    if (trueOrFa) Registers.C++;
                    Registers.Cycles += 8;
                    break;
                case 0x12:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.D, 0, 6, 0, 0, 3);
                    value16 = Registers.D * 2;
                    Registers.D = SeperateBytes(value16).Item2;
                    if (trueOrFa) Registers.D++;
                    Registers.Cycles += 8;
                    break;
                case 0x13:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.E, 0, 6, 0, 0, 3);
                    value16 = Registers.E * 2;
                    Registers.E = SeperateBytes(value16).Item2;
                    if (trueOrFa) Registers.E++;
                    Registers.Cycles += 8;
                    break;
                case 0x14:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.H, 0, 6, 0, 0, 3);
                    value16 = Registers.H * 2;
                    Registers.H = SeperateBytes(value16).Item2;
                    if (trueOrFa) Registers.H++;
                    Registers.Cycles += 8;
                    break;
                case 0x15:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.L, 0, 6, 0, 0, 3);
                    value16 = Registers.L * 2;
                    Registers.L = SeperateBytes(value16).Item2;
                    if (trueOrFa) Registers.L++;
                    Registers.Cycles += 8;
                    break;
                case 0x16:
                    addr = Registers.H * 256 + Registers.L;
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(memory[addr], 0, 6, 0, 0, 3);
                    value16 = memory[addr] * 2;
                    if (!IsRom(addr)) memory[addr] = SeperateBytes(value16).Item2;
                    if (trueOrFa) memory[addr]++;
                    Registers.Cycles += 16;
                    break;
                case 0x17:
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, 0, 6, 0, 0, 3);
                    value16 = Registers.A * 2;
                    Registers.A = SeperateBytes(value16).Item2;
                    if (trueOrFa) Registers.A++;
                    Registers.Cycles += 8;
                    break;

                case 0x18: //RR B
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.B, 0, 8, 0, 0, 6);
                    Registers.B = Math.Floor(Registers.B / 2);
                    if (trueOrFa) Registers.B += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x19: //RR C
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.C, 0, 8, 0, 0, 6);
                    Registers.C = Math.Floor(Registers.C / 2);
                    if (trueOrFa) Registers.C += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x1a: //RR D
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.D, 0, 8, 0, 0, 6);
                    Registers.D = Math.Floor(Registers.D / 2);
                    if (trueOrFa) Registers.D += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x1b: //RR E
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.E, 0, 8, 0, 0, 6);
                    Registers.E = Math.Floor(Registers.E / 2);
                    if (trueOrFa) Registers.E += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x1c: //RR H
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.H, 0, 8, 0, 0, 6);
                    Registers.H = Math.Floor(Registers.H / 2);
                    if (trueOrFa) Registers.H += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x1d: //RR L
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.L, 0, 8, 0, 0, 6);
                    Registers.L = Math.Floor(Registers.L / 2);
                    if (trueOrFa) Registers.L += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x1e: //RR A
                    addr = Registers.H * 256 + Registers.L;
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(memory[addr], 0, 8, 0, 0, 6);
                    if (!IsRom(addr)) memory[addr] = Math.Floor(memory[addr] / 2);
                    if (trueOrFa) memory[addr] += 128;
                    Registers.Cycles += 16;
                    break;
                case 0x1f: //RR A
                    trueOrFa = Registers.Flags.cy;
                    SetFlags(Registers.A, 0, 8, 0, 0, 6);
                    Registers.A = Math.Floor(Registers.A / 2);
                    if (trueOrFa) Registers.A += 128;
                    Registers.Cycles += 8;
                    break;

                case 0x20:
                    trueOrFa = Registers.B > 127;
                    Registers.B = SeperateBytes(Registers.B * 2).Item2;
                    SetFlags(Registers.B, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x21:
                    trueOrFa = Registers.C > 127;
                    Registers.C = SeperateBytes(Registers.C * 2).Item2;
                    SetFlags(Registers.C, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x22:
                    trueOrFa = Registers.D > 127;
                    Registers.D = SeperateBytes(Registers.D * 2).Item2;
                    SetFlags(Registers.D, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x23:
                    trueOrFa = Registers.E > 127;
                    Registers.E = SeperateBytes(Registers.E * 2).Item2;
                    SetFlags(Registers.E, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x24:
                    trueOrFa = Registers.H > 127;
                    Registers.H = SeperateBytes(Registers.H * 2).Item2;
                    SetFlags(Registers.H, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x25:
                    trueOrFa = Registers.L > 127;
                    Registers.L = SeperateBytes(Registers.L * 2).Item2;
                    SetFlags(Registers.L, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x26:
                    addr = Registers.H * 256 + Registers.L;
                    trueOrFa = memory[addr] > 127;
                    if (!IsRom(addr))
                        memory[addr] = SeperateBytes(memory[addr] * 2).Item2;
                    SetFlags(memory[addr], 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;
                case 0x27:
                    trueOrFa = Registers.A > 127;
                    Registers.A = SeperateBytes(Registers.A * 2).Item2;
                    SetFlags(Registers.A, 0, 2, 0, 0, trueOrFa);
                    Registers.Cycles += 8;
                    break;

                case 0x28: //SRA B
                    trueOrFa = Registers.B > 127;
                    SetFlags(Registers.B, 0, 9, 0, 0, 6);
                    Registers.B = Math.Floor(Registers.B / 2);
                    if (trueOrFa) Registers.B += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x29: //SRA C
                    trueOrFa = Registers.C > 127;
                    SetFlags(Registers.C, 0, 9, 0, 0, 6);
                    Registers.C = Math.Floor(Registers.C / 2);
                    if (trueOrFa) Registers.C += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x2a: //SRA D
                    trueOrFa = Registers.D > 127;
                    SetFlags(Registers.D, 0, 9, 0, 0, 6);
                    Registers.D = Math.Floor(Registers.D / 2);
                    if (trueOrFa) Registers.D += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x2b: //SRA E
                    trueOrFa = Registers.E > 127;
                    SetFlags(Registers.E, 0, 9, 0, 0, 6);
                    Registers.E = Math.Floor(Registers.E / 2);
                    if (trueOrFa) Registers.E += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x2c: //SRA H
                    trueOrFa = Registers.H > 127;
                    SetFlags(Registers.H, 0, 9, 0, 0, 6);
                    Registers.H = Math.Floor(Registers.H / 2);
                    if (trueOrFa) Registers.H += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x2d: //SRA L
                    trueOrFa = Registers.L > 127;
                    SetFlags(Registers.L, 0, 9, 0, 0, 6);
                    Registers.L = Math.Floor(Registers.L / 2);
                    if (trueOrFa) Registers.L += 128;
                    Registers.Cycles += 8;
                    break;
                case 0x2e: //SRA (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    trueOrFa = memory[addr] > 127;
                    SetFlags(memory[addr], 0, 9, 0, 0, 6);
                    if (!IsRom(addr)) memory[addr] = Math.Floor(memory[addr] / 2);
                    if (trueOrFa) memory[addr] += 128;
                    Registers.Cycles += 16;
                    break;
                case 0x2f: //SRA A
                    trueOrFa = Registers.A > 127;
                    SetFlags(Registers.A, 0, 9, 0, 0, 6);
                    Registers.A = Math.Floor(Registers.A / 2);
                    if (trueOrFa) Registers.A += 128;
                    Registers.Cycles += 8;
                    break;

                case 0x30:
                    SetFlags(Registers.B, 0, 2, 0, 0, 0);
                    value = Registers.B & 0x0F;
                    value2 = (Registers.B & 0xF0) >> 4;
                    Registers.B = value * 16 + value2;
                    Registers.Cycles += 8;
                    break;
                case 0x31:
                    SetFlags(Registers.C, 0, 2, 0, 0, 0);
                    value = Registers.C & 0x0F;
                    value2 = (Registers.C & 0xF0) >> 4;
                    Registers.C = value * 16 + value2;
                    Registers.Cycles += 8;
                    break;
                case 0x32:
                    SetFlags(Registers.D, 0, 2, 0, 0, 0);
                    value = Registers.D & 0x0F;
                    value2 = (Registers.D & 0xF0) >> 4;
                    Registers.D = value * 16 + value2;
                    Registers.Cycles += 8;
                    break;
                case 0x33:
                    SetFlags(Registers.E, 0, 2, 0, 0, 0);
                    value = Registers.E & 0x0F;
                    value2 = (Registers.E & 0xF0) >> 4;
                    Registers.E = value * 16 + value2;
                    Registers.Cycles += 8;
                    break;
                case 0x34:
                    SetFlags(Registers.H, 0, 2, 0, 0, 0);
                    value = Registers.H & 0x0F;
                    value2 = (Registers.H & 0xF0) >> 4;
                    Registers.H = value * 16 + value2;
                    Registers.Cycles += 8;
                    break;
                case 0x35:
                    SetFlags(Registers.L, 0, 2, 0, 0, 0);
                    value = Registers.L & 0x0F;
                    value2 = (Registers.L & 0xF0) >> 4;
                    Registers.L = value * 16 + value2;
                    Registers.Cycles += 8;
                    break;
                case 0x36:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 0, 2, 0, 0, 0);
                    value = memory[addr] & 0x0F;
                    value2 = (memory[addr] & 0xF0) >> 4;
                    if (!IsRom(addr)) memory[addr] = value * 16 + value2;
                    Registers.Cycles += 16;
                    break;
                case 0x37:
                    SetFlags(Registers.A, 0, 2, 0, 0, 0);
                    value = Registers.A & 0x0F;
                    value2 = (Registers.A & 0xF0) >> 4;
                    Registers.A = value * 16 + value2;
                    Registers.Cycles += 8;
                    break;

                case 0x38:
                    SetFlags(Registers.B, 254, 5, 0, 0, 6);
                    Registers.B = Math.Floor(Registers.B / 2);
                    Registers.Cycles += 8;
                    break;
                case 0x39:
                    SetFlags(Registers.C, 254, 5, 0, 0, 6);
                    Registers.C = Math.Floor(Registers.C / 2);
                    Registers.Cycles += 8;
                    break;
                case 0x3a:
                    SetFlags(Registers.D, 254, 5, 0, 0, 6);
                    Registers.D = Math.Floor(Registers.D / 2);
                    Registers.Cycles += 8;
                    break;
                case 0x3b:
                    SetFlags(Registers.E, 254, 5, 0, 0, 6);
                    Registers.E = Math.Floor(Registers.E / 2);
                    Registers.Cycles += 8;
                    break;
                case 0x3c:
                    SetFlags(Registers.H, 254, 5, 0, 0, 6);
                    Registers.H = Math.Floor(Registers.H / 2);
                    Registers.Cycles += 8;
                    break;
                case 0x3d:
                    SetFlags(Registers.L, 254, 5, 0, 0, 6);
                    Registers.L = Math.Floor(Registers.L / 2);
                    Registers.Cycles += 8;
                    break;
                case 0x3e:
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 254, 5, 0, 0, 6);
                    if (!IsRom(addr)) memory[addr] = Math.Floor(memory[addr] / 2);
                    Registers.Cycles += 16;
                    break;
                case 0x3f:
                    SetFlags(Registers.A, 254, 5, 0, 0, 6);
                    Registers.A = Math.Floor(Registers.A / 2);
                    Registers.Cycles += 8;
                    break;

                case 0x40: ///bit 0 of register b
                    SetFlags(Registers.B, 1 << 0, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x41: ///bit 0 of register c
                    SetFlags(Registers.C, 1 << 0, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x42: ///bit 0 of register d
                    SetFlags(Registers.D, 1 << 0, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x43: ///bit 0 of register e
                    SetFlags(Registers.E, 1 << 0, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x44: ///bit 0 of register h
                    SetFlags(Registers.H, 1 << 0, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x45: ///bit 0 of register l
                    SetFlags(Registers.L, 1 << 0, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x46: ///bit 0 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1 << 0, 5, 0, 1, -1);
                    Registers.Cycles += 16;
                    break;
                case 0x47: ///bit 0 of register a
                    SetFlags(Registers.A, 1 << 0, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;

                case 0x48: ///bit 1 of register b
                    SetFlags(Registers.B, 1 << 1, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x49: ///bit 1 of register c
                    SetFlags(Registers.C, 1 << 1, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x4a: ///bit 1 of register d
                    SetFlags(Registers.D, 1 << 1, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x4b: ///bit 1 of register e
                    SetFlags(Registers.E, 1 << 1, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x4c: ///bit 1 of register h
                    SetFlags(Registers.H, 1 << 1, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x4d: ///bit 1 of register l
                    SetFlags(Registers.L, 1 << 1, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x4e: ///bit 1 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1 << 1, 5, 0, 1, -1);
                    Registers.Cycles += 16;
                    break;
                case 0x4f: ///bit 1 of register a
                    SetFlags(Registers.A, 1 << 1, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;

                case 0x50: ///bit 2 of register b
                    SetFlags(Registers.B, 1 << 2, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x51: ///bit 2 of register c
                    SetFlags(Registers.C, 1 << 2, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x52: ///bit 2 of register d
                    SetFlags(Registers.D, 1 << 2, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x53: ///bit 2 of register e
                    SetFlags(Registers.E, 1 << 2, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x54: ///bit 2 of register h
                    SetFlags(Registers.H, 1 << 2, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x55: ///bit 2 of register l
                    SetFlags(Registers.L, 1 << 2, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x56: ///bit 2 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1 << 2, 5, 0, 1, -1);
                    Registers.Cycles += 16;
                    break;
                case 0x57: ///bit 2 of register a
                    SetFlags(Registers.A, 1 << 2, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;

                case 0x58: ///bit 3 of register b
                    SetFlags(Registers.B, 1 << 3, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x59: ///bit 3 of register c
                    SetFlags(Registers.C, 1 << 3, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x5a: ///bit 3 of register d
                    SetFlags(Registers.D, 1 << 3, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x5b: ///bit 3 of register e
                    SetFlags(Registers.E, 1 << 3, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x5c: ///bit 3 of register h
                    SetFlags(Registers.H, 1 << 3, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x5d: ///bit 3 of register l
                    SetFlags(Registers.L, 1 << 3, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x5e: ///bit 3 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1 << 3, 5, 0, 1, -1);
                    Registers.Cycles += 16;
                    break;
                case 0x5f: ///bit 3 of register a
                    SetFlags(Registers.A, 1 << 3, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;

                case 0x60: ///bit 4 of register b
                    SetFlags(Registers.B, 1 << 4, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x61: ///bit 4 of register c
                    SetFlags(Registers.C, 1 << 4, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x62: ///bit 4 of register d
                    SetFlags(Registers.D, 1 << 4, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x63: ///bit 4 of register e
                    SetFlags(Registers.E, 1 << 4, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x64: ///bit 4 of register h
                    SetFlags(Registers.H, 1 << 4, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x65: ///bit 4 of register l
                    SetFlags(Registers.L, 1 << 4, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x66: ///bit 4 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1 << 4, 5, 0, 1, -1);
                    Registers.Cycles += 16;
                    break;
                case 0x67: ///bit 4 of register a
                    SetFlags(Registers.A, 1 << 4, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;

                case 0x68: ///bit 5 of register b
                    SetFlags(Registers.B, 1 << 5, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x69: ///bit 5 of register c
                    SetFlags(Registers.C, 1 << 5, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x6a: ///bit 5 of register d
                    SetFlags(Registers.D, 1 << 5, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x6b: ///bit 5 of register e
                    SetFlags(Registers.E, 1 << 5, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x6c: ///bit 5 of register h
                    SetFlags(Registers.H, 1 << 5, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x6d: ///bit 5 of register l
                    SetFlags(Registers.L, 1 << 5, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x6e: ///bit 5 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1 << 5, 5, 0, 1, -1);
                    Registers.Cycles += 16;
                    break;
                case 0x6f: ///bit 5 of register a
                    SetFlags(Registers.A, 1 << 5, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;

                case 0x70: ///bit 6 of register b
                    SetFlags(Registers.B, 1 << 6, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x71: ///bit 6 of register c
                    SetFlags(Registers.C, 1 << 6, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x72: ///bit 6 of register d
                    SetFlags(Registers.D, 1 << 6, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x73: ///bit 6 of register e
                    SetFlags(Registers.E, 1 << 6, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x74: ///bit 6 of register h
                    SetFlags(Registers.H, 1 << 6, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x75: ///bit 6 of register l
                    SetFlags(Registers.L, 1 << 6, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x76: ///bit 6 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1 << 6, 5, 0, 1, -1);
                    Registers.Cycles += 16;
                    break;
                case 0x77: ///bit 6 of register a
                    SetFlags(Registers.A, 1 << 6, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;

                case 0x78: ///bit 7 of register b
                    SetFlags(Registers.B, 1 << 7, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x79: ///bit 7 of register c
                    SetFlags(Registers.C, 1 << 7, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x7a: ///bit 7 of register d
                    SetFlags(Registers.D, 1 << 7, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x7b: ///bit 7 of register e
                    SetFlags(Registers.E, 1 << 7, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x7c: ///bit 7 of register h
                    SetFlags(Registers.H, 1 << 7, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x7d: ///bit 7 of register l
                    SetFlags(Registers.L, 1 << 7, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;
                case 0x7e: ///bit 7 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    SetFlags(memory[addr], 1 << 7, 5, 0, 1, -1);
                    Registers.Cycles += 16;
                    break;
                case 0x7f: ///bit 7 of register a
                    SetFlags(Registers.A, 1 << 7, 5, 0, 1, -1);
                    Registers.Cycles += 8;
                    break;

                case 0x80: //RESET bit 0 of register B
                    Registers.B = Registers.B & ~(1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0x81: //RESET bit 0 of register C
                    Registers.C = Registers.C & ~(1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0x82: //RESET bit 0 of register D
                    Registers.D = Registers.D & ~(1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0x83: //RESET bit 0 of register E
                    Registers.E = Registers.E & ~(1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0x84: //RESET bit 0 of register H
                    Registers.H = Registers.H & ~(1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0x85: //RESET bit 0 of register L
                    Registers.L = Registers.L & ~(1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0x86: //RESET bit 0 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] & ~(1 << 0);
                    Registers.Cycles += 16;
                    break;
                case 0x87: //RESET bit 0 of register A
                    Registers.A = Registers.A & ~(1 << 0);
                    Registers.Cycles += 8;
                    break;

                case 0x88: //RESET bit 1 of register B
                    Registers.B = Registers.B & ~(1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0x89: //RESET bit 1 of register C
                    Registers.C = Registers.C & ~(1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0x8a: //RESET bit 1 of register D
                    Registers.D = Registers.D & ~(1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0x8b: //RESET bit 1 of register E
                    Registers.E = Registers.E & ~(1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0x8c: //RESET bit 1 of register H
                    Registers.H = Registers.H & ~(1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0x8d: //RESET bit 1 of register L
                    Registers.L = Registers.L & ~(1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0x8e: //RESET bit 1 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] & ~(1 << 1);
                    Registers.Cycles += 16;
                    break;
                case 0x8f: //RESET bit 1 of register A
                    Registers.A = Registers.A & ~(1 << 1);
                    Registers.Cycles += 8;
                    break;

                case 0x90: //RESET bit 2 of register B
                    Registers.B = Registers.B & ~(1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0x91: //RESET bit 2 of register C
                    Registers.C = Registers.C & ~(1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0x92: //RESET bit 2 of register D
                    Registers.D = Registers.D & ~(1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0x93: //RESET bit 2 of register E
                    Registers.E = Registers.E & ~(1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0x94: //RESET bit 2 of register H
                    Registers.H = Registers.H & ~(1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0x95: //RESET bit 2 of register L
                    Registers.L = Registers.L & ~(1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0x96: //RESET bit 2 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] & ~(1 << 2);
                    Registers.Cycles += 16;
                    break;
                case 0x97: //RESET bit 2 of register A
                    Registers.A = Registers.A & ~(1 << 2);
                    Registers.Cycles += 8;
                    break;

                case 0x98: //RESET bit 3 of register B
                    Registers.B = Registers.B & ~(1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0x99: //RESET bit 3 of register C
                    Registers.C = Registers.C & ~(1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0x9a: //RESET bit 3 of register D
                    Registers.D = Registers.D & ~(1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0x9b: //RESET bit 3 of register E
                    Registers.E = Registers.E & ~(1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0x9c: //RESET bit 3 of register H
                    Registers.H = Registers.H & ~(1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0x9d: //RESET bit 3 of register L
                    Registers.L = Registers.L & ~(1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0x9e: //RESET bit 3 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] & ~(1 << 3);
                    Registers.Cycles += 16;
                    break;
                case 0x9f: //RESET bit 3 of register A
                    Registers.A = Registers.A & ~(1 << 3);
                    Registers.Cycles += 8;
                    break;

                case 0xa0: //RESET bit 4 of register B
                    Registers.B = Registers.B & ~(1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xa1: //RESET bit 4 of register C
                    Registers.C = Registers.C & ~(1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xa2: //RESET bit 4 of register D
                    Registers.D = Registers.D & ~(1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xa3: //RESET bit 4 of register E
                    Registers.E = Registers.E & ~(1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xa4: //RESET bit 4 of register H
                    Registers.H = Registers.H & ~(1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xa5: //RESET bit 4 of register L
                    Registers.L = Registers.L & ~(1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xa6: //RESET bit 4 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] & ~(1 << 4);
                    Registers.Cycles += 16;
                    break;
                case 0xa7: //RESET bit 4 of register A
                    Registers.A = Registers.A & ~(1 << 4);
                    Registers.Cycles += 8;
                    break;


                case 0xa8: //RESET bit 5 of register B
                    Registers.B = Registers.B & ~(1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xa9: //RESET bit 5 of register C
                    Registers.C = Registers.C & ~(1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xaa: //RESET bit 5 of register D
                    Registers.D = Registers.D & ~(1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xab: //RESET bit 5 of register E
                    Registers.E = Registers.E & ~(1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xac: //RESET bit 5 of register H
                    Registers.H = Registers.H & ~(1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xad: //RESET bit 5 of register L
                    Registers.L = Registers.L & ~(1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xae: //RESET bit 5 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] & ~(1 << 5);
                    Registers.Cycles += 16;
                    break;
                case 0xaf: //RESET bit 5 of register A
                    Registers.A = Registers.A & ~(1 << 5);
                    Registers.Cycles += 8;
                    break;

                case 0xb0: //RESET bit 6 of register B
                    Registers.B = Registers.B & ~(1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xb1: //RESET bit 6 of register C
                    Registers.C = Registers.C & ~(1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xb2: //RESET bit 6 of register D
                    Registers.D = Registers.D & ~(1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xb3: //RESET bit 6 of register E
                    Registers.E = Registers.E & ~(1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xb4: //RESET bit 6 of register H
                    Registers.H = Registers.H & ~(1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xb5: //RESET bit 6 of register L
                    Registers.L = Registers.L & ~(1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xb6: //RESET bit 6 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] & ~(1 << 6);
                    Registers.Cycles += 16;
                    break;
                case 0xb7: //RESET bit 6 of register A
                    Registers.A = Registers.A & ~(1 << 6);
                    Registers.Cycles += 8;
                    break;

                case 0xb8: //RESET bit 7 of register B
                    Registers.B = Registers.B & ~(1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xb9: //RESET bit 7 of register C
                    Registers.C = Registers.C & ~(1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xba: //RESET bit 7 of register D
                    Registers.D = Registers.D & ~(1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xbb: //RESET bit 7 of register E
                    Registers.E = Registers.E & ~(1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xRegisters.BC: //RESET bit 7 of register H
                    Registers.H = Registers.H & ~(1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xbd: //RESET bit 7 of register L
                    Registers.L = Registers.L & ~(1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xbe: //RESET bit 7 of (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] & ~(1 << 7);
                    Registers.Cycles += 16;
                    break;
                case 0xbf: //RESET bit 7 of A
                    Registers.A = Registers.A & ~(1 << 7);
                    Registers.Cycles += 16;
                    break;

                case 0xc0: // SET bit 0 of register B
                    Registers.B = Registers.B | (1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0xc1: // SET bit 0 of register C
                    Registers.C = Registers.C | (1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0xc2: // SET bit 0 of register D
                    Registers.D = Registers.D | (1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0xc3: // SET bit 0 of register E
                    Registers.E = Registers.E | (1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0xc4: // SET bit 0 of register H
                    Registers.H = Registers.H | (1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0xc5: // SET bit 0 of register L
                    Registers.L = Registers.L | (1 << 0);
                    Registers.Cycles += 8;
                    break;
                case 0xc6: // SET bit 0 of register (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] | (1 << 0);
                    Registers.Cycles += 16;
                    break;
                case 0xc7: // SET bit 0 of register A
                    Registers.A = Registers.A | (1 << 0);
                    Registers.Cycles += 8;
                    break;

                case 0xc8: // SET bit 1 of register B
                    Registers.B = Registers.B | (1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0xc9: // SET bit 1 of register C
                    Registers.C = Registers.C | (1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0xca: // SET bit 1 of register D
                    Registers.D = Registers.D | (1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0xcb: // SET bit 1 of register E
                    Registers.E = Registers.E | (1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0xcc: // SET bit 1 of register H
                    Registers.H = Registers.H | (1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0xcd: // SET bit 1 of register L
                    Registers.L = Registers.L | (1 << 1);
                    Registers.Cycles += 8;
                    break;
                case 0xce: // SET bit 1 of register (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] | (1 << 1);
                    Registers.Cycles += 16;
                    break;
                case 0xcf: // SET bit 1 of register A
                    Registers.A = Registers.A | (1 << 1);
                    Registers.Cycles += 8;
                    break;

                case 0xd0: // SET bit 2 of register B
                    Registers.B = Registers.B | (1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0xd1: // SET bit 2 of register C
                    Registers.C = Registers.C | (1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0xd2: // SET bit 2 of register D
                    Registers.D = Registers.D | (1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0xd3: // SET bit 2 of register E
                    Registers.E = Registers.E | (1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0xd4: // SET bit 2 of register H
                    Registers.H = Registers.H | (1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0xd5: // SET bit 2 of register L
                    Registers.L = Registers.L | (1 << 2);
                    Registers.Cycles += 8;
                    break;
                case 0xd6: // SET bit 2 of register (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] | (1 << 2);
                    Registers.Cycles += 16;
                    break;
                case 0xd7: // SET bit 2 of register A
                    Registers.A = Registers.A | (1 << 2);
                    Registers.Cycles += 8;
                    break;

                case 0xd8: // SET bit 3 of register B
                    Registers.B = Registers.B | (1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0xd9: // SET bit 3 of register C
                    Registers.C = Registers.C | (1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0xda: // SET bit 3 of register D
                    Registers.D = Registers.D | (1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0xdb: // SET bit 3 of register E
                    Registers.E = Registers.E | (1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0xdc: // SET bit 3 of register H
                    Registers.H = Registers.H | (1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0xdd: // SET bit 3 of register L
                    Registers.L = Registers.L | (1 << 3);
                    Registers.Cycles += 8;
                    break;
                case 0xde: // SET bit 3 of register (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] | (1 << 3);
                    Registers.Cycles += 16;
                    break;
                case 0xdf: // SET bit 3 of register A
                    Registers.A = Registers.A | (1 << 3);
                    Registers.Cycles += 8;
                    break;

                case 0xe0: // SET bit 4 of register B
                    Registers.B = Registers.B | (1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xe1: // SET bit 4 of register C
                    Registers.C = Registers.C | (1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xe2: // SET bit 4 of register D
                    Registers.D = Registers.D | (1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xe3: // SET bit 4 of register E
                    Registers.E = Registers.E | (1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xe4: // SET bit 4 of register H
                    Registers.H = Registers.H | (1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xe5: // SET bit 4 of register L
                    Registers.L = Registers.L | (1 << 4);
                    Registers.Cycles += 8;
                    break;
                case 0xe6: // SET bit 4 of register (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] | (1 << 4);
                    Registers.Cycles += 16;
                    break;
                case 0xe7: // SET bit 4 of register A
                    Registers.A = Registers.A | (1 << 4);
                    Registers.Cycles += 8;
                    break;

                case 0xe8: // SET bit 5 of register B
                    Registers.B = Registers.B | (1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xe9: // SET bit 5 of register C
                    Registers.C = Registers.C | (1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xea: // SET bit 5 of register D
                    Registers.D = Registers.D | (1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xeb: // SET bit 5 of register E
                    Registers.E = Registers.E | (1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xec: // SET bit 5 of register H
                    Registers.H = Registers.H | (1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xed: // SET bit 5 of register L
                    Registers.L = Registers.L | (1 << 5);
                    Registers.Cycles += 8;
                    break;
                case 0xee: // SET bit 5 of register (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] | (1 << 5);
                    Registers.Cycles += 16;
                    break;
                case 0xef: // SET bit 5 of register A
                    Registers.A = Registers.A | (1 << 5);
                    Registers.Cycles += 8;
                    break;

                case 0xf0: // SET bit 6 of register B
                    Registers.B = Registers.B | (1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xf1: // SET bit 6 of register C
                    Registers.C = Registers.C | (1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xf2: // SET bit 6 of register D
                    Registers.D = Registers.D | (1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xf3: // SET bit 6 of register E
                    Registers.E = Registers.E | (1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xf4: // SET bit 6 of register H
                    Registers.H = Registers.H | (1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xf5: // SET bit 6 of register L
                    Registers.L = Registers.L | (1 << 6);
                    Registers.Cycles += 8;
                    break;
                case 0xf6: // SET bit 6 of register (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] | (1 << 6);
                    Registers.Cycles += 16;
                    break;
                case 0xf7: // SET bit 6 of register A
                    Registers.A = Registers.A | (1 << 6);
                    Registers.Cycles += 8;
                    break;

                case 0xf8: // SET bit 7 of register B
                    Registers.B = Registers.B | (1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xf9: // SET bit 7 of register C
                    Registers.C = Registers.C | (1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xfa: // SET bit 7 of register D
                    Registers.D = Registers.D | (1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xfb: // SET bit 7 of register E
                    Registers.E = Registers.E | (1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xfc: // SET bit 7 of register H
                    Registers.H = Registers.H | (1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xfd: // SET bit 7 of register L
                    Registers.L = Registers.L | (1 << 7);
                    Registers.Cycles += 8;
                    break;
                case 0xfe: // SET bit 7 of register (Registers.HL)
                    addr = Registers.H * 256 + Registers.L;
                    if (!IsRom(addr)) memory[addr] = memory[addr] | (1 << 7);
                    Registers.Cycles += 16;
                    break;
                case 0xff: // SET bit 7 of register A
                    Registers.A = Registers.A | (1 << 7);
                    Registers.Cycles += 8;
                    break;

                default:
                    throw new Exception("Unimplemented opCode " + cb_opCode);
            }
        }

        private void SetFlags(in byte stateB, int p1, int p2, int p3, int p4, int p5)
        {
            throw new NotImplementedException();
        }

        private bool IsRom(int addr)
        {
            throw new NotImplementedException();
        }

        private Tuple<byte, byte> SeperateBytes(int p0)
        {
            throw new NotImplementedException();
        }*/
    }
}
 