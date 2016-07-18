using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    public interface IReadOnlyOverlay<TId, TReferenced, TValue>
        where TId : IId<TId>
        where TReferenced : IIdReferenced<TId>
    {
        TValue this[TId id] { get; }

        TValue this[TReferenced referenced] { get; }
    }
}
