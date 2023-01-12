using L2JChat.Net.IO;
using System;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace L2JChat.Net
{
    internal class Server
    {
        private TcpClient _client;
        public PacketReader PacketReader;
        
        public bool Listening { get; private set; }

        public event Action connectedEvent, msgReceivedEvent, disconnectedEvent, loginSuccessEvent, msgSentEvent, invalidAuthEvent, NoInput, connectSuccessEvent, connectFailureEvent, passwordMismatch, usernameExists, registerSuccess, registerFailure, forceDisconnect, invalidPassword, invalidUsername, invalidMail, clientDisconnected, duplicateLogin, youConnectedEvent, couldNotConnect;
        
        public Server()
        {
        }

        public async Task ConnectAttemptAsync(string serverip, string port, string serverpassword)
        {
            _client = new TcpClient();
            if (!_client.Connected)
                try
                {
                     await _client.ConnectAsync(serverip, Int32.Parse(port));
                }
                catch (Exception)
                {
                    couldNotConnect?.Invoke();
                }

            if (_client.Connected)
            {
                PacketReader = new PacketReader(_client.GetStream());
                var connectPacket = new PacketBuilder();
                connectPacket.WriteOpCode(22);
                connectPacket.WriteMessage(serverpassword);
                _client.Client.Send(connectPacket.GetPacketBytes());
                Listening = true;
                ReadPackets();
                HeartBeatStart();
            }

        }

        public void LoginAttempt(string username, string password)
        {
            if (_client.Connected)
            {
                if (Validate.IsValidLoginString(username))
                {
                    Debug.WriteLine("valid login");
                    if (Validate.IsValidLoginString(password))
                    {
                        var connectPacket = new PacketBuilder();
                        connectPacket.WriteOpCode(0);
                        connectPacket.WriteMessage(username);
                        connectPacket.WriteMessage(password);
                        Debug.WriteLine("login packet sent");
                        _client.Client.Send(connectPacket.GetPacketBytes());
                    }
                    else
                    {
                        invalidAuthEvent?.Invoke();
                    }
                }
                else
                {
                    invalidAuthEvent?.Invoke();
                }
            }
        }

        public void RegistrationAttempt(string registeruser, string registerpass, string registerpassconfirm, string registermail = "noMail")
        {
            if (Validate.IsValidLoginString(registeruser))
            {
                Debug.WriteLine("valid username");
                if (Validate.IsValidLoginString(registerpass))
                {
                    if(registerpass == registerpassconfirm)
                    {
                        if(Validate.IsValidEmail(registermail) || registermail == "noMail")
                        {
                            var registerPack = new PacketBuilder();
                            registerPack.WriteOpCode(55);
                            registerPack.WriteMessage(registeruser);
                            registerPack.WriteMessage(registerpass);
                            registerPack.WriteMessage(registermail);
                            _client.Client.Send(registerPack.GetPacketBytes());
                            Debug.WriteLine("sent register pack");
                        }
                        else
                        {
                            invalidMail?.Invoke();
                        }
                    }
                    else
                    {
                        passwordMismatch?.Invoke();
                    }
                }
                else
                {
                    invalidPassword?.Invoke();
                }
            }
            else
            {
                Debug.WriteLine("invalid username");
                invalidUsername?.Invoke();
            }
        }

        private void ReadPackets()
        {
            Listening = true;
            Task.Run(() =>
            {
                while (Listening == true)
                {
                    try
                    {
                        var opcode = PacketReader.ReadByte();
                        Debug.WriteLine($"The client received the OPCODE: {opcode}");
                        switch (opcode)
                        {
                            case 1:
                                connectedEvent?.Invoke();
                                break;
                            case 2:
                                youConnectedEvent?.Invoke();
                                break;
                            case 5:
                                msgReceivedEvent?.Invoke();
                                break;
                            case 7:
                                invalidAuthEvent?.Invoke();
                                break;
                            case 8:
                                loginSuccessEvent?.Invoke();
                                break;
                            case 10:
                                disconnectedEvent?.Invoke();
                                break;
                            case 30:
                                connectSuccessEvent?.Invoke();
                                break;
                            case 31:
                                connectFailureEvent?.Invoke();
                                Listening = false;
                                break;
                            case 34:
                                usernameExists?.Invoke();
                                break;
                            case 60:
                                registerSuccess?.Invoke();
                                break;
                            case 61:
                                registerFailure?.Invoke();
                                break;
                            case 82:
                                duplicateLogin?.Invoke();
                                break;
                            case 233:
                                break;
                            default:
                                Console.WriteLine("ah yes...");
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Client-side exception caught.");
                        clientDisconnected?.Invoke();
                        _client.Client.Close();
                        throw;
                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                var messagePacket = new PacketBuilder();
                messagePacket.WriteOpCode(5);
                messagePacket.WriteJPMessage(message);

                _client.Client.Send(messagePacket.GetPacketBytes());
                msgSentEvent?.Invoke(); 
            }
        }

        public void DisconnectServer()
        {
            var disconnectPacket = new PacketBuilder();
            disconnectPacket.WriteOpCode(70);
            disconnectPacket.WriteMessage("User input disconnect.");
            _client.Client.Send(disconnectPacket.GetPacketBytes());
            Listening = false;
        }

        public void LogoutServer()
        {
            var logoutPacket = new PacketBuilder();
            logoutPacket.WriteOpCode(71);
            logoutPacket.WriteMessage("User input logout.");
            _client.Client.Send(logoutPacket.GetPacketBytes());
        }

        public void QuickDC()
        {
            _client.Client.Close();
        }

        public void HeartBeatStart()
        {
            System.Timers.Timer heartbeatTimer = new System.Timers.Timer();
            heartbeatTimer.Interval = 10000; //5 seconds
            heartbeatTimer.Elapsed += heartbeatTimer_Elapsed;
            heartbeatTimer.Start();
        }

        void heartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var hbPacket = new PacketBuilder();
            hbPacket.WriteOpCode(233);
            _client.Client.Send(hbPacket.GetPacketBytes());
        }

    }
}
