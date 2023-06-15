using System.Net;
using System.Text;
using System.Net.Sockets;

namespace Client
{
    public partial class Form1 : Form
    {
        private Button[] _map = new Button[9];
        public TcpClient _server = new TcpClient();
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private NetworkStream _serverStream;
        private Thread _threadListen;
        public Form1()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            Connect();
            prefCharComboBox.Enabled = false;

        }

        private void Connect()
        {
            try
            {
                _server.Connect("127.0.0.1", 6666);
                _serverStream = _server.GetStream();
                _writer = new BinaryWriter(_serverStream, Encoding.UTF8);
                _reader = new BinaryReader(_serverStream, Encoding.UTF8);
                _threadListen = new Thread(ListenServer);
                _threadListen.IsBackground = true;
                _threadListen.Start();
                _writer.Write("newchar");   
                _writer.Write(prefCharComboBox.SelectedItem.ToString());
                _writer.Flush();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ListenServer()
        {
            int index;
            string mapChar = default; 
            while (true)
            {
                try
                {
                    string message = _reader.ReadString();
                    switch (message)
                    {
                        case "map":
                            index = _reader.ReadInt32();
                            mapChar = _reader.ReadString();
                            tableLayoutPanel1.Refresh();
                            if (tableLayoutPanel1.Controls.Count < 9)
                            {
                                this.Invoke(new MethodInvoker(() => {
                                    _map[index] = new Button();
                                    _map[index].Text = mapChar;
                                    _map[index].Click += gameButtonClick;
                                    _map[index].Dock = DockStyle.Fill;
                                    tableLayoutPanel1.Controls.Add(_map[index]);
                                }));
                            }
                            else
                            {
                                Invoke(new MethodInvoker(() => { _map[index].Text = mapChar; }));
                            }
                            break;
                        case "win":
                            string winChar = _reader.ReadString();
                            MessageBox.Show($"{winChar} has won!");
                            break;
                        case "charerr":
                            Close();
                            break;
                        case "currchar":
                            string currChar = _reader.ReadString();
                            currentCharLabel.Text = $"Current char: {currChar}";
                            break;
                        case "draw":
                            MessageBox.Show("Draw!");
                            break;
                    }


                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Close();
                }
            }
        }

        private void gameButtonClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Text == "")
            {
                int index = tableLayoutPanel1.Controls.IndexOf(btn);
                _writer.Write("move");
                _writer.Write(index);
                _writer.Flush();

            }
            else
                MessageBox.Show("Wrong button", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            prefCharComboBox.SelectedIndex = 0;
        }
    }
}