using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketAsyncEventArgsTest

{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 8088;
            string host = "127.0.0.1";

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            Server ser = new Server(ipe);

            Console.ReadKey();

        }
    }
}
