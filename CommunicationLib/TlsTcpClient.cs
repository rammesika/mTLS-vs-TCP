using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TcpClientCommon
{
    public class TlsTcpClient : ITcpClient
    {
        private TcpClient _tcpClient;
        private SslStream _sslStream;
        private X509Certificate2 _clientCertificate;

        // Constructor to initialize with a client certificate loaded from the store
        public TlsTcpClient(string clientCertificateSubjectName)
        {
            _clientCertificate = LoadCertificateFromStore(clientCertificateSubjectName);
        }

        private X509Certificate2 LoadCertificateFromStore(string subjectName)
        {
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, validOnly: false);
                if (certs.Count > 0)
                {
                    Console.WriteLine($"Loaded client certificate: {certs[0].Subject}");
                    return certs[0];
                }
                else
                {
                    throw new Exception("Client certificate not found.");
                }
            }
        }

        public void Connect(string ip, int port)
        {
            _tcpClient = new TcpClient();
            try
            {
                _tcpClient.Connect(ip, port);
                Console.WriteLine($"Connected to server at {ip}:{port}");

                // Create SSL stream and authenticate as a client with a client certificate
                _sslStream = new SslStream(
                    _tcpClient.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate)
                );

                try
                {
                    // Authenticate as a client with the client certificate
                    _sslStream.AuthenticateAsClient(ip, new X509CertificateCollection { _clientCertificate }, System.Security.Authentication.SslProtocols.Tls12, false);

                }
                catch (System.Security.Authentication.AuthenticationException ex)
                {
                    Console.WriteLine($"TLS client handling failed: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
               }
            catch (Exception ex)
            {
                Console.WriteLine($"TLS connection failed: {ex.Message}");
                throw;
            }
        }

        public void SendMessage(string message)
        {
            if (_sslStream == null)
            {
                throw new InvalidOperationException("Connection must be established before sending messages.");
            }

            byte[] data = Encoding.ASCII.GetBytes(message);
            _sslStream.Write(data, 0, data.Length);
        }

        public string ReceiveMessage()
        {
            if (_sslStream == null)
            {
                throw new InvalidOperationException("Connection must be established before receiving messages.");
            }

            byte[] buffer = new byte[256];
            int bytesRead = _sslStream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        public void Disconnect()
        {
            _sslStream?.Close();
            _tcpClient?.Close();
            Console.WriteLine("Disconnected from the server.");
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Implement proper certificate validation logic here
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true; // The certificate is valid
            }
            // Allow a specific error for development/testing (Not recommended for production)
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                Console.WriteLine("Accepting certificate despite name mismatch.");
                return true;
            }
            Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            return false; // The certificate is not valid
        }
    }
}
