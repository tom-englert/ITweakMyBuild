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

    using PropertyChanged;

    using TomsToolbox.Core;
    using TomsToolbox.Desktop;
    using TomsToolbox.ObservableCollections;
    using TomsToolbox.Wpf.Composition;

    [Export]
    [VisualCompositionExport("Shell")]
    [DoNotNotify]
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

        public ICollection<AnalyzerViewModel> Analyzers => _session.Analyzers;

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
                OnPropertyChanged(nameof(Analyzers));
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
            private readonly ObservableCollection<AnalyzerViewModel> _analyzers;
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
                        new Property { Name = "RunCodeAnalysisOnThisProject", Value = "false", Comment = "Disable Code Analysis on Build." }
                    };
                }

                var properties = _settings.Properties;

                // ReSharper disable once PossibleNullReferenceException
                _properties = new ObservableCollection<PropertyViewModel>(properties.Select(item => new PropertyViewModel { Name = item.Name, Value = item.Value, Comment = item.Comment }));

                // ReSharper disable once PossibleNullReferenceException
                _properties.ForEach(item => item.IsEnabled = !string.IsNullOrEmpty(item.Name) && _customTargetsFile.Properties.ContainsKey(item.Name));

                _properties.CollectionChanged += Content_Changed;

                var propertyChangeTracker1 = new ObservablePropertyChangeTracker<PropertyViewModel>(_properties);

                propertyChangeTracker1.ItemPropertyChanged += Content_Changed;

                var analyzers = _settings.Analyzers ?? new Analyzer[0];

                _analyzers = new ObservableCollection<AnalyzerViewModel>(analyzers.Select(item => new AnalyzerViewModel { Path = item.Path }));

                _analyzers.ForEach(item => item.IsEnabled = !string.IsNullOrEmpty(item.Path) && _customTargetsFile.Analyzers.Contains(item.Path, StringComparer.OrdinalIgnoreCase));

                _analyzers.CollectionChanged += Content_Changed;

                var propertyChangeTracker2 = new ObservablePropertyChangeTracker<AnalyzerViewModel>(_analyzers);

                propertyChangeTracker2.ItemPropertyChanged += Content_Changed;
            }

            public ICollection<PropertyViewModel> Properties => _properties;

            public ICollection<AnalyzerViewModel> Analyzers => _analyzers;

            public bool HasExternalChanges => _settings.HasExternalChanges || _customTargetsFile.HasExternalChanges;

            public bool IsActive => _customTargetsFile.Properties.Any() || _customTargetsFile.Analyzers.Any();

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
            private void ApplyChanges()
            {
                _settings.Properties = _properties
                    .Where(item => !string.IsNullOrEmpty(item?.Name))
                    .Select(item => new Property { Name = item.Name, Value = item.Value, Comment = item.Comment })
                    .ToArray();

                _settings.Analyzers = _analyzers
                    .Where(item => !string.IsNullOrEmpty(item?.Path))
                    .Select(item => new Analyzer { Path = item.Path })
                    .ToArray();

                _settings.Save();

                _customTargetsFile.Properties = _properties
                    .Where(item => (item?.IsEnabled == true) && !string.IsNullOrEmpty(item.Name))
                    .Distinct(new DelegateEqualityComparer<PropertyViewModel>(item => item.Name))
                    .ToDictionary(item => item.Name, item => item.Value);

                _customTargetsFile.Analyzers = _analyzers
                    .Where(item => (item?.IsEnabled == true))
                    .Select(item => item.Path)
                    .Where(item => !string.IsNullOrEmpty(item))
                    .ToArray();
            }

            private void Content_Changed(object sender, EventArgs e)
            {
                _applyChangesThrottle.Tick();
            }
        }
    }
}
