using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace AskTheCode.Vsix.Highlighting
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(HighlightTag))]
    [ContentType("CSharp")]
    internal class HighlightTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer)
            where T : ITag
        {
            // Only provide highlighting on the top-level buffer
            if (textView.TextBuffer != buffer)
            {
                return null;
            }

            var highlightService = Package.GetGlobalService(typeof(SHighlightService)) as IHighlightService;

            return new HighlightTagger(textView, buffer, highlightService) as ITagger<T>;
        }
    }
}
