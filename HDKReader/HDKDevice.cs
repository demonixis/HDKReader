using HidSharp;
using HidSharp.Utility;
using System;
using System.Threading;

namespace HDKReader
{
    public enum HDKStatus
    {
        Unknown = 0,
        NoVideoInput,
        PortraitVideoInput,
        LandscapeVideoInput
    }

    public class HDKDevice
    {
        private HidStream m_Stream;
        private byte[] m_Buffer;

        private float[] m_Quaternion = new float[4];
        private float[] m_AngularVelocity = new float[3];
        private HDKStatus m_HDKStatus = HDKStatus.Unknown;

        public int BufferMaxLength { get; protected set; }

        public ref float[] Quaternion => ref m_Quaternion;
        public ref float[] AngularVelocity => ref m_AngularVelocity;

        public Action<HDKStatus> HDKStatusChanged = null;

        public HDKDevice()
        {
            HidSharpDiagnostics.EnableTracing = true;
            HidSharpDiagnostics.PerformStrictChecks = true;
        }

        /// <summary>
        /// Start the connection with the HDK
        /// </summary>
        /// <returns>It returns true if the connection is OK, otherwise it returns false.</returns>
        public bool Start()
        {
            Close();

            var result = DeviceList.Local.TryGetHidDevice(out HidDevice device, 0x1532, 0x0b00);

            if (result)
            {
                m_Stream = device.Open();
                m_Stream.ReadTimeout = Timeout.Infinite;
                m_Buffer = new byte[device.GetMaxInputReportLength()];
                BufferMaxLength = m_Buffer.Length;
            }

            return result;
        }

        /// <summary>
        /// Close the connection with the HDK.
        /// </summary>
        public void Close()
        {
            if (m_Stream != null)
            {
                m_Stream.Dispose();
                m_Stream.Close();
                m_Stream = null;
            }
        }

        /// <summary>
        /// Fetch data from the HDK and update the array with raw values.
        /// The HDKStatus, Quaternion and AngularVelocity properties are NOT updated.
        /// </summary>
        /// <param name="data">An array which will be populated with raw values.</param>
        /// <returns>Returns true if the Stream is open, otherwise it returns false.</returns>
        public bool Fetch(byte[] data)
        {
            var valid = m_Stream != null;

            if (valid)
                m_Stream.Read(data);

            return valid;
        }

        /// <summary>
        /// Fetch data from the HDK and decode them.
        /// The HDKStatus, Quaternion and AngularVelocity properties are updated.
        /// </summary>
        /// <returns>Returns true if the Stream is open, otherwise it returns false.</returns>
        public bool Fetch()
        {
            if (m_Stream == null)
                return false;

            m_Stream.Read(m_Buffer);

            var status = HDKDataReader.DecodeStatus(m_Buffer);
            if (status != m_HDKStatus)
            {
                m_HDKStatus = status;
                HDKStatusChanged?.Invoke(m_HDKStatus);
            }

            HDKDataReader.DecodeQuaternion(m_Buffer, m_Quaternion);
            HDKDataReader.DecodeAngularVelocity(m_Buffer, m_AngularVelocity);
           
            return true;
        }
    }
}
