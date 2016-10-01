namespace ITweakMyBuild
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.InteropServices;

    using ITweakMyBuild.Properties;

    using JetBrains.Annotations;

    using Microsoft.VisualStudio.Shell;

    using TomsToolbox.Desktop.Composition;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Package handles disposing!")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Product.Version, IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ToolWindow))]
    public sealed class VSPackage : Package
    {
        /// <summary>
        /// VSPackage1 GUID string.
        /// </summary>
        private const string PackageGuidString = "2f1912c8-3493-4dc1-815d-f683124de933";

        [NotNull]
        public static readonly string ConfigurationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"tom-englert.de", @"ITweakMyBuild");

        [NotNull]
        public static VSPackage Instance { get; private set; }

        [NotNull]
        public ICompositionHost CompositionHost { get; } = new CompositionHost();

        // ReSharper disable once NotAccessedField.Local
        private ToolWindowCommand _toolWindowCommand;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            Instance = this;

            CompositionHost.Container?.ComposeExportedValue(nameof(VSPackage), (IServiceProvider)this);
            CompositionHost.AddCatalog(GetType().Assembly);

            var tracer = CompositionHost.GetExportedValue<Tracer>();

            Contract.Assume(tracer != null);

            tracer.WriteLine("ITweakMyBuild " + Product.Version);

            try
            {
                Directory.CreateDirectory(ConfigurationFolder);
            }
            catch (Exception ex)
            {
                tracer.TraceError(ex.Message);
            }

            _toolWindowCommand = new ToolWindowCommand(this);

            BuildNotification.Register(CompositionHost.Container);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _toolWindowCommand?.Dispose();

            CompositionHost.Dispose();
        }
    }
}
