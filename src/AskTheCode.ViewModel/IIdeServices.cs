using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ViewModel
{
    /// <summary>
    /// Types of highlighting in an IDE.
    /// </summary>
    // TODO: Tailor accordingly to the needs of the plugin
    public enum HighlightType
    {
        Standard,
        Dummy
    }

    /// <summary>
    /// A collection of user interaction services provided by an IDE.
    /// </summary>
    public interface IIdeServices
    {
        /// <summary>
        /// Gets the current Roslyn workspace we are operating on.
        /// </summary>
        Workspace Workspace { get; }

        /// <summary>
        /// Queries the currently active document.
        /// </summary>
        Document GetOpenedDocument();

        /// <summary>
        /// Opens a document and display it to the user.
        /// </summary>
        void OpenDocument(Document document);

        /// <summary>
        /// Selects a text span in a document.
        /// </summary>
        void SelectText(SourceText text, TextSpan selectedSpan);

        /// <summary>
        /// Queries the current text selection.
        /// </summary>
        bool TryGetSelectedText(out Document document, out TextSpan selectedSpan);

        /// <summary>
        /// Queries the current position of the text editor caret.
        /// </summary>
        bool TryGetCaretPosition(out Document document, out int position);

        /// <summary>
        /// Displays highlights at the given locations.
        /// </summary>
        // TODO: Create custom class type for highlights
        void HighlightText(
            SourceText text,
            IDictionary<HighlightType, IEnumerable<TextSpan>> highlights);
    }
}
