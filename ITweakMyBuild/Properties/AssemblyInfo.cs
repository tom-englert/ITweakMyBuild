using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

using ITweakMyBuild.Properties;

[assembly: AssemblyTitle("ITweakMyBuild")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("tom-englert.de")]
[assembly: AssemblyProduct("ITweakMyBuild")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion(Product.Version)]
[assembly: AssemblyFileVersion(Product.Version)]

namespace ITweakMyBuild.Properties
{
    internal static class Product
    {
        public const string Version = "1.0.0.0";
    }
}
