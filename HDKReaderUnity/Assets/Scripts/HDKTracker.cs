using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public class HDKTracker : MonoBehaviour
{
    private WebSocket m_WebSocket = null;
    private Quaternion m_Quaternion;
    private float[] m_DataBuffer = new float[7];

    private void Start()
    {
        Connect();
    }

    public void Connect()
    {
        if (m_WebSocket != null)
            return;

        m_WebSocket = new WebSocket($"ws://127.0.0.1:8181");
        m_WebSocket.ConnectAsync();

        m_WebSocket.OnError += (s, e) => Debug.Log(e.Message);
        m_WebSocket.OnClose += (s, e) => Debug.Log("Cloised");
        m_WebSocket.OnMessage += OnWebSocketMessage; ;
    }

    public void Close()
    {
        if (m_WebSocket != null)
        {
            if (m_WebSocket.IsAlive)
                m_WebSocket.Close();

            m_WebSocket = null;
        }
    }

    private void Update()
    {
        transform.rotation = m_Quaternion;
    }

    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        m_DataBuffer = JsonConvert.DeserializeObject<float[]>(e.Data);

        if (m_DataBuffer.Length == 4)
        {
            m_Quaternion.x = m_DataBuffer[0];
            m_Quaternion.y = m_DataBuffer[1];
            m_Quaternion.z = m_DataBuffer[2];
            m_Quaternion.w = m_DataBuffer[3];
        }
    }
}
