using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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

        // Do checkings when the window is loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Mutex(true, "ROBLOX_singletonMutex");
            mutexLabel.Content = "Mutex Removed";
            mutexLabel.Foreground = Brushes.Green;
            Checks();
        }
        
        private async void patchGame()
        {
            robloxLabel.Content = "Roblox Running";
            robloxLabel.Foreground = Brushes.Green;

            fpsButton.IsEnabled = true;
            fpsTextBox.IsEnabled = true;

            Console.WriteLine("Hello Warudo");
            // Get raw body of "https://setup.rbxcdn.com/version"
            // Get the version from the body

            string versionUrl = "https://setup.rbxcdn.com/version";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Roblox");
                HttpResponseMessage response = await client.GetAsync(versionUrl);
                if (response.IsSuccessStatusCode)
                {
                    string latestVersion = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Latest version: {latestVersion}");
                    Globals.LatestVersion = latestVersion;
                    Globals.RobloxPath = System.IO.Path.Combine(Globals.VersionsFolder, latestVersion);
                }
                else
                {
                    // Handle the error case when the API request fails
                    Debug.WriteLine($"Failed to retrieve the latest version. Status code: {response.StatusCode}");
                    throw new HttpRequestException($"Failed to retrieve the latest version. Status code: {response.StatusCode}");
                }
            }

            if (!Directory.Exists(Globals.RobloxPath))
            {
                Globals.OldVersion = true;
                Console.WriteLine("Folder doesn't exist");
                System.IO.Directory.CreateDirectory(Globals.RobloxPath);
            }

                

            if (!Directory.Exists(System.IO.Path.Combine(Globals.RobloxPath, "ClientSettings")))
            {
                Console.WriteLine("Folder doesn't exist");
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Globals.RobloxPath, "ClientSettings"));
            }

            Globals.AppConfigPath = System.IO.Path.Combine(Globals.RobloxPath, "ClientSettings", "ClientAppSettings.json");

            // Create the text file if it doesn't exist
            if (!File.Exists(Globals.AppConfigPath))
            {
                Console.WriteLine("File Config doesn't exist");
                File.Create(Globals.AppConfigPath).Dispose();
            }

            // Check if file exists and contains "DFIntTaskSchedulerTargetFps" over 60
            // If it does get the value and write it to the textbox
            // If it doesn't write 144 to the textbox
            if (!File.Exists(Globals.AppConfigPath))
            {
                GetLatestFFLags();
            }
            // CloseProcess("RobloxPlayerBeta");
        }

        private async void GetLatestFFLags()
        {
            // Insert the json of the pastebin "https://pastebin.com/raw/actWUApG"
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Roblox");
                HttpResponseMessage response = await client.GetAsync("https://pastebin.com/raw/actWUApG");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    // With Newtonsoft.Json change the value of "DFIntTaskSchedulerTargetFps" to the value of the textbox fpsTextBox.Text
                    dynamic release = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    release.DFIntTaskSchedulerTargetFps = Convert.ToInt32(fpsTextBox.Text);
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(release, Newtonsoft.Json.Formatting.Indented);

                    File.WriteAllText(Globals.AppConfigPath, json);
                }
                else
                {
                    // Handle the error case when the API request fails
                    Debug.WriteLine($"Failed to retrieve the config file. Status code: {response.StatusCode}");
                    throw new HttpRequestException($"Failed to retrieve the config file. Status code: {response.StatusCode}");
                }
            }
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


            fpsButton.IsEnabled = true;

            patchGame();

            // Check if roblox is running
            var process = Process.GetProcessesByName("RobloxPlayerBeta").FirstOrDefault();
            if (process != null)
            {
                // Get the folder of the running roblox
                var cVersion = process.MainModule.FileName;
                robloxLabel.Content = "Roblox Running, Restart it!";
                robloxLabel.Foreground = Brushes.Orange;
                MessageBox.Show($"You are currently running version {cVersion}\nLatest version is {Globals.LatestVersion}");
            }
            else
            {
                robloxLabel.Content = "Roblox";
                robloxLabel.Foreground = Brushes.Green;

            }
        }

        private void Mutex_Click(object sender, RoutedEventArgs e)
        {
            mutexLabel.Content = "Mutex Removed";
            mutexLabel.Foreground = Brushes.Green;
        }



        private void FpsButton_Click(object sender, RoutedEventArgs e)
        {
            // I will Replace the value of "DFIntTaskSchedulerTargetFps" JSON to the value of the textbox fpsTextBox.Text
            var content = File.ReadAllText(Globals.AppConfigPath);
            dynamic release = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
            release.DFIntTaskSchedulerTargetFps = Convert.ToInt32(fpsTextBox.Text);
            content = Newtonsoft.Json.JsonConvert.SerializeObject(release, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(Globals.AppConfigPath, content);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Checks();
            GetLatestFFLags();
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            // Open the about window
            var about = new About();
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

    internal class About
    {
        public About()
        {
            // Generate the about window
            var about = new Window();
            about.Title = "About";
            about.Width = 300;
            about.Height = 300;
            about.ResizeMode = ResizeMode.NoResize;
            about.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Define the colors
            var backgroundColor = (Color)ColorConverter.ConvertFromString("#FF444444");
            var foregroundColor = Colors.White;

            // Create a Grid to hold the content
            var grid = new Grid
            {
                Background = new SolidColorBrush(backgroundColor)
            };
            about.Content = grid;

            // Create a TextBlock for the text
            var textBlock = new TextBlock
            {
                Text = $"Roblox Unlocker Tool\n\nMade by: Xavi Fortes\n\nVersion: {Assembly.GetEntryAssembly().GetName().Version}",
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(backgroundColor),
                Foreground = new SolidColorBrush(foregroundColor)
            };
            // Add the TextBlock to the grid
            grid.Children.Add(textBlock);

            // Create a Button
            var button = new Button
            {
                Content = "GitHub",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidColorBrush(backgroundColor),
                Foreground = new SolidColorBrush(foregroundColor)
            };
            button.Click += (sender, args) => Process.Start("https://github.com/XaviFortes");
            // Add the Button to the grid
            grid.Children.Add(button);

            // Set the colors as resources in the About window
            about.Resources.Add("MenuBackground", new SolidColorBrush(backgroundColor));
            about.Resources.Add("MenuForeground", new SolidColorBrush(foregroundColor));

            about.Show();
        }

    }
}
