namespace GameboyNetcore.Core
{
    public class Display
    {
        public int Width { get; }
        public int Height { get; }

        public Display(int width, int height)
        {
            Width = width;
            Height = height;

            /*
             * Video RAM - 8K Byte (16K Byte for CGB)
             * Screen Size - 2.6"
             * Resolution - 160x144 (20x18 tiles)
             * Max sprites - Max 40 per screen, 10 per line
             * Sprite sizes - 8x8 or 8x16 pixels
             * Palettes - 1x4 BG, 2x3 OBJ (for CGB: 8x4 BG, 8x3 OBJ)
             */
        }
    }
}