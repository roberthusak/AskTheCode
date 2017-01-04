using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    /// <summary>
    /// Represents identifier generated as an integer from a sequence.
    /// </summary>
    public interface IOrdinalId<TSelf> : IId<TSelf>
    {
        int Value { get; }
    }
}
