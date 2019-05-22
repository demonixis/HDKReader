using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using WebSocketSharp;

public class HDKNetTracker : MonoBehaviour
{
    private WebSocket m_WebSocket = null;
    private Quaternion m_Quaternion;
    private Coroutine m_ConnectCoroutine;
    private Transform m_Transform;
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
        m_Transform.rotation = m_Quaternion;
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
        }
    }
}
