using ClientApp;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

class TcpOrTlsServer
{
    TcpListener tcpListener;
    TcpListener tlsListener;
    bool isServerRunning;
    private X509Certificate2 _serverCertificate;

    ManualResetEvent tcpThreadStarted = new ManualResetEvent(false);
    ManualResetEvent tlsThreadStarted = new ManualResetEvent(false);

    public void StartServer()
    {
        _serverCertificate = LoadServerCertificate();

        // Start the plain TCP server (separate thread)
        Thread tcpThread = new Thread(() =>
        {
            tcpThreadStarted.Set();  // Signal - TCP thread has started
            ListenForPlainTcpConnections();
        });
        tcpThread.Start();

        // Start the TLS server 
        Thread tlsThread = new Thread(() =>
        {
            tlsThreadStarted.Set();  // Signal - TLS thread has started
            ListenForTlsConnections();
        });
        tlsThread.Start();

        // Wait for both threads to signal they have started
        tcpThreadStarted.WaitOne();
        tlsThreadStarted.WaitOne();

        Console.WriteLine("Server is now listening on both TCP and TLS ports.");
    }

    private X509Certificate2 LoadServerCertificate()
    {
        // Load the server certificate from the store or file
        using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
        {
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "server.local", validOnly: false);
            if (certs.Count > 0)
            {
                Console.WriteLine($"Loaded server certificate: {certs[0].Subject}");
                return certs[0];
            }
            else
            {
                throw new Exception("Server certificate not found.");
            }
        }
    }


    // Start listening for plain TCP connections
    public void ListenForPlainTcpConnections()
    {
        tcpListener = new TcpListener(IPAddress.Parse(Conf.ServerIp), Conf.TcpPort);
        tcpListener.Start();
        isServerRunning = true;
        Console.WriteLine($"Listening for plain TCP connections on port {Conf.TcpPort}...");

        while (isServerRunning)
        {
            TcpClient client = tcpListener.AcceptTcpClient();
            Console.WriteLine("Plain TCP client connected.");
            Thread clientThread = new Thread(() => HandleTcpClient(client));
            clientThread.Start();
        }
    }

    // Start listening for TLS connections
    public void ListenForTlsConnections()
    {
        tlsListener = new TcpListener(IPAddress.Parse(Conf.ServerIp), Conf.TlsPort);
        tlsListener.Start();
        isServerRunning = true;
        Console.WriteLine($"Listening for TLS connections on port {Conf.TlsPort}...");

        while (isServerRunning)
        {
            TcpClient client = tlsListener.AcceptTcpClient();
            Console.WriteLine("TLS client connected.");
            Thread clientThread = new Thread(() => HandleTlsClient(client));
            clientThread.Start();
        }
    }

    // Handle plain TCP client connections
    private void HandleTcpClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[256];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received message: {message}");

                string[] words = message.Split(' ');
                string trimmedMessage = words.Length > 3 ? string.Join(" ", words.Take(3)) + " ..." : message;
                byte[] response = Encoding.ASCII.GetBytes("Message received: " + trimmedMessage);
                stream.Write(response, 0, response.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client handling failed: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    // Handle TLS client connections
    private void HandleTlsClient(TcpClient client)
    {
        SslStream sslStream = new SslStream(client.GetStream(), false);
        try
        {
            sslStream.AuthenticateAsServer(_serverCertificate, false, System.Security.Authentication.SslProtocols.Tls12, true);
            //Console.WriteLine("TLS connection established.");
            //sslStream.AuthenticateAsServer(GetServerCertificate(), false, System.Security.Authentication.SslProtocols.Tls12, true);
            Console.WriteLine("TLS connection established.");

            byte[] buffer = new byte[256];
            int bytesRead;
            while ((bytesRead = sslStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received message: {message}");

                string[] words = message.Split(' ');
                string trimmedMessage = words.Length > 3 ? string.Join(" ", words.Take(3)) + " ..." : message;
                byte[] response = Encoding.ASCII.GetBytes("Message received: " + trimmedMessage);
                sslStream.Write(response, 0, response.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TLS client handling failed: {ex.Message}");
        }
        finally
        {
            sslStream.Close();
            client.Close();
        }
    }

    // Stop the server
    public void StopServer()
    {
        isServerRunning = false;
        tcpListener?.Stop();
        tlsListener?.Stop();
        Console.WriteLine("Server stopped.");
    }

    // Load the server's certificate
    private X509Certificate2 GetServerCertificate()
    {
        using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
        {
            store.Open(OpenFlags.ReadOnly);
            // Find the certificate by subject name (e.g., "CN=server.local")
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "server.local", validOnly: false);
            if (certs.Count > 0)
            {
                X509Certificate2 certificate = certs[0];
                Console.WriteLine($"Loaded server certificate: {certificate.Subject}, Thumbprint: {certificate.Thumbprint}");
                return certificate;
            }
            else
            {
                throw new Exception("Server certificate not found.");
            }
        }
    }
}
