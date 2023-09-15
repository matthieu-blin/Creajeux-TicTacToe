using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Network;

class BoardServer
{

    static Board m_Board = new Board();

    static Dictionary<int, int> m_Players = new Dictionary<int, int>();
    static Connection m_Connection;
    public static void Main(string[] args)
    {

        m_Connection = new Connection();
        m_Connection.Listen(25000);
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
        using (MemoryStream m = new MemoryStream())
        {
            using (BinaryWriter w = new BinaryWriter(m))
            {
                w.Write((byte)Protocol.GAME_START_PLAYER);
                w.Write(9);
                w.Write((int)m_Players[0]);
                w.Write((int)m_Board.GetCurrentTurnPlayer() );
                w.Write(false);
                m_Connection.SendSpecific(m_Players.Keys.ElementAt(0), m.ToArray());
            }
        }

        using (MemoryStream m = new MemoryStream())
        {
            using (BinaryWriter w = new BinaryWriter(m))
            {
                w.Write((byte)Protocol.GAME_START_PLAYER);
                w.Write(9);
                w.Write((int)m_Players[1]);
                w.Write((int)m_Board.GetCurrentTurnPlayer());
                w.Write(false);
                m_Connection.SendSpecific(m_Players.Keys.ElementAt(1), m.ToArray());
            }
        }

    }

    public static void ProcessBoardData(int id, byte[] data)
    {
        Console.Out.WriteLine("receive");
        int index = 0;
        using (MemoryStream m = new MemoryStream(data))
        {
            using (BinaryReader w = new BinaryReader(m))
            {
                byte protocol  = w.ReadByte();
                //should be == 2 since it's the only protocol server could receive
                Debug.Assert(protocol == Protocol.GAME_PLAYER_MOVE);
                int size = w.ReadInt32();
                index = w.ReadInt32();
            }
        }
        Board.ePlayer player = (Board.ePlayer)m_Players[id];

        if (m_Board.GetCurrentTurnPlayer() != player)
        {
            Console.Out.WriteLine("player with id " + id + " is cheating");
            return;
        }
        Console.Out.WriteLine("player with id " + id + " play " + index);
        m_Board.PlayerMove(index);
        SendPlayerMove(player, index);

    }
    public static void SendPlayerMove(Board.ePlayer player, int index)
    {

        using (MemoryStream m = new MemoryStream())
        {
            using (BinaryWriter w = new BinaryWriter(m))
            {
                w.Write((byte)Protocol.GAME_PLAYER_MOVE);
                w.Write(4);
                w.Write(index);
                m_Connection.Send(m.ToArray());
            }
        }
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

