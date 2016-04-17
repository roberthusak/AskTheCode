using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ViewModel;
using Microsoft.VisualStudio.Text;

namespace AskTheCode.Vsix.Highlighting
{
    internal class HighlightService : SHighlightService, IHighlightService
    {
        public event EventHandler<HighlightEventArgs> HighlightChanged;

        public void HighlightText(ITextSnapshot snapshot, IDictionary<HighlightType, NormalizedSnapshotSpanCollection> highlights)
        {
            var args = new HighlightEventArgs(snapshot, highlights);
            this.HighlightChanged?.Invoke(this, args);
        }
    }
}
