using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ViewModel
{
    public enum HighlightType
    {
        Standard
    }

    public interface IIdeServices
    {
        Workspace Workspace { get; }

        void HighlightText(SourceText text, IDictionary<HighlightType, IEnumerable<TextSpan>> highlights);

        void SelectText(SourceText text, TextSpan selectedSpan);

        bool TryGetSelectedText(out Document document, out TextSpan selectedSpan);

        bool TryGetCaretPosition(out Document document, out LinePosition position);
    }
}
