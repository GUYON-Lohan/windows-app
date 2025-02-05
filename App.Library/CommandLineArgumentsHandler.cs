using EduRoam.Connect.Install;
using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using Microsoft.Toolkit.Uwp.Notifications;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace App.Library
{
    public static class CommandLineArgumentsHandler
    {
        /// <summary>
        /// Handles command line args not related to wpf behaviour
        /// </summary>
        /// <returns>true if startup is to be aborted</returns>
        public static bool PreGuiCommandLineArgs(string[] args)
        {
            if (args.Length == 0) {
                return false; // continue to app
            }
            if (args[0][0] != '/')
            {
                Settings.Settings.EapConfigFileLocation = args[0];
                return false; // continue to app
            }

            var force = false;
            switch (args[0].ToLowerInvariant())
            {
                case "/install":
                    {
                        InstallTask.Install();

                        if (args.Length == 2)
                        {
                            bool.TryParse(args[1], out var forceStart);
                            if (forceStart)
                            {
                                var process = new ProcessStartInfo
                                {
                                    FileName = SelfInstaller.DefaultInstance.InstallExePath
                                };

                                Process.Start(process);
                            }
                        }
                        return true; // terminate after
                    }

                case "/uninstall":
                    {
                        UninstallTask.Uninstall(_ => { Environment.Exit(0); });

                        return true; // terminate after
                    }

                case "/force-refresh":
                case "/refresh-force":
                    force = true;
                    goto case "/refresh";
                case "/refresh":
                    {
                        force |= args.Length >= 2 && string.Equals(args[1], "/force", StringComparison.OrdinalIgnoreCase);
                        Task.Run(async () => { await RefreshTask.RefreshAsync(force: force); });

                        return true; // terminate after
                    }

                case "/check-certificate":
                    {
                        var st = new StatusTask();
                        var gst = st.GetStatus();
                        var diffDate = (gst.ExpirationDate - DateTime.Now).Value.Days;

                        if (diffDate <= Settings.Settings.DaysLeftForNotification)
                        {
                            new ToastContentBuilder()
                                .AddText(string.Format(Resources.CheckCertificateToastP1, Settings.Settings.ApplicationIdentifier))
                                .AddText(string.Format(Resources.CheckCertificateToastP2, diffDate))
                                .AddButton(new ToastButton()
                                    .SetContent(Resources.CheckCertificateToastButton)
                                    .SetBackgroundActivation()
                                 )
                                .Show();
                        }

                        return true; // terminate after
                    }

                case "/close":
                case "/background":
                    return true; // Deprecated flags, just terminate

                case "/?":
                case "/help":
                default:
                    {
                        ShowHelpText();

                        return true; // terminate after
                    }
            }
        }

        private static void ShowHelpText() =>
            MessageBox.Show(
                string.Join(
                    "\n",
                    new List<string>
                    {
                        Resources.AppCommandsHelp,
                    }),
                caption: Assembly.GetEntryAssembly()!.GetName().Name);
    }
}
