using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server
{
    class Listener
    {
        private Socket socket;

        public Listener(int port)
        {
            Port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            if (Listening)
                return;
            socket.Bind(new IPEndPoint(0, Port));
            socket.Listen(0);
            socket.BeginAccept(Callback, null);
            Listening = true;
        }

        public void Stop()
        {
            if (!Listening)
                return;
            if (socket.Connected)
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Listening { get; private set; }

        public int Port { get; private set; }

        public delegate void SocketAcceptedHandler(Socket e);
        public event SocketAcceptedHandler SocketAccepted;
        void Callback(IAsyncResult ar)
        {
            try
            {
                var s = socket.EndAccept(ar);
                if (SocketAccepted != null) SocketAccepted(s);
                socket.BeginAccept(Callback, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}