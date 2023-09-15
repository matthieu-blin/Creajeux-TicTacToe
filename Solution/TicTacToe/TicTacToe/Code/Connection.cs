using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Network
{
    public class Connection 
    {
		//main socket
        private Socket m_MainSocket;
		//client sockets for server
        private Dictionary<int,Socket> m_ClientSockets;

		//data received in thread for processing in main loop
        Queue<KeyValuePair<int, byte[]>> m_DataReceived = new Queue<KeyValuePair<int, byte[]>>();
        
		//data pushed by game to send next tick / loop
		Queue<KeyValuePair<int, byte[]>> m_DataToSend = new Queue<KeyValuePair<int, byte[]>>();

		//list of accepted client
        Queue<int> m_AcceptedClient = new Queue<int>();

		//callback call when accepting client
		public delegate void AcceptClient(int index);
		public AcceptClient AcceptClientCallback;

		//callback call when you need to process the data
        public delegate void ProcessData(int id, byte[] data);
        public ProcessData SendDataCallback;

		//thread listening message reception from socket(s)
        private Thread m_ListenThread;
		//thread listening connecting client
        private Thread m_AcceptThread;

		int m_ClientUIDCounter = 0;

		/// <summary>
		/// return true if the main socket is connected
		/// </summary>
        public bool Connected()
        {
            return m_MainSocket.Connected;
        }

		/// <summary>
		/// return the main socket
		/// </summary>
        public Socket Socket()
        {
            return m_MainSocket;
        }

		/// <summary>
		/// constructor (empty)
		/// </summary>
        public Connection()
        {
        }

		//push data for sending to every socket available
        public void Send(byte[] msg)
        {
            m_DataToSend.Enqueue(new KeyValuePair<int, byte[]>(-1, msg));
        }

		//push data for sending to one specific client (server only)
        public void SendSpecific(int id, byte[] msg)
        {
            m_DataToSend.Enqueue(new KeyValuePair<int, byte[]>(id, msg));
        }

        /// <summary>
        /// retrieve the ip in string from a string host, using DNS
        /// </summary>
        /// <returns>The adr.</returns>
        /// <param name="host">Host.</param>
        private String GetAdr(string host)
        {
            try
            {
                IPHostEntry iphostentry = Dns.GetHostEntry(host);
                String IPStr = "";
                foreach (IPAddress ipaddress in iphostentry.AddressList)
                {
                    IPStr = ipaddress.ToString();
                    return IPStr;
                }
            }
            catch (SocketException E)
            {
                Console.WriteLine(E.Message);
            }

            return "";
        }

		/// <summary>
		/// Server method to start server socket : listening client connection and so on
		/// </summary>
		/// <param name="port">Port.</param>
        public void Listen( int port)
        {
			ResetMainSocket();
            m_ClientSockets = new Dictionary<int, Socket>();
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Console.WriteLine("IP=" + ip.ToString());
            try
            {
				//bind our socket on our local network hardware on specific port
                m_MainSocket.Bind(new IPEndPoint(ip, port));
				//listen up to 10 concurrent users
                m_MainSocket.Listen(10);
            }
            catch (SocketException E)
            {
                Console.WriteLine(E.Message);
                return;
            }
            try
            {
				//start a thread waiting for client to connect
                m_AcceptThread = new Thread(new ThreadStart(WaitingClient));
                m_AcceptThread.Start();

				//start a thread for listening connected clients
                m_ListenThread = new Thread(new ThreadStart(ProcessSockets));
                m_ListenThread.Start();

            }
            catch (Exception E)
            {
                Console.Out.WriteLine("[socket] Démarrage Thread" + E.Message);
            }
            Console.Out.WriteLine("[socket] Listening on port "+port);
        }
		/// <summary>
		/// Client method to open a connection on a server
		/// </summary>
		/// <param name="host">Host.</param>
		/// <param name="port">Port.</param>
        public void Open(string host, int port)
        {
			ResetMainSocket();
            try
            {
				//retrieve the host ip endpoint
                IPAddress ip = IPAddress.Parse(GetAdr(host));
                IPEndPoint ipEnd = new IPEndPoint(ip, port);
				//connect to it
                m_MainSocket.Connect(ipEnd);
            }
            catch (SocketException E)
            {
                Console.Out.WriteLine("[socket] socket exception " + E.ToString());
                return;
            }

            try
            {
				//start listening the server
                m_ListenThread = new Thread(new ThreadStart(ProcessMainSocket));
                m_ListenThread.Start();

            }
            catch (Exception E)
            {
                Console.Out.WriteLine("[socket] Démarrage Thread" + E.Message);
            }
            Console.Out.WriteLine("[socket] Opened to "+ host +":"+port);
        }

 
		/// <summary>
		/// just close the main socket if there's an existing one, and create a new one
		/// </summary>
        public void ResetMainSocket()
        {
            if (m_MainSocket != null)
            {
                m_MainSocket.Close();
            }
            m_MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

		/// <summary>
		/// function looping and  waiting for client 
		/// using Socket.Accept blocking function
		/// push accepted client id the the m_AcceptedClient queue for retreiving it in main loop
		/// </summary>
         private void WaitingClient()
        {
            while (true)
            {
                int clientId = 0;
                Socket clientSocket = m_MainSocket.Accept();
                
                
				clientId = m_ClientUIDCounter;
				m_ClientUIDCounter++;

				lock (m_ClientSockets) //for thread safe
				{
                    m_ClientSockets.Add(clientId, clientSocket);
                }

				lock (m_AcceptedClient)//for thread safe
                {
                    m_AcceptedClient.Enqueue(clientId);
                }

                Console.Out.WriteLine("[socket] accepted client");

            }

        }

		/// <summary>
		/// process only the main socket : threaded function 
		/// </summary>
         private void ProcessMainSocket()
         {
             try
             {
                 while (true)
                 {
                     ProcessSocket(m_MainSocket);
					//give hand on others thread
                     Thread.Sleep(10);
                 }
             }
             catch
             {
                 //this thread can be aborted at anytime, just catch the exception
                 Thread.ResetAbort();
             }
         }
		/// <summary>
		/// Processes all the client sockets : threaded function
		/// </summary>
        private void ProcessSockets()
        {
            try
            {
                while (true)
                {
                    lock (m_ClientSockets)
                    {
                        foreach (Socket socket in m_ClientSockets.Values)
                        {
                            ProcessSocket(socket);
                        }
                    }
					//give hand on others thread
                    Thread.Sleep(10);
                }
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.ToString());
				///this thread can be aborted at anytime, just catch the exception
                Thread.ResetAbort();
            }
        }
		/// <summary>
		/// Process the socket pass as parameter
		/// </summary>
		/// <param name="socket">Socket.</param>
        private void ProcessSocket(Socket socket)
        {
            if (socket.Connected) //do nothing if the socket is not connected
            {
                //check if we can retrieve some data
                if (socket.Poll(10, SelectMode.SelectRead) && socket.Available == 0)
                {
                    //La connexion a été cloturée par le serveur ou bien un problème réseau est apparu
                    Console.Error.WriteLine("[socket] error: Disconnected from server");
                    Thread.CurrentThread.Abort();
                }
                //if we have data for reading ...
                if (socket.Available > 0)
                {
					//create a byte array for storing these
                    byte[] buff = new byte[socket.Available];
                    try
                    {
                        //and receive it
                        socket.Receive(buff, 0, buff.Length, SocketFlags.None);
                      }
                    catch (SocketException E)
                    {
                        Console.Error.WriteLine("[socket] error receiving message :" + E.ToString());
                    }

                    try
                    {
                        int id = -1;
                        if (m_MainSocket != socket)
                        {
                            lock (m_ClientSockets)
                            {
								foreach(KeyValuePair<int,Socket> kvp in m_ClientSockets)
								{
									if( kvp.Value == socket)
									{
										id = kvp.Key;
										break;
									}
								}
								if( id < 0)
								{
									Console.Error.WriteLine("no id for this socket, skip processing");
									return;
								}

                            }
                        }
                        //enqueue data to process it in the main loop
                        lock (m_DataReceived)
                        {
                            m_DataReceived.Enqueue(new KeyValuePair<int, byte[]>(id, buff));
                        }
                    }
                    catch (Exception E)
                    {
                        Console.Error.WriteLine(E.Message);
                    }

                }
            }
        }

		/// <summary>
		/// Close the connection, sockets and all threads, call this on quitting application
		/// </summary>
        public void Close()
        {
            if (m_ListenThread != null)
            {
                try
                {
                    m_ListenThread.Abort();
                    m_ListenThread.Join();
                }
                catch (Exception E)
                {
                    Console.Error.WriteLine("[socket] thread error: " + E.Message);
                }
            }
            if (m_MainSocket != null && m_MainSocket.Connected)
            {
                try
                {
                    m_MainSocket.Shutdown(SocketShutdown.Both);
                    m_MainSocket.Close();
                    if (m_MainSocket.Connected)
                    {
                         Console.Error.WriteLine("[socket] error: " + Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
                    }

                }
                catch (SocketException SE)
                {
                     Console.Error.WriteLine("[socket] error:" + SE.Message);
                }
                Console.Out.WriteLine("[socket] closed");
            }

			foreach(KeyValuePair<int,Socket> kvp in m_ClientSockets)
			{
				kvp.Value.Shutdown(SocketShutdown.Both);
				kvp.Value.Close();
				if (kvp.Value.Connected)
				{
					Console.Error.WriteLine("[socket] error: " + Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
				}
			}


        }

		/// <summary>
		/// Main update loop, call this to update network on your game loop
		/// </summary>
        public void Update()
        {
            //process new client
            int count = m_AcceptedClient.Count;
            int clientId = 0;
            for (int i = 0; i < count; ++i)
            {
                lock (m_AcceptedClient)
                {
                    clientId = m_AcceptedClient.Dequeue();
				}
				if( AcceptClientCallback != null)
				    AcceptClientCallback(clientId);
            }
            //send all data
            KeyValuePair<int, byte[]> data;
            while (m_DataToSend.Count > 0)
            {
                data = m_DataToSend.Dequeue();
                if( m_ClientSockets!= null)
                {
                    lock (m_ClientSockets)
                    {
                            for (int id = 0; id < m_ClientSockets.Count; ++id)
                            {
                                if (data.Key < 0 || data.Key == id)
                                {
                                    try
                                    {
                                        m_ClientSockets[id].Send(data.Value);
                                    }catch(Exception e){};
                                }
                            }
                    }
                }
                else
                {
                     try{
                         m_MainSocket.Send(data.Value);
                     }
                     catch (Exception e) { };
                }
            }

            //process received data
            count = m_DataReceived.Count;
            for (int i = 0; i < count; ++i)
            {
                lock (m_DataReceived)
                {
                   data = m_DataReceived.Dequeue();
                }
				//send to callback
				if( SendDataCallback != null)
               	 	SendDataCallback(data.Key, data.Value);
            }
        }
    }
}
