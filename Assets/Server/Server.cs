using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Network;

namespace Server
{

    class MainClass
    {
        


        static Board m_Board = new Board();

        static Dictionary<int, int> m_Players = new Dictionary<int, int>();
        static Connection m_Connection;
        public static void Main(string[] args)
        {

            m_Connection = new Connection();
            m_Connection.Listen(8000);
            m_Connection.SendDataCallback += ProcessBoardData;
            m_Connection.AcceptClientCallback += AcceptClient;
            while (true)
            {
                Thread.Sleep(10);
                m_Connection.Update();
                if (m_Board.GetWinner() != Board.ePlayer.eNone)
                    break;
            }
            m_Connection.Update(); //flush last message
            Thread.Sleep(10000);
            m_Connection.Close();
        }


        public static void SendStartGame()
        {
            Console.Out.WriteLine("start game");
            m_Board.Init();
            m_Board.Start();
            byte[] msg = new byte[3];
            msg[0] = (byte)TicTacToeProtocol.START;
            msg[1] = (byte)m_Players[0];
            msg[2] = (byte)m_Board.GetCurrentTurnPlayer(); 
            m_Connection.SendSpecific(m_Players.Keys.ElementAt(0),msg);
            byte[] msg2 = new byte[3];
            msg2[0] = (byte)TicTacToeProtocol.START;
            msg2[1] = (byte)m_Players[1];
            msg2[2] = (byte)m_Board.GetCurrentTurnPlayer();
            m_Connection.SendSpecific(m_Players.Keys.ElementAt(1), msg2);
        }

        public static void ProcessBoardData(int id, byte[] data)
        {
            Console.Out.WriteLine("receive");
            int index = data[0];
            Board.ePlayer player =(Board.ePlayer) m_Players[id];

            if( m_Board.GetCurrentTurnPlayer() != player)
            {
                Console.Out.WriteLine("player with id "+id+" is cheating");
                return;
            }
            Console.Out.WriteLine("player with id " + id + " play "+ index);
            m_Board.PlayerMove(index);
            SendPlayerMove(player, index);

        }
        public static void SendPlayerMove(Board.ePlayer player, int index)
        {
            
            byte[] msg = new byte[3];
            msg[0] = (byte) TicTacToeProtocol.MOVE;
            msg[1] = (byte)player;
            msg[2] = (byte)index;
            m_Connection.Send(msg);
        }


        public static void AcceptClient(int id)
        {
            Console.Out.WriteLine("board client to player");
            if (m_Players.Count == 1)
            {
                m_Players.Add(id, (int)Board.ePlayer.eCross);
                SendStartGame();
            }
            if (m_Players.Count == 0)
            {
                m_Players.Add(id, (int)Board.ePlayer.eCircle);
            }
            

        }

    }
}
