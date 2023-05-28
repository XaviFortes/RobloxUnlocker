using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RobloxUnlocker
{
    /// <summary>
    /// Interaction logic for ModernOverlay.xaml
    /// </summary>
    public partial class ModernOverlay : Window
    {
        public string IP = "";
        public const string WINDOW_NAME = "RobloxPlayerBeta";
        private bool isDarkened = false; // Flag to track if the background is currently darkened
        private SolidColorBrush originalBackground; // Stores the original background color

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        public static IntPtr handle = FindWindow(null, WINDOW_NAME);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT IpRect);

        public static RECT rect;

        public struct RECT
        {
            public int left, top, right, bottom;
        }

        HwndSource hwndSource;

        private void CheckPing(object sender, EventArgs e)
        {
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(IP);
            if (pingReply.Status == IPStatus.Success)
            {
                labelPing.Content = "Ping to server: " + pingReply.RoundtripTime.ToString() + "ms";
            }
            else
            {
                labelPing.Content = "Ping: Failed";
            }
        }

        public ModernOverlay()
        {
            Before_All();
            InitializeComponent();
        }

        private void Before_All()
        {
            // Windows Size same as Display
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            // Set window position to top left
            this.Left = 0;
            this.Top = 0;
            // Set window to be always on top
            this.Topmost = true;
            // Set background transparency to 100%
            this.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            // Remove window border
            this.BorderThickness = new Thickness(0);

            // Store the original background color
            originalBackground = (SolidColorBrush)this.Background;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            IntPtr hwnd = hwndSource.Handle;

            int initialStyle = GetWindowLong(hwnd, -20);
            SetWindowLong(hwnd, -20, initialStyle | 0x8000 | 0x20);
            getIP();

            // Check ping every 3 seconds
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
            {
                Interval = 3000
            };
            timer.Tick += new EventHandler(CheckPing);
            timer.Start();

            // Subscribe to the ThreadPreprocessMessage event to handle system commands
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageHandler;
        }

        private void getIP()
        {
            int counter = 0;
            string line;
            string appDataFolder = Environment.GetEnvironmentVariable("LocalAppData");
            string filePath = System.IO.Path.Combine(appDataFolder, "Roblox\\logs");
            DirectoryInfo info = new DirectoryInfo(filePath);
            FileInfo[] files = info.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();
            foreach (FileInfo fil in files)
            {
                if (Convert.ToString(fil).Contains("last.log"))
                {
                    filePath = System.IO.Path.Combine(filePath, Convert.ToString(fil));
                    break;
                }
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader file = new StreamReader(fs))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Contains("Connecting to"))
                        {
                            int index = line.IndexOf("to") + 3;
                            string piece = line.Substring(index);
                            if (piece.Contains(":"))
                            {
                                piece = piece.Substring(0, piece.IndexOf(":"));
                            }
                            IP = piece;
                        }
                        counter++;
                    }
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isDarkened)
            {
                this.Background = new SolidColorBrush(Color.FromArgb((byte)slider.Value, 0, 0, 0));
            }
        }

        private void ThreadPreprocessMessageHandler(ref MSG msg, ref bool handled)
        {
            const int WM_SYSKEYDOWN = 0x0104;
            const int VK_X = 0x58;

            if (msg.message == WM_SYSKEYDOWN && msg.wParam.ToInt32() == VK_X)
            {
                if (!isDarkened)
                {
                    this.Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
                    isDarkened = true;
                }
                else
                {
                    this.Background = originalBackground;
                    isDarkened = false;
                }
                handled = true;
            }
        }
    }
}
