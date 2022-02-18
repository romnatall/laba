using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {

        bool done = false;
        UdpClient client;
        IPAddress groupAddress;
        int localPort;
        int remotePort;
        int ttl;

        IPEndPoint remoteEP;
        UnicodeEncoding encoding = new UnicodeEncoding();

        string name;
        string message;

        SynchronizationContext _syncContext;

        public Form1()
        {
            InitializeComponent();
            button1.Enabled = false;

            groupAddress = IPAddress.Parse("234.5.6.11");

            textBox1.Text = "8080";
            textBox5.Text = "8080";
            textBox4.Text = "Anon";

            ttl = 32;
            _syncContext = SynchronizationContext.Current;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            localPort = int.Parse(textBox1.Text);
            remotePort = int.Parse(textBox5.Text);
            try
            {
                name = textBox4.Text;
                textBox4.ReadOnly = true;

                client = new UdpClient(localPort);
                client.JoinMulticastGroup(groupAddress, ttl);
                remoteEP = new IPEndPoint(groupAddress, remotePort);

                Thread receiver = new Thread(new ThreadStart(Listener));
                receiver.IsBackground = true;
                receiver.Start();
                byte[] data = encoding.GetBytes(name + " has joined the chat");
                client.Send(data, data.Length, remoteEP);
                button2.Enabled = false;
                button3.Enabled = true;
                button1.Enabled = true;
            }
            catch(Exception er)
            {
                MessageBox.Show("error");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StopListener();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        void Listener()
        {
            done = false;
            try
            {
                while (!done)
                {
                    IPEndPoint ep = null;
                    byte[] buffer = client.Receive(ref ep);
                    message = encoding.GetString(buffer);
                    _syncContext.Post(o => DisplayReceivedMessage(), null);
                }
            }
            catch (Exception e)
            {
                if (done)
                    return;
            }

        }

        void DisplayReceivedMessage()
        {
            string time = DateTime.Now.ToString("t");
            textBox2.Text += time + " " + message + "\r\n" + textBox3.Text+"\n";
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string time = DateTime.Now.ToString("t");
            if(textBox1.Text!=textBox5.Text)
                textBox2.Text += time + " [" + name +"] "+ textBox3.Text + "\r\n";
            try
            {
                byte[] data = encoding.GetBytes("["+name + "] " + textBox3.Text);
                client.Send(data, data.Length, remoteEP);
                textBox3.Clear();
                textBox3.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error MulticastChat");
            }
        }

        void StopListener()
        {
            try
            {
                byte[] data = encoding.GetBytes(name + " has left the chart");
                client.Send(data, data.Length, remoteEP);
                client.DropMulticastGroup(groupAddress);
                client.Close();
                done = true;
                button2.Enabled = true;
                button3.Enabled = false;
                button1.Enabled = false;
                textBox4.ReadOnly = false;
            }
            catch(Exception e)
            {
                ;
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!done)
                StopListener();
        }



        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keys.Enter)
                button1_Click(null, null);
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            textBox2.Size = new Size(this.Size.Width-300,this.Size.Height-100) ;
        }
    }
}
