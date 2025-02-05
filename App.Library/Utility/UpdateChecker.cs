using EduRoam.Connect.Install;
using Semver;

using System.Net.Http;
using System.Net;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Security.Policy;
using Newtonsoft.Json;
using App.Library.Models;
using System.Windows.Media;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

namespace App.Library.Utility;

public static class UpdateChecker
{
    private const string UpdateUrlBase = "{0}/windows/{1}/update.json"; // {0} has to be replaced with the base url from the settings and {1} has to be replaced with the arch
    private const string RegistryBase = @"Software\{0}"; // {0} has to be replaced with the applicationIdentifier

    public static UpdateResponseRootDto UpdateData { get; set; } = new();       
    public static bool IsUpdateAvailable { get; set; }
    public static SemVersion? MinimalSupportedVersion { get; set; } 
    public static string NewVersion { get; set; }

    // http objects
    public async static Task<bool> CheckIfUpdateAvailableAsync()
    {
        if(!IsUpdateAllowedByPolicy(Registry.CurrentUser) || !IsUpdateAllowedByPolicy(Registry.LocalMachine))
        {
            return false;
        }

        await DownloadUpdateJsonAsync();
        NewVersion = UpdateData.CurrentVersion;
        var parsedVersion = Version.Parse(UpdateData.CurrentVersion);
        var newVersion = new SemVersion(parsedVersion.Major, parsedVersion.Minor, parsedVersion.Build);

        var parsedMinimalSupportedVersion = Version.Parse(UpdateData.MinimalSupportedVersion);
        MinimalSupportedVersion = new SemVersion(parsedMinimalSupportedVersion.Major, parsedMinimalSupportedVersion.Minor, parsedMinimalSupportedVersion.Build);

        IsUpdateAvailable = SelfInstaller.DefaultInstance.CanBeUpdated(newVersion);

        return IsUpdateAvailable;
    }

    private static bool IsUpdateAllowedByPolicy(RegistryKey registryBaseKey)
    {
        var key = registryBaseKey.OpenSubKey(string.Format(RegistryBase, Settings.Settings.ApplicationIdentifier));
        if (key == null) return true;
        return !Convert.ToBoolean(key.GetValue("DisableAutoUpdate", false));
    }

    /// <summary>
    /// Downloads the correct executable from the location specified in the updateUrl response
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> DownloadUpdateAsync()
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}_{Settings.Settings.ApplicationIdentifier}.exe");


            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(UpdateData.DownloadUrl, tempPath);

                var batchFilePath = Path.Combine(Path.GetTempPath(), "update.bat");
                var batchFileContent = $@"
@echo off
timeout /t 2 /nobreak
start """" ""{tempPath}"" /install true
del ""%~f0""
";
                File.WriteAllText(batchFilePath, batchFileContent);

                Process.Start(new ProcessStartInfo
                {
                    FileName = batchFilePath,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                Environment.Exit(0);
            }

        } catch(Exception e)
        {
            // Skip for now
        }

        return true;
    }

    #region Private helper functions
    private static Uri GetUpdateUrl()
    {
        var arch = ArchitectureHelper.GetArchitecture();
        var updateUrl = string.Format(UpdateUrlBase, Settings.Settings.UpdateBaseUrl, arch);

        return new Uri(updateUrl);
    }

    private static async Task DownloadUpdateJsonAsync()
    {
        var url = GetUpdateUrl();

        try
        {
            var webClient = new WebClient();
            var response = await webClient.DownloadStringTaskAsync(url);

            var deserializedObject = JsonConvert.DeserializeObject<UpdateResponseDto>(response);
            UpdateData = deserializedObject.UpdateRoot;
        } catch(Exception e)
        {
            // maybe log this?!
        }
    }
    #endregion

}
