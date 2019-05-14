using Fleck;
using HDKReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HDKFrontEnd
{
    public static class UIExtensions
    {
        public static void SetThreadSafeValue(this TextBlock thisTextBlock, string text)
        {
            thisTextBlock.Dispatcher.BeginInvoke((Action)(() => thisTextBlock.Text = text));
        }
    }

    public partial class MainWindow : Window
    {
        public enum AppStatus
        {
            Stopped, Starting, Started
        }

        public delegate void UpdateTextCallback(string message);

        private HDKDevice m_HDKDevice;
        private DispatcherTimer m_DispatchTimer;
        private WebSocketServer m_Server;
        private List<IWebSocketConnection> m_Clients;

        public MainWindow()
        {
            InitializeComponent();

            m_HDKDevice = new HDKDevice();
            m_HDKDevice.Initialize();

            m_Clients = new List<IWebSocketConnection>();

            UpdateStatus(AppStatus.Stopped);
        }

        private void UpdateStatus(AppStatus status)
        {
            StatusTB.SetThreadSafeValue(status.ToString());
        }

        private void Loop(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                var data = await m_HDKDevice.FetchAsync();

                if (data[1] == 3 || data[1] == 19)
                {
                    var values = HDKDataReader.DecodeSensorData(ref data, data.Length);
                    Send(JsonConvert.SerializeObject(values));

                    AccelerationTB.SetThreadSafeValue(Vector3.FromValues(ref values).ToString());
                    OrientationTB.SetThreadSafeValue(Quaternion.FromValues(ref values).ToString());
                }
            });
        }

        private void StartLoop()
        {
            StopLoop();

            m_DispatchTimer = new DispatcherTimer();
            m_DispatchTimer.Interval = TimeSpan.FromMilliseconds(1);
            m_DispatchTimer.Tick += Loop;
            m_DispatchTimer.Start();

            StartServer();

            UpdateStatus(AppStatus.Starting);
        }

        private void StopLoop()
        {
            if (m_DispatchTimer == null)
                return;

            m_DispatchTimer.Stop();
            m_DispatchTimer = null;

            StopServer();

            UpdateStatus(AppStatus.Stopped);
        }

        private void StartServer()
        {
            StopServer();

            m_Server = new WebSocketServer($"ws://127.0.0.1:8181");
            m_Server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine(string.Format("* -> Connected to {0}", socket.ConnectionInfo.ClientIpAddress));
                    m_Clients.Add(socket);
                };

                socket.OnClose = () =>
                {
                    Console.WriteLine(string.Format("* -> Disconnected from {0}", socket.ConnectionInfo.ClientIpAddress));
                    m_Clients.Remove(socket);
                };

                socket.OnError = (m) =>
                {
                    var available = socket.IsAvailable;

                    if (!available)
                    {
                        m_Clients.Remove(socket);
                        socket.Close();
                    }
                };
            });
        }

        private void Send(string data)
        {
            if (m_Server == null)
                return;

            foreach (var client in m_Clients)
                client.Send(data);
        }

        private void StopServer()
        {
            if (m_Server == null)
                return;

            foreach (var client in m_Clients)
                client.Close();

            m_Clients.Clear();
            m_Server.Dispose();
            m_Server = null;
        }

        private void OnStartClicked(object sender, RoutedEventArgs e) => StartLoop();
        private void OnStopClicked(object sender, RoutedEventArgs e) => StopLoop();
    }
}
