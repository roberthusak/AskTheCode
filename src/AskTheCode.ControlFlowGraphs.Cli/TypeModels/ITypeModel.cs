using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public interface ITypeModel
    {
        /// <summary>
        /// Gets a value indicating whether another model can be assigned to it.
        /// </summary>
        bool IsLValue { get; }

        IReadOnlyList<Variable> AssignmentLeft { get; }

        IReadOnlyList<Expression> AssignmentRight { get; }

        ITypeSymbol Type { get; }

        ITypeModelFactory Factory { get; }
    }

    [Serializable]
    public class InvalidAssignmentModelException : InvalidOperationException
    {
        public InvalidAssignmentModelException()
        {
        }

        public InvalidAssignmentModelException(string message)
            : base(message)
        {
        }

        public InvalidAssignmentModelException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidAssignmentModelException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
