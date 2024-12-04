using System.Data;

namespace App.Settings
{
    public static class Settings
    {
        public static string OAuthClientId { get; set; } = "app.geteduroam.win";
        public static string ApplicationIdentifier { get; set; } = "geteduroam";
        public static string UpdateBaseUrl { get; set; } = "https://dl.eduroam.app";
        public static int DaysLeftForNotification { get; set; } = 10;
        public static string? EapConfigFileLocation { get; set; } = null;
    }
}
