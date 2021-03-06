﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    /// <summary>
    /// Provides a standard to reference objects by their identifiers.
    /// </summary>
    public interface IOverlay<TId, TReferenced, TValue>
        where TId : IId<TId>
        where TReferenced : IIdReferenced<TId>
    {
        Func<TValue> DefaultValueFactory { get; set; }

        TValue this[TId id] { get; set; }

        TValue this[TReferenced referenced] { get; set; }
    }
}
