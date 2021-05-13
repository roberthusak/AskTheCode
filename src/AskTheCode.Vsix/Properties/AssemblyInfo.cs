using Microsoft.VisualStudio.Shell;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Reactive.Core",
    PublicKeyToken = "94bc3704cddfc263",
    Culture = "neutral",
    OldVersionLowerBound = "0.0.0.0",
    OldVersionUpperBound = "3.0.3000.0",
    NewVersion = "3.0.3000.0")]

[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Reactive.Interfaces",
    PublicKeyToken = "94bc3704cddfc263",
    Culture = "neutral",
    OldVersionLowerBound = "0.0.0.0",
    OldVersionUpperBound = "3.0.1000.0",
    NewVersion = "3.0.1000.0")]