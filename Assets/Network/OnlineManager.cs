using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineManager : MonoBehaviour
{

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
            m_Net.SendMessage("Hello World");
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
}
