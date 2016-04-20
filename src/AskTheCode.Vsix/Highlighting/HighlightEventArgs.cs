using System.Collections.Generic;
using AskTheCode.ViewModel;
using Microsoft.VisualStudio.Text;

namespace AskTheCode.Vsix.Highlighting
{
    // TODO: Rename this class if the text highlighting system doesn't revert back to events
    internal class HighlightEventArgs
    {
        public HighlightEventArgs(
            ITextSnapshot snapshot,
            IDictionary<HighlightType, NormalizedSnapshotSpanCollection> highlights)
        {
            this.Snapshot = snapshot;
            this.Highlights = highlights;
        }

        public ITextSnapshot Snapshot { get; set; }

        public IDictionary<HighlightType, NormalizedSnapshotSpanCollection> Highlights { get; set; }
    }
}