namespace ITweakMyBuild
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.InteropServices;

    using DataGridExtensions;

    using ITweakMyBuild.Properties;

    using JetBrains.Annotations;

    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using TomsToolbox.Desktop.Composition;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Package handles disposing!")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Product.Version, IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ToolWindow))]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    public sealed class VSPackage : Package, IContentFilter
    {
        /// <summary>
        /// VSPackage1 GUID string.
        /// </summary>
        private const string PackageGuidString = "2f1912c8-3493-4dc1-815d-f683124de933";

        [NotNull]
        // ReSharper disable once AssignNullToNotNullAttribute
        public static readonly string ConfigurationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"tom-englert.de", @"ITweakMyBuild");

        public static VSPackage Instance { get; private set; }

        [NotNull]
        public ICompositionHost CompositionHost { get; } = new CompositionHost();

        public void ShowToolWindow()
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = (ToolWindow)FindToolWindow(typeof(ToolWindow), 0, true);

            if (window?.Frame == null)
                throw new NotSupportedException("Cannot create tool window");

            var windowFrame = (IVsWindowFrame)window.Frame;

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


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

            CompositionHost.Container.ComposeExportedValue(nameof(VSPackage), (IServiceProvider)this);
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

            if (!Setup.IsComplete)
            {
                Setup.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _toolWindowCommand?.Dispose();

            CompositionHost.Dispose();
        }

        bool IContentFilter.IsMatch(object value)
        {
            // not used, just to add some hard coded refrence to dgx.
            return false;
        }
    }
}
