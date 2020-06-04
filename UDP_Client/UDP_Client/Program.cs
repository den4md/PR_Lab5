using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UDP_Client
{
    class Program
    {
        static void Main(string[] args)
        { 
            byte[] byteScreenshot;
            int count_array = 0;

            System.Console.WriteLine("Нажмите <Enter> для старта трансляции");
            System.Console.ReadLine();

            var ip = IPAddress.Parse("127.0.0.1");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, 1);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, 16000);

            byte[] idle = new byte[0];
            socket.SendTo(idle, ipEndPoint);

            System.Console.WriteLine("Трансляция началась!");

            do
            {
                Thread.Sleep(5);
                int position = 0;

                byteScreenshot = MakeScreenshot();

                List<byte[]> fragments = new List<byte[]>();

                if (byteScreenshot.Length % 65002 != 0)
                {
                    count_array = byteScreenshot.Length / 65002 + 1;
                }
                else
                    count_array = byteScreenshot.Length / 65002;

                while (position < byteScreenshot.Length)
                {
                    byte[] fragmentScreenshot;
                    if ((byteScreenshot.Length - position) >= 65000)
                        fragmentScreenshot = new byte[65002];
                    else
                    {
                        fragmentScreenshot = new byte[byteScreenshot.Length - position + 2];
                    }

                    Array.Copy(byteScreenshot, position, fragmentScreenshot, 0,
                        fragmentScreenshot.Length - 2);
                    position += fragmentScreenshot.Length - 2;
                    fragmentScreenshot[fragmentScreenshot.Length - 2] = (byte)(count_array);

                    fragments.Add(fragmentScreenshot);
                }

                for (int i = 0; i < count_array; i++)
                {
                    socket.SendTo(fragments[i], ipEndPoint);
                    Thread.Sleep(5);
                }

            } while (true);
        }

        private static byte[] MakeScreenshot()
        {
            Bitmap bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size,
                CopyPixelOperation.SourceCopy);
            return CompressBitmap(bmpScreenshot);
        }

        private static byte[] CompressBitmap(Bitmap bmp)
        {
            using (var zipped = new MemoryStream())
            {
                using (var gzip = new GZipStream(zipped, CompressionMode.Compress))
                    bmp.Save(gzip, ImageFormat.Bmp);

                return zipped.ToArray();
            }
        }
    }
}
