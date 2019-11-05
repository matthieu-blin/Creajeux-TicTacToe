using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

public class BoardClientSimple : MonoBehaviour {



    Board m_Board = new Board();

    int m_MyId;

    public Board Board { get { return m_Board; } }


    // Use this for initialization
    void Start() {
        m_Board.Init();
        m_Board.SetCurrentTurnPlayer(Board.ePlayer.eCircle);
    }


    // Update is called once per frame
    void Update() {


    }

    public void SendPlayerMove(int index)
    {
        Console.Out.WriteLine("player move");

            m_Board.PlayerMove(index);
  
    }

    public void ProcessBoardData(int id, byte[] data)
    {
      

    }



    
}
