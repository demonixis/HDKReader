using Newtonsoft.Json;
using OSVR.Unity;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using WebSocketSharp;

[RequireComponent(typeof(Camera))]
public class HDKNetTracker : MonoBehaviour
{
    private const float EyeOffset = 0.0031f;
    private readonly Quaternion m_RotationFix = new Quaternion(Mathf.Sqrt(0.5f), 0.0f, 0.0f, Mathf.Sqrt(0.5f));

    private WebSocket m_WebSocket = null;
    private Coroutine m_ConnectCoroutine = null;
    private Transform m_Transform = null;
    private Quaternion m_Quaternion;

    private float[] m_DataBuffer = new float[7];
    private bool m_Connected;

    [SerializeField]
    private bool m_AutoConstruct = true;

    private void Start()
    {
        m_Transform = transform;

        if (m_AutoConstruct)
        {
            var camera = GetComponent<Camera>();
            camera.enabled = false;

            var leftEye = CreateCamera("LeftEye", camera, true);
            var rightEye = CreateCamera("RightEye", camera, false);

#if UNITY_POST_PROCESSING_STACK_V2
            var layer = GetComponent<PostProcessLayer>();
            if (layer != null)
            {
                CopyPostProcessing(layer, leftEye);
                CopyPostProcessing(layer, rightEye);
                layer.enabled = false;
            }
#endif 
        }

        Connect();
    }

    private void OnDestroy() => Close();

    private void Update()
    {
        m_Transform.localRotation = m_Quaternion;
    }

    #region SDK Setup

    private GameObject CreateCamera(string name, Camera parent, bool left)
    {
        var eyeGameObject = new GameObject(name);
        eyeGameObject.layer = parent.gameObject.layer;
        eyeGameObject.AddComponent<K1RadialDistortion>();

        var eyeCamera = eyeGameObject.AddComponent<Camera>();
        eyeCamera.depth = 3;
        eyeCamera.renderingPath = parent.renderingPath;
        eyeCamera.useOcclusionCulling = parent.useOcclusionCulling;
        eyeCamera.rect = new Rect(left ? 0.0f : 0.5f, 0.0f, 0.5f, 1.0f);
        eyeCamera.farClipPlane = parent.farClipPlane;
        eyeCamera.nearClipPlane = parent.nearClipPlane;
        eyeCamera.allowHDR = parent.allowHDR;
        eyeCamera.allowMSAA = parent.allowMSAA;
        eyeCamera.allowDynamicResolution = parent.allowDynamicResolution;
        eyeCamera.fieldOfView = 90.0f;
        eyeCamera.cullingMask = parent.cullingMask;
        eyeCamera.backgroundColor = parent.backgroundColor;
        eyeCamera.stereoTargetEye = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;

        var eyeTransform = eyeGameObject.transform;
        eyeTransform.parent = parent.transform;
        eyeTransform.localPosition = new Vector3(EyeOffset * (left ? -1.0f : 1.0f), 0.0f, 0.0f);
        eyeTransform.localRotation = Quaternion.identity;

        return eyeGameObject;
    }

#if UNITY_POST_PROCESSING_STACK_V2
    private void CopyPostProcessing(PostProcessLayer layer, GameObject eye)
    {
        var eyeLayer = eye.AddComponent<PostProcessLayer>();
        eyeLayer.enabled = layer.enabled;
        eyeLayer.antialiasingMode = layer.antialiasingMode;
        eyeLayer.fastApproximateAntialiasing = layer.fastApproximateAntialiasing;
        eyeLayer.stopNaNPropagation = layer.stopNaNPropagation;
        eyeLayer.volumeLayer = layer.volumeLayer;
        eyeLayer.volumeTrigger = layer.volumeTrigger;
    }
#endif

    #endregion

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
