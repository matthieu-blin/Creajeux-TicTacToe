using UnityEngine;
using System.Collections;
using Network;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

public class BoardClientSimple : MonoBehaviour {
    
    Connection m_Connection;

    Board m_Board = new Board();

    int m_MyId;

    public Board Board { get { return m_Board; } }


	// Use this for initialization
	void Start () {
        m_Connection = new Connection();
        m_Connection.Open("localhost", 8000);

        
        m_Connection.SendDataCallback += ProcessBoardData;
	}

    float time = 0f;
	// Update is called once per frame
	void Update () {

        m_Connection.Update();
    }

    public void SendPlayerMove(int index)
    {
       Debug.Log("send");
       byte[] msg = new byte[1];
       msg[0] = (byte)index;
       m_Connection.Send(msg);
    }

    public void ProcessBoardData(int id, byte[] data)
    {
        Debug.Log("receive");
        byte protocolId = data[0];
        switch (protocolId)
        {
            case TicTacToeProtocol.START:
                {
                    m_MyId =  data[1];
                    m_Board.Init();
                    m_Board.SetCurrentTurnPlayer((Board.ePlayer)data[2]);

                }break;
            case TicTacToeProtocol.MOVE:
                {
                    m_Board.PlayerMove((Board.ePlayer)data[1], data[2]);
                } break;
        }

    }



    
}
