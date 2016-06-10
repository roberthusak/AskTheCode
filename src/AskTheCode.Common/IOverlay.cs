using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    public interface IOverlay<TId, TReferenced, TValue>
        where TId : IId<TId>
        where TReferenced : IIdReferenced<TId>
    {
        TValue this[TId id] { get; set; }

        TValue this[TReferenced referenced] { get; set; }
    }
}
