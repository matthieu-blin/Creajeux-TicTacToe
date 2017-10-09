using System;
using System.Collections;

public class Board 
{


    public enum ePlayer
    {
        eNone = -1,
        eCross = 0, //corresponding to m_BoardButtons textures
        eCircle =1
    }

    private ePlayer[] m_BoardState; // State of the board
    private ePlayer m_CurrentTurn = ePlayer.eNone; // -1 = X, 1 = O
    private ePlayer m_Winner = ePlayer.eNone;

    private int[] m_WinnerStateIndex ;

    public delegate void PlayerMoved(int index, ePlayer player);
    public event PlayerMoved OnPlayerMoved;

    public void Init()
    {
 
        //initialize board state
        m_BoardState = new ePlayer[9];
        for (int istate = 0; istate < m_BoardState.Length; ++istate)
        {
            m_BoardState[istate] = ePlayer.eNone;
        }


    }

    public void Start()
    {

        //random player start
        if (m_CurrentTurn == ePlayer.eNone)
        {
            Random random = new Random();
            m_CurrentTurn = (ePlayer) random.Next(0,2);
        }

}


    public ePlayer[] GetBoard()
    {
        return m_BoardState;
    }

    public int[] GetWinningLine()
    {
        return m_WinnerStateIndex;
    }

    public void PlayerMove(int index)
    {
        PlayerMove(m_CurrentTurn, index);
    }

    public void PlayerMove(Board.ePlayer player, int index)
    {
        if (m_Winner != ePlayer.eNone)
            return;

        m_BoardState[index] = player;
        ePlayer nextturn = (ePlayer)(((int)m_CurrentTurn + 1) % 2);
        m_CurrentTurn = nextturn;
        //check win
        CheckWinner();

        if (m_Winner != ePlayer.eNone)
            Console.Out.WriteLine("winner");
    }

    public ePlayer GetWinner()
    {
        return m_Winner;
    }

    public ePlayer GetCurrentTurnPlayer()
    {
        return m_CurrentTurn;
    }

    public void SetCurrentTurnPlayer(ePlayer  player)
    {
        m_CurrentTurn = player;
    }
    //Check every position (fastest to write since we have 8 possibilties)
    void CheckWinner()
    {
        CheckSameState(0, 1, 2);
        CheckSameState(3, 4, 5);
        CheckSameState(6, 7, 8);
        CheckSameState(0, 4, 8);
        CheckSameState(6, 4, 2);
        CheckSameState(0, 3, 6);
        CheckSameState(1, 4, 7);
        CheckSameState(2, 5, 8);
    }

    //check 3 cells, if identical mark this owner as winner
    void CheckSameState(int i1, int i2, int i3)
    {
        if (m_Winner != ePlayer.eNone)
            return;
        if (m_BoardState[i1] == m_BoardState[i2] && m_BoardState[i2] == m_BoardState[i3])
        {   
            
            m_Winner = m_BoardState[i1];
            m_WinnerStateIndex = new int[] {i1,i2,i3};
        }
    }

}
