﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs.Heap
{
    /// <summary>
    /// Represents a field definition in a program.
    /// </summary>
    public interface IFieldDefinition
    {
        Sort Sort { get; }

        IClassDefinition ReferencedClass { get; }
    }

    public static class IFieldDefinitionExtensions
    {
        public static bool IsReference(this IFieldDefinition field) => field.ReferencedClass != null;
    }
}
