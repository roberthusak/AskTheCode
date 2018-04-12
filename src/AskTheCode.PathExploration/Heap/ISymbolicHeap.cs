using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.TypeSystem;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.PathExploration.Heap
{
    public interface ISymbolicHeap
    {
        bool CanBeSatisfiable { get; }

        ImmutableArray<BoolHandle> GetAssumptions();

        ISymbolicHeap Clone(ISymbolicHeapContext context);

        void AllocateNew(VersionedVariable result);

        void AssertEquality(bool areEqual, VersionedVariable left, VersionedVariable right);

        Expression GetEqualityExpression(bool areEqual, VersionedVariable left, VersionedVariable right);

        void ReadField(VersionedVariable result, VersionedVariable reference, IFieldDefinition field);

        void WriteField(VersionedVariable reference, IFieldDefinition field, Expression value);

        void Retract(int operationCount = 1);

        IHeapModelRecorder GetModelRecorder(IModel smtModel);
    }

    public interface ISymbolicHeapFactory
    {
        ISymbolicHeap Create(ISymbolicHeapContext context);
    }
}
