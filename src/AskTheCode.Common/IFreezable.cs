using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    public interface IFreezable<TSelf>
        where TSelf : IFreezable<TSelf>
    {
        bool CanFreeze { get; }

        bool IsFrozen { get; }

        FrozenHandler<TSelf> Freeze();
    }

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
    }
}
