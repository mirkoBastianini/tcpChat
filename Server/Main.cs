using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    public partial class Main : Form
    {
        private Listener listener;

        public List<Socket> clients = new List<Socket>();

        public void BroadcastData(string data) 
        {
            foreach (var socket in clients)
            {
                try {
                    socket.Send(Encoding.ASCII.GetBytes(data));
                }
                catch (Exception) { }
            }
        }

        public Main()
        {
            InitializeComponent();
            btnStop.Enabled = false;
            btnSend.Enabled = false;
        }

        private void listener_SocketAccepted(Socket e)
        {
            var client = new Client(e);
            client.Received += client_Received;
            client.Disconnected += client_Disconnected;
            this.Invoke(() =>
            {
                string ip = client.Ip.ToString().Split(':')[0];
                var item = new ListViewItem(ip); 
                item.SubItems.Add(" "); 
                item.SubItems.Add(" "); 
                item.Tag = client;
                clientList.Items.Add(item);
                clients.Add(e);
            });
        }

        private void client_Disconnected(Client sender)
        {
            this.Invoke(() =>
            {
                for (int i = 0; i < clientList.Items.Count; i++)
                {
                    var client = clientList.Items[i].Tag as Client;
                    if (client.Ip == sender.Ip)
                    {
                        txtReceive.Text += "<< " + clientList.Items[i].SubItems[1].Text + " ha abbandonato la stanza. >>\r\n";
                        BroadcastData("RefreshChat|" + txtReceive.Text);
                        clientList.Items.RemoveAt(i);
                    }
                }
            });
        }

        private void client_Received(Client sender, byte[] data)
        {
            this.Invoke(() =>
            {
                for (int i = 0; i < clientList.Items.Count; i++)
                {
                    var client = clientList.Items[i].Tag as Client;
                    if (client == null || client.Ip != sender.Ip) continue;
                    var command = Encoding.ASCII.GetString(data).Split('|');
                    switch (command[0])
                    {
                        case "Connect":
                            txtReceive.Text += "<< " + command[1] + " e' entrato nella stanza >>\r\n";
                            clientList.Items[i].SubItems[1].Text = command[1];
                            clientList.Items[i].SubItems[2].Text = command[2];
                            string users = string.Empty;
                            for (int j = 0; j < clientList.Items.Count; j++)
                            {
                                users += clientList.Items[j].SubItems[1].Text + "|";
                            }
                            BroadcastData("Users|" + users.TrimEnd('|'));
                            BroadcastData("RefreshChat|" + txtReceive.Text);
                            break;
                        case "Message":
                            txtReceive.Text += command[1] + " scrive: " + command[2] + "\r\n";
                            BroadcastData("RefreshChat|" + txtReceive.Text);
                            break;
                       }
                }
            });
        }

           protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            listener.Stop();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtInput.Text != string.Empty)
            {
                BroadcastData("Message|" + txtInput.Text);
                txtReceive.Text += txtInput.Text + "\r\n";
                txtInput.Text = "Server: ";
            }
        }

     
        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend.PerformClick();
            }
        }

        private void txtReceive_TextChanged(object sender, EventArgs e)
        {
            txtReceive.SelectionStart = txtReceive.TextLength;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int port = int.Parse(textPort.Text);
            listener = new Listener(port);
            listener.SocketAccepted += listener_SocketAccepted;
            listener.Start();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            btnSend.Enabled = true;
            textPort.Enabled = false;

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            listener.Stop();
            btnStop.Enabled = false;
            btnSend.Enabled = false;
            textPort.Enabled = true;
            btnStart.Enabled = true;

        }
    }
}