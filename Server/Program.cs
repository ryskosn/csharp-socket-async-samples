using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;

namespace Server
{
    public class ReceivedData
    {
        public const int bufferSize = 1024;
        public byte[] buffer = new byte[bufferSize];
    }

    public class SynchronousSocketListener
    {
        private const int port = 11000;

        public static ReceivedData data = new ReceivedData();
        public static string str = null;

        public static void StartListeningSync()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine("=== {0}() ===", methodName);

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(
                addressFamily: ipAddress.AddressFamily,
                socketType: SocketType.Stream,
                protocolType: ProtocolType.Tcp
            );

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = listener.Accept();

                    // Start listening for connection.
                    while (true)
                    {
                        int bytesReceived = handler.Receive(data.buffer);
                        str += Encoding.ASCII.GetString(data.buffer, 0, bytesReceived);
                        if (str.IndexOf("<EOF>") > 01)
                        {
                            break;
                        }
                    }

                    Console.WriteLine("Text received : {0}", str);
                    byte[] msg = Encoding.ASCII.GetBytes(str + " sent from client.");
                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }



                // int bufSize = 1024;
                // byte[] buffer = new byte[bufSize];
                // StringBuilder sb = new StringBuilder();

                // int bytesReceived = handler.Receive(buffer, bufSize, 0);

                // if (bytesReceived > 0)
                // {
                //     Console.WriteLine("bytesReceived: {0}", bytesReceived);
                //     sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesReceived));

                //     var content = sb.ToString();
                //     if (content.IndexOf("<EOF>") > -1)
                //     {
                //         Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                //             content.Length, content);
                //         handler.Send(buffer);
                //     }
                // }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress Enter to continue...");
            Console.Read();
        }


        public static int Main(string[] args)
        {
            Console.WriteLine("Hello, this is server!");
            StartListeningSync();
            return 0;
        }
    }
}