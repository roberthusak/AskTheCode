using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ViewModel;
using AskTheCode.Vsix.Highlighting;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace AskTheCode.Vsix
{
    /// <summary>
    /// Implementation of <see cref="IIdeServices"/> for Visual Studio.
    /// </summary>
    internal sealed class VisualStudioIdeServices : IIdeServices
    {
        private readonly EnvDTE80.DTE2 dte2;
        private readonly IHighlightService highlightService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioIdeServices"/> class.
        /// </summary>
        public VisualStudioIdeServices(EnvDTE80.DTE2 dte2, IHighlightService highlightService, Workspace workspace)
        {
            Contract.Requires<ArgumentNullException>(dte2 != null, nameof(dte2));
            Contract.Requires<ArgumentNullException>(workspace != null, nameof(workspace));
            Contract.Requires<ArgumentNullException>(highlightService != null, nameof(highlightService));

            this.dte2 = dte2;
            this.highlightService = highlightService;
            this.Workspace = workspace;
        }

        /// <summary>
        /// Gets the current Roslyn workspace we are operating on.
        /// </summary>
        public Workspace Workspace { get; private set; }

        /// <summary>
        /// Queries the currently active document.
        /// </summary>
        public Document GetOpenedDocument()
        {
            var activeDteDocument = this.dte2.ActiveDocument;
            if (activeDteDocument == null)
            {
                return null;
            }

            return this.GetCodeAnalysisDocumentFromDteDocument(activeDteDocument);
        }

        /// <summary>
        /// Opens a document and display it to the user.
        /// </summary>
        public void OpenDocument(Document document)
        {
            Contract.Requires<ArgumentNullException>(document != null, nameof(document));

            this.OpenDocumentGetFrame(document);
        }

        /// <summary>
        /// Selects a text span in a document.
        /// </summary>
        public void SelectText(SourceText text, TextSpan selectedSpan)
        {
            Contract.Requires<ArgumentNullException>(text != null, nameof(text));

            var snapshot = text.FindCorrespondingEditorTextSnapshot();
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            var windowFrame = this.OpenDocumentGetFrame(document);

            var textView = VsShellUtilities.GetTextView(windowFrame);
            var lineSpan = text.Lines.GetLinePositionSpan(selectedSpan);
            textView.SetSelection(
                lineSpan.Start.Line,
                lineSpan.Start.Character,
                lineSpan.End.Line,
                lineSpan.End.Character);
        }

        /// <summary>
        /// Queries the current position of the text editor caret.
        /// </summary>
        public bool TryGetCaretPosition(out Document document, out int position)
        {
            EnvDTE.TextSelection selection;
            if (!this.TryGetDocumentAndSelection(out document, out selection))
            {
                position = -1;
                return false;
            }

            var selectionPoint = selection.AnchorPoint;

            // TODO: Replace this hack or verify it by finding the reason for the DTE-Roslyn position mismatch
            position = selectionPoint.AbsoluteCharOffset + selectionPoint.Line;
            return true;
        }

        /// <summary>
        /// Queries the current text selection.
        /// </summary>
        public bool TryGetSelectedText(out Document document, out TextSpan selectedSpan)
        {
            EnvDTE.TextSelection selection;
            if (!this.TryGetDocumentAndSelection(out document, out selection))
            {
                selectedSpan = default(TextSpan);
                return false;
            }

            var selectionPoint = selection.AnchorPoint;
            int start = selection.TopPoint.AbsoluteCharOffset;
            int end = selection.BottomPoint.AbsoluteCharOffset;
            selectedSpan = new TextSpan(start, end - start);
            return true;
        }

        /// <summary>
        /// Displays highlights at the given locations.
        /// </summary>
        public void HighlightText(
            SourceText text,
            IDictionary<HighlightType, IEnumerable<TextSpan>> highlights)
        {
            Contract.Requires<ArgumentNullException>(text != null, nameof(text));
            Contract.Requires<ArgumentNullException>(highlights != null, nameof(highlights));

            var snapshot = text.FindCorrespondingEditorTextSnapshot();
            if (snapshot == null)
            {
                return;
            }

            var vsHighlights = new Dictionary<HighlightType, NormalizedSnapshotSpanCollection>();

            foreach (var highlight in highlights)
            {
                var vsSpans = highlight.Value
                    .Select(span => new SnapshotSpan(snapshot, span.Start, span.End - span.Start));
                var vsSpanCollection = new NormalizedSnapshotSpanCollection(vsSpans);
                vsHighlights.Add(highlight.Key, vsSpanCollection);
            }

            this.highlightService.HighlightText(snapshot, vsHighlights);
        }

        private IVsWindowFrame OpenDocumentGetFrame(Document document)
        {
            string filePath = document.FilePath;
            uint itemId;
            IVsUIHierarchy hierarchy;
            IVsWindowFrame windowFrame;
            if (VsShellUtilities.IsDocumentOpen(
                    ServiceProvider.GlobalProvider,
                    filePath,
                    VSConstants.LOGVIEWID_Any,
                    out hierarchy,
                    out itemId,
                    out windowFrame))
            {
                windowFrame.Show();
            }
            else
            {
                VsShellUtilities.OpenDocument(
                    ServiceProvider.GlobalProvider,
                    filePath,
                    VSConstants.LOGVIEWID_Primary,
                    out hierarchy,
                    out itemId,
                    out windowFrame);
            }

            return windowFrame;
        }

        private Document GetCodeAnalysisDocumentFromDteDocument(EnvDTE.Document dteDocument)
        {
            var solution = this.Workspace.CurrentSolution;
            var documentId = solution.GetDocumentIdsWithFilePath(dteDocument.FullName).FirstOrDefault();
            if (documentId == null)
            {
                return null;
            }

            return solution.GetDocument(documentId);
        }

        private bool TryGetDocumentAndSelection(out Document document, out EnvDTE.TextSelection selection)
        {
            document = null;
            selection = null;

            var activeDteDocument = this.dte2.ActiveDocument;
            var dteTextDocument = activeDteDocument?.Object() as EnvDTE.TextDocument;
            if (dteTextDocument == null)
            {
                return false;
            }

            document = this.GetCodeAnalysisDocumentFromDteDocument(activeDteDocument);
            selection = dteTextDocument.Selection;
            if (selection == null || document == null)
            {
                return false;
            }

            return true;
        }
    }
}
