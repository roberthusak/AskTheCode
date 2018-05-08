using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration.Heap
{
    public partial class ArrayTheorySymbolicHeap : ISymbolicHeap
    {
        private readonly ISymbolicHeapContext context;
        private readonly Stack<AlgorithmState> stateStack = new Stack<AlgorithmState>();

        public ArrayTheorySymbolicHeap(ISymbolicHeapContext context)
        {
            this.context = context;

            this.stateStack.Push(AlgorithmState.BasicState);
        }

        private ArrayTheorySymbolicHeap(ISymbolicHeapContext context, Stack<AlgorithmState> stateStack)
        {
            this.context = context;

            foreach (var state in stateStack.Reverse())
            {
                this.stateStack.Push(state);
            }
        }

        public bool CanBeSatisfiable => this.CurrentState != AlgorithmState.ConflictState;

        private AlgorithmState CurrentState => this.stateStack.Peek();

        public ImmutableArray<BoolHandle> GetAssumptions() => this.CurrentState.GetAssumptions();

        public ISymbolicHeap Clone(ISymbolicHeapContext context)
        {
            return new ArrayTheorySymbolicHeap(context, this.stateStack);
        }

        public void AllocateNew(VersionedVariable result, bool mightBeRepeated)
        {
            if (!this.CanBeSatisfiable)
            {
                this.stateStack.Push(AlgorithmState.ConflictState);
                return;
            }

            var newState = this.CurrentState.AllocateNew(result, this.context, mightBeRepeated);
            this.stateStack.Push(newState);
        }

        public void AssignReference(VersionedVariable result, VersionedVariable value)
        {
            if (!this.CanBeSatisfiable)
            {
                this.stateStack.Push(AlgorithmState.ConflictState);
                return;
            }

            var newState = this.CurrentState.AssignReference(result, value, this.context);
            this.stateStack.Push(newState);
        }

        public void AssertEquality(bool areEqual, VersionedVariable left, VersionedVariable right)
        {
            if (!this.CanBeSatisfiable)
            {
                this.stateStack.Push(AlgorithmState.ConflictState);
                return;
            }

            var newState = areEqual
                ? this.CurrentState.AssertEquality(left, right, this.context)
                : this.CurrentState.AssertInequality(left, right, this.context);
            this.stateStack.Push(newState);
        }

        public Expression GetEqualityExpression(bool areEqual, VersionedVariable left, VersionedVariable right)
        {
            if (!this.CanBeSatisfiable)
            {
                // Doesn't matter, the path is unreachable anyway
                return ExpressionFactory.False;
            }

            (var newState, var expr) = this.CurrentState.GetEqualityExpression(areEqual, left, right, this.context);
            this.stateStack.Push(newState);

            return expr;
        }

        public void ReadField(VersionedVariable result, VersionedVariable reference, IFieldDefinition field)
        {
            if (!this.CanBeSatisfiable)
            {
                this.stateStack.Push(AlgorithmState.ConflictState);
                return;
            }

            var newState = this.CurrentState.ReadField(result, reference, field, this.context);
            this.stateStack.Push(newState);
        }

        public void WriteField(VersionedVariable reference, IFieldDefinition field, Expression value)
        {
            if (!this.CanBeSatisfiable)
            {
                this.stateStack.Push(AlgorithmState.ConflictState);
                return;
            }

            var newState = this.CurrentState.WriteField(reference, field, value, this.context);
            this.stateStack.Push(newState);
        }

        public void Retract(int operationCount = 1)
        {
            for (int i = 0; i < operationCount; i++)
            {
                this.stateStack.Pop();
            }
        }

        public IHeapModelRecorder GetModelRecorder(IModel smtModel)
        {
            return new ModelRecorder(smtModel, this.CurrentState);
        }
    }

    public class ArrayTheorySymbolicHeapFactory : ISymbolicHeapFactory
    {
        public ISymbolicHeap Create(ISymbolicHeapContext context)
        {
            return new ArrayTheorySymbolicHeap(context);
        }
    }
}
