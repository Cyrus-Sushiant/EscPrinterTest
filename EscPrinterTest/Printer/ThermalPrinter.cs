using SkiaSharp;
using System.Collections;
using System.Drawing;
using System.Net.Sockets;

namespace EscPrinterTest.Printer;

public class ThermalPrinter
{
    private readonly string _ipAddress;
    private readonly int _port;

    public ThermalPrinter(string ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
    }

    public async Task Print(string imagePath)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(5);

        using (Socket socket = new Socket(SocketType.Stream, ProtocolType.IP)
        {
            SendTimeout = timeout.Milliseconds,
            ReceiveTimeout = timeout.Milliseconds
        })
        {
            await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, _ipAddress, _port, null)
                .WithCancellationToken(CancellationToken.None);

            byte[] cutCommand = { 29, 86, 1 };

            List<byte> outputList = new List<byte>();

            outputList.AddRange(GetImageBytes(imagePath));
            outputList.AddRange(cutCommand);

            socket.Send(outputList.ToArray());
        }
    }

    private static List<byte> GetImageBytes(string imagePath)
    {
        using (Bitmap imageBitmap = new Bitmap(imagePath))
        {
            int width = imageBitmap.Width;
            int height = imageBitmap.Height;

            byte[,] imgArray = new byte[width, height];

            if (width != 384 || height > 65635)
            {
                throw (new Exception("Image width must be 384px, height cannot exceed 65635px."));
            }

            //Processing image data	
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < (width / 8); x++)
                {
                    imgArray[x, y] = 0;
                    for (byte n = 0; n < 8; n++)
                    {
                        Color pixel = imageBitmap.GetPixel(x * 8 + n, y);
                        if (pixel.GetBrightness() < 0.5)
                        {
                            imgArray[x, y] += (byte)(1 << n);
                        }
                    }
                }
            }

            List<byte> bytes = new List<byte>();
            //Print LSB first bitmap
            bytes.Add(18);
            bytes.Add(118);

            bytes.Add((byte)(height & 255));   //height LSB
            bytes.Add((byte)(height >> 8));    //height MSB


            for (int y = 0; y < height; y++)
            {
                //System.Threading.Thread.Sleep(40);
                for (int x = 0; x < (width / 8); x++)
                {
                    bytes.Add(imgArray[x, y]);
                }
            }

            return bytes;
        }
    }
}