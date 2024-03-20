using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

var ip = IPAddress.Parse("127.0.0.1");
var port = 27001;

var listenerEP = new IPEndPoint(ip, port);
var listener = new UdpClient(listenerEP);
Console.WriteLine($"{listenerEP} Listener.....");




while (true)
{
    var result = await listener.ReceiveAsync();
    Console.WriteLine($"{result.RemoteEndPoint} Connected.....");
    _ = Task.Run(async () =>
    {
        IPEndPoint? remoteEP = result.RemoteEndPoint;


        while (true)
        {
            var screenImage = await TakeScreenShotAsync();
            var imgBytes = await ImageToByteAsync(screenImage);

            var chunks = imgBytes?.Chunk(ushort.MaxValue - 29);

            foreach (var chunk in chunks!)
                await listener.SendAsync(chunk, chunk.Length, remoteEP);
        }
    });
}



async Task<Image?> TakeScreenShotAsync()
{

    var width = Screen.PrimaryScreen.Bounds.Width;
    var height = Screen.PrimaryScreen.Bounds.Height;

    Bitmap? bitmap = new Bitmap(width, height);

    using Graphics graphics = Graphics.FromImage(bitmap);
    graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

    return bitmap;
}

async Task<byte[]?> ImageToByteAsync(Image? image)
{
    using MemoryStream ms = new MemoryStream();
    image?.Save(ms, ImageFormat.Jpeg);

    return ms.ToArray();
}

