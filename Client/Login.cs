using System;
using System.Windows.Forms;

namespace Client
{
    public partial class Login : Form
    {
        public Client Client { get; set; }

        public Login()
        {
            Client = new Client();
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            int port = int.Parse(textBox1.Text);
            Client.Connected += Client_Connected;
            Client.Connect(txtIP.Text, port);
            Client.Send("Connect|" + txtNickname.Text + "|connesso");
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            this.Invoke(Close);
        }
    }
}