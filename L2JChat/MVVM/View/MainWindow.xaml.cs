using L2JChat.MVVM.ViewModel;
using NHotkey;
using NHotkey.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Security.Principal;

namespace L2JChat
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        IntPtr hwnd = default(IntPtr);
        private bool IsTopmost;
        private Boolean AutoScroll = true;
        Mutex myMutex;



        public MainWindow()
        {
            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "L2JChatWindow", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                Environment.Exit(0);
            }
            InitializeComponent();
            AdminRelauncher();
            IsTopmost = true;
            Topmost = true;
            imeImage.DataContext = this;
            SetupLogFile();
            SetupUnhandledExceptionHandling();
            HotkeyManager.Current.AddOrReplace("FocusChat", Key.Enter, ModifierKeys.Shift, FocusWindow);
            var vm = (MainViewModel)DataContext;
            var scrollViewer = FindScrollViewer(fDoc);
        }

        // Move window

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            if (e.LeftButton == MouseButtonState.Pressed && vm.IsLocked == false)
            {

                DragMove();

            }
        }

        // Handling Lang Input Button

        public event PropertyChangedEventHandler PropertyChanged;
        public bool ImeFeedback { get; set; }

        private void SatusKeyUpHandler(object sender, KeyEventArgs e)
        {
            string imeStatus = InputMethod.Current.ImeState.ToString();
            string imeConv = InputMethod.Current.ImeConversionMode.ToString();
            string imeLang = System.Windows.Input.InputLanguageManager.Current.CurrentInputLanguage.Name.ToString();

            if (imeStatus == "On")
            {
                if (imeLang == "ja-JP")
                {
                    if (imeConv.IndexOf("Native") == 0)
                    {
                        ImeFeedback = true;
                    }
                    else { ImeFeedback = false; }
                }
                else { ImeFeedback = false; }
            }
            else
            {
                ImeFeedback = false;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImeFeedback)));
        }


        private void pwbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pwbPassword.Password.Length > 0)
                tbPassword.Visibility = Visibility.Collapsed;
            else
                tbPassword.Visibility = Visibility.Visible;

            if (this.DataContext != null)
            { ((MainViewModel)this.DataContext).Password = ((PasswordBox)sender).Password; }
        }


        private void pwbServerPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pwbServerPassword.Password.Length > 0)
                tbServerPassword.Visibility = Visibility.Collapsed;
            else
                tbServerPassword.Visibility = Visibility.Visible;

            if (this.DataContext != null)
            { ((MainViewModel)this.DataContext).ServerPassword = ((PasswordBox)sender).Password; }
        }


        private void pwbRegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pwbRegisterPassword.Password.Length > 0)
                tbRegisterPassword.Visibility = Visibility.Collapsed;
            else
                tbRegisterPassword.Visibility = Visibility.Visible;
            { ((MainViewModel)this.DataContext).RegisterPassword = ((PasswordBox)sender).Password; }
        }

        private void pwb2RegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pwb2RegisterPassword.Password.Length > 0)
                tb2RegisterPassword.Visibility = Visibility.Collapsed;
            else
                tb2RegisterPassword.Visibility = Visibility.Visible;
            { ((MainViewModel)this.DataContext).RegisterPasswordConfirm = ((PasswordBox)sender).Password; }
        }

        private void Window_Deactivated(object sender, System.EventArgs e)
        {
            if(IsTopmost == true)
            {
                Window window = (Window)sender;
                window.Topmost = true;
            }

        }

        private void FocusWindow(object sender, HotkeyEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            [DllImport("user32.dll")]
            static extern IntPtr GetForegroundWindow();

            [DllImport("User32.dll")]
            static extern void SetForegroundWindow(IntPtr hWnd);

            if (!Main.IsActive)
            {
                hwnd = GetForegroundWindow();
                Main.Activate();
                tb1.Focus();
            }
            else if (hwnd != default(IntPtr))
            {
                vm._server.SendMessageToServer(vm.Message);
                SetForegroundWindow(hwnd);
            }
        }

        void ClickCopy(Object sender, RoutedEventArgs args) { System.Windows.Clipboard.SetText(fDoc.Selection.Text); }

        void OnOpened(Object sender, RoutedEventArgs args)
        {
            if (fDoc.Selection.Text == null || fDoc.Selection.Text == "")
                fCMCopy.IsEnabled = false;
            else
                fCMCopy.IsEnabled = true;
        }

        private void fDocRightClick(object sender, MouseButtonEventArgs e)
        {
            if (fDoc.Selection.Text == null || fDoc.Selection.Text == "")
            {
                return;
            }
            else
            {
                fDocu.ContextMenu.IsOpen = true;
            }
        }

        private void ClickGoogleT(object sender, RoutedEventArgs e)
        {
            string sText = fDoc.Selection.Text;

            var psi = new ProcessStartInfo
            {
                FileName = $"https://translate.google.com/?sl=ja&tl=en&text={sText}&op=translate",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void ClickJPGoogleT(object sender, RoutedEventArgs e)
        {
            string sText = fDoc.Selection.Text;

            var psi = new ProcessStartInfo
            {
                FileName = $"https://translate.google.com/?sl=en&tl=ja&text={sText}&op=translate",
                UseShellExecute = true
            };
            Process.Start(psi);

        }

        private void ClickJisho(object sender, RoutedEventArgs e)
        {
            string sText = fDoc.Selection.Text;

            var psi = new ProcessStartInfo
            {
                FileName = $"https://jisho.org/search/{sText}",
                UseShellExecute = true
            };
            Process.Start(psi);

        }

        private void ClickWeblio(object sender, RoutedEventArgs e)
        {
            string sText = fDoc.Selection.Text;

            var psi = new ProcessStartInfo
            {
                FileName = $"https://ejje.weblio.jp/content/{sText}",
                UseShellExecute = true
            };
            Process.Start(psi);

        }

        private void ClickL2Jru(object sender, RoutedEventArgs e)
        {
            string sText = fDoc.Selection.Text;

            var psi = new ProcessStartInfo
            {
                FileName = $"http://l2j.ru/hellbound/index.php?s={sText}",
                UseShellExecute = true
            };
            Process.Start(psi);

        }

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void ToggleTopmost(object sender, RoutedEventArgs e)
        {
            if(IsTopmost == true)
            {
                IsTopmost = false;
                Topmost = false;
            }
            else
            {
                IsTopmost = true;
                Topmost = true;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = FindScrollViewer(fDoc);
            scrollViewer.ScrollToBottom();
        }

        public static ScrollViewer FindScrollViewer(FlowDocumentScrollViewer flowDocumentScrollViewer)
        {
            if (VisualTreeHelper.GetChildrenCount(flowDocumentScrollViewer) == 0)
            {
                return null;
            }

            // Border is the first child of first child of a ScrolldocumentViewer
            DependencyObject firstChild = VisualTreeHelper.GetChild(flowDocumentScrollViewer, 0);
            if (firstChild == null)
            {
                return null;
            }

            Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;

            if (border == null)
            {
                return null;
            }

            return border.Child as ScrollViewer;
        }

        public void scrollFDoc()
        {
            var scrollViewer = FindScrollViewer(fDoc);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                //scrollViewer.ScrollToBottom();
            }

        }

        private void ParagraphLoaded(object sender, RoutedEventArgs e)
        {
            scrollFDoc();
        }

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = FindScrollViewer(fDoc);
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }
        }

        private void DisableAutoLogin(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autoLogin = false;
            Properties.Settings.Default.Save();
        }

        private void DisableAutoConnect(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autoConnect = false;
            Properties.Settings.Default.Save();
        }


        private void SetupUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                LogUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                LogUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += (sender, args) =>
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    LogUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };

            // Catch exceptions from the main UI dispatcher thread.
            // Typically we only need to catch this OR the Dispatcher.UnhandledException.
            // Handling both can result in the exception getting handled twice.
            //Application.Current.DispatcherUnhandledException += (sender, args) =>
            //{
            //	// If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
            //	if (!Debugger.IsAttached)
            //	{
            //		args.Handled = true;
            //		ShowUnhandledException(args.Exception, "Application.Current.DispatcherUnhandledException", true);
            //	}
            //};
        }

        void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred:\n\n{e}";
            var messageBoxButtons = MessageBoxButton.OK;

            if (promptUserForShutdown)
            {
                messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
                messageBoxButtons = MessageBoxButton.YesNo;
            }

            // Let the user decide if the app should die or not (if applicable).
            if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        void LogUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred:\n\n{e}";
            string filePath = "Log.txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine("Date : " + DateTime.Now.ToString());
                writer.WriteLine();

                while (e != null)
                {
                    writer.WriteLine(e.GetType().FullName);
                    writer.WriteLine("Message : " + e.Message);
                    writer.WriteLine("StackTrace : " + e.StackTrace);

                    e = e.InnerException;
                }
            }

        }

            private void PWBVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            pwbPassword.Password = string.Empty;
        }

        private void RPWBVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            pwbRegisterPassword.Password = string.Empty;
        }

        private void RPWB2VisiblityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            pwb2RegisterPassword.Password = string.Empty;
        }

        private void SetupLogFile()
        {
            if (File.Exists("Log.txt"))
            {
                if (File.Exists("Log.bak"))
                {
                    File.Delete("Log.bak");
                }
                File.Move("Log.txt", "Log.bak");
            }
            else
            {
                File.Create("Log.txt");
            }
        }

        private void ServerPWBVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            pwbServerPassword.Password = string.Empty;
        }

        private void FocusOnVisible(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((TextBox)sender).IsVisible)
            {
                ((TextBox)sender).Focus();
            }
        }

        private void FocusChatBox(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((TextBox)sender).IsEnabled)
            {
                ((TextBox)sender).Focus();
            }
        }

        private void FocusLoginButton(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((Button)sender).IsVisible)
            {
                ((Button)sender).Focus();
            }
        }

        private static bool IsRunAsAdmin()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void AdminRelauncher()
        {
            if (!IsRunAsAdmin())
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");

                proc.Verb = "runas";

                try
                {
                    Process.Start(proc);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("This program must be run as an administrator! \n\n" + ex.ToString());
                }
            }
        }
    }
}
