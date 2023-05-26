using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobloxUnlocker
{
    public static class Globals
    {
        public static readonly string AppDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XaviFortes", "RobloxUnlocker");
        public static readonly string RobloxFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");
        public static readonly string VersionsFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "Versions");
        public static string LatestVersion { get; set; }
        public static string RobloxPath { get; set; }
        public static string AppConfigPath { get; set; }
        public static bool OldVersion { get; set; }
    }
}
