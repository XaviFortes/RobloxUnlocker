using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RobloxUnlocker.Roblox
{
    public class Oof
    {
        public static string OofPath = "oof";
    }

    public class Roblox
    {
        private List<string> robloxPaths = new List<string>();

        public List<string> RobloxPaths
        {
            get { return robloxPaths; }
            set { robloxPaths = value; }
        }

        public List<string> GetRobloxPaths()
        {
            string platform = Environment.OSVersion.Platform.ToString();
            List<string> paths = new List<string>();

            switch (platform)
            {
                case "MacOSX":
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ROBLOX")) && Directory.Exists(Environment.GetEnvironmentVariable("ROBLOX")))
                    {
                        paths.Add(Environment.GetEnvironmentVariable("ROBLOX"));
                    }
                    string robloxAppPath = "/Applications/Roblox.app";
                    if (Directory.Exists(robloxAppPath))
                    {
                        paths.Add(robloxAppPath);
                    }
                    break;
                case "Win32NT":
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ROBLOX")) && Directory.Exists(Environment.GetEnvironmentVariable("ROBLOX")))
                    {
                        paths.Add(Environment.GetEnvironmentVariable("ROBLOX"));
                    }
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string robloxPath = Path.Combine(localAppData, "Roblox");
                    if (Directory.Exists(robloxPath) && Directory.Exists(Path.Combine(robloxPath, "Versions")))
                    {
                        paths.Add(robloxPath);
                    }
                    string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    string programFilesRobloxPath = Path.Combine(programFilesPath, "Roblox");
                    if (Directory.Exists(programFilesRobloxPath) && Directory.Exists(Path.Combine(programFilesRobloxPath, "Versions")))
                    {
                        paths.Add(programFilesRobloxPath);
                    }
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string appDataRobloxPath = Path.Combine(appData, "Roblox");
                    if (Directory.Exists(appDataRobloxPath) && Directory.Exists(Path.Combine(appDataRobloxPath, "Versions")))
                    {
                        paths.Add(appDataRobloxPath);
                    }
                    break;
                case "Unix":
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ROBLOX")) && Directory.Exists(Environment.GetEnvironmentVariable("ROBLOX")))
                    {
                        paths.Add(Environment.GetEnvironmentVariable("ROBLOX"));
                    }
                    paths.Add("/");
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")) && Directory.Exists(Environment.GetEnvironmentVariable("HOME")))
                    {
                        paths.Add(Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Roblox"));
                    }
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOCALAPPDATA")) && Directory.Exists(Environment.GetEnvironmentVariable("LOCALAPPDATA")))
                    {
                        paths.Add(Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Roblox"));
                    }
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPDATA")) && Directory.Exists(Environment.GetEnvironmentVariable("APPDATA")))
                    {
                        paths.Add(Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Roblox"));
                    }
                    break;
            }

            return paths;
        }

        public List<string> GetRobloxVersionsFromPath(string p)
        {
            List<string> versions = new List<string>();

            if (Environment.OSVersion.Platform.ToString() == "MacOSX")
            {
                versions.Add("Contents");
            }
            else
            {
                if (string.IsNullOrEmpty(p))
                {
                    throw new Exception("No Path Specified");
                }
                if (!Directory.Exists(p))
                {
                    throw new Exception("Path does not exist");
                }
                p = Path.GetFullPath(p);
                string versionsDir = p.EndsWith("Versions") ? p : Path.Combine(p, "Versions");
                if (!Directory.Exists(versionsDir))
                {
                    throw new Exception("Versions directory does not exist");
                }
                string[] versionDirs = Directory.GetDirectories(versionsDir);
                foreach (string versionDir in versionDirs)
                {
                    if (Directory.Exists(Path.Combine(versionsDir, versionDir)) && Directory.Exists(Path.Combine(versionsDir, versionDir)))
                    {
                        versions.Add(versionDir);
                    }
                }
                if (versions.Count == 0)
                {
                    throw new Exception("Versions directory is empty");
                }
                if (Environment.GetEnvironmentVariable("NO_STUDIO") != null)
                {
                    List<string> playerVersions = new List<string>();
                    foreach (string version in versions)
                    {
                        string playerPath = Path.Combine(versionsDir, version, "RobloxPlayerBeta.exe");
                        if (File.Exists(playerPath))
                        {
                            playerVersions.Add(version);
                        }
                    }
                    if (playerVersions.Count == 0)
                    {
                        throw new Exception("No RobloxPlayerBeta.exe found in Versions directory");
                    }
                    return playerVersions;
                }
            }

            return versions;
        }

        public string GetClientSettings(string versionDir)
        {
            switch (Environment.OSVersion.Platform.ToString())
            {
                case "MacOSX":
                    return Path.Combine(versionDir, "MacOS", "ClientSettings", "ClientAppSettings.json");
                case "Win32NT":
                case "Unix":
                    return Path.Combine(versionDir, "ClientSettings", "ClientAppSettings.json");
                default:
                    if (Environment.GetEnvironmentVariable("ROBLOXCAS") == null)
                    {
                        throw new Exception("Unsupported Platform - Please pass the environment variable ROBLOXCAS to specify the path to Roblox ClientAppSettings.json relative to " + versionDir);
                    }
                    else
                    {
                        return Path.GetFullPath(Path.Combine(versionDir, Environment.GetEnvironmentVariable("ROBLOXCAS")));
                    }
            }
        }

        public List<string> GetClientAppSettings()
        {
            List<string> clientAppSettings = new List<string>();
            foreach (string path in RobloxPaths)
            {
                string casPath = GetClientSettings(path);
                if (File.Exists(casPath))
                {
                    clientAppSettings.Add(casPath);
                }
            }
            return clientAppSettings;
        }


        public void SetFlags(Dictionary<string, object> flags)
        {
            List<string> clientAppSettings = GetClientAppSettings();
            foreach (string cas in clientAppSettings)
            {
                File.WriteAllText(cas, Newtonsoft.Json.JsonConvert.SerializeObject(flags));
            }
        }

        public void DelFlags()
        {
            SetFlags(new Dictionary<string, object>());
            List<string> clientAppSettings = GetClientAppSettings();
            foreach (string cas in clientAppSettings)
            {
                if (File.Exists(cas))
                {
                    File.Delete(cas);
                }
            }
        }

        public List<string> GetRobloxVersionDirFromPath(string p)
        {
            List<string> versionDirs = new List<string>();
            List<string> versions = GetRobloxVersionsFromPath(p);
            string versionDirPrefix = (Environment.OSVersion.Platform == PlatformID.MacOSX) ? "." : "Versions";
            foreach (string version in versions)
            {
                string versionDir = Path.Combine(p, versionDirPrefix, version);
                versionDirs.Add(versionDir);
            }
            return versionDirs;
        }



        public List<string> DiscoverRobloxPaths()
        {
            List<string> newPaths = GetRobloxPaths().SelectMany(GetRobloxVersionDirFromPath).Where(Directory.Exists).ToList();
            List<string> oldPaths = RobloxPaths;
            RobloxPaths = newPaths.Where(unique => oldPaths.Contains(unique)).ToList();
            return newPaths;
        }


        public void CallRobloxDiscoveredListeners()
        {
            foreach (Action<List<string>> listener in robloxDiscoveredListeners)
            {
                listener.Invoke(RobloxPaths);
            }
        }

        private List<Action<List<string>>> robloxDiscoveredListeners = new List<Action<List<string>>>();

        public void AddRobloxDiscoveredListener(Action<List<string>> listener)
        {
            robloxDiscoveredListeners.Add(listener);
        }

        public void RemoveRobloxDiscoveredListener(Action<List<string>> listener)
        {
            robloxDiscoveredListeners.Remove(listener);
        }

        public void Init(string[] robloxPaths)
        {
            if (robloxPaths == null)
            {
                robloxPaths = DiscoverRobloxPaths().ToArray();
                Timer timer = new Timer(2000);
                timer.Elapsed += (sender, e) =>
                {
                    List<string> newPaths = DiscoverRobloxPaths().FindAll(unique => !robloxPaths.Contains(unique));
                    if (newPaths.Count != robloxPaths.Length)
                    {
                        RobloxPaths = newPaths.FindAll(Directory.Exists);
                        CallRobloxDiscoveredListeners();
                    }
                };
                timer.Start();
            }
            RobloxPaths = robloxPaths.ToList();
        }

        public Roblox(string[] robloxPaths)
        {
            Init(robloxPaths);
        }
    }
}
