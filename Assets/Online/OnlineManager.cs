using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    public void Join()
    {
        m_api.Join(m_Host, m_port);
        m_connected = true;
    }

    public void Send()
    {
        byte[] buffer = new byte[0];
        m_api.Send(buffer);
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