using L2JChat.MVVM.Core;
using L2JChat.MVVM.Model;
using L2JChat.Net;
using L2JChat.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using System.Xml.XPath;

namespace L2JChat.MVVM.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {

        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<ChatMessage> ChatMessages { get; set; }
        public RelayCommand LoginAttemptCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand ToLoginCommand { get; set; }
        public RelayCommand BackLoginCommand { get; set; }
        public RelayCommand ToRegisterCommand { get; set; }
        public RelayCommand BackRegisterCommand { get; set; }
        public RelayCommand ConnectAttemptCommand { get; set; }
        public RelayCommand RegisterAttemptCommand { get; set; }
        public RelayCommand UserDisconnectCommand { get; set; }
        public RelayCommand UserLogoutCommand { get; set; }
        public RelayCommand LockWindowCommand { get; set; }



        private bool login, register, disconnected, connected, canchat, islocked, savelogin, saveserver;
        private string message, servermessage, servermessagecolor, username, registerusername, registermail, usercount, usersstring;
        private int width, height;
        public bool Login
        {
            get { return login; }
            set
            {
                if (value != login)
                {
                    login = value;
                    OnPropertyChanged("Login");
                }
            }
        }
        public bool Register
        {
            get { return register; }
            set
            {
                if (value != register)
                {
                    register = value;
                    OnPropertyChanged("Register");
                }
            }
        }
        public bool Disconnected
        {
            get { return disconnected; }
            set
            {
                if (value != disconnected)
                {
                    disconnected = value;
                    OnPropertyChanged("Disconnected");
                }
            }
        }
        public bool Connected
        {
            get { return connected; }
            set
            {
                if (value != connected)
                {
                    connected = value;
                    OnPropertyChanged("Connected");
                }
            }
        }
        public bool CanChat
        {
            get { return canchat; }
            set
            {
                if (value != canchat)
                {
                    canchat = value;
                    OnPropertyChanged("CanChat");
                }
            }
        }

        public string UsersString
        {
            get { return usersstring; }
            set
            {
                if (value != usersstring)
                {
                    usersstring = value;
                    OnPropertyChanged("UsersString");
                }
            }
        }
        public string UserCount
        {
            get { return usercount; }
            set
            {
                if (value != usercount)
                {
                    usercount = value;
                    OnPropertyChanged("UserCount");
                }
            }
        }
        public string Username
        {
            get { return username; }
            set
            {
                if (value != username)
                {
                    username = value;
                    OnPropertyChanged("Username");
                }
            }
        }
        public string Password { get; set; }
        public string ServerPort { get; set; }
        public string ServerIP { get; set; }
        public string ServerPassword { get; set; }
        public string Message
        {
            get { return message; }
            set
            {
                if (value != message)
                {
                    message = value;
                    OnPropertyChanged("Message");
                }
            }
        }
        public string ServerMessage
        {
            get { return servermessage; }
            set
            {
                if (value != servermessage)
                {
                    servermessage = value;
                    OnPropertyChanged("ServerMessage");
                }
            }
        }
        public string ServerMessageColor
        {
            get { return servermessagecolor; }
            set
            {
                if (value != servermessagecolor)
                {
                    servermessagecolor = value;
                    OnPropertyChanged("ServerMessageColor");
                }
            }
        }
        public string RegisterUsername
        {
            get { return registerusername; }
            set
            {
                if (value != registerusername)
                {
                    registerusername = value;
                    OnPropertyChanged("RegisterUsername");
                }
            }
        }
        public string RegisterPassword { get; set; }
        public string RegisterPasswordConfirm { get; set; }
        public string RegisterMail
        {
            get { return registermail; }
            set
            {
                if (value != registermail)
                {
                    registermail = value;
                    OnPropertyChanged("RegisterMail");
                }
            }
        }
        public bool IsLocked
        {
            get { return islocked; }
            set
            {
                if (value != islocked)
                {
                    islocked = value;
                    OnPropertyChanged("IsLocked");
                }
            }
        }
        public bool SaveLogin
        {
            get { return savelogin; }
            set
            {
                if (value != savelogin)
                {
                    savelogin = value;
                    OnPropertyChanged("SaveLogin");
                }
            }
        }
        public bool SaveServer
        {
            get { return saveserver; }
            set
            {
                if (value != saveserver)
                {
                    saveserver = value;
                    OnPropertyChanged("SaveServer");
                }
            }
        }
        public int Width
        {
            get { return width; }
            set
            {
                if (value != width)
                {
                    width = value;
                    OnPropertyChanged("Width");
                }
            }
        }
        public int Height
        {
            get { return height; }
            set
            {
                if (value != height)
                {
                    height = value;
                    OnPropertyChanged("Height");
                }
            }
        }



        public Server _server;
        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            ChatMessages = new ObservableCollection<ChatMessage>();
            _server = new Server();
            UserCount = "0";
            Width = 348;
            Height = 100;
            CheckAuto();
            IsLocked = false;
            DisableAllScreens();
            Disconnected = true;
            ServerMessageColor = "red";

            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.disconnectedEvent += RemoveUser;
            _server.loginSuccessEvent += LoginSuccess;
            _server.msgSentEvent += clearMessageBox;
            _server.invalidAuthEvent += LoginError;
            _server.connectFailureEvent += ConnectFailure;
            _server.connectSuccessEvent += ConnectSuccess;
            _server.registerSuccess += RegisterSuccess;
            _server.registerFailure += RegisterFailure;
            _server.usernameExists += UsernameTaken;
            _server.passwordMismatch += PasswordMismatch;
            _server.youConnectedEvent += YouConnected;
            _server.invalidPassword += InvalidPassword;
            _server.invalidUsername += InvalidUsername;
            _server.invalidMail += InvalidMail;
            _server.couldNotConnect += CouldNotConnect;
            _server.clientDisconnected += UnexpectedDisconnect;
            _server.duplicateLogin += DuplicateLogin;

            LoginAttemptCommand = new RelayCommand(o => _server.LoginAttempt(Username, Password), o => IsAnyFieldEmpty(Username, Password));
            ConnectAttemptCommand = new RelayCommand(o => _server.ConnectAttemptAsync(ServerIP, ServerPort, ServerPassword), o => IsAnyFieldEmpty(ServerIP, ServerPort));
            SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message), o => !string.IsNullOrEmpty(Message));
            RegisterAttemptCommand = new RelayCommand(o => _server.RegistrationAttempt(RegisterUsername, RegisterPassword, RegisterPasswordConfirm, RegisterMail), o => IsAnyFieldEmpty(RegisterUsername, RegisterPassword, RegisterPasswordConfirm));
            ToLoginCommand = new RelayCommand(o => ToLoginButtonClick());
            BackLoginCommand = new RelayCommand(o => BackLoginButtonClick());
            ToRegisterCommand = new RelayCommand(o => ToRegisterButtonClick());
            BackRegisterCommand = new RelayCommand(o => BackRegisterButtonClick());
            UserDisconnectCommand = new RelayCommand(o => BackToConnectScreen(), o => !Disconnected);
            LockWindowCommand = new RelayCommand(o => LockWindow());
            UserLogoutCommand = new RelayCommand(o => BackToLoginScreen(), o => (CanChat));
            if (Properties.Settings.Default.autoConnect == true)
            {
                if (Properties.Settings.Default.savedServerIP != "" && Properties.Settings.Default.savedServerPort != "")
                {
                    ServerIP = Properties.Settings.Default.savedServerIP;
                    ServerPort = Properties.Settings.Default.savedServerPort;
                    ServerPassword = Properties.Settings.Default.savedServerPassword;
                    _server.ConnectAttemptAsync(Properties.Settings.Default.savedServerIP, Properties.Settings.Default.savedServerPort, Properties.Settings.Default.savedServerPassword);
                }
            }


        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RemoveUser()
        {
            var dUsername = _server.PacketReader.ReadMessage();
            var user = Users.Where(x => x.Username == dUsername).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
            ChatMessage currentMsg = new ChatMessage($"{user.Username}さんはサーバーから切断されました。", "Disconnected");
            Application.Current.Dispatcher.Invoke(() => ChatMessages.Add(currentMsg));
            UpdateUsersInfo();
        }

        private void MessageReceived()
        {
            var usrn = _server.PacketReader.ReadMessage();
            var msg = _server.PacketReader.ReadJPMessage();
            var chatcolor = "#FFADAD";
            foreach (var usr in Users)
            {
                if (usr.Username == usrn)
                {
                    chatcolor = usr.ChatColor;
                    break;
                }
            }
            ChatMessage currentMsg = new ChatMessage(msg, "Message", usrn, chatcolor);
            Application.Current.Dispatcher.Invoke(() => ChatMessages.Add(currentMsg));

        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage(),
                ChatColor = _server.PacketReader.ReadMessage()
            };
            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
                ChatMessage currentMsg = new ChatMessage($"{user.Username}さんはサーバーに接続しました！", "Connected");
                Application.Current.Dispatcher.Invoke(() => ChatMessages.Add(currentMsg));
            }
            
            UpdateUsersInfo();

        }

        private void YouConnected()
        {
            var user = new UserModel
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage(),
                ChatColor = _server.PacketReader.ReadMessage()
            };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
                if (user.Username == Username)
                {
                    ChatMessage currentMsg = new ChatMessage("サーバーに接続しました！", "Connected");
                    Application.Current.Dispatcher.Invoke(() => ChatMessages.Add(currentMsg));
                }
            }
            UpdateUsersInfo();
        }

        public void BackLoginButtonClick()
        {
            DisableAllScreens();
            Username = string.Empty;
            Connected = true;
        }

        public void ToLoginButtonClick()
        {
            DisableAllScreens();
            Login = true;
        }

        public void BackRegisterButtonClick()
        {
            DisableAllScreens();
            RegisterUsername = string.Empty;
            RegisterMail = string.Empty;
            Connected = true;

        }

        public void ToRegisterButtonClick()
        {
            DisableAllScreens();
            Register = true;
        }

        public void LoginSuccess()
        {
            var msg = _server.PacketReader.ReadMessage();
            Debug.WriteLine(msg);
            if (SaveLogin == true)
            {
                Properties.Settings.Default.autoLogin = true;
                Properties.Settings.Default.savedUsername = Username;
                Properties.Settings.Default.savedPassword = Password;
                Properties.Settings.Default.Save();
            }
            DisableAllScreens();
            CanChat = true;
        }

        public void clearMessageBox()
        {
            Message = string.Empty;
        }

        public void LoginError()
        {
            ServerMessage = "ユーザーネームまたはパスワードが違います。";
        }

        public void ConnectSuccess()
        {

            var msg = _server.PacketReader.ReadMessage();
            Debug.WriteLine(msg);
            if (SaveServer == true)
            {
                Properties.Settings.Default.autoConnect = true;
                Properties.Settings.Default.savedServerIP = ServerIP;
                Properties.Settings.Default.savedServerPort = ServerPort;
                Properties.Settings.Default.savedServerPassword = ServerPassword;
                Properties.Settings.Default.Save();

            }

            DisableAllScreens();

            if (Properties.Settings.Default.autoLogin == true)
            {
                if (Properties.Settings.Default.savedUsername != "" && Properties.Settings.Default.savedPassword != "")
                {
                    Username = Properties.Settings.Default.savedUsername;
                    Password = Properties.Settings.Default.savedPassword;
                    _server.LoginAttempt(Properties.Settings.Default.savedUsername, Properties.Settings.Default.savedPassword);

                }
                else
                {
                    Connected = true;
                }

            }
            else
            {
                Connected = true;
            }

        }

        public void ConnectFailure()
        {
            var msg = _server.PacketReader.ReadMessage();
            Debug.WriteLine(msg);
        }

        public void UsernameTaken()
        {
            var msg = _server.PacketReader.ReadMessage();
            ServerMessageColor = "red";
            ServerMessage = "ユーザー名は既に使われています。";
        }

        public void RegisterSuccess()
        {
            var msg = _server.PacketReader.ReadMessage();
            DisableAllScreens();
            RegisterUsername = string.Empty;
            RegisterMail = string.Empty;
            Login = true;
            ServerMessageColor = "LimeGreen";
            ServerMessage = msg;


        }

        public void RegisterFailure()
        {
            var msg = _server.PacketReader.ReadMessage();
            ServerMessageColor = "red";
            ServerMessage = msg;

        }

        public void PasswordMismatch()
        {
            ServerMessageColor = "red";
            ServerMessage = "パスワードが一致していません。";

        }

        public void InvalidUsername()
        {
            ServerMessageColor = "red";
            ServerMessage = "無効なユーザー名。ユーザー名が4～16桁で、英字または数字のみが含まれていることを確認してください。";
        }

        public void InvalidPassword()
        {
            ServerMessageColor = "red";
            ServerMessage = "無効なパスワード。パスワードは4～16桁で、英字または数字のみが含まれていることを確認してください。";
        }

        public void InvalidMail()
        {
            ServerMessageColor = "red";
            ServerMessage = "電子メールが無効です。電子メールが正しい形式(例:example@example.com)または空欄(電子メールは不要)であることを確認してください。";
        }

        public void BackToConnectScreen()
        {
            //_server.DisconnectServer();
            _server.QuickDC();
            DisableAllScreens();
            ResetSize();
            Username = string.Empty;
            SaveServer = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChatMessages)));
            Disconnected = true;
        }

        public void BackToLoginScreen()
        {
            _server.LogoutServer();
            DisableAllScreens();
            ResetSize();
            Username = string.Empty;
            SaveLogin = false;
            Login = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChatMessages)));
        }

        public void LockWindow()
        {

            if (IsLocked == false)
            {
                IsLocked = true;
            }
            else
            {
                IsLocked = false;
            }
        }

        private void CheckAuto()
        {
            SaveLogin = Properties.Settings.Default.autoLogin;
            SaveServer = Properties.Settings.Default.autoConnect;
        }

        private bool IsAnyFieldEmpty(params string[] fields)
        {
            bool result = true;
            foreach (string field in fields)
            {
                if (string.IsNullOrEmpty(field))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private void DisableAllScreens()
        {
            SafeClearChat();
            Login = false;
            Register = false;
            Connected = false;
            Disconnected = false;
            CanChat = false;
            ServerMessage = string.Empty;

        }

        private void ResetSize()
        {
            Width = 348;
            Height = 100;
        }


        private void UnexpectedDisconnect()
        {
            DisableAllScreens();
            ResetSize();
            Username = string.Empty;
            Disconnected = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChatMessages)));
        }

        private void DuplicateLogin()
        {
            BackToLoginScreen();
            ServerMessageColor = "red";
            ServerMessage = "切断されました:重複したユーザーが別の場所でログインしました。";
        }

        private void SafeClearChat()
        {
            if (CanChat == true)
            {
                Application.Current.Dispatcher.Invoke(() => ChatMessages.Clear());
            }
        }

        private void CouldNotConnect()
        {
            ServerMessageColor = "red";
            ServerMessage = "接続できませんでした。ホストアドレスとポートを確認してください。";

        }

        private void UpdateUsersInfo()
        {
            string usrString = "";
            if(Users.Count <= 1)
            {
                usrString = "誰もオンラインではない";
            }
            else
            {
                foreach (var user in Users)
                {
                    if (user.Username == Username)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(usrString))
                    {
                        usrString = user.Username;
                    }
                    else
                    {
                        usrString += $"\n{user.Username}";
                    }
                }
            }

            UsersString = usrString;
            UserCount = (Users.Count - 1).ToString();
        }
    }
}
