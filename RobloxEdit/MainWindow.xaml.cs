using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using RobloxUnlocker.Roblox;

namespace RobloxUnlocker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        public FFlags Flags { get; set; }

        // Do checkings when the window is loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Mutex(true, "ROBLOX_singletonMutex");
            mutexLabel.Content = "Mutex Removed";
            mutexLabel.Foreground = Brushes.Green;
            Checks();
        }
        
        private void patchGame(Process p)
        {
            robloxLabel.Content = "Roblox Running";
            robloxLabel.Foreground = Brushes.Green;

            fpsButton.IsEnabled = true;
            fpsTextBox.IsEnabled = true;

            Console.WriteLine("Hello Warudo");

            var path = System.IO.Path.GetDirectoryName(p.MainModule.FileName);
            Globals.AppVersionPath = path;

            if (!Directory.Exists(System.IO.Path.Combine(path, "ClientSettings")))
            {
                Console.WriteLine("Folder doesn't exist");
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "ClientSettings"));
            }

            Globals.AppConfigPath = System.IO.Path.Combine(path, "ClientSettings", "ClientAppSettings.json");

            // Check if file exists and contains "DFIntTaskSchedulerTargetFps" over 60
            // If it does get the value and write it to the textbox
            // If it doesn't write 144 to the textbox
            if (File.Exists(Globals.AppConfigPath))
            {
                Console.WriteLine("File exists");
                var content = File.ReadAllText(Globals.AppConfigPath);
                if (content.Contains("DFIntTaskSchedulerTargetFps"))
                {
                    var fps = content.Split(':')[1].Replace("}", "").Replace(" ", "");
                    if (Convert.ToInt32(fps) > 60)
                    {
                        fpsTextBox.Text = fps;
                        fpsLabel.Content = "Unlocked!!!";
                        fpsLabel.Foreground = Brushes.Green;
                    }
                }
            }
            CloseProcess("RobloxPlayerBeta");
        }

        public static void CloseProcess(string processName)
        {
            // Get all running processes with the given name
            Process[] processes = Process.GetProcessesByName(processName);

            // Close each process
            foreach (Process process in processes)
            {
                try
                {
                    // Close the process and wait for it to exit
                    process.CloseMainWindow();
                    if (!process.WaitForExit(5000))
                    {
                        // Forcefully terminate the process if it does not exit gracefully within 5 seconds
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur while closing the process
                    Console.WriteLine($"Error closing process: {ex.Message}");
                }
            }
        }

        private async void Checks()
        {
            if (!Directory.Exists(Globals.AppDataFolder))
            {
                Directory.CreateDirectory(Globals.AppDataFolder);
            }

            GitHubReleaseChecker releaseChecker = new GitHubReleaseChecker();
            string currentVersion = releaseChecker.GetCurrentVersion();

            // Check for updates
            await releaseChecker.CheckForUpdates(currentVersion);

            var process = Process.GetProcessesByName("RobloxPlayerBeta").FirstOrDefault();
            if (process == null) {
                // Run Launcher to update game
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "roblox-player://",
                    // Arguments = "-app",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                Debug.WriteLine("Starting Process " + processStartInfo);

                var pLaunch = Process.Start(processStartInfo);
                // patchGame(pLaunch);
                pLaunch.WaitForExit();
                if (Process.GetProcessesByName("RobloxPlayerBeta").FirstOrDefault() != null)
                {
                    process = Process.GetProcessesByName("RobloxPlayerBeta").FirstOrDefault();
                    patchGame(process);
                    fpsButton.IsEnabled = true;
                }
            }
            
            // Check if roblox is running
            
            if (process != null)
            {
                
            }
            else
            {
                robloxLabel.Content = "Roblox Not Running";
                robloxLabel.Foreground = Brushes.Red;
                fpsButton.IsEnabled = false;
                fpsTextBox.IsEnabled = false;

            }
        }

        private void Mutex_Click(object sender, RoutedEventArgs e)
        {
            mutexLabel.Content = "Mutex Removed";
            mutexLabel.Foreground = Brushes.Green;
        }



        private void FpsButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if file exists and if read if the file contains "DFIntTaskSchedulerTargetFps"
            // It should write something like this '{"DFIntTaskSchedulerTargetFps": 144}' to the file
            // If it doesn't exist it should create the file and write the same thing
            // Also get the value from the textbox and write it to the file instead of 144
            // Get the route from the process "RobloxPlayerBeta" and add the file name to the route

            
            // var path = System.IO.Path.GetDirectoryName(process.MainModule.FileName);
            // if folder doesn't exist create it
            var file = Globals.AppConfigPath;
            if (System.IO.File.Exists(file))
            {
                Console.WriteLine("File exists");
                var content = System.IO.File.ReadAllText(file);
                if (content.Contains("DFIntTaskSchedulerTargetFps"))
                {
                    var fps = content.Split(':')[1].Replace("}", "").Replace(" ", "");
                    Console.WriteLine("File contains DFIntTaskSchedulerTargetFps set to" + fps);
                    System.IO.File.WriteAllText(file, content.Replace(content.Split(':')[1].Replace("}", "").Replace(" ", ""), fpsTextBox.Text));
                    fpsLabel.Content = "FPS Changed";
                    fpsLabel.Foreground = Brushes.Green;
                }
                else
                {
                    Console.WriteLine("File doesn't contain DFIntTaskSchedulerTargetFps");
                    System.IO.File.WriteAllText(file, content + "\n" + "{\"DFIntTaskSchedulerTargetFps\": " + fpsTextBox.Text + "}");
                    fpsLabel.Content = "FPS Changed";
                    fpsLabel.Foreground = Brushes.Green;
                }
            }
            else
            {
                System.IO.File.WriteAllText(file, "{\"DFIntTaskSchedulerTargetFps\": " + fpsTextBox.Text + "}");
                fpsLabel.Content = "FPS Changed";
                fpsLabel.Foreground = Brushes.Green;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Checks();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ButtonSorter_Click(object sender, RoutedEventArgs e)
        {
            // var searchDnD = new searchImageDnD();
            // searchDnD.Show();
            /*
            var invSort = new InventorySort();
            invSort.Show();
            */
        }
    }
}
