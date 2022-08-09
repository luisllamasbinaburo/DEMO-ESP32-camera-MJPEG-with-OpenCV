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

            Dispatcher.Invoke(() => player.Source = image);
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

