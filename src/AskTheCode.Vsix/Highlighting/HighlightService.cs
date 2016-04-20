using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ViewModel;
using Microsoft.VisualStudio.Text;

namespace AskTheCode.Vsix.Highlighting
{
    // TODO: Check the mechanism for one file opened in two windows
    internal class HighlightService : SHighlightService, IHighlightService
    {
        private Dictionary<ITextBuffer, List<HighlightTagger>> bufferToTaggersMap =
            new Dictionary<ITextBuffer, List<HighlightTagger>>();

        private Dictionary<ITextBuffer, List<HighlightEventArgs>> bufferToPendingEventsMap =
            new Dictionary<ITextBuffer, List<HighlightEventArgs>>();

        public event EventHandler<HighlightEventArgs> HighlightChanged;

        public void HighlightText(
            ITextSnapshot snapshot,
            IDictionary<HighlightType, NormalizedSnapshotSpanCollection> highlights)
        {
            List<HighlightTagger> taggers;
            if (this.bufferToTaggersMap.TryGetValue(snapshot.TextBuffer, out taggers))
            {
                foreach (var tagger in taggers)
                {
                    tagger.HighlightText(snapshot, highlights);
                }
            }
            else
            {
                var newPendingEvent = new HighlightEventArgs(snapshot, highlights);
                List<HighlightEventArgs> pendingEvents;
                if (this.bufferToPendingEventsMap.TryGetValue(snapshot.TextBuffer, out pendingEvents))
                {
                    pendingEvents.Add(newPendingEvent);
                }
                else
                {
                    pendingEvents = new List<HighlightEventArgs>() { newPendingEvent };
                    this.bufferToPendingEventsMap.Add(snapshot.TextBuffer, pendingEvents);
                }
            }
        }

        // TODO: Consider reworking this mechanism to be built upon events
        public void RegisterTagger(HighlightTagger tagger)
        {
            List<HighlightTagger> taggers;
            if (this.bufferToTaggersMap.TryGetValue(tagger.Buffer, out taggers))
            {
                taggers.Add(tagger);
            }
            else
            {
                taggers = new List<HighlightTagger>() { tagger };
                this.bufferToTaggersMap.Add(tagger.Buffer, taggers);
            }

            List<HighlightEventArgs> pendingEvents;
            if (this.bufferToPendingEventsMap.TryGetValue(tagger.Buffer, out pendingEvents))
            {
                foreach (var pendingEvent in pendingEvents)
                {
                    tagger.HighlightText(pendingEvent.Snapshot, pendingEvent.Highlights);
                }

                this.bufferToPendingEventsMap.Remove(tagger.Buffer);
            }
        }

        public void UnregisterTagger(HighlightTagger tagger)
        {
            this.bufferToTaggersMap[tagger.Buffer].Remove(tagger);
        }
    }
}
