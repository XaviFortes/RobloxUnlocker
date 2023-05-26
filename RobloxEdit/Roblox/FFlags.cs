using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RobloxUnlocker.Roblox
{
    public class FFlags
    {
        public string Root { get; set; }
        public Dictionary<string, object> Overrides { get; set; }
        public Dictionary<string, object> Flags { get; set; } = new Dictionary<string, object>();
        public string BaseURL { get; set; } = "https://roblox-client-optimizer.simulhost.com";
        public string FlagHash { get; set; }

        private readonly HttpClient client = new HttpClient();
        private readonly List<Action<Dictionary<string, object>>> postWriteListeners = new List<Action<Dictionary<string, object>>>();
        private System.Threading.Timer interval;

        public FFlags(Dictionary<string, object> overrides = null, string root = null)
        {
            Construct(overrides, root);
        }

        public void Construct(Dictionary<string, object> overrides = null, string root = null)
        {
            Root = root ?? Root;
            Overrides = overrides ?? Overrides;
            var flagHashFilePath = Path.Combine(Globals.VersionsPath, "ClientAppSettings.json.sha512");
            FlagHash = File.Exists(flagHashFilePath) ? File.ReadAllText(flagHashFilePath).Trim() : string.Empty;

            var flagsFilePath = Path.Combine(Globals.VersionsPath, "ClientAppSettings.json");
            if (File.Exists(flagsFilePath))
            {
                var json = File.ReadAllText(flagsFilePath);
                Flags = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }

            Flags = MergeFlags(Flags, Overrides);
        }

        public void OnWrite(Action<Dictionary<string, object>> callback)
        {
            postWriteListeners.Add(callback);
        }

        public async Task Init(Roblox roblox = null)
        {
            if (roblox != null)
            {
                OnWrite(flags =>
                {
                    roblox.SetFlags(flags);
                });
            }

            await UpdateFlagsList();
            interval = new System.Threading.Timer(async _ => await UpdateFlagsList(), null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        }

        public void Uninit()
        {
            interval?.Dispose();
            interval = null;
        }

        public async Task Install(Roblox roblox = null)
        {
            await UpdateFlagsList(true);
            roblox?.SetFlags(Flags);
        }

        public Task Uninstall(Roblox roblox = null)
        {
            roblox?.DelFlags();
            return Task.CompletedTask;
        }

        public void WriteFlags()
        {
            var flagsFilePath = Path.Combine(Root, "ClientAppSettings.json");
            var flagHashFilePath = Path.Combine(Root, "ClientAppSettings.json.sha512");

            var json = JsonConvert.SerializeObject(Flags, Formatting.Indented);
            File.WriteAllText(flagsFilePath, json);
            File.WriteAllText(flagHashFilePath, FlagHash);

            foreach (var listener in postWriteListeners)
            {
                listener(Flags);
            }
        }

        public string ClientAppSettingsURL => $"{BaseURL}/ClientAppSettings.json";

        public string ClientAppSettingsHashURL => $"{BaseURL}/ClientAppSettings.json.sha512";

        public async Task UpdateFlagsList(bool callWrite = true)
        {
            var flagHashResponse = await client.GetStringAsync(ClientAppSettingsHashURL);
            var flagHash = flagHashResponse.Trim();

            if (flagHash != FlagHash)
            {
                var flagsJsonResponse = await client.GetStringAsync(ClientAppSettingsURL);
                var flags = JsonConvert.DeserializeObject<Dictionary<string, object>>(flagsJsonResponse);
                Flags = MergeFlags(flags, Overrides);
                FlagHash = flagHash;

                if (callWrite)
                {
                    WriteFlags();
                }
            }
        }

        private Dictionary<string, object> MergeFlags(Dictionary<string, object> flags1, Dictionary<string, object> flags2)
        {
            var mergedFlags = new Dictionary<string, object>(flags1);
            foreach (var kvp in flags2)
            {
                mergedFlags[kvp.Key] = kvp.Value;
            }
            return mergedFlags;
        }
    }
}
