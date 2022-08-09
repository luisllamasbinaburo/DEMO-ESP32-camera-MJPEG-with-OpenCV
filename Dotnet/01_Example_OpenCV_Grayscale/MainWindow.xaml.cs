using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp.XImgProc;

namespace MPJEG_Viewer
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
            var dst = src.Clone();
            Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2GRAY);
            foreach (var rect in rects)
            {
                Cv2.Rectangle(src, rect, Scalar.Red);
            }

            Dispatcher.Invoke(() => player.Source = src.ToBitmapSource());
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
