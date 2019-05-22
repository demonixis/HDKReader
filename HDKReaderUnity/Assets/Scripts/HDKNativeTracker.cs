using HDKReader;
using System.Collections;
using System.Threading;
using UnityEngine;

public class HDKNativeTracker : MonoBehaviour
{
    private HDKDevice m_HDKDevice;
    private Thread m_Thread;
    private Quaternion m_Quaternion;
    private Transform m_Transform;
    private bool m_IsRunning;

    private void Start()
    {
        m_Transform = transform;
        m_HDKDevice = new HDKDevice();

        if (m_HDKDevice.Initialize())
        {
            m_Thread = new Thread(new ThreadStart(Loop));
            m_Thread.Start();
        }

        StartCoroutine(UpdateRotation());
    }

    public void OnDestroy()
    {
        if (m_Thread != null && m_Thread.IsAlive)
        {
            m_IsRunning = false;
            m_Thread.Join();
            m_Thread = null;
        }

        m_HDKDevice.Close();
    }

    private void Loop()
    {
        m_IsRunning = true;

        byte[] buffer = new byte[17];
        var x = 0.0f;
        var y = 0.0f;
        var z = 0.0f;
        var w = 0.0f;

        while (m_IsRunning)
        {
            m_HDKDevice.Fetch(ref buffer);
            HDKDataReader.DecodeQuaternion(ref buffer, ref x, ref y, ref z, ref w);
            m_Quaternion.x = x;
            m_Quaternion.y = y;
            m_Quaternion.z = z;
            m_Quaternion.w = w;
            Thread.Sleep(10);
        }
    }

    private void Update()
    {
        m_Transform.rotation = m_Quaternion;
    }

    private void LateUpdate()
    {
        m_Transform.rotation = m_Quaternion;
    }

    private IEnumerator UpdateRotation()
    {
        var wait = new WaitForEndOfFrame();

        while (true)
        {
            yield return wait;
            m_Transform.rotation = m_Quaternion;
        }
    }
}
