using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using AskTheCode.Core;
using AskTheCode.ViewModel;
using AskTheCode.Vsix.Highlighting;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShellPackageAlias = Microsoft.VisualStudio.Shell.Package;

namespace AskTheCode.Vsix
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("b681790b-5809-4896-972c-6bc5b293c1ca")]
    public class MainWindow : ToolWindowPane
    {
        private readonly ControlView viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
            : base(null)
        {
            this.Caption = "AskTheCode";

            var dte2 = ShellPackageAlias.GetGlobalService(typeof(SDTE)) as EnvDTE80.DTE2;
            var highlightService = ShellPackageAlias.GetGlobalService(typeof(SHighlightService)) as IHighlightService;
            var componentModel = ShellPackageAlias.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            var workspace = componentModel?.GetService<VisualStudioWorkspace>();
            Contract.Assert(dte2 != null);
            Contract.Assert(highlightService != null);
            Contract.Assert(workspace != null);

            var ideServices = new VisualStudioIdeServices(dte2, highlightService, workspace);
            this.viewModel = new ControlView(ideServices, InspectionContextProvider.Default);

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new MainWindowControl()
            {
                DataContext = this.viewModel
            };
        }
    }
}
