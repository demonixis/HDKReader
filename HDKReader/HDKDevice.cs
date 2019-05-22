using HidSharp;
using HidSharp.Utility;
using System.Threading;

namespace HDKReader
{
    public class HDKDevice
    {
        private HidStream m_Stream;
        private byte[] m_Buffer;
        private float[] m_Quaternion = new float[4];

        public ref float[] Quaternion => ref m_Quaternion;

        public HDKDevice()
        {
            HidSharpDiagnostics.EnableTracing = true;
            HidSharpDiagnostics.PerformStrictChecks = true;
        }

        public bool Initialize()
        {
            Close();

            var result = DeviceList.Local.TryGetHidDevice(out HidDevice device, 0x1532, 0x0b00);

            if (result)
            {
                m_Stream = device.Open();
                m_Stream.ReadTimeout = Timeout.Infinite;
                m_Buffer = new byte[device.GetMaxInputReportLength()];
            }

            return result;
        }

        public void Close()
        {
            if (m_Stream != null)
            {
                m_Stream.Close();
                m_Stream = null;
            }
        }

        public bool Fetch(ref byte[] data)
        {
            var valid = m_Stream != null;

            if (valid)
                data = m_Stream.Read();

            return valid;
        }

        public bool Fetch()
        {
            if (m_Stream == null)
                return false;

            m_Stream.Read(m_Buffer);

            HDKDataReader.DecodeQuaternion(ref m_Buffer, ref m_Quaternion);
           
            return true;
        }
    }
}
