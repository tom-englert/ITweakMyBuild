namespace ITweakMyBuild
{
    using System;
    using System.ComponentModel.Design;

    using JetBrains.Annotations;

    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ToolWindowCommand : IDisposable
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        private const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        private static readonly Guid _commandSet = new Guid("568b795b-e5a9-4312-8641-c1fea2450cb2");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        [NotNull]
        private readonly VSPackage _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public ToolWindowCommand([NotNull] VSPackage package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            _package = package;

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (commandService == null)
                return;

            var menuCommandID = new CommandID(_commandSet, CommandId);
            var menuItem = new MenuCommand(ShowToolWindow, menuCommandID);

            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        [NotNull]
        private IServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            _package.ShowToolWindow();
        }

        public void Dispose()
        {
        }
    }
}
