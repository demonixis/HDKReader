using HidSharp;
using HidSharp.Utility;

namespace HDKReader
{
    public class HDKDevice
    {
        private HidStream m_Stream;
        private byte[] m_Buffer = new byte[17];
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
                m_Stream = device.Open();

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

            if (m_Buffer[1] != 3 && m_Buffer[1] != 19)
                return false;

            HDKDataReader.DecodeQuaternion(ref m_Buffer, ref m_Quaternion);
           
            return true;
        }
    }
}
