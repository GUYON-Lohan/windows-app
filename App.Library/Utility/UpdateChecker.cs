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

namespace App.Library.Utility;

public static class UpdateChecker
{
    private const string UpdateUrlBase = "{0}/windows/{1}/update.json"; // {0} has to be replaced with the arch
    public static UpdateResponseRootDto UpdateData { get; set; } = new();       
    public static bool IsUpdateAvailable { get; set; }
    public static SemVersion? MinimalSupportedVersion { get; set; } 
    public static string NewVersion { get; set; }
    
    // http objects
    public static bool CheckIfUpdateAvailable()
    {
        DownloadUpdateJson();
        NewVersion = UpdateData.CurrentVersion;
        var parsedVersion = Version.Parse(UpdateData.CurrentVersion);
        var newVersion = new SemVersion(parsedVersion.Major, parsedVersion.Minor, parsedVersion.Build);

        var parsedMinimalSupportedVersion = Version.Parse(UpdateData.MinimalSupportedVersion);
        MinimalSupportedVersion = new SemVersion(parsedMinimalSupportedVersion.Major, parsedMinimalSupportedVersion.Minor, parsedMinimalSupportedVersion.Build);

        IsUpdateAvailable = SelfInstaller.DefaultInstance.CanBeUpdated(newVersion);

        return IsUpdateAvailable;
    }

    /// <summary>
    /// Downloads the correct executable from the location specified in the updateUrl response
    /// </summary>
    /// <returns></returns>
    public static bool DownloadUpdate()
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}_{Settings.Settings.ApplicationIdentifier}.exe");


            using (WebClient client = new WebClient())
            {
                client.DownloadFile(UpdateData.DownloadUrl, tempPath);

                var process = new ProcessStartInfo
                {
                    FileName = tempPath,
                    Arguments = "/install"
                };

                Process.Start(process);

                Thread.Sleep(5000);

                Process.Start(SelfInstaller.DefaultInstance.InstallExePath);

                Environment.Exit(0);
            }

        } catch(Exception e)
        {
            // Skip for now
        }

        return true;
    }

    #region Private helper functions
    private static string GetUpdateUrl()
    {
        var arch = ArchitectureHelper.GetArchitecture();
        var updateUrl = string.Format(UpdateUrlBase, Settings.Settings.UpdateBaseUrl, arch);

        return updateUrl;
    }

    private static void DownloadUpdateJson()
    {
        var url = GetUpdateUrl();

        try
        {
            var webClient = new WebClient();
            var response = webClient.DownloadString(url);
            var deserializedObject = JsonConvert.DeserializeObject<UpdateResponseDto>(response);
            UpdateData = deserializedObject.UpdateRoot;
        } catch(Exception e)
        {
            // maybe log this?!
        }
    }
    #endregion

}