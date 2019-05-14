using System;
using System.Threading;
using System.Threading.Tasks;

namespace HDKReader
{
    public class HDKCore
    {
        private HDKDevice m_HDKDevice;
        private Thread m_Thread;
        private Quaternion m_Orientation;
        private Vector3 m_Acceleration;
        private bool m_Running;

        public Quaternion Orientation => m_Orientation;
        public Vector3 Acceleration => m_Acceleration;

        public event Action<string> Log = null;
        public event Action DataReceived = null;

        public HDKCore()
        {
            m_HDKDevice = new HDKDevice();
            m_HDKDevice.DeviceConnected += OnDeviceConnected;
        }

        public void Initialize()
        {
            m_HDKDevice.Initialize();
        }

        public void Shutdown()
        {
            m_HDKDevice.Shutdown();
        }

        private void OnDeviceConnected(bool connected)
        {
            m_Running = false;

            if (m_Thread != null)
            {
                if (m_Thread.IsAlive)
                    m_Thread?.Join();

                m_Thread = null;
            }

            if (connected)
            {
                m_Running = true;
                m_Thread = new Thread(new ThreadStart(FetchData));
                m_Thread.Start();
            }

            Log?.Invoke(connected ? "Ready" : "Stopped");
        }

        private void FetchData()
        {
            Task task = null;

            while (m_Running)
            {
                task = m_HDKDevice.Fetch();
                task.Wait();

                if (m_HDKDevice.DataBuffer[1] == 3 || m_HDKDevice.DataBuffer[1] == 19)
                {
                    HDKDataReader.DecodeSensorData(ref m_HDKDevice.DataBuffer, m_HDKDevice.DataBuffer.Length, ref m_Acceleration, ref m_Orientation);
                    DataReceived?.Invoke();
                }
            }
        }
    }
}
