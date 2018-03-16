using System;
using System.Collections.Generic;
using System.Linq;
using AskTheCode.ViewModel;
using CodeContractsRevival.Runtime;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace AskTheCode.Vsix.Highlighting
{
    internal class HighlightTagger : ITagger<HighlightTag>, IDisposable
    {
        private ITextBuffer buffer;
        private IHighlightService highlightService;
        private ITextView textView;

        public HighlightTagger(ITextView textView, ITextBuffer buffer, IHighlightService highlightService)
        {
            this.textView = textView;
            this.Buffer = buffer;
            this.highlightService = highlightService;

            var textDocument = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));

            this.highlightService.RegisterTagger(this);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public ITextBuffer Buffer { get; private set; }

        private IDictionary<HighlightType, NormalizedSnapshotSpanCollection> Highlights { get; set; }

        public IEnumerable<ITagSpan<HighlightTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            // Get a local copy for the case of multiple threads
            var highlights = this.Highlights;

            if (highlights == null || highlights.Count == 0 || highlights.First().Value.Count == 0)
            {
                yield break;
            }

            var ourSnapshot = highlights.First().Value[0].Snapshot;
            var targetSnapshot = spans[0].Snapshot;
            var transformToSnapshot = (targetSnapshot != ourSnapshot) ? targetSnapshot : null;

            foreach (var highlight in highlights)
            {
                var type = highlight.Key;
                foreach (var span in highlight.Value)
                {
                    SnapshotSpan resultSpan;
                    if (transformToSnapshot == null)
                    {
                        resultSpan = span;
                    }
                    else
                    {
                        resultSpan = span.TranslateTo(targetSnapshot, SpanTrackingMode.EdgeExclusive);
                    }

                    // TODO: Create it every time or is it enough to do it once for every type?
                    var tag = new HighlightTag(type);

                    yield return new TagSpan<HighlightTag>(resultSpan, tag);
                }
            }
        }

        public void Dispose()
        {
            this.highlightService.UnregisterTagger(this);
        }

        public void HighlightText(ITextSnapshot snapshot, IDictionary<HighlightType, NormalizedSnapshotSpanCollection> highlights)
        {
            Contract.Requires(snapshot.TextBuffer == this.Buffer);
            Contract.Requires<ArgumentNullException>(highlights != null, nameof(highlights));

            this.Highlights = highlights;

            // TODO: Return only the smallest span that contains all the spans in the collection
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);

            this.TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(snapshotSpan));
        }
    }
}