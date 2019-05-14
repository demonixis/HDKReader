using System.Threading;
using System.Threading.Tasks;

namespace HDKReader
{
    public class HDKCore
    {
        private HDKDevice m_HDKDevice;
        private Thread m_Thread;
        private HDKQuaternion m_Orientation;
        private HDKVector3 m_Acceleration;
        private bool m_Running;

        public HDKCore()
        {
            m_HDKDevice = new HDKDevice();
            m_HDKDevice.DeviceConnected += OnDeviceConnected;
        }

        public void Initialize()
        {
            m_HDKDevice.Initiliaze();
            m_HDKDevice.InitializeAsync();
        }

        public void Shutdown()
        {
            m_HDKDevice.Shutdown();
        }

        public string Log()
        {
            return $"Acceleration: {m_Acceleration}\nQuaternion: {m_Orientation}";
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
        }

        private void FetchData()
        {
            Task task = null;

            while (m_Running)
            {
                task = m_HDKDevice.Fetch();
                task.Wait();
                HDKDataReader.DecodeSensorConfig(ref m_HDKDevice.DataBuffer, m_HDKDevice.DataBuffer.Length, ref m_Acceleration, ref m_Orientation);
            }
        }
    }
}
