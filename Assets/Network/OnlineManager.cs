using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class OnlineManager : MonoBehaviour
{
    private static readonly OnlineManager instance = new OnlineManager();

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static OnlineManager()
    {
    }

    private OnlineManager()
    {
    }

    public static OnlineManager Instance
    {
        get
        {
            return instance;
        }
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
        }
        if (GUILayout.Button("Start Client "))
        {
            m_Net.StartClient("127.0.0.1", 50123);
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

    public int OnGameMessage(Assets.Net.Message msg)
    {
        return 0;
    }

    public int GetPlayerID()
    {
        if(m_Net != null)
            return m_Net.GetPlayerIndex();
        return 0;
    }
}
