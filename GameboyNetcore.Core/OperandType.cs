namespace GameboyNetcore.Core
{
    public enum OperandType
    {
        Unset,
        Register8bit,
        Register16bit,
        Value8bitImmediate,
        ValueFrom16bitRegister,
    }
}