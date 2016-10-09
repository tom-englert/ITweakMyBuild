namespace ITweakMyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    using JetBrains.Annotations;

    using TomsToolbox.Core;
    using TomsToolbox.Desktop;
    using TomsToolbox.ObservableCollections;
    using TomsToolbox.Wpf.Composition;

    [Export]
    [VisualCompositionExport("Shell")]
    internal class ShellViewModel : ObservableObject, IStateMonitor, IDisposable
    {
        [NotNull]
        private readonly ExportProvider _exportProvider;
        [NotNull]
        private readonly Throttle _fileChangedThrottle;
        [NotNull]
        private readonly FileSystemWatcher _fileSystemWatcher;
        [NotNull]
        private Session _session;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [ImportingConstructor]
        public ShellViewModel([NotNull] ExportProvider exportProvider)
        {
            _exportProvider = exportProvider;
            _fileChangedThrottle = new Throttle(Reload);

            _session = CreateSession();

            _fileSystemWatcher = new FileSystemWatcher(VSPackage.ConfigurationFolder) { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
            _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
        }

        public ICollection<PropertyViewModel> Properties => _session.Properties;

        public bool IsActive => _session.IsActive;

        [NotNull]
        private Session CreateSession()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return _exportProvider.GetExportedValue<Session>();
        }

        private void Reload()
        {
            if (_session.HasExternalChanges)
            {
                _session = CreateSession();
                OnPropertyChanged(nameof(Properties));
            }

            OnPropertyChanged(nameof(IsActive));
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            _fileChangedThrottle.Tick();
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }

        [Export]
        [PartCreationPolicy(CreationPolicy.NonShared)]
        private class Session
        {
            [NotNull]
            private readonly ObservableCollection<PropertyViewModel> _properties;
            [NotNull]
            private readonly DispatcherThrottle _applyChangesThrottle;
            [NotNull]
            private readonly Settings _settings;
            [NotNull]
            private readonly CustomTargetsFile _customTargetsFile;

            [ImportingConstructor]
            public Session([NotNull] Settings settings, [NotNull] CustomTargetsFile customTargetsFile)
            {
                _settings = settings;
                _customTargetsFile = customTargetsFile;

                _applyChangesThrottle = new DispatcherThrottle(ApplyChanges);

                if (_settings.Properties == null)
                {
                    _settings.Properties = new[]
                    {
                        new Property { Name = "CodeContractsRunCodeAnalysis", Value = "false", Comment = "Disable Static CodeContracts Analysis." },
                        new Property { Name = "CodeContractsEnableRuntimeChecking", Value = "false", Comment = "Disable CodeContracts Runtime Checking." },
                        new Property { Name = "RunCodeAnalysis", Value = "false", Comment = "Disable Code Analysis on Build." }
                    };
                }

                var properties = _settings.Properties;

                // ReSharper disable once PossibleNullReferenceException
                _properties = new ObservableCollection<PropertyViewModel>(properties.Select(item => new PropertyViewModel { Name = item.Name, Value = item.Value, Comment = item.Comment }));

                // ReSharper disable once PossibleNullReferenceException
                _properties.ForEach(item => item.IsEnabled = !string.IsNullOrEmpty(item.Name) && _customTargetsFile.Properties.ContainsKey(item.Name));

                _properties.CollectionChanged += Properties_Changed;

                var propertyChangeTracker = new ObservablePropertyChangeTracker<PropertyViewModel>(_properties);

                propertyChangeTracker.ItemPropertyChanged += Properties_Changed;
            }

            public ICollection<PropertyViewModel> Properties => _properties;

            public bool HasExternalChanges => _settings.HasExternalChanges || _customTargetsFile.HasExternalChanges;

            public bool IsActive => _customTargetsFile.Properties.Any();

            [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
            private void ApplyChanges()
            {
                _settings.Properties = _properties
                    .Where(item => !string.IsNullOrEmpty(item?.Name))
                    .Select(item => new Property { Name = item.Name, Value = item.Value, Comment = item.Comment })
                    .ToArray();

                _settings.Save();

                _customTargetsFile.Properties = _properties
                    .Where(item => (item?.IsEnabled == true) && !string.IsNullOrEmpty(item.Name))
                    .Distinct(new DelegateEqualityComparer<PropertyViewModel>(item => item.Name))
                    .ToDictionary(item => item.Name, item => item.Value);
            }

            private void Properties_Changed(object sender, EventArgs e)
            {
                _applyChangesThrottle.Tick();
            }
        }
    }
}
