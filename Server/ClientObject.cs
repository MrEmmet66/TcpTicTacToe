using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class ClientObject
    {
        protected internal BinaryWriter Writer { get; set; }
        protected internal BinaryReader Reader { get; set; }
        public string GameChar { get; set; }
        NetworkStream Stream { get; set; }

        public TcpClient _client;

        public ClientObject(TcpClient client)
        {
            _client = client;
        }

        public void Process()
        {
            Stream = _client.GetStream();
            Writer = new BinaryWriter(Stream, Encoding.UTF8);
            Reader = new BinaryReader(Stream, Encoding.UTF8);
            Console.WriteLine($"New client. ");
        }
    }
}
