
using ClientApp;
using System.Net.Security;
using TcpClientCommon;


class MessageClient
{
    static void Main(string[] args)
    {

        ITcpClient client = null;

        Console.WriteLine("Choose communication type: (1) - Plain TCP, (2) - Secure TCP (TLS)");
        string userChoice = Console.ReadLine();

        if (userChoice == "1")
        {
            client = new PlainTcpClient();
        }
        else if (userChoice == "2")
        {
            client = new TlsTcpClient(Conf.ClientCertificateSubjectName);
        }
        
        else
        {
            Console.WriteLine("Wrong choise");
            return;
        }
        string ip = Conf.ServerIp;

        int port = userChoice == "1" ? Conf.TcpPort : Conf.TlsPort;

        try
        {
            
            client.Connect(ip, port);
            Console.WriteLine("Connected to the server.");

            while (true)
            {
                Console.Write("Enter message to send (or type 'exit' to quit): ");
                string message = Console.ReadLine();

                if (message.ToLower() == "exit")
                {
                    break;
                }

                client.SendMessage(message);
                string response = client.ReceiveMessage();
                Console.WriteLine($"Server response: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Disconnect();
            Console.WriteLine("Disconnected from the server.");
        }
    }

}


