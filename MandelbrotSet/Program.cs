using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace MandelbrotSet
{
    class Program
    {
        static void Main(string[] args)
        {
            var start = DateTime.Now;
            var width = 2500 * 8;
            var height = 2000 * 8;
            var filename = "output.bmp";
            var tr = new Complex(0.5, 1);
            var bl = new Complex(-2, -1);
            var maxcycles = 1000;
            Bitmap map = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(map)) g.Clear(Color.White);

            int[] result = new int[width * height];

            Parallel.For(0, width * height, i =>
            {
                int x = i % width;
                int y = i / width;
                var rl = (tr.Real - bl.Real) * (x / (double)width) + bl.Real;
                var im = (tr.Imaginary - bl.Imaginary) * (y / (double)height) + bl.Imaginary;
                result[i] = GetCycles(new Complex(rl, im), maxcycles);
            });
            for (int i = 0; i < width * height; i++)
            {
                int x = i % width;
                int y = i / width;
                int v = 255 - (int)(255.0 * (double)result[i] / (double)maxcycles);
                map.SetPixel(x, y, Color.FromArgb(v, v, v));
            }
            map.Save(filename);
            var stop = DateTime.Now;
            Console.WriteLine($"{filename} generated in {(stop - start).TotalSeconds}seconds");
        }

        private static int GetCycles(Complex c, int maxcycles)
        {
            var last = new Complex(0, 0);
            for(int i = 0; i<maxcycles; i++)
            {
                var z = last * last + c;
                if (z.Magnitude >= 4) return i;
                else last = z;
            }
            return maxcycles;
        }
    }
}
