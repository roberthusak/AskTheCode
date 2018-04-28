using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using AskTheCode.Vsix.Highlighting;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace AskTheCode.Vsix
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideService(typeof(SHighlightService))]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MainWindow), MultiInstances = false, Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindOutput)]
    [ProvideToolWindow(typeof(ReplayWindow), MultiInstances = false, Style = VsDockStyle.Float, Window = EnvDTE.Constants.vsWindowKindMainWindow, Width = 800, Height = 450)]
    [Guid(AskTheCodePackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class AskTheCodePackage : Package
    {
        /// <summary>
        /// MainWindowPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "6aebcfee-b153-4761-809b-3562cc85e675";

        /// <summary>
        /// Initializes a new instance of the <see cref="AskTheCodePackage"/> class.
        /// </summary>
        public AskTheCodePackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            var createServiceCallback = new ServiceCreatorCallback(this.CreateService);
            (this as IServiceContainer).AddService(typeof(SHighlightService), createServiceCallback, true);
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            MainWindowCommand.Initialize(this);
            base.Initialize();
        }

        private object CreateService(IServiceContainer container, Type serviceType)
        {
            if (serviceType == typeof(SHighlightService))
            {
                return new HighlightService();
            }

            return null;
        }
    }
}
