using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ViewModel
{
    // TODO: Tailor accordingly to the needs of the plugin
    public enum HighlightType
    {
        Standard,
        Dummy
    }

    public interface IIdeServices
    {
        Workspace Workspace { get; }

        Document GetOpenedDocument();

        void OpenDocument(Document document);

        void SelectText(SourceText text, TextSpan selectedSpan);

        bool TryGetSelectedText(out Document document, out TextSpan selectedSpan);

        bool TryGetCaretPosition(out Document document, out int position);

        // TODO: Create custom class type for highlights
        void HighlightText(
            SourceText text,
            IDictionary<HighlightType, IEnumerable<TextSpan>> highlights);
    }
}
