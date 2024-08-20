using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public static class Conf
    {
        public static string ServerIp { get; set; } = "127.0.0.1";
        public static int TcpPort { get; set; } = 8080;

        public static int TlsPort { get; set; } = 8443;

         
        public static string ClientCertificateSubjectName { get; set; } = "client.local";
        
    }
}
