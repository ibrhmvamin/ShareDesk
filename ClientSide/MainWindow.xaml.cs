using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClientSide
{
    public partial class MainWindow : Window
    {
        public UdpClient client;
        public IPAddress remoteIP;
        public int remotePort;
        public IPEndPoint remoteEP;

        bool isStart = false;
        bool isStoped = false;
        bool isFirst = true;
        bool isRecord = false;

        static BigInteger count = 0;

        public MainWindow()
        {
            InitializeComponent();
            remoteIP = IPAddress.Parse("127.0.0.1");
            remotePort = 27001;
            remoteEP = new IPEndPoint(remoteIP, remotePort);

            client = new UdpClient();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            if (!isStart)
            {
                isStart = true;
                submitbtn.Content = "Stop";

                var maxLen = ushort.MaxValue - 29;
                var len = 0;
                var buffer = new byte[maxLen];

                if (isFirst)
                {
                    await client.SendAsync(buffer, buffer.Length, remoteEP);
                    isFirst = false;
                }

                var list = new List<byte>();
                while (true)
                {
                    if (isStoped)
                    {
                        isStoped = false;
                        break;
                    }
                    do
                    {
                        try
                        {
                            var result = await client.ReceiveAsync();

                            buffer = result.Buffer;
                            len = buffer.Length;
                            list.AddRange(buffer);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    } while (len == maxLen);

                    var image = await ByteToImageAsync(list.ToArray());

                    if (image is not null)
                        ImageShare.Source = image;

                    list.Clear();
                }
            }
            else
            {
                isStart = false;
                submitbtn.Content = "Start";
                isStoped = true;
            }
        }

        public async Task<BitmapImage?> ByteToImageAsync(byte[]? imageData)
        {
            var image = new BitmapImage();

            image.BeginInit();


            if (isRecord)
            {
                using MemoryStream ms = new MemoryStream(imageData);

                if (!Directory.Exists("Records"))
                    Directory.CreateDirectory("Records");


                var imagePath = $"Records\\image (" + count++ + ").jpeg";
                using FileStream file = new FileStream(imagePath, FileMode.Create, FileAccess.Write);

                ms.CopyTo(file);
            }


            image.StreamSource = new MemoryStream(imageData);
            image.CacheOption = BitmapCacheOption.OnLoad;

            image.EndInit();
            return image;
        }
      
    }
}
