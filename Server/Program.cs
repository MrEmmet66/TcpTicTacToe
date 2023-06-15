using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Program
    {
        static List<ClientObject> _clients = new List<ClientObject>();
        static string _turnChar = "O";
        static string[] _map = new string[9];
        static TcpListener _listener = new TcpListener(IPAddress.Any, 6666);

        static void Main(string[] args)
        {
            CreateMap();

            Console.WriteLine("Server started");
            try
            {
                _listener.Start();
                while (true)
                {
                    TcpClient _client = _listener.AcceptTcpClient();
                    ClientObject client = new ClientObject(_client);
                    _clients.Add(client);
                    client.Process();
                    client.Writer.Write("currchar");
                    client.Writer.Write(_turnChar);
                    Task.Run(() => ReceiveClientResponse(client));

                    SendMap();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void SendMap()
        {
            try
            {

                foreach (ClientObject client in _clients)
                { 
                    for (int i = 0; i < _map.Length; i++)
                    {
                        client.Writer.Write("map");
                        client.Writer.Write(i);
                        client.Writer.Write(_map[i]);
                    }
                    client.Writer.Flush();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        static void CreateMap()
        {
            _turnChar = "O";
            for (int i = 0; i < _map.Length; i++)
            {
                _map[i] = "";
            }
        }


        static void ReceiveClientResponse(ClientObject client)
        {
            try
            {
                while (true)
                {
                    string message = client.Reader.ReadString();
                    Console.WriteLine($"Got message: {message}");
                    switch (message)
                    {
                        case "move":
                            int index = client.Reader.ReadInt32();
                            if (client.GameChar == _turnChar)
                            {
                                string response = default;
                                response = client.GameChar;
                                Console.WriteLine($"Client sent {response} at {index}");
                                _map[index] = response;
                                SendMap();
                                if (CheckDraw())
                                {
                                    CreateMap();
                                    SendMap();
                                    foreach (var cl in _clients)
                                        cl.Writer.Write("draw");
                                }
                                if (CheckWin())
                                {
                                    CreateMap();
                                    SendMap();
                                    foreach (ClientObject clientOb in _clients)
                                    {
                                        clientOb.Writer.Write("win");
                                        Console.WriteLine($"{client.GameChar} won the game");
                                        clientOb.Writer.Write(client.GameChar);
                                    }
                                }
                                else
                                {
                                    if (_turnChar == "O")
                                        _turnChar = "X";
                                    else
                                        _turnChar = "O";
                                }
                                SendCurrentChar();
                            }
                            break;
                        case "newchar":
                            string gameChar = client.Reader.ReadString();
                            ClientObject clientTarget = _clients.FirstOrDefault();
                            if (clientTarget.GameChar == gameChar)
                            {
                                client.Writer.Write("charerr");
                                _clients.Remove(client);
                                
                            }

                            else
                            {
                                client.GameChar = gameChar;
                                Console.WriteLine("Got char " + client.GameChar);
                            }
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                _clients.Remove(client);
                ClientObject clientOb = _clients.FirstOrDefault();
                clientOb.Writer.Write("win");
                clientOb.Writer.Write(clientOb.GameChar);
                Console.WriteLine($"{client.GameChar} leaved. Auto-win for {clientOb.GameChar}");
                CreateMap();
                SendMap();
            }
        }

        static void SendCurrentChar()
        {
            foreach(ClientObject client in _clients)
            {
                client.Writer.Write("currchar");
                client.Writer.Write(_turnChar);
            }
        }

        static bool CheckDraw()
        {
            int shitCount = 0;
            foreach(string s in _map)
            {
                if (s == "")
                    shitCount++;
            }
            if (shitCount == 0)
                return true;
            return false;

        }

        static bool CheckWin()
        {
            int[][] winCoords = new int[][]
                {

                new int[] {0, 1, 2},
                new int[] {3, 4, 5},
                new int[] {6, 7, 8},
                new int[] {0, 3, 6},
                new int[] {1, 4, 7},
                new int[] {2, 5, 8},
                new int[] {0, 4, 8},
                new int[] {2, 4, 6},
                };
            string tempChar = "";
            foreach (var coord in winCoords)
            {
                if (_map[coord[0]] == _map[coord[1]] && _map[coord[1]] == _map[coord[2]])
                {
                    tempChar = _map[coord[0]];
                }
            }
            if (tempChar == "O" || tempChar == "X")
                return true;
            return false;
        }

    }
}

