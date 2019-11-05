using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class OnlineManager : MonoBehaviour
{
    private static OnlineManager instance = null;
    bool m_Connected = false;

   public delegate int GameMessageCallback(Assets.Net.Message msg);
   public GameMessageCallback OnMessageReceived;

    public static OnlineManager Instance
    {
        get
        {
            return instance;
        }
    }
    public void Awake()
    {
        instance = this;
    }

    public void StartHost()
    {
        m_Net = new Assets.Net(Assets.Net.Type.TCP);
        m_Net.Log += Log;
        m_Net.OnMessageReceived += OnGameMessage;
        m_Net.StartServer("127.0.0.1", 50123);
        m_Connected = true;
    }

    public void StartClient()
    {
        m_Net = new Assets.Net(Assets.Net.Type.TCP);
        m_Net.Log += Log;
        m_Net.OnMessageReceived += OnGameMessage;
        m_Net.StartClient("127.0.0.1", 50123);
        m_Connected = true;
    }

    public bool IsConnected()
    {
        return m_Connected;
    }

    Assets.Net m_Net = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_Net != null)
            m_Net.Process();
    }

    void OnGUI()
    {
        if(GUILayout.Button("TCP "))
        {
            m_Net = new Assets.Net(Assets.Net.Type.TCP);
            m_Net.Log += Log;
            m_Net.OnMessageReceived += OnGameMessage; 
        }
        if (GUILayout.Button("UDP "))
        {
            m_Net = new Assets.Net(Assets.Net.Type.TCP);
            m_Net.Log += Log;
            m_Net.OnMessageReceived += OnGameMessage; 
        }
        if(GUILayout.Button("Start Server"))
        {
            m_Net.StartServer("127.0.0.1", 50123);
            m_Connected = true;
        }
        if (GUILayout.Button("Start Client "))
        {
            m_Net.StartClient("127.0.0.1", 50123);
            m_Connected = true;
        }
        if (GUILayout.Button("Send test message"))
        {
            byte[] message = Encoding.ASCII.GetBytes("Hello World");
            m_Net.SendMessage(message);
        }
        if (GUILayout.Button("Close"))
        {
            m_Net.End();
        }

    }
    public void Log(string txt)
    {
        Debug.Log(txt);
    }
    public void SendMessage(byte[] _msg)
    {
        m_Net.SendMessage(_msg);
    }

    public int OnGameMessage(Assets.Net.Message msg)
    {
        OnMessageReceived(msg);
     
        return 0;
    }

    public int GetPlayerID()
    {
        if(m_Net != null)
            return m_Net.GetPlayerIndex();
        return 0;
    }
}
