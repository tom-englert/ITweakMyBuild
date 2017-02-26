namespace ITweakMyBuild
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Windows;

    using ITweakMyBuild.Properties;

    using JetBrains.Annotations;

    public static class Setup
    {
        [NotNull]
        private static readonly string[] _msBuildVersions = new[] { "4.0", "12.0", "14.0", "15.0" };

        private const string ImportAfterTargetsFileName = "tom-englert.de.ITweakMyBuild.ImportAfter.targets";

        public static bool IsComplete => _msBuildVersions.Select(GetImportAfterTargetsFilePath).All(File.Exists);

        public static void Start()
        {
            if (MessageBox.Show("ITweakMyBuild needs administrative rights to finish setup and generate the global MSBuild targets", "ITweakMyBuild", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
            {
                MessageBox.Show("ITweakMyBuild setup is not complete, it may not work as expected.", "ITweakMyBuild", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var startInfo = new ProcessStartInfo(Path.ChangeExtension(typeof(Setup).Assembly.Location, ".Setup.exe"))
            {
                CreateNoWindow = true,
                UseShellExecute = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }

        public static void RunInProcess()
        {
            try
            {
                foreach (var folder in _msBuildVersions.Select(GetImportAfterFolder))
                {
                    Directory.CreateDirectory(folder);

                    // ReSharper disable once AssignNullToNotNullAttribute
                    File.WriteAllBytes(Path.Combine(folder, ImportAfterTargetsFileName), Resources.ImportAfter);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [NotNull]
        private static string GetImportAfterTargetsFilePath([NotNull] string version)
        {
            Contract.Requires(version != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return Path.Combine(GetImportAfterFolder(version), ImportAfterTargetsFileName);
        }

        [NotNull]
        private static string GetImportAfterFolder([NotNull] string version)
        {
            Contract.Requires(version != null);
            Contract.Ensures(Contract.Result<string>() != null);

            // ReSharper disable once AssignNullToNotNullAttribute
            return Path.Combine(MSBuildFolder, version, "Microsoft.Common.Targets", "ImportAfter");
        }

        [NotNull]
        private static string MSBuildFolder => Path.Combine(ProgramFilesFolder, "MSBuild");

        [NotNull]
        private static string ProgramFilesFolder
        {
            get
            {
                var value = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                return !string.IsNullOrEmpty(value) ? value : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
        }
    }
}
