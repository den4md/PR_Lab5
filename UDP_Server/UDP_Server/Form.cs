using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace UDP_Server
{
   
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        }


        private static int portReceive = 16000;

        private Socket socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Dgram, ProtocolType.Udp);

        private byte[] fragmentScreenshot = new byte[UInt16.MaxValue];

        public List<byte[]> fragments = new List<byte[]>();

        public int count_array = 0;

        private static IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, portReceive);

        private EndPoint endPoint = ipEndPoint;


        private void Form1_Load(object sender, EventArgs e)
        {
            socket.Bind(endPoint);
            socket.BeginReceiveFrom(fragmentScreenshot, 0, fragmentScreenshot.Length, SocketFlags.None,
                ref endPoint, new AsyncCallback(ReceiveCallback), socket);
        }


        public void ReceiveCallback(IAsyncResult ar)
        {
                while (true)
                {               
                    int nFragment = 0;
                    short value;
                    do
                    {
                        byte[] recieve_bytes = new byte[65002];

                        int num_bytes = socket.ReceiveFrom(recieve_bytes, ref endPoint);
                   
                        count_array += num_bytes - 2; 
                        byte[] fragment_bytes = new byte[num_bytes - 2];
                        byte[] value_bytes = new byte[2];

                        Array.Copy(recieve_bytes, num_bytes - 2, value_bytes, 0, 2);
                        Array.Copy(recieve_bytes, 0, fragment_bytes, 0, fragment_bytes.Length);

                        value = BitConverter.ToInt16(value_bytes, value_bytes.Length - 2);
                        fragments.Add(fragment_bytes);

                        nFragment++;
                    } while (nFragment != value);

                    byte[] screenshot_bytes = new byte[count_array];

                    for (int i = 0; i < fragments.Count; i++)
                    {
                        Array.Copy(fragments[i], 0, screenshot_bytes, i * 65000, fragments[i].Length);
                    }

                    byte[] result = Decompress(screenshot_bytes);

                    MemoryStream memory_stream = new MemoryStream(result);
                    System.Drawing.Bitmap bmp =
                       (System.Drawing.Bitmap)System.Drawing.Image.FromStream(memory_stream);

                    pictureBox1.Image = bmp;

                    fragments.Clear();
                    count_array = 0;
                }
        }


        static byte[] Decompress(byte[] full_array)
        {
                using (var compressedMs = new MemoryStream(full_array))
                {
                    using (var decompressedMs = new MemoryStream())
                    {
                        using (var gzs = new BufferedStream(new GZipStream(compressedMs,
                            CompressionMode.Decompress), 4096))
                        {
                            gzs.CopyTo(decompressedMs);
                        }
                        return decompressedMs.ToArray();
                    }
                }
        }


      

        
    }   
}
