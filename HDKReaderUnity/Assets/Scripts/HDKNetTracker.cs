using Newtonsoft.Json;
using OSVR.Unity;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using WebSocketSharp;

namespace HDKReader.Unity
{
    [RequireComponent(typeof(Camera))]
    public class HDKNetTracker : MonoBehaviour
    {
        private readonly Quaternion m_RotationFix = new Quaternion(Mathf.Sqrt(0.5f), 0.0f, 0.0f, Mathf.Sqrt(0.5f));

        private WebSocket m_WebSocket = null;
        private Coroutine m_ConnectCoroutine = null;
        private Transform m_Transform = null;
        private Quaternion m_Quaternion;
        private Camera m_Camera = null;
        private HDKEye[] m_HDKEyes = new HDKEye[2];
        private float[] m_DataBuffer = new float[7];
        private bool m_Connected;

        private void Start()
        {
            m_Transform = transform;

            m_Camera = GetComponent<Camera>();
            m_Camera.cullingMask = 0;
            m_Camera.clearFlags = CameraClearFlags.Nothing;
            m_Camera.enabled = true;

            Connect();

            SetupSDK(false);
        }

        private void SetupSDK(bool hdk2)
        {
            m_HDKEyes[0] = Setup(false);
            m_HDKEyes[1] = Setup(true);

            HDKEye Setup(bool left)
            {
                var go = new GameObject(left ? "LeftEye" : "RightEye");
                var eye = go.AddComponent<HDKEye>();
                eye.Setup(m_Camera, left, hdk2, !left);
                return eye;
            }
        }

        private void OnDestroy() => Close();

        private void Update()
        {
            m_Transform.localRotation = m_Quaternion;
        }

        #region WebSocket Management

        private void Connect()
        {
            m_WebSocket = new WebSocket($"ws://127.0.0.1:8181");
            m_WebSocket.ConnectAsync();
            m_WebSocket.OnMessage += OnWebSocketServerMessage;
        }

        private void Close()
        {
            if (m_WebSocket != null)
            {
                m_WebSocket.OnMessage -= OnWebSocketServerMessage;

                if (m_WebSocket.IsAlive)
                    m_WebSocket.Close();

                m_WebSocket = null;
            }
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

        #endregion
    }
}