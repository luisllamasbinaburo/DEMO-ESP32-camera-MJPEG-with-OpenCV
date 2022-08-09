using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Demo_Esp32_Camera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        Mjpeg_Decoder_NET.MjpegDecoder decoder = new Mjpeg_Decoder_NET.MjpegDecoder();

        public MainWindow()
        {
            InitializeComponent();

            decoder.ParseStream(new Uri(@"http://192.168.1.xxxx:yy/video"));
            decoder.FrameReady += Decoder_FrameReady;
        }

        private void Decoder_FrameReady(object sender, Mjpeg_Decoder_NET.FrameReadyEventArgs e)
        {
            var buffer = e.FrameBuffer;
            var image = ToImage(buffer);

            var src = image.ToMat();
            BallTracking(src);

            Dispatcher.Invoke(() => player.Source = src.ToBitmapSource());
        }

        private void BallTracking(Mat src)
        {
            Cv2.Rotate(src, src, RotateFlags.Rotate90Clockwise);
            var mask = new Mat();
            Cv2.CvtColor(src, mask, ColorConversionCodes.BGR2HSV);

            var greenLower = InputArray.Create(new[] { 22, 86, 6 });
            var greenUpper = InputArray.Create(new[] { 42, 255, 255 });

            Cv2.InRange(mask, greenLower, greenUpper, mask);
            Cv2.Erode(mask, mask, null, iterations: 2);
            Cv2.Dilate(mask, mask, null, iterations: 2);

            var output = new Mat();
            Cv2.FindContours(mask, out Mat[] contours, OutputArray.Create(output), RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            if (contours.Length > 0)
            {
                var maxC = contours.Max(x => x.ContourArea());
                var contour = contours.First(x => x.ContourArea() == maxC);
                Cv2.MinEnclosingCircle(contour, out Point2f center, out float radius);

                if (radius > 10)
                {
                    Cv2.Circle(src, new OpenCvSharp.Point(center.X, center.Y), (int)radius, 255, 5);
                    Cv2.PutText(src, $"CenX {center.X}", new OpenCvSharp.Point(50, 50), HersheyFonts.HersheySimplex, 1, 1, 2);
                    Cv2.PutText(src, $"CenY {center.Y}", new OpenCvSharp.Point(50, 100), HersheyFonts.HersheySimplex, 1, 1, 2);
                    Cv2.PutText(src, $"Rad {radius}", new OpenCvSharp.Point(50, 150), HersheyFonts.HersheySimplex, 1, 1, 2);
                }
            }
        }

        private BitmapImage ToImage(byte[] buffer)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(buffer);
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}

