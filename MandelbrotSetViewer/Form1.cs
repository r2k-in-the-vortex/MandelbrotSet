using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MandelbrotSetViewer
{
    public partial class Form1 : Form
    {
        private Mandelbrot graph;


        public Form1()
        {
            InitializeComponent();
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            this.SizeChanged += PictureBox1_SizeChanged;
            graph = new Mandelbrot(pictureBox1.Size, new Complex(-2, -1), new Complex(1, 1));
            pictureBox1.Image = graph.CalculateImage();
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            graph.Zoom(e.Delta, e.Location);
            pictureBox1.Image = graph.CalculateImage();
        }

        private void PictureBox1_SizeChanged(object sender, EventArgs e)
        {
            graph.size = pictureBox1.Size;
            graph.StopZoom = new Point(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = graph.CalculateImage();
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            graph.StopZoom = e.Location;
            pictureBox1.Image = graph.CalculateImage();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            graph.StartZoom = e.Location;
        }
    }

    internal class Mandelbrot
    {
        public Size size { get; set; }
        private Complex bl;
        private Complex tr;
        public Point StartZoom { get; set; }
        public Point StopZoom { get; internal set; }

        private int maxcycles;

        public Mandelbrot(Size size, Complex complex1, Complex complex2)
        {
            this.size = size;
            this.bl = complex1;
            this.tr = complex2;
            StartZoom = new Point(0, 0);
            StopZoom = new Point(size.Width, size.Height);
            maxcycles = 255;
        }

        internal Image CalculateImage()
        {
            updateZoom();

            Bitmap map = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(map)) g.Clear(Color.White);


            int[] result = new int[size.Width * size.Height];

            Parallel.For(0, size.Width * size.Height, i =>
            {
                int x = i % size.Width;
                int y = i / size.Width;
                var rl = (tr.Real - bl.Real) * (x / (double)size.Width) + bl.Real;
                var im = (tr.Imaginary - bl.Imaginary) * (y / (double)size.Height) + bl.Imaginary;
                result[i] = GetCycles(new Complex(rl, im), maxcycles);
            });
            for (int i = 0; i < size.Width * size.Height; i++)
            {
                int x = i % size.Width;
                int y = i / size.Width;
                int v = 255 - (int)(255.0 * (double)result[i] / (double)maxcycles);
                map.SetPixel(x, y, Color.FromArgb(v, v, v));
            }


            return map;
        }
        private static int GetCycles(Complex c, int maxcycles)
        {
            var last = new Complex(0, 0);
            for (int i = 0; i < maxcycles; i++)
            {
                var z = last * last + c;
                if (z.Magnitude >= 4) return i;
                else last = z;
            }
            return maxcycles;
        }

        private void updateZoom()
        {
            var wr = tr.Real - bl.Real;
            var wpx = size.Width;
            var ws = Math.Abs(StartZoom.X - StopZoom.X);
            var newwr = wr * ws / wpx;
            var crpx = (StartZoom.X + StopZoom.X) / 2;
            var cr = wr * crpx / wpx + bl.Real;
            var rstart = cr - newwr / 2.0;
            var rstop = cr + newwr / 2.0;

            var hi = wr * size.Height / size.Width;
            var hpx = size.Height;
            var newhi = hi * ws / wpx;
            var cipx = (StartZoom.Y + StopZoom.Y) / 2;
            var ci = hi * cipx / hpx + bl.Imaginary;
            var istart = ci - newhi / 2.0;
            var istop = ci + newhi / 2.0;

            bl = new Complex(rstart, istart);
            tr = new Complex(rstop, istop);
            StartZoom = new Point(0, 0);
            StopZoom = new Point(size.Width, size.Height);
        }

        internal void Zoom(int delta, Point location)
        {
            var zf = delta < 0 ? 100 / (double)-delta : (double)delta / 100;
            var cr = (tr.Real + bl.Real) / 2.0;
            var ci = (tr.Imaginary + bl.Imaginary) / 2.0;
            var wr = tr.Real - bl.Real;
            var hi = wr * size.Height / size.Width;

            var rstart = cr - zf * wr / 2.0;
            var rstop = cr + zf * wr / 2.0;

            var istart = ci - zf * hi / 2.0;
            var istop = ci + zf * hi / 2.0;

            bl = new Complex(rstart, istart);
            tr = new Complex(rstop, istop);
            StartZoom = new Point(0, 0);
            StopZoom = new Point(size.Width, size.Height);
        }
    }
}
