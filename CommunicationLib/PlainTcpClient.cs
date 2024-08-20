using System.Net.Sockets;

namespace TcpClientCommon
{
    public class PlainTcpClient : ITcpClient
    {
        TcpClient tcpClient = new TcpClient();

        public void Connect(string ip, int port)
        {


            try
            {
                tcpClient.Connect(ip, port);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"connection failed: {ex.Message}");
                throw;
            }



        }

        public void SendMessage(string message)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            tcpClient.GetStream().Write(data, 0, data.Length);


        }

        public string ReceiveMessage()
        {
            byte[] data = new byte[256];
            int bytesRead = tcpClient.GetStream().Read(data, 0, data.Length);
            string textReceived = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead);
            return textReceived;
        }

        public void Disconnect()
        {
            tcpClient.Close();

        }


    }
}