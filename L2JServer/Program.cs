// See https://aka.ms/new-console-template for more information
using L2JServer.Net.IO;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace L2JServer
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
        public static string ServerPassword = "pass";
        private static List<int> opencolors { get; set; }
        private static List<string> colorPool = new List<string>() { "#FFADAD", "#FFD6A5", "#FDFFB6", "#CAFFBF", "#9BF6FF", "#A0C4FF", "#BDB2FF", "#FFC6FF" };

        public string ServerPort { get; private set; }

        static void Main(string[] args)

        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Any, 11112);
            _listener.Start();
            Console.WriteLine($"-----------> [{DateTime.Now}]: The server is running.");
            opencolors = new List<int>();
            Task.Run(() =>
            {
                ListenForCommands();
            });
            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);

            }
        }


        public static void BroadcastConnection(Client newlyConnectedClient)
        {
            string color = ColorPicker();
            newlyConnectedClient.Color = color;
            foreach (var user in _users)
            {
                if(user.Uid == newlyConnectedClient.Uid)
                {
                    foreach(var usr in _users)
                    {
                        if (usr.Username != null)
                        {
                            var broadcastPacket = new PacketBuilder();
                            broadcastPacket.WriteOpCode(2);
                            broadcastPacket.WriteMessage(usr.Username);
                            broadcastPacket.WriteMessage(usr.Uid.ToString());
                            broadcastPacket.WriteMessage(usr.Color);
                            newlyConnectedClient.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes()); 
                        }
                    }
                }
                else
                {
                    if (user.Authd == true)
                    {
                        var broadcastPacket = new PacketBuilder();
                        broadcastPacket.WriteOpCode(1);
                        broadcastPacket.WriteMessage(newlyConnectedClient.Username);
                        broadcastPacket.WriteMessage(newlyConnectedClient.Uid.ToString());
                        broadcastPacket.WriteMessage(color);
                        user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes()); 
                    }
                }
            }
        }


        public static void BroadcastMessage(string username, string message)
        {
            foreach (var user in _users) 
            {
                if (user.Authd == true)
                {

                    var msgPacket = new PacketBuilder();
                    msgPacket.WriteOpCode(5);
                    msgPacket.WriteMessage(username);
                    msgPacket.WriteJPMessage(message);
                    user.ClientSocket.Client.Send(msgPacket.GetPacketBytes()); 
                }
            }
        }

        public static void RemoveUser(string uid)
        {
            var disconnectedUser = _users.FirstOrDefault(x => x.Uid.ToString() == uid);
            _users.Remove(disconnectedUser);
        }

        public static void BroadcastDisconnect(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                foreach (var user in _users)
                {
                    if (user.Authd == true && user.Username != username)
                    {
                        var broadcastPacket = new PacketBuilder();
                        broadcastPacket.WriteOpCode(10);
                        broadcastPacket.WriteMessage(username);
                        user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                    }
                } 
            }
        }

        public static string ColorPicker()
        {
            Random random = new Random();
            Debug.WriteLine(opencolors.Count());
            if (opencolors.Count() == 0)
            {
                opencolors = Enumerable.Range(0, 7).ToList();
            }
            var randomIndex = random.Next(0, opencolors.Count);

            string pickedColor = colorPool.ElementAt(randomIndex);
            opencolors.Remove(randomIndex);
            return pickedColor;
        }

        public static void CheckDuplicate(string username) 
        {
            foreach (var user in _users)
            {
                if (user.Username == username && user.Username != "" && user.Username != null)
                {
                    user.Authd = false;
                    user.Username = string.Empty;
                    var dupUser = new PacketBuilder();
                    dupUser.WriteOpCode(82);
                    user.ClientSocket.Client.Send(dupUser.GetPacketBytes());
                }
            }
        }

        private static void ListenForCommands()
        {
            while (true)
            {
                string userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "-users":
                        string usersString = "";
                        foreach (var usr in _users)
                        {
                            usersString += usr.Username + " ";
                        }
                        Console.WriteLine($"-----------> Connected users ({_users.Count()}): {usersString}");
                        break;

                    case "-colors":
                        string colorIntList = "";
                        foreach (var color in opencolors)
                        {
                            colorIntList += color.ToString();
                        }
                        Console.WriteLine(colorIntList);
                        break;
                    default:
                        Console.WriteLine($"-----------> {userInput} is not an available command.");
                        break;
                }
            }
        }
    }
}
