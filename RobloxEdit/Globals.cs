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
        public static string VersionsPath { get; set; }
        public static string AppVersionPath { get; set; }
        public static string AppConfigPath { get; set; }
        public static readonly string AppDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XaviFortes", "RobloxUnlocker");
    }
}
