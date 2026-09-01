using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using WixSharp;

namespace App.MsiCreator.Commands
{
    internal class CreateMsi : ICommand
    {
        public static readonly string CommandName = "create";

        public static readonly string CommandDescription = "Create Msi (.msi) package";

        public Command GetCommand()
        {
            var installerTemplateOption = Options.GetInstallerTemplateOption();
            var exePathOption = Options.GetExePath();

            var command = new Command(CommandName, CommandDescription)
            {
                installerTemplateOption,
                exePathOption
            };

            command.SetHandler((FileInfo installerTemplatePath, FileInfo exePath) =>
            {
                var installerTemplateStr = System.IO.File.ReadAllText(installerTemplatePath.FullName);
                var installerTemplate = Newtonsoft.Json.JsonConvert.DeserializeObject<MsiTemplate>(installerTemplateStr);
                if (installerTemplate != null)
                {
                    Create(installerTemplate, exePath);
                }

                Console.WriteLine(".msi created");
            }, installerTemplateOption, exePathOption);

            return command;
        }

        private static ushort GetPeMachineType(string path)
        {
            try
            {
                using var fileStream = System.IO.File.OpenRead(path);
                var buffer = new byte[8];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, 2);
                if (BitConverter.ToUInt16(buffer, 0) != 0x5A4D) return 0;
                fileStream.Seek(0x3c, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, 4);
                var pePointer = BitConverter.ToInt32(buffer, 0);
                fileStream.Seek(pePointer, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, 8);
                var signature = BitConverter.ToUInt32(buffer, 0);
                var machineType = BitConverter.ToUInt16(buffer, 4);

                return signature == 0x00004550 ? machineType : (ushort)0;
            }
            catch
            {
                return 0;
            }
        }

        internal static void Create(MsiTemplate appTemplate, FileInfo exePath)
        {
            var machineType = GetPeMachineType(exePath.FullName);
            var isArm64 = machineType == 0xAA64;
            var isX64 = machineType == 0x8664;

            var targetPlatform = isArm64 ? Platform.arm64 : (isX64 ? Platform.x64 : Platform.x86);
            var programFilesRoot = (isArm64 || isX64) ? "%ProgramFiles64Folder%" : "%ProgramFiles%";

            var project = new Project(appTemplate.AppTitle,
                          new Dir($@"{programFilesRoot}\{appTemplate.ProgramFolder}",
                              new WixSharp.File(exePath.FullName)))
            {
                GUID = appTemplate.InstallerId,
                UI = WUI.WixUI_ProgressOnly,
                Version = new Version(AssemblyName.GetAssemblyName(exePath.FullName).Version?.ToString() ?? "1.0.0.0"),
                Platform = targetPlatform,
                InstallerVersion = isArm64 ? 500 : 200
            };

            if (isArm64)
            {
                Compiler.WixOptions = (Compiler.WixOptions ?? "") + " -arch arm64";
            }
            else if (isX64)
            {
                Compiler.WixOptions = (Compiler.WixOptions ?? "") + " -arch x64";
            }

            if (!string.IsNullOrEmpty(appTemplate.AppIconPath))
            {
                var appIconFileInfo = new FileInfo(appTemplate.AppIconPath);
                if (appIconFileInfo.Directory != null && appIconFileInfo.Directory.FullName != Directory.GetCurrentDirectory())
                {
                    appIconFileInfo = appIconFileInfo.CopyTo(Path.Combine(Directory.GetCurrentDirectory(), appIconFileInfo.Name), true);
                }
                project.ControlPanelInfo.ProductIcon = appIconFileInfo.Name;
            }

            project.ControlPanelInfo.Manufacturer = appTemplate.Manufacturer;
            project.ControlPanelInfo.NoModify = true;

            var msi = Compiler.BuildMsi(project);

            if (msi == null)
            {
                Debug.WriteLine("Could not create .msi");
            }
            else
            {
                Debug.WriteLine($".msi created ({msi})");
            }
        }
    }
}
