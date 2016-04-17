using System.Collections.Generic;
using AskTheCode.ViewModel;
using Microsoft.VisualStudio.Text;

namespace AskTheCode.Vsix.Highlighting
{
    internal class HighlightEventArgs
    {
        public HighlightEventArgs(ITextSnapshot snapshot, IDictionary<HighlightType, NormalizedSnapshotSpanCollection> highlights)
        {
            this.Snapshot = snapshot;
            this.Highlights = highlights;
        }

        public ITextSnapshot Snapshot { get; set; }

        public IDictionary<HighlightType, NormalizedSnapshotSpanCollection> Highlights { get; set; }
    }
}