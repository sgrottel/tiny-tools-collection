using System.Drawing;
using System.Text;

namespace ColorVariantsWheelTest
{

    internal class Program
    {

        static void PrintColorMatrix(int width, int height, Func<int, int, Color> colors, Color background)
        {
            string sepLine = new string(' ', width * 4 + 2);

            Color c = background;
            Console.Write(ConsoleUtility.VtsBC(c.R, c.G, c.B));
            Console.WriteLine(sepLine);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    c = colors(x, y);
                    Console.Write(ConsoleUtility.VtsFC(c.R, c.G, c.B));
                    Console.Write("  ██");
                }
                Console.WriteLine("  ");
                Console.WriteLine(sepLine);
            }
            Console.Write(ConsoleUtility.VtsReset);
        }

        static Color Convert(ColorVariantsWheel.ColorVariantsWheel.RGB rgb)
        {
            return Color.FromArgb(
                (byte)Math.Clamp((int)(rgb.R * 255.0), 0, 255),
                (byte)Math.Clamp((int)(rgb.G * 255.0), 0, 255),
                (byte)Math.Clamp((int)(rgb.B * 255.0), 0, 255)
                );
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (!ConsoleUtility.EnableVirtualTerminalProcessing())
            {
                Console.Error.WriteLine("Failed to enable Virtual Terminal Processing");
                return;
            }

            Console.WriteLine("ColorVariantsWheelTest");
            Console.WriteLine();

            ColorVariantsWheel.ColorVariantsWheel colorWheel = new();
            colorWheel.BaseHueShift = 5.0;
            colorWheel.BaseLuminance = 0.8;
            colorWheel.BaseSaturation = 0.9;

            Func<int, int, Color> colors
                = (int x, int y)
                => Convert(
                    colorWheel.MakeColorVariant(
                        x,
                        ColorVariantsWheel.ColorVariantsWheel.AlternatingVariant(y)
                    ));

            PrintColorMatrix(12, 5, colors, background: Color.White);
            Console.WriteLine();
            PrintColorMatrix(12, 5, colors, background: Color.Gray); // 50%
            Console.WriteLine();
            PrintColorMatrix(12, 5, colors, background: Color.Black);

            Console.WriteLine();
            Console.WriteLine("Done.");
        }

    }

}
