using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class BoardClientSimple : MonoBehaviour {



    Board m_Board = new Board();
    bool m_boardReady = false;
    int m_MyId;

    public Board Board { get { return m_Board; } }


    // Use this for initialization
    void Start() {


    }


    // Update is called once per frame
    void Update() {
        if (OnlineManager.Instance.IsConnected() && !m_boardReady)
        {
            m_Board.Init();
            m_Board.SetCurrentTurnPlayer(Board.ePlayer.eCircle);
            m_boardReady = true;
            OnlineManager.Instance.OnMessageReceived = OnPlayerMoveReceived;
        }

    }

    public void SendPlayerMove(int index)
    {
        Console.Out.WriteLine("player move");
        if (OnlineManager.Instance.GetPlayerID() == 0 && m_Board.GetCurrentTurnPlayer() == Board.ePlayer.eCircle
            || OnlineManager.Instance.GetPlayerID() == 1 && m_Board.GetCurrentTurnPlayer() == Board.ePlayer.eCross)
        {
            byte[] msg = new byte[1];
            msg[0] = (byte)index;
            OnlineManager.Instance.SendMessage(msg);
            m_Board.PlayerMove(index);
        }
  
    }

    public int  OnPlayerMoveReceived(Assets.Net.Message _msg)
    {
        m_Board.PlayerMove((int)_msg.m_message[0]);
        return 0;
    }

    public void ProcessBoardData(int id, byte[] data)
    {
      

    }



    
}
