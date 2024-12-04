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

namespace App.Library.Utility;

public static class UpdateChecker
{
    private const string UpdateUrlBase = "https://dl.eduroam.app/windows/{0}/update.json"; // {0} has to be replaced with the arch
    public static UpdateResponseRootDto UpdateData { get; set; } = new();       
    public static bool IsUpdateAvailable { get; set; }
    
    // http objects
    public static bool CheckIfUpdateAvailable()
    {
        DownloadUpdateJson();

        var parsedVersion = Version.Parse(UpdateData.CurrentVersion);
        var newVersion = new SemVersion(parsedVersion.Major, parsedVersion.Minor, parsedVersion.Build);

        IsUpdateAvailable = SelfInstaller.DefaultInstance.CanBeUpdated(newVersion);

        return IsUpdateAvailable;
    }

    /// <summary>
    /// Downloads the correct executable from the location specified in the updateUrl response
    /// </summary>
    /// <returns></returns>
    public static bool DownloadUpdate()
    {
        return true;
    }

    #region Private helper functions
    private static string GetUpdateUrl()
    {
        var arch = ArchitectureHelper.GetArchitecture();
        var updateUrl = string.Format(UpdateUrlBase, arch);

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