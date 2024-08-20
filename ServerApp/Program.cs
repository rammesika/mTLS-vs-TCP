using System;
using System.Threading;

namespace ServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpOrTlsServer server = new TcpOrTlsServer();

            // Start the server, (inside it will initiate the two threads for TCP and TLS)
            server.StartServer();

            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.StopServer();
            Console.WriteLine("Server stopped.");
        }
    }
}
