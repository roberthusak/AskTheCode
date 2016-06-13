using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    // TODO: Reconsider whether is the equality comparison really necessary
    public interface IId<TSelf> : IEquatable<TSelf>
    {
        bool IsValid { get; }
    }

    public interface IIdProvider<TId>
        where TId : IId<TId>
    {
        TId GenerateNewId();
    }

    public interface IIdReferenced<TId>
        where TId : IId<TId>
    {
        TId Id { get; }
    }
}
