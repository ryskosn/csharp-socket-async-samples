using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;

namespace Client
{
    public class SynchronousClient
    {
        private const int port = 11000;
        private static string response = string.Empty;
        public static void StartClient()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine("=== {0}() ===", methodName);

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);
            Socket client = new Socket(
                addressFamily: ipAddress.AddressFamily,
                socketType: SocketType.Stream,
                protocolType: ProtocolType.Tcp
            );

            try
            {
                client.Connect(remoteEndPoint);
                Console.WriteLine("Connect success.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static int Main(string[] args)
        {
            Console.WriteLine("Hello, this is synchronous client!");
            StartClient();
            return 0;
        }
    }
}