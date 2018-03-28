using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GitHubIssuesCli
{
    internal sealed class SettingsStore
    {
        private readonly string _settingsFilePath;
        private readonly IDictionary<string, string> _settings;
        private const string SettingsFileName = "settings.json";

        public SettingsStore()
        {
            _settingsFilePath = GetSettingsFileName();

            // Ensure directory is created
            var secretDir = Path.GetDirectoryName(_settingsFilePath);
            Directory.CreateDirectory(secretDir);

            _settings = Load();
        }

        public string this[string key] => _settings[key];

        public int Count => _settings.Count;

        public bool ContainsKey(string key) => _settings.ContainsKey(key);

        public IEnumerable<KeyValuePair<string, string>> AsEnumerable() => _settings;

        public string GetSettingsFileName()
        {
            var root = Environment.GetEnvironmentVariable("APPDATA") ??         // On Windows it goes to %APPDATA%\Microsoft\UserSecrets\
                       Environment.GetEnvironmentVariable("HOME");             // On Mac/Linux it goes to ~/.microsoft/usersecrets/

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPDATA")))
            {
                return Path.Combine(root, "GithubIssuesCli", SettingsFileName);
            }
            else
            {
                return Path.Combine(root, ".github-issues-cli", SettingsFileName);
            }
        }

        public void Clear() => _settings.Clear();

        public void Set(string key, string value) => _settings[key] = value;

        public void Remove(string key)
        {
            if (_settings.ContainsKey(key))
            {
                _settings.Remove(key);
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));

            var contents = new JObject();
            if (_settings != null)
            {
                foreach (var secret in _settings.AsEnumerable())
                {
                    contents[secret.Key] = secret.Value;
                }
            }

            File.WriteAllText(_settingsFilePath, contents.ToString(), Encoding.UTF8);
        }

        private IDictionary<string, string> Load()
        {
            return new ConfigurationBuilder()
                .AddJsonFile(_settingsFilePath, optional: true)
                .Build()
                .AsEnumerable()
                .Where(i => i.Value != null)
                .ToDictionary(i => i.Key, i => i.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}