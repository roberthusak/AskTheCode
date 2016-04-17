using AskTheCode.ViewModel;
using Microsoft.VisualStudio.Text.Tagging;

namespace AskTheCode.Vsix.Highlighting
{
    internal class HighlightTag : TextMarkerTag
    {
        public HighlightTag(HighlightType type)
            : base(GetFormatNameFromType(type))
        {
        }

        private static string GetFormatNameFromType(HighlightType type)
        {
            // TODO: Extend together with the types
            switch (type)
            {
                case HighlightType.Standard:
                    return "blue";
                case HighlightType.Dummy:
                default:
                    return "green";
            }
        }
    }
}