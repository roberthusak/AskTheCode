using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public struct SymbolName
    {
        public SymbolName(string text, int? number)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(text) || number != null, nameof(text));

            this.Text = text;
            this.Number = number;
        }

        public string Text { get; private set; }

        public int? Number { get; private set; }

        public bool IsValid
        {
            get { return (!string.IsNullOrEmpty(this.Text) || this.Number != null); }
        }

        public override string ToString()
        {
            bool textSet = !string.IsNullOrEmpty(this.Text);
            bool numberSet = (this.Number != null);

            if (textSet && numberSet)
            {
                return $"{this.Text}!{this.Number.Value}";
            }
            else if (numberSet)
            {
                return $"!{this.Number.Value}";
            }
            else if (textSet)
            {
                return this.Text;
            }
            else
            {
                // TODO: Add some descprition of the error (and put it to the resources)
                throw new InvalidOperationException();
            }
        }
    }
}
