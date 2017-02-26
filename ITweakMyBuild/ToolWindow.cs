namespace ITweakMyBuild
{
    using System.Runtime.InteropServices;

    using Microsoft.VisualStudio.Shell;

    using TomsToolbox.Desktop.Composition;

    [Guid("2346b8a0-af91-463f-b587-7965df62d47f")]
    public class ToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow"/> class.
        /// </summary>
        public ToolWindow()
            : base(null)
        {
            Caption = "ITweakMyBuild";
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            Content = VSPackage.Instance?.CompositionHost.GetExportedValue<ToolWindowControl>();
        }
    }
}
