﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace thecalcify.Helper
{
    public class CredentialManager
    {
        private static readonly string CredentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.dat");
        private static StorageData _cachedStorage;

        #region Credential Save & Load Methods
        public static void SaveCredentials(string username, string password, bool remember)
        {
            try
            {
                if (!remember)
                {
                    DeleteCredentials();
                    return;
                }

                EnsureStorageInitialized();
                _cachedStorage.Credentials = new UserCredentials
                {
                    Username = username,
                    Password = password
                };
                SaveStorage();
            }
            catch (Exception ex)
            {
                ShowError("Failed to save credentials", ex);
                ApplicationLogger.LogException(ex);
            }
        }

        public static (string Username, string Password) LoadCredentials()
        {
            try
            {
                EnsureStorageInitialized();
                return (_cachedStorage.Credentials?.Username, _cachedStorage.Credentials?.Password);
            }
            catch
            {
                return (null, null);
            }
        }

        public static void DeleteCredentials()
        {
            try
            {
                if (File.Exists(CredentialsPath))
                    File.Delete(CredentialsPath);
                _cachedStorage = new StorageData();
            }
            catch { }
        }

        #endregion

        #region MarketWatch Load Method

        public static void SaveMarketWatchWithColumns(string marketWatchName, List<string> columnPreferences)
        {
            try
            {
                EnsureStorageInitialized();

                var existing = _cachedStorage.MarketWatchList.FirstOrDefault(mw => mw.MarketWatchName == marketWatchName);
                if (existing == null)
                {
                    existing = new MarketWatchInfo { MarketWatchName = marketWatchName };
                    _cachedStorage.MarketWatchList.Add(existing);
                }

                existing.ColumnPreferences = new ColumnPreferences
                {
                    ColumnNames = columnPreferences ?? new List<string>()
                };

                _cachedStorage.CurrentMarketWatchName = marketWatchName;
                SaveStorage();
            }
            catch (Exception ex)
            {
                ShowError("Failed to save MarketWatch with columns", ex);
                ApplicationLogger.LogException(ex);
            }
        }

        public static (string CurrentMarketWatch, List<string> Columns) GetCurrentMarketWatchWithColumns()
        {
            try
            {
                EnsureStorageInitialized();

                if (string.IsNullOrEmpty(_cachedStorage.CurrentMarketWatchName))
                    return (null, null);

                var marketWatch = _cachedStorage.MarketWatchList
                    .FirstOrDefault(mw => mw.MarketWatchName == _cachedStorage.CurrentMarketWatchName);

                return (marketWatch?.MarketWatchName, marketWatch?.ColumnPreferences?.ColumnNames);
            }
            catch
            {
                return (null, null);
            }
        }

        public static Dictionary<string, List<string>> GetAllMarketWatchesWithColumns()
        {
            try
            {
                EnsureStorageInitialized();
                return _cachedStorage.MarketWatchList
                    .ToDictionary(
                        mw => mw.MarketWatchName,
                        mw => mw.ColumnPreferences?.ColumnNames ?? new List<string>()
                    );
            }
            catch
            {
                return new Dictionary<string, List<string>>();
            }
        }
        #endregion

        #region Helper Methods
        private static void ShowError(string message, Exception ex)
        {
            MessageBox.Show($"{message}: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static StorageData LoadStorageDataSafe()
        {
            try
            {
                byte[] encryptedData = File.ReadAllBytes(CredentialsPath);
                return DecryptAndDeserialize<StorageData>(encryptedData);
            }
            catch
            {
                return new StorageData();
            }
        }

        private static void EnsureStorageInitialized()
        {
            if (_cachedStorage == null)
            {
                _cachedStorage = File.Exists(CredentialsPath)
                    ? LoadStorageDataSafe()
                    : new StorageData();
            }
        }

        private static void SaveStorage()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CredentialsPath));
                byte[] encryptedData = SerializeAndEncrypt(_cachedStorage);
                File.WriteAllBytes(CredentialsPath, encryptedData);
            }
            catch (Exception ex)
            {
                ShowError("Failed to save data", ex);
                ApplicationLogger.LogException(ex);
            }
        }

        private static byte[] SerializeAndEncrypt<T>(T data)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, data);
                return ProtectedData.Protect(memoryStream.ToArray(), null, DataProtectionScope.CurrentUser);
            }
        }

        private static T DecryptAndDeserialize<T>(byte[] encryptedData) where T : new()
        {
            try
            {
                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                var serializer = new XmlSerializer(typeof(T));
                using (var memoryStream = new MemoryStream(decryptedData))
                {
                    return (T)serializer.Deserialize(memoryStream);
                }
            }
            catch
            {
                return new T();
            }
        }

        #endregion

    }

    [Serializable, XmlRoot("StorageData")]
    public class StorageData
    {
        [XmlElement("Credentials")]
        public UserCredentials Credentials { get; set; } = new UserCredentials();

        [XmlArray("MarketWatches")]
        [XmlArrayItem("MarketWatch")]
        public List<MarketWatchInfo> MarketWatchList { get; set; } = new List<MarketWatchInfo>();

        [XmlElement("CurrentMarketWatch")]
        public string CurrentMarketWatchName { get; set; }

        [XmlArray("Alerts")]
        [XmlArrayItem("Alert")]
        public List<AlertInfo> Alerts { get; set; } = new List<AlertInfo>();
    }


    [Serializable]
    public class UserCredentials
    {
        [XmlElement("Username")]
        public string Username { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }
    }

    [Serializable]
    public class MarketWatchInfo
    {
        [XmlElement("Name")]
        public string MarketWatchName { get; set; }

        [XmlElement("Columns")]
        public ColumnPreferences ColumnPreferences { get; set; } = new ColumnPreferences();
    }

    [Serializable]
    public class ColumnPreferences
    {
        [XmlArray("Columns")]
        [XmlArrayItem("Column")]
        public List<string> ColumnNames { get; set; } = new List<string>();
    }

    [Serializable]
    public class AlertInfo
    {
        [XmlElement("id")]
        public int id { get; set; }

        [XmlElement("identifier")]
        public string identifier { get; set; }

        [XmlElement("type")]
        public string type { get; set; }

        [XmlElement("condition")]
        public string condition { get; set; }

        [XmlElement("rate")]
        public decimal rate { get; set; }

        [XmlElement("NotifyStatusBar")]
        public bool NotifyStatusBar { get; set; }

        [XmlElement("NotifyPopup")]
        public bool NotifyPopup { get; set; }

        [XmlElement("TriggerTime")]
        public DateTime? TriggerTime { get; set; }

        [XmlElement("createDate")]
        public DateTime createDate { get; set; }

        [XmlElement("IsTriggered")]
        public bool IsTriggered { get; set; } = false;
    }

}