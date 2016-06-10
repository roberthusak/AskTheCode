using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    public interface IIdReferenced<TId>
        where TId : IId<TId>
    {
        TId Id { get; }
    }

    public interface IId<TSelf> : IEquatable<TSelf>
    {
    }
}
