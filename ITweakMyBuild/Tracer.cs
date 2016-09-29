namespace ITweakMyBuild
{
    using System;
    using System.ComponentModel.Composition;

    using JetBrains.Annotations;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    [Export]
    public class Tracer
    {
        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public Tracer([NotNull] [Import(nameof(VSPackage))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private void LogMessageToOutputWindow(string value)
        {
            var outputWindow = _serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null)
                return;

            var outputPaneGuid = new Guid("322115B2-FE25-472F-A76D-80460DCDFE07");

            IVsOutputWindowPane pane;
            var errorCode = outputWindow.GetPane(ref outputPaneGuid, out pane);

            if (ErrorHandler.Failed(errorCode) || pane == null)
            {
                outputWindow.CreatePane(ref outputPaneGuid, "ITweakMyBuild", Convert.ToInt32(true), Convert.ToInt32(false));
                outputWindow.GetPane(ref outputPaneGuid, out pane);
            }

            pane?.OutputString(value);
        }

        public void TraceError(string value)
        {
            WriteLine(string.Concat("Error", @" ", value));
        }

        public void TraceWarning(string value)
        {
            WriteLine(string.Concat("Warning", @" ", value));
        }

        public void WriteLine(string value)
        {
            LogMessageToOutputWindow(value + Environment.NewLine);
        }
    }
}
