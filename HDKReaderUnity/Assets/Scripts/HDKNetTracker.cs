using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public class HDKNetTracker : MonoBehaviour
{
    private WebSocket m_WebSocket = null;
    private Quaternion m_Quaternion;
    private Coroutine m_ConnectCoroutine;
    private Transform m_Transform;
    private readonly Quaternion m_RotationFix = new Quaternion(Mathf.Sqrt(0.5f), 0.0f, 0.0f, Mathf.Sqrt(0.5f));
    private float[] m_DataBuffer = new float[7];
    private bool m_Connected;

    private void Start()
    {
        m_Transform = transform;

        m_WebSocket = new WebSocket($"ws://127.0.0.1:8181");
        m_WebSocket.ConnectAsync();
        m_WebSocket.OnMessage += OnWebSocketServerMessage;
    }

    private void OnDestroy()
    {
        m_WebSocket?.Close();
    }

    private void Update()
    {
        m_Transform.localRotation = m_Quaternion;
    }

    private void OnWebSocketServerMessage(object sender, MessageEventArgs e)
    {
        m_DataBuffer = JsonConvert.DeserializeObject<float[]>(e.Data);

        if (m_DataBuffer.Length == 4)
        {
            m_Quaternion.x = m_DataBuffer[0];
            m_Quaternion.y = m_DataBuffer[1];
            m_Quaternion.z = m_DataBuffer[2];
            m_Quaternion.w = m_DataBuffer[3];

            m_Quaternion = m_Quaternion * m_RotationFix;

            var tmp = m_Quaternion.y;
            m_Quaternion.w = -m_Quaternion.w;
            m_Quaternion.y = m_Quaternion.z;
            m_Quaternion.z = tmp;
        }
    }
}
