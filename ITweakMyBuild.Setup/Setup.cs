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
        private const string _title = "ITweakMyBuild";

        private const string ImportAfterTargetsFileName = "tom-englert.de.ITweakMyBuild.ImportAfter.targets";

        [NotNull]
        private static readonly string[] _msBuildVersions = { "4.0", "12.0", "14.0", "15.0" };

        public static bool IsComplete => _msBuildVersions.Select(GetImportAfterTargetsFilePath).All(File.Exists);

        public static void Start()
        {
            if (MessageBox.Show("ITweakMyBuild needs administrative rights to finish setup and generate the global MSBuild targets", _title, MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
            {
                MessageBox.Show("ITweakMyBuild setup is not complete, it may not work as expected.", _title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo(typeof(Setup).Assembly.Location)
                {
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _title);
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
