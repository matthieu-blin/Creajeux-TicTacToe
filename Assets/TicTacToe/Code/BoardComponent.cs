using UnityEngine;
using System.Collections;

public class BoardComponent : MonoBehaviour {


    public Texture2D m_BackGround;
    public Texture2D[] m_BoardButtons;
    public GUISkin m_CustomGUISkin;

    private int m_BoardButtonWidth;
    private int m_BoardButtonHeight;

    private const int m_xOffset = 13; //offset of the first button on board texture
    private const int m_yOffset = 80; //''
    private const int m_xSpacing = 6;//spacing between two button on board texture
    private const int m_ySpacing = 5;

   
    private const float m_FlashDelay = 0.7f;
    private float m_FlashTime = 0.3f;

    BoardClientSimple m_Network;

    void Start()
    {
 
        //all button textures have the same properties, just store these values
        m_BoardButtonWidth = m_BoardButtons[0].width;
        m_BoardButtonHeight = m_BoardButtons[0].height;

        m_Network = gameObject.GetComponent<BoardClientSimple>();
    }
    void Update()
    {
              //flash line
        if (m_Network.Board.GetWinner() != Board.ePlayer.eNone)
        {
            m_FlashTime += Time.deltaTime;
            if (m_FlashTime > m_FlashDelay * 2)
            {
                m_FlashTime = 0f;
            }
        }
    }

   
    //Main rendering function
    void OnGUI()
    {
        //aply custom skin
        GUI.skin = m_CustomGUISkin;

        // Render out the blank background
        GUI.Label(new Rect(0, 0, 320, 480), new GUIContent(m_BackGround));

        //if game as not start, do nothing
        if (m_Network.Board.GetCurrentTurnPlayer() == Board.ePlayer.eNone)
            return;

        //if winner, just render the board as labels, and flash winner line
        if (m_Network.Board.GetWinner() != Board.ePlayer.eNone)
        {
            bool hideflash = m_FlashTime >= m_FlashDelay;
            for (int istate = 0; istate < m_Network.Board.GetBoard().Length; ++istate)
            {
                //compute cell position
                int xPos = m_xOffset + istate % 3 * m_BoardButtonWidth + istate % 3 * m_xSpacing;
                int yPos = m_yOffset + istate / 3 * m_BoardButtonHeight + istate / 3 * m_ySpacing;

                bool hide = false; //show cell by default
                if (istate == m_Network.Board.GetWinningLine()[0] || istate == m_Network.Board.GetWinningLine()[1] || istate == m_Network.Board.GetWinningLine()[2])
                {
                    hide = hideflash; //hide only if this cell is on winner liner, and flashing
                }

                Board.ePlayer state = m_Network.Board.GetBoard()[istate];
                //render cell
                if( state != Board.ePlayer.eNone && !hide)
                    GUI.Label(new Rect(xPos, yPos, m_BoardButtonWidth, m_BoardButtonHeight), m_BoardButtons[(int)state]);
               
            }
            return;
        }

        ///in game //////////////////////////////////
        
        //render owner of current turn;
        float smallbtnwidth = m_BoardButtonWidth / 2;
        float smallbtnheight = m_BoardButtonHeight / 2;
        GUI.Label(new Rect(m_BackGround.width / 2 - smallbtnwidth / 2, 36 - smallbtnheight / 2, smallbtnwidth, smallbtnheight), m_BoardButtons[(int)m_Network.Board.GetCurrentTurnPlayer()]);


        //render board
        for (int istate = 0; istate < m_Network.Board.GetBoard().Length; ++istate)
        {
            //compute cell position
            int xPos = m_xOffset + istate % 3 * m_BoardButtonWidth + istate % 3 * m_xSpacing;
            int yPos = m_yOffset + istate / 3 * m_BoardButtonHeight + istate / 3 * m_ySpacing;

            Board.ePlayer state = m_Network.Board.GetBoard()[istate];
            //render as button if empty
            if (state == Board.ePlayer.eNone)
            {
                //if the current player clic on this cell, mark the cell state as his own, and switch turn
                if (GUI.Button(new Rect(xPos, yPos, m_BoardButtonWidth, m_BoardButtonHeight),GUIContent.none))
                {
                    m_Network.SendPlayerMove(istate);
                }
            }
            //or as label
            else
            {
                GUI.Label(new Rect(xPos, yPos, m_BoardButtonWidth, m_BoardButtonHeight), m_BoardButtons[(int)state]);
            }
        }
    }
}
