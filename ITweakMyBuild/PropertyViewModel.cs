namespace ITweakMyBuild
{
    using System.ComponentModel.DataAnnotations;

    using TomsToolbox.Desktop;

    internal class PropertyViewModel : ObservableObject
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        public string Value { get; set; }

        public string Comment { get; set; }

        public bool IsEnabled { get; set; }
    }
}
