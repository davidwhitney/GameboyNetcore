namespace GameboyNetcore.Core
{
    public class MemoryMap
    {
        private readonly byte[] _values;

        public MemoryMap()
        {
            _values = new byte[65536];
        }

        public void Load(Cartridge cartridge)
        {
            var array = cartridge.Bytes.ToArray();
            for (var index = 0; index < array.Length; index++)
            {
                _values[index] = array[index];
            }
        }

        public void Clear()
        {
            for (var index = 0; index < _values.Length; index++)
            {
                _values[index] = 0;
            }
        }

        public byte this[int i]
        {
            get => Get(_values[i]);
            set => Set(i, value);
        }

        public void Set(int location, byte value)
        {
            _values[location] = value;
        }

        public byte Get(int location)
        {
            return _values[location];
        }
    }
}