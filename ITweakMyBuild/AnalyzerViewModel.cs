namespace ITweakMyBuild
{
    using System.ComponentModel.DataAnnotations;

    using PropertyChanged;

    using TomsToolbox.Desktop;

    [ImplementPropertyChanged]
    internal class AnalyzerViewModel : ObservableObject
    {
        [Required(AllowEmptyStrings = false)]
        public string Path { get; set; }

        public bool IsEnabled { get; set; }
    }
}