using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public class Client
    {
        readonly Socket socket;
        public delegate void ReceivedEventHandler(Client cs, string received);
        public event ReceivedEventHandler Received = delegate { };
        public event EventHandler Connected = delegate { };
        public delegate void DisconnectedEventHandler(Client cs);
        public event DisconnectedEventHandler Disconnected = delegate {};
        bool connected;

        public Client()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ip, int port)
        {
            try
            {
                var ep = new IPEndPoint(IPAddress.Parse(ip), port);
                socket.BeginConnect(ep, ConnectCallback, socket);
            }
            catch { }
        }

        public void Close()
        {
            socket.Dispose();
            socket.Close();
        }

        void ConnectCallback(IAsyncResult ar)
        {
                socket.EndConnect(ar);
                connected = true;
                Connected(this, EventArgs.Empty);
                var buffer = new byte[socket.ReceiveBufferSize];
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReadCallback, buffer);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var buffer = (byte[]) ar.AsyncState;
            var rec = socket.EndReceive(ar);
            if (rec != 0)
            {
                var data = Encoding.ASCII.GetString(buffer, 0, rec);
                Received(this, data);
            }
            else
            {
                Disconnected(this);
                connected = false;
                Close();
                return;
            }
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReadCallback, buffer);
        }

        public void Send(string data)
        {
            try
            {
                var buffer = Encoding.ASCII.GetBytes(data);
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, buffer);
            }
            catch { Disconnected(this); }
        }

        void SendCallback(IAsyncResult ar)
        {
            socket.EndSend(ar);
        }
    }
}