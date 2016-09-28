namespace ITweakMyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using TomsToolbox.Desktop;
    using TomsToolbox.ObservableCollections;
    using TomsToolbox.Wpf.Composition;

    [VisualCompositionExport("Shell")]
    internal class ShellViewModel : ObservableObject
    {
        private readonly ObservableCollection<PropertyViewModel> _properties = new ObservableCollection<PropertyViewModel>();
        private readonly DispatcherThrottle _applyChangesThrottle;

        public ShellViewModel()
        {
            _applyChangesThrottle = new DispatcherThrottle(ApplyChanges);
            _properties.Add(new PropertyViewModel() { Name = "CodeContractsRunCodeAnalysis", Value = "false" });
            _properties.Add(new PropertyViewModel() { Name = "CodeContractsEnableRuntimeChecking", Value = "false" });

            _properties.CollectionChanged += Properties_Changed;

            new ObservablePropertyChangeTracker<PropertyViewModel>(_properties).ItemPropertyChanged += Properties_Changed;
        }

        public ICollection<PropertyViewModel> Properties => _properties;

        private void Properties_Changed(object sender, EventArgs e)
        {
            _applyChangesThrottle.Tick();
        }

        private void ApplyChanges()
        {
        }
    }
}
