namespace ITweakMyBuild
{
    using TomsToolbox.Wpf.Composition;

    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    [DataTemplate(typeof(ShellViewModel))]
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();
        }
    }
}
