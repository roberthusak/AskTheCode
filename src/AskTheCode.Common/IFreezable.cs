using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    /// <summary>
    /// An interface for objects adhering to freezable pattern.
    /// </summary>
    public interface IFreezable<TSelf>
        where TSelf : IFreezable<TSelf>
    {
        bool CanFreeze { get; }

        bool IsFrozen { get; }

        FrozenHandler<TSelf> Freeze();
    }

    /// <summary>
    /// Reference to an object that guarantees it is frozen.
    /// </summary>
    public struct FrozenHandler<TFreezable>
        where TFreezable : IFreezable<TFreezable>
    {
        public FrozenHandler(TFreezable value)
        {
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));
            Contract.Requires<ArgumentException>(value.IsFrozen, nameof(value));

            this.Value = value;
        }

        public TFreezable Value { get; }

        public static implicit operator TFreezable(FrozenHandler<TFreezable> handler)
        {
            return handler.Value;
        }

        public static explicit operator FrozenHandler<TFreezable>(TFreezable value)
        {
            return new FrozenHandler<TFreezable>(value);
        }
    }

    /// <summary>
    /// Exception thrown in the case of modification of a frozen object.
    /// </summary>
    [Serializable]
    public class FrozenObjectModificationException : InvalidOperationException
    {
        public FrozenObjectModificationException() { }

        public FrozenObjectModificationException(string message)
            : base(message)
        {
        }

        public FrozenObjectModificationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected FrozenObjectModificationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
