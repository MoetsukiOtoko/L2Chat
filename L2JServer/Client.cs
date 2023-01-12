using L2JServer.Net.IO;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace L2JServer
{
    internal class Client
    {
        public string Username { get; set; }
        private string Password { get; set; }
        public string Color { get; set; }
        public IPAddress UserIP { get; private set; }
        private string RegisterUser { get; set; }
        private string RegisterPassword { get; set; }
        private string RegisterMail { get; set; }
        private string ServerPassword { get; set; }
        public bool Authd { get; set; }
        private bool Connected { get; set; }
        public Guid Uid { get; set; }
        public TcpClient ClientSocket { get; set; }
        private bool receiveHeartBeat { get; set; }

        public bool Listening { get; private set; }

        readonly PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UserIP = ((IPEndPoint)(ClientSocket.Client.RemoteEndPoint)).Address;
            
            ServerPassword = Program.ServerPassword;
            Uid = Guid.NewGuid();
            Connected = true;
            _packetReader = new PacketReader(ClientSocket.GetStream());


            var opcode = _packetReader.ReadByte();
            if (opcode == 22)
            {
                if(ServerPassword != _packetReader.ReadMessage())
                {
                    Console.WriteLine($"-----------> [{DateTime.Now}]: {UserIP} failed authentication.");
                    var denyCon = new PacketBuilder();
                    denyCon.WriteOpCode(31);
                    denyCon.WriteMessage("Connection Denied!");
                    ClientSocket.Client.Send(denyCon.GetPacketBytes());
                    //ClientSocket.Client.Shutdown(SocketShutdown.Both);
                    ClientSocket.Close();

                }
                else
                {
                    Connected = true;
                    Console.WriteLine($"-----------> [{DateTime.Now}]: [{UserIP}] connected.");
                    var acceptCon = new PacketBuilder();
                    acceptCon.WriteOpCode(30);
                    acceptCon.WriteMessage("Connection Success!");
                    ClientSocket.Client.Send(acceptCon.GetPacketBytes());
                    Task.Run(Process);

                }
            }
            
        }

        void Process()
        {
            Listening = true;
            Authd = false;
            while (Listening == true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    Debug.WriteLine($"The server received the OPCODE: {opcode}");
                    switch (opcode)
                    {

                        
                        case 5: //   Receiving a message from a user and distributing it to all users.
                            var msg = _packetReader.ReadJPMessage();
                            if (Authd == true)
                            {
                                Program.BroadcastMessage(Username, msg);
                            }
                            break;
                        case 0: //   Login packet and authenticaton/
                            Debug.WriteLine("login packet received");
                            string tUsername = _packetReader.ReadMessage();
                            string tPassword = _packetReader.ReadMessage();
                            string sSql = $"SELECT * from tbl_UserDB WHERE Username = '{tUsername}'";
                            DataTable dt = Database.GetDataTable(sSql);
                            string passW = "";
                            if (dt.Rows.Count != 0)
                            {
                                passW = dt.Rows[0]["Password"].ToString();
                            }

                            if (dt.Rows.Count == 0 || tPassword != passW)
                            {
                                var broadcastPacket = new PacketBuilder();
                                broadcastPacket.WriteOpCode(7);
                                ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                            }
                            else
                            {
                                Program.CheckDuplicate(tUsername);
                                Username = tUsername;
                                Password = tPassword;
                                var broadcastPacket = new PacketBuilder();
                                broadcastPacket.WriteOpCode(8);
                                broadcastPacket.WriteMessage("Authentication Complete");
                                ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                                Console.WriteLine($"-----------> [{DateTime.Now}]: User [{Username}] authenticated. ");
                                Authd = true;
                                Program.BroadcastConnection(this);
                            }
                            break;
                        case 55: //   Registration packet.
                            RegisterUser = _packetReader.ReadMessage();
                            RegisterPassword = _packetReader.ReadMessage();
                            RegisterMail = _packetReader.ReadMessage();
                            string rsSql = $"SELECT * from tbl_UserDB WHERE Username = '{RegisterUser}'";
                            DataTable rdt = Database.GetDataTable(rsSql);
                            if (rdt.Rows.Count != 0)
                            {
                                var duplicateUserRegisterError = new PacketBuilder();
                                duplicateUserRegisterError.WriteOpCode(34);
                                duplicateUserRegisterError.WriteMessage("The username is already taken.");
                                ClientSocket.Client.Send(duplicateUserRegisterError.GetPacketBytes());
                            }
                            else
                            {
                                string iSql = $"INSERT INTO tbl_UserDB (Username, Password, Mail) VALUES ({RegisterUser},{RegisterPassword},{RegisterMail})";

                                try
                                {
                                    Debug.WriteLine("We trying");
                                    string query = "INSERT INTO tbl_UserDB (Username, Password, Mail) VALUES (@Username, @Password, @Mail)";
                                    Debug.WriteLine(query);
                                    SqlConnection myConnection = Database.GetDBConnection();
                                    SqlCommand myCommand = new SqlCommand(query, myConnection);
                                    myCommand.Parameters.AddWithValue("@Username", RegisterUser);
                                    myCommand.Parameters.AddWithValue("@Password", RegisterPassword);
                                    myCommand.Parameters.AddWithValue("@Mail", RegisterMail);
                                    myCommand.ExecuteNonQuery();
                                    Database.CloseDBConnection();
                                    var entrySuccessPacket = new PacketBuilder();
                                    entrySuccessPacket.WriteOpCode(60);
                                    entrySuccessPacket.WriteMessage("Account Created! Please Log in.");
                                    ClientSocket.Client.Send(entrySuccessPacket.GetPacketBytes());

                                    Debug.WriteLine("Sent register success packet!");
                                    Console.WriteLine($"----------->  [{DateTime.Now}]: User [{RegisterUser}] registered.");
                                }
                                catch (SqlException e)
                                {
                                    Debug.WriteLine("We failed");
                                    string displayE = Database.DisplaySqlErrors(e);
                                    var entryErrorPacket = new PacketBuilder();
                                    entryErrorPacket.WriteOpCode(61);
                                    entryErrorPacket.WriteMessage(displayE);
                                    ClientSocket.Client.Send(entryErrorPacket.GetPacketBytes());
                                }

                            }
                            break;
                        case 70: //    User-forced disconnect.
                            Debug.WriteLine(_packetReader.ReadMessage());
                            Authd = false;
                            Listening = false;
                            Console.WriteLine($"----------->  [{DateTime.Now}]:{UserIP} - {Username} disconnected.");
                            Program.BroadcastDisconnect(Username);
                            Program.RemoveUser(Uid.ToString());
                            ClientSocket.Close();
                            break;
                        case 71: //    User-forced logout.
                            Debug.WriteLine(_packetReader.ReadMessage());
                            Authd = false;
                            Console.WriteLine($"----------->  [{DateTime.Now}]: User {Username} logged out.");
                            Program.BroadcastDisconnect(Username);
                            Username = null;
                            break;
                        case 233: //    Receive and reply to heartbeat.
                            var hbReply = new PacketBuilder();
                            hbReply.WriteOpCode(233);
                            ClientSocket.Client.Send(hbReply.GetPacketBytes());
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Authd = false;
                    Debug.WriteLine("Server-side exception caught.");
                    Console.WriteLine($"----------->  [{DateTime.Now}]:{UserIP} - {Username} disconnected.");
                    Program.BroadcastDisconnect(Username);
                    Program.RemoveUser(Uid.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
        //end class
    }
}
