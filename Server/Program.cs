using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        public SynchronousSocketListener() { }
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
            listener.Bind(localEndPoint);
            listener.Listen(100);
            Console.WriteLine("Waiting for a connection...");

            // Accept, Receive
            Socket handler;
            try
            {
                handler = listener.Accept();
                Console.WriteLine("Accept success.");
                int bufSize = 1024;
                byte[] buffer = new byte[bufSize];
                StringBuilder sb = new StringBuilder();

                int bytesReceived = handler.Receive(buffer, bufSize, 0);

                if (bytesReceived > 0)
                {
                    Console.WriteLine("bytesReceived: {0}", bytesReceived);
                    sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesReceived));

                    var content = sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content);
                        handler.Send(buffer);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static int Main(string[] args)
        {
            Console.WriteLine("Hello, this is server!");
            StartListeningSync();
            return 0;
        }
    }

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        // The port number for the remote device.
        private const int port = 11000;

        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        // constructor
        public AsynchronousSocketListener() { }

        public static void StartListening()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine("=== {0}() ===", methodName);

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com" <- ???
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(
                addressFamily: ipAddress.AddressFamily,
                socketType: SocketType.Stream,
                protocolType: ProtocolType.Tcp
            );

            // Bind the socket to the local endpoint and listen for incoming connection.
            try
            {
                listener.Bind(localEndPoint);
                // param backlog: 
                // The maximum length of the pending connections queue.
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    // Set() が呼ばれるまでスレッドをブロックして待つ。
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine("=== {0}() ===", methodName);

            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject
            {
                workSocket = handler
            };

            handler.BeginReceive(
                buffer: state.buffer,
                offset: 0,
                size: StateObject.BufferSize,
                socketFlags: 0,
                callback: new AsyncCallback(ReadCallback),
                state: state
            );
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine("=== {0}() ===", methodName);

            string content = string.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    bytes: state.buffer,
                    index: 0,
                    count: bytesRead));

                // Check for end-of-file tag. If it is not there,
                // read more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the client.
                    // Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(
                        buffer: state.buffer,
                        offset: 0,
                        size: StateObject.BufferSize,
                        socketFlags: 0,
                        callback: new AsyncCallback(ReadCallback),
                        state: state
                    );
                }
            }
        }
        private static void Send(Socket handler, string data)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine("=== {0}() ===", methodName);

            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(
                buffer: byteData,
                offset: 0,
                size: byteData.Length,
                socketFlags: 0,
                callback: new AsyncCallback(SendCallback),
                state: handler
            );
        }

        private static void SendCallback(IAsyncResult ar)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine("=== {0}() ===", methodName);

            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static int Main_(string[] args)
        {
            Console.WriteLine("Hello, this is server!");
            StartListening();
            return 0;
        }
    }
}