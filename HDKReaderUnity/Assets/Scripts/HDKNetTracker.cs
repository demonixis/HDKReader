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
    private Material m_PreviewMaterial;
    private Camera[] m_Cameras = new Camera[2];
    private RenderTexture m_RenderTexture = null;
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
            camera.enabled = true;

            m_Cameras[0] = CreateCamera("LeftEye", camera, true);
            m_Cameras[1] = CreateCamera("RightEye", camera, false);

            m_RenderTexture = new RenderTexture(1920, 1080, 24);
            m_PreviewMaterial = new Material(
                 "Shader \"Hidden/Invert\" {" +
                 "SubShader {" +
                 "    Pass {" +
                 "        ZTest Always Cull Off ZWrite Off" +
                 "        SetTexture [_RenderTex] { combine one-texture }" +
                 "    }" +
                 "}" +
                 "}"
             );

#if UNITY_POST_PROCESSING_STACK_V2
            var layer = GetComponent<PostProcessLayer>();
            if (layer != null)
            {
                CopyPostProcessing(layer, m_Cameras[0].gameObject);
                CopyPostProcessing(layer, m_Cameras[1].gameObject);
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

    private void OnPreRender()
    {
        RenderTexture.active = m_RenderTexture;

        foreach (var camera in m_Cameras)
        {
            camera.Render();

            DrawFullScreenQuad(m_PreviewMaterial);
        }

        RenderTexture.active = null;
    }

    public static void DrawFullScreenQuad(Material material)
    {
        GL.PushMatrix();
        GL.LoadOrtho();

        var i = 0;
        while (i < material.passCount)
        {
            material.SetPass(i);
            GL.Begin(GL.QUADS);
            GL.Color(Color.white);
            GL.TexCoord2(0, 0);
            GL.Vertex3(0, 0, 0.1f);

            GL.TexCoord2(1, 0);
            GL.Vertex3(1, 0, 0.1f);

            GL.TexCoord2(1, 1);
            GL.Vertex3(1, 1, 0.1f);

            GL.TexCoord2(0, 1);
            GL.Vertex3(0, 1, 0.1f);
            GL.End();
            ++i;
        }

        GL.PopMatrix();
    }

    #region SDK Setup

    private Camera CreateCamera(string name, Camera parent, bool left)
    {
        var eyeGameObject = new GameObject(name);
        eyeGameObject.layer = parent.gameObject.layer;
        eyeGameObject.AddComponent<K1RadialDistortion>();

        var eyeCamera = eyeGameObject.GetComponent<Camera>();
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
        eyeCamera.enabled = false;

        var eyeTransform = eyeGameObject.transform;
        eyeTransform.parent = parent.transform;
        eyeTransform.localPosition = new Vector3(EyeOffset * (left ? -1.0f : 1.0f), 0.0f, 0.0f);
        eyeTransform.localRotation = Quaternion.identity;

        return eyeCamera;
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
        eyeLayer.enabled = false;// PostProcessResources is not copied, we've to use relection for that.
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
