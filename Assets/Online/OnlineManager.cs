using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class OnlineManager : MonoBehaviour
{
    private static OnlineManager instance = null;
    public static OnlineManager Instance
    {
        get
        {
            return instance;
        }
    }
    
    
    [SerializeField] private String m_Host = "localhost";

    [SerializeField] private int m_port = 25000;

    private SocketAPI m_api;

    
    private bool m_connected = false;

    private bool m_host = false;
    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        m_api = new SocketAPI();
        m_api.Log = Debug.Log;
        m_api.OnMessage = OnMessage; 
    }

    public void Host()
    {
        m_api.Host(m_Host, m_port);
        m_connected = true;
        m_host = true;
    }
    
    public bool IsHost()
    {
        return m_host;
    }

    public int GetNumConnectedClient()
    {
        return m_api.GetNumConnectedClient();
    }
    

    public void Join()
    {
        m_api.Join(m_Host, m_port);
        m_connected = true;
    }

    public void SendPlayerStart( int player)
    {
        using (MemoryStream m = new MemoryStream())
        {
            using (BinaryWriter w = new BinaryWriter(m))
            {
                w.Write(Protocol.GAME_START_PLAYER);
                w.Write(player);
                m_api.Send(m.ToArray());
            }
        }
    }
    
    public void SendPlayerMove( int player, int cellIndex)
    {
        using (MemoryStream m = new MemoryStream())
        {
            using (BinaryWriter w = new BinaryWriter(m))
            {
                w.Write(Protocol.GAME_PLAYER_MOVE);
                w.Write(player);
                w.Write(cellIndex);
                m_api.Send(m.ToArray());
            }
        }
    }

    private int OnMessage(byte[] _msg)
    {
        return _msg.Length;
    }


// Update is called once per frame
    void Update()
    {
        if(m_connected)
            m_api.Process();
    }
}