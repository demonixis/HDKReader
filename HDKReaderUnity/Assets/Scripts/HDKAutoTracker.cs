using HDKReader;
using UnityEngine;

public class HDKAutoTracker : MonoBehaviour
{
    private HDKDevice m_HDKDevice;
    private Quaternion m_Quaternion;

    private void Start()
    {
        m_HDKDevice = new HDKDevice();
        m_HDKDevice.Initialize();
    }

    public void OnDestroy()
    {
        m_HDKDevice.Close();
    }

    private void Update()
    {
        m_HDKDevice.Fetch();
        var values = m_HDKDevice.Quaternion;
        transform.rotation = new Quaternion(values[0], values[1], values[2], values[3]);
    }
}
