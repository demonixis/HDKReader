using Fleck;
using HDKReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Threading;

namespace HDKFrontEnd
{
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
            StatusTB.Text = status.ToString();
        }

        public Vector3 FromQ(Quaternion q2)
        {
            var q = new Quaternion(q2.W, q2.Z, q2.X, q2.Z);
            Vector3 pitchYawRoll;
            pitchYawRoll.Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));
            pitchYawRoll.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y));
            pitchYawRoll.Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));
            return new Vector3(Rad2Deg(pitchYawRoll.X), Rad2Deg(pitchYawRoll.Y), Rad2Deg(pitchYawRoll.Z));
        }

        private float Rad2Deg(float rad) => (180.0f * rad / ((float)Math.PI));

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

        private void Loop(object sender, EventArgs e)
        {
            if (!m_HDKDevice.Fetch())
                return;

            var values = m_HDKDevice.Quaternion;
            Send(JsonConvert.SerializeObject(values));

            OrientationTB.Text = string.Format("X: {0:0.000}, Y: {1:0.000}, Z: {2:0.000}, W: {3:0.000}", values[0], values[1], values[2], values[3]);

            var q = new Quaternion(values[0], values[1], values[2], values[3]);
            var v = FromQ(q);
            RotationTB.Text = v.ToString();
        }

        #region Server Management

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

        private void Send(string data)
        {
            if (m_Server == null)
                return;

            foreach (var client in m_Clients)
                client.Send(data);
        }

        #endregion

        private void OnStartClicked(object sender, RoutedEventArgs e) => StartLoop();
        private void OnStopClicked(object sender, RoutedEventArgs e) => StopLoop();
    }
}
