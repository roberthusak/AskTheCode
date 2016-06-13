using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    public interface IOrdinalId<TSelf> : IId<TSelf>
    {
        int Value { get; }
    }
}
