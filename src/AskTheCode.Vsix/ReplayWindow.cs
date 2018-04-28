using System;
using System.Runtime.InteropServices;
using AskTheCode.ViewModel;
using AskTheCode.Vsix.Highlighting;
using AskTheCode.Wpf;
using CodeContractsRevival.Runtime;
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
    [Guid("46e7ead0-a7e2-4c54-bd0a-4ddb6976cb2e")]
    public class ReplayWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayWindow"/> class.
        /// </summary>
        public ReplayWindow()
            : base(null)
        {
            this.Caption = "AskTheCode: Replay";
            this.Content = new ReplayPanel();
        }

        public new ReplayPanel Content
        {
            get => (ReplayPanel)base.Content;
            set => base.Content = value;
        }
    }
}
