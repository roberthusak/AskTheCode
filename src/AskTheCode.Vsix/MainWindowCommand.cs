using System;
using System.ComponentModel.Design;
using System.Globalization;
using CodeContractsRevival.Runtime;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AskTheCode.Vsix
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MainWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a215294f-b14c-4ccc-849d-c9290ef0e025");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private MainWindowCommand(Package package)
        {
            Contract.Requires<ArgumentNullException>(package != null, nameof(package));

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.ShowToolWindows, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MainWindowCommand Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get { return this.package; }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new MainWindowCommand(package);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ShowToolWindows(object sender, EventArgs e)
        {
            var mainWindow = this.ShowToolWindow<MainWindow>();

            var replayWindow = this.ShowToolWindow<ReplayWindow>();
            replayWindow.Content.DataContext = mainWindow.ViewModel.Replay;

            var callGraphWindow = this.ShowToolWindow<CallGraphWindow>();
            callGraphWindow.Content.DataContext = mainWindow.ViewModel;

            var traceWindow = this.ShowToolWindow<TraceWindow>();
            traceWindow.Content.DataContext = mainWindow.ViewModel;
        }

        private TWindow ShowToolWindow<TWindow>()
            where TWindow : ToolWindowPane
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created
            var window = (TWindow)this.package.FindToolWindow(typeof(TWindow), 0, true);
            var frame = window?.Frame as IVsWindowFrame;
            if (window == null || frame == null)
            {
                throw new NotSupportedException($"Cannot create {typeof(TWindow).Name}");
            }

            // Show the window
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.Show());

            return window;
        }
    }
}
