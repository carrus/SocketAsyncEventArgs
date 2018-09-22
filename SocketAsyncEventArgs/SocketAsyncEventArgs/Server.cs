using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketAsyncEventArgsTest
{
    class Server
    {
        Socket listenSocket;            // the socket used to listen for incoming connection requests
        SocketAsyncEventArgs readEventArgs;
        Socket clientSocket;
        byte[] buffer = new byte[1024];

        public Server(IPEndPoint localEndPoint)
        {
            // create the socket which listens for incoming connections
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);

            readEventArgs = new SocketAsyncEventArgs();
            readEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            readEventArgs.SetBuffer(buffer, 0, 1024);

            // start the server with a listen backlog of 100 connections
            listenSocket.Listen(0);

            // post accepts on the listening socket
            StartAccept(null);

        }

     //  This method is called whenever a receive or send operation is completed on a socket
    //
    // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                  
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        // Begins an operation to accept a connection request from the client 
        //
        // <param name="acceptEventArg">The context object to use when issuing 
        // the accept operation on the server's listening socket</param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

           
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }


        public void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {                     
           clientSocket = e.AcceptSocket;

            Console.WriteLine("A client[{0}] has been connected to the server ", clientSocket.RemoteEndPoint.ToString());

            // As soon as the client is connected, post a receive to the connection
            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            // Accept the next connection request
            StartAccept(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
         
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {

                //echo the data received back to the client
                byte[] byteArray = e.Buffer;
                string str = System.Text.Encoding.Default.GetString(byteArray);
                Console.WriteLine("Receive frome A client {0} {1} ", clientSocket.RemoteEndPoint.ToString(),str);

                bool willRaiseEvent = clientSocket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }

            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
         
            // close the socket associated with the client
            try
            {
                clientSocket.Shutdown(SocketShutdown.Send);
            }
            // throws if client process has already closed
            catch (Exception) { }

            Console.WriteLine("A client[{0}] has been disconnected from the server ",clientSocket.RemoteEndPoint.ToString());

            clientSocket.Close();

           
        }
    }
}
