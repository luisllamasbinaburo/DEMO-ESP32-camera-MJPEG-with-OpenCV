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
            DrawHoughLines(src);

            Dispatcher.Invoke(() => player.Source = src.ToBitmapSource());
        }

        private void DrawHoughLines(Mat src)
        {
            var gray = src.Clone();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            Cv2.Canny(gray, gray, 75, 150, 3, false);           
            LineSegmentPolar[] segStd = Cv2.HoughLines(gray, 1, Math.PI / 180, 50, 0, 0);

            int limit = Math.Min(segStd.Length, 10);
            for (int i = 0; i < limit; i++)
            {                
                float rho = segStd[i].Rho;
                float theta = segStd[i].Theta;
                double a = Math.Cos(theta);
                double b = Math.Sin(theta);
                double x0 = a * rho;
                double y0 = b * rho;
                OpenCvSharp.Point pt1 = new OpenCvSharp.Point { X = (int)Math.Round(x0 + 1000 * (-b)), Y = (int)Math.Round(y0 + 1000 * (a)) };
                OpenCvSharp.Point pt2 = new OpenCvSharp.Point { X = (int)Math.Round(x0 - 1000 * (-b)), Y = (int)Math.Round(y0 - 1000 * (a)) };
                src.Line(pt1, pt2, Scalar.Red, 3, LineTypes.AntiAlias, 0);
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

