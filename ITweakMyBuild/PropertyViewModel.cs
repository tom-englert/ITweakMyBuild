namespace ITweakMyBuild
{
    using System.ComponentModel.DataAnnotations;

    using TomsToolbox.Desktop;

    internal class PropertyViewModel : ObservableObject
    {
        private string _name;
        private string _value;
        private string _comment;
        private bool _isEnabled;

        [Required(AllowEmptyStrings = false)]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                SetProperty(ref _name, value);
            }
        }

        [Required(AllowEmptyStrings = false)]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                SetProperty(ref _value, value);
            }
        }

        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                SetProperty(ref _comment, value);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                SetProperty(ref _isEnabled, value);
            }
        }
    }
}
