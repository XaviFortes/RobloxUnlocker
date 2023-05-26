using System;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace RobloxUnlocker
{
    public class GitHubReleaseChecker
    {
        private const string RepositoryOwner = "XaviFortes";
        private const string RepositoryName = "RobloxUnlocker";
        readonly string appDataFolder = Globals.AppDataFolder;

        public string GetCurrentVersion()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Version version = assembly.GetName().Version;
            string currentVersion = version.ToString();

            return currentVersion;
        }

        public async Task CheckForUpdates(string currentVersion)
        {
            string latestVersion = await GetLatestReleaseVersion();
            
                if ((CompareVersions(currentVersion, latestVersion) < 0))
                {
                    MessageBoxResult result = MessageBox.Show("A new version is available. Do you want to download it?", "Update Available", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        await DownloadAndInstallLatestVersion();
                    }
                }
        }

        public static int CompareVersions(string v1, string v2)
        {
                int vnum1 = 0, vnum2 = 0;
            for (int i = 0, j = 0; (i < v1.Length || j < v2.Length);)
            {

                // storing numeric part of
                // version 1 in vnum1
                while (i < v1.Length && v1[i] != '.')
                {
                    vnum1 = vnum1 * 10 + (v1[i] - '0');
                    i++;
                }

                // storing numeric part of
                // version 2 in vnum2
                while (j < v2.Length && v2[j] != '.')
                {
                    vnum2 = vnum2 * 10 + (v2[j] - '0');
                    j++;
                }

                if (vnum1 > vnum2)
                    return 1;
                if (vnum2 > vnum1)
                    return -1;

                // if equal, reset variables and
                // go for next numeric part
                vnum1 = vnum2 = 0;
                i++;
                j++;
            }
            return 0;
        }   

        public async Task<string> GetLatestReleaseVersion()
        {
            string apiUrl = $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "SSHarp");

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    dynamic release = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    string latestVersion = release.tag_name;
                    Debug.WriteLine($"Latest version: {latestVersion}");
                    return latestVersion;
                }
                else
                {
                    // Handle the error case when the API request fails
                    Debug.WriteLine($"Failed to retrieve the latest release version. Status code: {response.StatusCode}");
                    throw new HttpRequestException($"Failed to retrieve the latest release version. Status code: {response.StatusCode}");
                }
            }
        }

        private async Task DownloadAndInstallLatestVersion()
        {
            string latestVersion = await GetLatestReleaseVersion();
            string downloadUrl = $"https://github.com/{RepositoryOwner}/{RepositoryName}/releases/latest/download/RobloxUnlocker.exe";

            // Perform the download and installation process using appropriate methods
            // For example, you can use WebClient.DownloadFileAsync to download the installer
            // and Process.Start to launch the installer.

            // Here's an example of using WebClient to download the installer file:
            string installerPath = Path.Combine(appDataFolder, $"RobloxUnlocker-{latestVersion}.exe");
            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(downloadUrl, installerPath);
            }

            // After the download, you can launch the installer
            // Process.Start(installerPath);

            // Close the current application
            // Application.Current.Shutdown();

            // Open the installer folder
            Process.Start("explorer.exe", appDataFolder);
        }
    }
}
