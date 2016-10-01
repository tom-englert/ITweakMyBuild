namespace ITweakMyBuild
{
    using System.ComponentModel.Composition;
    using System.Windows;

    using TomsToolbox.Core;
    using TomsToolbox.Wpf.Styles;

    [Export(typeof(IThemeResourceProvider))]
    internal class ThemeResourceProvider : IThemeResourceProvider, DataGridExtensions.IContentFilter
    {
        public void LoadThemeResources(ResourceDictionary resource)
        {
            resource?.MergedDictionaries.Insert(0, new ResourceDictionary { Source = GetType().Assembly.GeneratePackUri("Resources/VSColorScheme.xaml") });
        }

        bool DataGridExtensions.IContentFilter.IsMatch(object value)
        {
            // Not exposed, just to have a hard reference to DGX
            throw new System.NotImplementedException();
        }
    }
}
