using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Assets
{
    public class Net
    {
        Socket m_socket = null;

        public const uint BufferSize = 4096;

        public delegate void LogDelegate (string txt);
        public delegate int MessageCallback(Message msg);

        public LogDelegate Log;
        public MessageCallback OnMessageReceived;


        public enum Type{
            UDP,
            TCP,
            NONE

        }
        Type m_netType = Type.NONE;
        EndPoint LocalEndPoint;
        EndPoint RemoteEndPoint;
        bool m_shutdown = false;

        Thread m_mainThread = null;
        bool m_Host = false;

        class Client
        {
            public uint m_ID = 0;
            public EndPoint m_endPoint = null;
            public Thread m_receiveThread = null;
            public Socket m_socket = null;
        }
        private readonly Object lockClients = new Object();
        List<Client> m_clients = null;
        uint ClientIDGenerator = 0;

        public class Message
        {
            public uint m_playerID = 0;
            public byte[] m_message = null;
        }

        private readonly Object lockMessage = new Object();
        private List<Assets.Net.Message> m_pendingMessages = new List<Message>();

        public Net(Type _type)
        {

            m_netType = _type;
            switch( _type)
            {
                case Type.UDP:
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    break;
                case Type.TCP:
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    break;
            }
        }

        private EndPoint ComputeEndPoint(string host, int port)
        {
            IPAddress ipAddress;
            if (host.Length == 0)
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = ipHost.AddressList[0];
            }
            else
            {
                ipAddress = IPAddress.Parse(host);
            }
            return  new IPEndPoint(ipAddress , port);
        }

        public void Process()
        {
            lock(lockMessage)
            {
                foreach(Message msg in m_pendingMessages)
                {
                    OnMessageReceived(msg);
                }
                m_pendingMessages.Clear();
            }

        }

        public void StartServer(string localHost = "", int localPort = 0)
        {
            m_clients = new List<Client>();
            LocalEndPoint = ComputeEndPoint( localHost, localPort);
            m_Host = true;

            switch(m_netType)
            {
                case Type.TCP:  StartTCPServer(); return;
                case Type.UDP:  StartUDPServer(); return;
            }
        }
        private void StartUDPServer(bool _binding = true)
        {
            //bind the socket 
            if(_binding)
                m_socket.Bind(LocalEndPoint);
            byte[] bytes = new byte[BufferSize];
            m_mainThread = new Thread(() =>
           {
               while (!m_shutdown)
               {
                   //blocking function
                   int numByte = m_socket.ReceiveFrom(bytes, ref RemoteEndPoint);
                   if (numByte == 4096)
                   {
                       Log("error : buffer size exceeded");
                       break;
                   }

                   uint playerID = 0;
                   lock (lockClients)
                   {
                       Client client = m_clients.Where(x => IPEndPoint.Equals(x.m_endPoint, RemoteEndPoint)).Select(x => x).FirstOrDefault();
                       if (client == null)
                       {
                           ClientIDGenerator++;
                           playerID = ClientIDGenerator;
                           client = new Client();
                           client.m_ID = playerID;
                           client.m_endPoint = RemoteEndPoint;
                           m_clients.Add(client);
                       }
                   }

                   Log("msg received with size " + numByte + " from " + RemoteEndPoint.ToString());

                   Message msg = new Message();
                   msg.m_message = new byte[numByte];
                   Array.Copy(bytes, msg.m_message, numByte);
                   msg.m_playerID = playerID;
                   lock (m_pendingMessages)
                   {
                       m_pendingMessages.Add(msg);
                   }
               }
           }
           );
            m_mainThread.Start();

        }

        private void StartTCPServer()
        {
            Log("start tcp server");
            m_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            //bind the socket 
            m_socket.Bind(LocalEndPoint);
            //start listening for connection
            m_socket.Listen(200);
            //start accepting client thread
            Log("Waiting connection ... ");
            m_mainThread = new Thread(() =>
           {
               while (!m_shutdown)
               {
                   Socket clientSocket = m_socket.Accept();
                   Client newClient = new Client();
                   ClientIDGenerator++;
                   uint playerID = ClientIDGenerator;
                   newClient.m_ID = playerID;
                   newClient.m_socket = clientSocket;
                   Log("client accepted " + newClient.m_ID);
                   lock (lockClients)
                   {
                       newClient.m_receiveThread = new Thread(() =>
                      {
                          //accept new client - blocking function
                          byte[] bytes = new byte[BufferSize];
                          while (true)
                          {
                              //blocking function
                              int numByte = clientSocket.Receive(bytes);
                              if (numByte == 4096)
                              {
                                  Log("error : buffer size exceeded");
                                  break;
                              }
                              if (numByte == 0)
                              {
                                  break;
                              }
                              Log("msg received with size " + numByte + " from " + newClient.m_socket.RemoteEndPoint.ToString());
                              Message msg = new Message();
                              msg.m_message = new byte[numByte];
                              Array.Copy(bytes, msg.m_message, numByte);
                              msg.m_playerID = playerID;
                              lock (m_pendingMessages)
                              {
                                  m_pendingMessages.Add(msg);
                              }

                          }
                      });
                       newClient.m_receiveThread.Start();
                       m_clients.Add(newClient);

                   }
               }
           });
            m_mainThread.Start();
        }

        public void StartClient(string targetHost, int targetPort, string localHost = "", int localPort = 0 )
        {
            m_Host = false;
            LocalEndPoint = ComputeEndPoint(localHost, localPort);
            RemoteEndPoint = ComputeEndPoint(targetHost, targetPort);
            switch(m_netType)
            {
                case Type.TCP:  StartTCPClient(); return;
                case Type.UDP:  StartUDPClient(); return;
            }
            Log("Client Started");
        }

   
        private void StartUDPClient()
        {
            //nothing to do
            byte[] bytes = new byte[1];
            bytes[0] = 255;
            SendUDPMessage(bytes);
            StartUDPServer(false);
        }

        private void StartTCPClient()
        {
            m_socket.Connect(RemoteEndPoint);
            Log("Socket connected to " + m_socket.RemoteEndPoint.ToString());
            Thread thread = new Thread(() =>
                      {
                          //accept new client - blocking function
                          byte[] bytes = new byte[BufferSize];
                          while (true)
                          {
                              //blocking function
                              int numByte = m_socket.Receive(bytes);
                              if (numByte == 4096)
                              {
                                  Log("error : buffer size exceeded");
                                  break;
                              }
                              if (numByte == 0)
                              {
                                  break;
                              }
                              Log("msg received with size " + numByte + " from " + m_socket.RemoteEndPoint.ToString());
                              Message msg = new Message();
                              msg.m_message = new byte[numByte];
                              Array.Copy(bytes, msg.m_message, numByte);
                              msg.m_playerID = 0;
                              lock (m_pendingMessages)
                              {
                                  m_pendingMessages.Add(msg);
                              }

                          }
                      });
            thread.Start();
        }

        public int SendMessage(string _txt)
        {
            byte[] message = Encoding.ASCII.GetBytes(_txt);
            if(message.Length > BufferSize)
            {
                Log("error : buffer size exceeded");
                return -1; 
            }
            int sizeSent = 0;
            switch (m_netType)
            {
                case Type.TCP:  sizeSent = SendTCPMessage(message); break;
                case Type.UDP:  sizeSent = SendUDPMessage(message); break;
            }
            Log("message sent of size " + sizeSent);
            return sizeSent;
        }

        private int SendUDPMessage(byte[] _msg)
        {
            int numBytes = 0;
            lock (lockClients)
            {
                foreach (Client client in m_clients)
                {
                    numBytes += m_socket.SendTo(_msg, client.m_endPoint);
                }
            }
            return numBytes;
        }
        private int SendTCPMessage(byte[] _msg)
        {
            int numBytes = 0;
            if (m_Host)
            {
                lock (lockClients)
                {
                    foreach (Client client in m_clients)
                    {
                        numBytes += client.m_socket.Send(_msg);
                    }
                }
                return numBytes;
            }
            else
            {
                return m_socket.Send(_msg);
            }
        }

        public void End()
        {
            Log("Ending connection");
            m_shutdown = true;
            m_socket.Shutdown(SocketShutdown.Both);
            m_socket.Close();
            Log("Connection ended");
        }

    }
}
