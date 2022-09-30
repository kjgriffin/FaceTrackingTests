using OpenCvSharp;
using OpenCvSharp.Extensions;

using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TestScreenshot
{


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Rectangle bounds = Screen.GetBounds(System.Drawing.Point.Empty);
            using (Bitmap b = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
                    using (Graphics gfx = CreateGraphics())
                    {
                        gfx.DrawImage(b, 0, 0, 1920, 1080);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var proc = Process.GetProcessesByName("obs64").FirstOrDefault();
            var obsHandle = proc.MainWindowHandle;

            GraphicsHelpers.GetWindowRect(obsHandle, out var rect);
            Rectangle bounds = new Rectangle(rect.Location, rect.Size);
            using (Bitmap b = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);
                    using (Graphics gfx = CreateGraphics())
                    {
                        gfx.DrawImage(b, 0, 0, 1920, 1080);
                    }
                }
            }

        }


        OpenCvSharp.VideoCapture capture;
        OpenCvSharp.CascadeClassifier classifier;


        CancellationTokenSource cts;
        Thread worker;

        private void button3_Click(object sender, EventArgs e)
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            worker = new Thread(DoWork);
            capture = new OpenCvSharp.VideoCapture();
            classifier = new OpenCvSharp.CascadeClassifier("haarcascade_frontalface_default.xml");


            //capture.Open(0, OpenCvSharp.VideoCaptureAPIs.ANY);
            capture.Open(@"D:\Main\church\capture\2020-12-25 09-49-12.mp4");
            //capture.Open(@"D:\Main\church\capture\clip.mp4");


            worker.Start();
        }

        private void DoWork()
        {
            while (!cts.IsCancellationRequested)
            {
                var frameMat = capture.RetrieveMat();
                var rects = classifier.DetectMultiScale(frameMat, 1.1, 5, OpenCvSharp.HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));
                //foreach (var rect in rects)
                //{
                //    Cv2.Rectangle(frameMat, rect, Scalar.Red);
                //}

                Bitmap bmp = frameMat.ToBitmap();
                Draw(bmp, rects);

                bmp.Dispose();
                frameMat.Dispose();

                //Thread.Sleep(30);
            }
            capture.Release();
        }

        private void Draw(Bitmap bmp, IEnumerable<Rect> rects)
        {
            if (InvokeRequired)
            {
                Invoke(Draw, bmp, rects);
                return;
            }
            using (Graphics gfx = CreateGraphics())
            {
                gfx.DrawImage(bmp, 10, 10);

                foreach (var rect in rects)
                {
                    gfx.DrawRectangle(Pens.Red, new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
                }

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            cts.Cancel();
        }
    }
}