using Fleck;
using HDKReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HDKFrontEnd
{
    public static class UIExtension
    {
        public static void SetText(this TextBlock block, string text)
        {
            block.Dispatcher.BeginInvoke((Action)(() => block.Text = text));
        }
    }

    public partial class MainWindow : Window
    {
        public enum AppStatus
        {
            Stopped, Starting, Started
        }

        private HDKDevice m_HDKDevice;
        private WebSocketServer m_Server;
        private List<IWebSocketConnection> m_Clients;
        private Thread m_Thread;
        private bool m_IsRunning;

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

        private void StartLoop()
        {
            StopLoop();

            m_Thread = new Thread(new ThreadStart(Loop));
            m_Thread.Start();

            StartServer();

            UpdateStatus(AppStatus.Starting);
        }

        private void StopLoop()
        {
            if (m_Thread == null)
                return;

            if (m_Thread.IsAlive)
            {
                m_IsRunning = false;
                m_Thread.Join();
                m_Thread = null;
            }

            StopServer();

            UpdateStatus(AppStatus.Stopped);
        }

        private void Loop()
        {
            m_IsRunning = true;

            while (m_IsRunning)
            {
                if (!m_HDKDevice.Fetch())
                {
                    Thread.Sleep(100);
                    return;
                }

                var values = m_HDKDevice.Quaternion;
                Send(JsonConvert.SerializeObject(values));

#if DEBUG
                OrientationTB.SetText(string.Format("X: {0:0.00}, Y: {1:0.00}, Z: {2:0.00}, W: {3:0.00}", values[0], values[1], values[2], values[3]));

                var quaternion = new Quaternion(values[0], values[1], values[2], values[3]);
                var euler = MathUtils.ToEuler(quaternion);
                MathUtils.Rad2Deg(ref euler);
                RotationTB.SetText(string.Format("X: {0:0.00}, Y: {1:0.00}, Z: {2:0.00}", euler.X, euler.Y, euler.Z));
#endif
            }
        }

        private void Loop(object sender, EventArgs e)
        {
            if (!m_HDKDevice.Fetch())
                return;

            var values = m_HDKDevice.Quaternion;
            Send(JsonConvert.SerializeObject(values));

            OrientationTB.Text = string.Format("X: {0:0.00}, Y: {1:0.00}, Z: {2:0.00}, W: {3:0.00}", values[0], values[1], values[2], values[3]);

            var quaternion = new Quaternion(values[0], values[1], values[2], values[3]);
            var euler = MathUtils.ToEuler(quaternion);
            MathUtils.Rad2Deg(ref euler);
            RotationTB.Text = string.Format("X: {0:0.00}, Y: {1:0.00}, Z: {2:0.00}", euler.X, euler.Y, euler.Z);
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
