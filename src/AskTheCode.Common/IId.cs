using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    /// <summary>
    /// Represents strongly typed identifier used to reference a certain class.
    /// </summary>
    // TODO: Reconsider whether is the equality comparison really necessary
    public interface IId<TSelf> : IEquatable<TSelf>
    {
        bool IsValid { get; }
    }

    /// <summary>
    /// Strongly typed generator of a certain identifier.
    /// </summary>
    public interface IIdProvider<TId>
        where TId : IId<TId>
    {
        TId GenerateNewId();
    }

    /// <summary>
    /// Object referenced by a given identifier.
    /// </summary>
    public interface IIdReferenced<TId>
        where TId : IId<TId>
    {
        TId Id { get; }
    }
}
