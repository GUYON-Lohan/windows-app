using App.Library;
using App.Library.Utility;

using Microsoft.Extensions.DependencyInjection;

using System.Windows;

using LanguageResources = EduRoam.Localization.Resources;

namespace Eduroam.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            LanguageResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;

            if (CommandLineArgumentsHandler.PreGuiCommandLineArgs(e.Args))
            {
                this.Shutdown(1);
                return;
            }

            #region SelfInstaller AutoInstall
            var resultObject = AutoInstaller.CheckIfInstalled();
            if (!resultObject)
            {
                AutoInstaller.StartApplicationFromInstallLocation();
                this.Shutdown(1);
                return;
            }
            #endregion


            this.serviceProvider = ServicesConfiguration.ConfigureServices();

            var mainWindow = this.serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}