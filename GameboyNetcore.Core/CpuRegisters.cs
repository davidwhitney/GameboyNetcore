using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameboyNetcore.Core
{
    public class CpuRegisters : IEnumerable<KeyValuePair<string, byte>>
    {
        public byte A { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte F { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }

        public ushort AF
        {
            get => A.Combine(F);
            set { A = value.High(); F = value.LowerBits(); }
        }

        public ushort BC
        {
            get => B.Combine(B);
            set { B = value.High(); C = value.LowerBits(); }
        }

        public ushort DE
        {
            get => D.Combine(E);
            set { D = value.High(); E = value.LowerBits(); }
        }

        public ushort HL
        {
            get => H.Combine(L);
            set { H = value.High(); L = value.LowerBits(); }
        }

        public ushort SP { get; set; }
        public ushort PC { get; set; }

        public byte Flags
        {
            get => AF.LowerBits();
            set => AF = AF.SetLowerBits(value);
        }

        private readonly Dictionary<string, PropertyInfo> _props;
       
        public CpuRegisters()
        {
            _props = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(x => x.Name, y => y);
        }

        public object this[string name]
        {
            get => _props[name].GetValue(this);
            set => _props[name].SetValue(this, value);
        }

        /*
        Bit  Name  Set Clr  Expl.
         7    zf    Z   NZ   Zero Flag
         6    n     -   -    Add/Sub-Flag (BCD)
         5    h     -   -    Half Carry Flag (BCD)
         4    cy    C   NC   Carry Flag
         3-0  -     -   -    Not used (always zero)
         */

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<KeyValuePair<string, byte>> GetEnumerator()
        {
            yield return new KeyValuePair<string, byte>("A", A);
            yield return new KeyValuePair<string, byte>("B", B);
            yield return new KeyValuePair<string, byte>("C", C);
            yield return new KeyValuePair<string, byte>("D", D);
            yield return new KeyValuePair<string, byte>("E", E);
            yield return new KeyValuePair<string, byte>("F", F);
            yield return new KeyValuePair<string, byte>("H", H);
            yield return new KeyValuePair<string, byte>("L", L);
        }
    }
}