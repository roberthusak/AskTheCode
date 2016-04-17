using System;
using System.Collections.Generic;
using System.Linq;
using AskTheCode.ViewModel;
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
            this.buffer = buffer;
            this.highlightService = highlightService;

            this.highlightService.HighlightChanged += this.HighlightChanged;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

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
            this.highlightService.HighlightChanged -= this.HighlightChanged;
        }

        private void HighlightChanged(object sender, HighlightEventArgs e)
        {
            if (e.Snapshot.TextBuffer != this.buffer)
            {
                return;
            }

            this.Highlights = e.Highlights;

            // TODO: Return only the smallest span that contains all the spans in the collection
            var snapshotSpan = new SnapshotSpan(e.Snapshot, 0, e.Snapshot.Length);

            this.TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(snapshotSpan));
        }
    }
}