namespace TcpClientCommon
{
    public interface ITcpClient
    {
        void Connect(string ip, int port);
        void SendMessage(string message);
        string ReceiveMessage();
        void Disconnect();


    }
}