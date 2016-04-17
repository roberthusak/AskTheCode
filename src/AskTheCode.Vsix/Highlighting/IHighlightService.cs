using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AskTheCode.ViewModel;
using Microsoft.VisualStudio.Text;

namespace AskTheCode.Vsix.Highlighting
{
    internal interface IHighlightService
    {
        event EventHandler<HighlightEventArgs> HighlightChanged;

        void HighlightText(
            ITextSnapshot snapshot,
            IDictionary<HighlightType, NormalizedSnapshotSpanCollection> highlights);
    }

    [Guid("6F0C66C1-487E-4534-8926-6BB88AE84694")]
    internal interface SHighlightService
    {
    }
}