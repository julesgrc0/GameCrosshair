using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WIndowsParticulesSystem
{
    public partial class Form1 : Form
    {
        private Graphics g;
        private Pen pen = new Pen(Color.Red);
        private int size = 50;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWdn,int index,int newL);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWdn, int index);

        public Form1()
        {
            InitializeComponent();
            LoadWindow();
            
        }

        private void LoadWindow()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "config.conf";
            if (!File.Exists(path))
            {
                createFile(path);
            }
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Wheat;
            TransparencyKey = Color.Wheat;
            //ShowInTaskbar = false;
            TopMost = true;
            int index = GetWindowLong(Handle, -20);
            SetWindowLong(Handle, -20, index | 0x80000 | 0x20);
            
            if(LoadImage() != String.Empty)
            {
                image.Image = Image.FromFile(LoadImage());
                LoadConfig(this.Width);
                ImageAction();
            }
            else
            {
                image.Paint += new PaintEventHandler(paintCursor);
            }
            //SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);  
        }

        private void ImageAction()
        {
            image.Size = new Size(this.Width, this.Height);
            image.Image = resizeImage(image.Image, new Size(this.Width, this.Height));
        }


        private string LoadImage()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "images";
            if (Directory.Exists(path))
            {
                string f = "";
                foreach (string file in GetAllFiles(path, @"*.png"))
                {
                    f = file;
                }
                return f;
            }
            else
            {
                createDir(path);
                string f = "";
                foreach (string file in GetAllFiles(path, @"*.png"))
                {
                    f = file;
                }
                return f;
            }
        }
        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }
        private void createDir(string path)
        {
            Directory.CreateDirectory(path);
        }
        private void createFile(string path)
        {
            File.Create(path);
        }

        private string[] GetAllFiles(string path,string s)
        {
            return Directory.GetFiles(path,s);
        }

        private void paintCursor(object sender, PaintEventArgs e)
        {
            LoadConfig(this.Width);
            int w = this.Width;
            int h = this.Height;
            g = e.Graphics;

            int calc1 = (h - 2) / 2;
            int calc2 = (w - 2) / 2;

            if (this.isMidleFill)
            {
                NoFill(calc1, calc2);
            }
            else
            {
                Fill(calc1, calc2);
            }
        }

        private void Fill(int calc1,int calc2)
        {
            Point center = new Point(0, calc2);
            Point wh = new Point(0, calc2 * 2);
            /**
             *  vertical
             */
            center = new Point(calc1, 0);
            wh = new Point(calc1, calc1-(this.Height/ this.diviseurD));
            g.DrawLine(pen, wh, center);

            center = new Point(calc1, calc1 * 2);
            wh = new Point(calc1, calc1 + (this.Height / this.diviseurD));
            g.DrawLine(pen, wh, center);

            /**
             *  horizontal
             */
            center = new Point(0, calc1);
            wh = new Point(calc1 - (this.Height / this.diviseurD), calc1);
            g.DrawLine(pen, wh, center);


            center = new Point(calc1 * 2, calc1);
            wh = new Point(calc1 +(this.Height / this.diviseurD), calc1);
            g.DrawLine(pen, wh, center);
        }

        private void NoFill(int calc1,int calc2)
        {
            Point center = new Point(0, calc2);
            Point wh = new Point(0, calc2 * 2);
            //g.DrawLine(pen,wh,center);

            /**
             *  vertical
             */
            center = new Point(calc1, 0);
            wh = new Point(calc1, calc1);
            g.DrawLine(pen, wh, center);

            center = new Point(calc1, calc1 * 2);
            wh = new Point(calc1, calc1);
            g.DrawLine(pen, wh, center);

            /**
             *  horizontal
             */
            center = new Point(0, calc1);
            wh = new Point(calc1, calc1);
            g.DrawLine(pen, wh, center);


            center = new Point(calc1 * 2, calc1);
            wh = new Point(calc1, calc1);
            g.DrawLine(pen, wh, center);
        }

        public bool isMidleFill = true;
        public int diviseurD = 3;

        private void LoadConfig(int w)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory +"config.conf";
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach(string line in lines)
                {
                    string pattern = @"[a-zA-Z]+\:[a-zA-Z0-9]+";
                    string patternR = @"[a-zA-Z]+\:";
                    string px = @"[0-9]+(px)";
                    string unpx = @"(px)";
                    string type = @"\:[a-zA-Z0-9]+";
                    if (Regex.Match(line,pattern) != null)
                    {
                        string output = Regex.Replace(line, patternR, String.Empty);
                        string outtype = Regex.Replace(line,type,String.Empty);
                        if(output.ToLower() == "none" && outtype == "midle")
                        {
                            this.isMidleFill = false;
                        }else if(outtype == "diviseur")
                        {
                            int result = 3;
                            int.TryParse(output,out result);
                            this.diviseurD = result;
                        }else
                        {
                            foreach (object color in Enum.GetValues(typeof(KnownColor)))
                            {
                                if (output.ToLower() == color.ToString().ToLower())
                                {
                                    pen = new Pen(Color.FromName(color.ToString()));
                                }
                            }
                            if (Regex.Match(output, px) != null)
                            {
                                int size = w;
                                try
                                {
                                    size = int.Parse(Regex.Replace(output, unpx, String.Empty));
                                }
                                catch { }
                                this.Size = new Size(size, size);
                                image.Size = new Size(size, size);
                                image.Location = new Point(0, 0);
                                this.Location = new Point((Screen.PrimaryScreen.Bounds.Width - size) / 2, (Screen.PrimaryScreen.Bounds.Height - size) / 2);
                            }
                        }
                    }
                }
            }
            else
            {
               using(StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("size:50px");
                    sw.WriteLine("color:red");
                }
            }
        }
    }
}
