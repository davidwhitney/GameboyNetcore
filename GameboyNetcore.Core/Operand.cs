using System;
using System.Linq;

namespace GameboyNetcore.Core
{
    public class Operand
    {
        public string Value { get; }
        public OperandType Type { get; set; }

        private string[] _8Bitregisters = new[]
        {
            "A", "B", "C", "D", "E", "F", "H", "L"
        };

        private string[] _16Bitregisters = new[]
        {
            "AF", "BC", "DE", "HL"
        };

        public Operand(string value)
        {
            Value = value;
            EstablishOperandType(value);
        }

        private void EstablishOperandType(string value)
        {
            if (value == null)
            {
                Type = OperandType.Unset;
                return;
            }

            if (_8Bitregisters.Contains(value))
            {
                Type = OperandType.Register8bit;
            }
            else if (_16Bitregisters.Contains(value))
            {
                Type = OperandType.Register16bit;
            }
            else if (value.StartsWith("("))
            {
                Type = OperandType.ValueFrom16bitRegister;
            }
            else
            {
                Type = OperandType.Value8bitImmediate;
            }
        }

        public static implicit operator Operand(string input)
        {
            return new Operand(input);
        }

        public static implicit operator string(Operand input)
        {
            return input.Value;
        }
    }
}