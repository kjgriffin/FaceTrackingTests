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
            this.DoubleBuffered = true;
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

        CentroidTracker _tracker = new CentroidTracker();

        private void button3_Click(object sender, EventArgs e)
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            worker = new Thread(DoWork);
            capture = new OpenCvSharp.VideoCapture();
            classifier = new OpenCvSharp.CascadeClassifier("haarcascade_frontalface_default.xml");


            capture.Open(0, OpenCvSharp.VideoCaptureAPIs.ANY);
            //capture.Open(@"D:\Main\church\capture\2020-12-25 09-49-12.mp4");
            //capture.Open(@"D:\Main\church\capture\clip.mp4");


            worker.Start();
        }

        private void DoWork()
        {
            while (!cts.IsCancellationRequested)
            {


                var frameMat = capture.RetrieveMat();

                //frameMat = frameMat.Blur(new OpenCvSharp.Size(16, 16));

                var rects = classifier.DetectMultiScale(frameMat, 1.1, 5, OpenCvSharp.HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));

                _tracker.UpdateTracks(rects);

                Bitmap bmp = frameMat.ToBitmap();
                Draw(bmp, _tracker.GetTracks());

                bmp.Dispose();
                frameMat.Dispose();

                Thread.Sleep(10);
            }
            capture.Release();
        }

        private void Draw(Bitmap bmp, List<CentroidTrack> tracks)
        {
            if (InvokeRequired)
            {
                Invoke(Draw, bmp, tracks);
                return;
            }
            using (Graphics g = CreateGraphics())
            using (Bitmap b = new Bitmap(Size.Width, Size.Height))
            using (Graphics gfx = Graphics.FromImage(b))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(bmp, 10, 10);

                foreach (var track in tracks)
                {
                    gfx.DrawRectangle(track.Stale ? Pens.Red : Pens.Green, new System.Drawing.Rectangle(track.Centroid.Bounds.X, track.Centroid.Bounds.Y, track.Centroid.Bounds.Width, track.Centroid.Bounds.Height));
                    gfx.FillRectangle(Brushes.Black, (int)track.Centroid.Center.X, (int)track.Centroid.Center.Y, 150, 25);
                    gfx.DrawString(track.Name, DefaultFont, Brushes.White, new System.Drawing.Point((int)track.Centroid.Center.X, (int)track.Centroid.Center.Y));
                }

                for (int i = 0; i < tracks.Count; i++)
                {
                    gfx.FillRectangle(Brushes.White, 10, 23 + 20 * i, 150, 20);
                    gfx.DrawString(tracks[i].Name, DefaultFont, Brushes.Black, new System.Drawing.Point(10, 20 + i * 20));
                }

                g.DrawImage(b, 0, 0);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            cts.Cancel();
        }
    }
}