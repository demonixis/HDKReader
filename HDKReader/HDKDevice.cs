using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDKReader
{
    public class HDKDevice
    {
#if LIBUSB
        private const int PollMilliseconds = 6000;
#else
        private const int PollMilliseconds = 3000;
#endif

        private readonly List<FilterDeviceDefinition> m_DeviceDefinitions = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition { DeviceType= DeviceType.Hid, VendorId= 0x1532, ProductId=0x0b00, Label="OSVR HMD Communication Device" },
            new FilterDeviceDefinition { DeviceType= DeviceType.Usb, VendorId= 0x1532, ProductId=0x0b00, Label="OSVR HMD Communication Device" },
        };

        private DeviceListener m_DeviceListener;
        internal byte[] DataBuffer;

        public IDevice Device { get; private set; }

        public event Action<bool> DeviceConnected = null;

        public HDKDevice()
        {
            m_DeviceListener = new DeviceListener(m_DeviceDefinitions, PollMilliseconds) { Logger = new DebugLogger() };

            WindowsHidDeviceFactory.Register();
        }

        public void Initialize()
        {
            Device?.Close();
            m_DeviceListener.DeviceInitialized += OnDeviceInitialized;
            m_DeviceListener.DeviceDisconnected += OnDeviceDisconnected;
            m_DeviceListener.Start();

            Console.WriteLine("HDKDevice Initialization");
        }

        public Task InitializeAsync()
        {
            return Task.Run(async () =>
            {
                var devices = await DeviceManager.Current.GetDevicesAsync(m_DeviceDefinitions);
                var device = devices.FirstOrDefault();

                if (device == null)
                    throw new Exception("No devices found");

                await device.InitializeAsync();

                OnDeviceInitialized(this, new DeviceEventArgs(device));
            });
        }

        public void Close()
        {
            Device?.Close();
            m_DeviceListener.DeviceInitialized -= OnDeviceInitialized;
            m_DeviceListener.DeviceDisconnected -= OnDeviceDisconnected;

            Console.WriteLine("HDKDevice Shutdown");
        }

        public Task Fetch()
        {
            return Task.Run(async () =>
            {
                DataBuffer = await Device.ReadAsync();
            });
        }

        public async Task<byte[]> FetchAsync() => await Device.ReadAsync();

        private void OnDeviceInitialized(object sender, DeviceEventArgs e)
        {
            Device = e.Device;
            DeviceConnected?.Invoke(true);
            Console.WriteLine("+ Device Connected");
        }

        private void OnDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Device = null;
            DeviceConnected?.Invoke(false);
            Console.WriteLine("- Device Disconnected");
        }
    }
}
