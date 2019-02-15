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
    [Guid("79A5D224-90F4-43DE-A68F-8747A714F8D3")]
    public class TraceWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceWindow"/> class.
        /// </summary>
        public TraceWindow()
            : base(null)
        {
            this.Caption = "AskTheCode: Trace Explorer";
            this.Content = new TracePanel();
        }

        public new TracePanel Content
        {
            get => (TracePanel)base.Content;
            set => base.Content = value;
        }
    }
}
