namespace ITweakMyBuild
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Windows;
    using System.Windows.Controls;

    using EnvDTE;

    using JetBrains.Annotations;

    using Microsoft.VisualStudio.Shell.Interop;

    [Export]
    internal sealed class BuildNotification : IDisposable
    {
        [NotNull]
        private readonly IServiceProvider _serviceProvider;
        [NotNull]
        private readonly ShellViewModel _shellViewModel;

        private BuildEvents _buildEvents;
        private FrameworkElement _notificationControl;

        [ImportingConstructor]
        public BuildNotification([Import(nameof(VSPackage))] [NotNull] IServiceProvider serviceProvider, [NotNull] ShellViewModel shellViewModel)
        {
            _serviceProvider = serviceProvider;
            _shellViewModel = shellViewModel;

            var events = (EnvDTE80.Events2)Dte.Events;

            _buildEvents = events?.BuildEvents;

            if (_buildEvents == null)
                return;

            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildDone += BuildEvents_OnBuildDone;
        }

        public static void Register([NotNull] ExportProvider exportProvider)
        {
            exportProvider.GetExportedValue<BuildNotification>();
        }

        private void BuildEvents_OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            if (_notificationControl != null)
                _notificationControl.Visibility = Visibility.Collapsed;
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            if (_notificationControl == null)
                _notificationControl = CreateNotificationControl();

            if (_notificationControl == null)
                return;

           _notificationControl.Visibility = Visibility.Visible;
        }

        private FrameworkElement CreateNotificationControl()
        {
            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow == null)
                return null;

            var frameControlContainer = (FrameworkElement)mainWindow.Template?.FindName(@"PART_TitleBarFrameControlContainer", mainWindow);
            var dockPanelChildren = (frameControlContainer?.Parent as DockPanel)?.Children;

            if (dockPanelChildren == null)
                return null;

            var index = dockPanelChildren.IndexOf(frameControlContainer);

            var control = new BuildNotificationControl(_shellViewModel);

            dockPanelChildren.Insert(index + 1, control);

            return control;
        }

        public void Dispose()
        {
            if (_buildEvents == null)
                return;

            _buildEvents.OnBuildBegin -= BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildDone -= BuildEvents_OnBuildDone;
            _buildEvents = null;
        }

        [NotNull]
        // ReSharper disable once AssignNullToNotNullAttribute
        private EnvDTE80.DTE2 Dte => (EnvDTE80.DTE2)_serviceProvider.GetService(typeof(SDTE));
    }
}
