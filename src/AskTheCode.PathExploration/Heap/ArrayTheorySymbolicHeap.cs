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
        private readonly Stack<HeapState> stateStack = new Stack<HeapState>();

        private HeapState currentState;

        public ArrayTheorySymbolicHeap(ISymbolicHeapContext context)
        {
            this.context = context;
            this.currentState = HeapState.BasicState;
        }

        private ArrayTheorySymbolicHeap(
            ISymbolicHeapContext context,
            Stack<HeapState> stateStack,
            HeapState currentState)
        {
            this.context = context;
            this.currentState = currentState;

            foreach (var state in stateStack.Reverse())
            {
                this.stateStack.Push(state);
            }
        }

        public bool CanBeSatisfiable => this.CurrentState != HeapState.ConflictState;

        private HeapState CurrentState => this.currentState;

        public ImmutableArray<BoolHandle> GetAssumptions() => this.CurrentState.GetAssumptions();

        public ISymbolicHeap Clone(ISymbolicHeapContext context)
        {
            return new ArrayTheorySymbolicHeap(context, this.stateStack, this.currentState);
        }

        public void AllocateNew(VersionedVariable result)
        {
            if (this.CanBeSatisfiable)
            {
                this.currentState = this.CurrentState.AllocateNew(result, this.context);
            }
        }

        public void AssignReference(VersionedVariable result, VersionedVariable value)
        {
            if (this.CanBeSatisfiable)
            {
                this.currentState = this.CurrentState.AssignReference(result, value, this.context);
            }
        }

        public void AssertEquality(bool areEqual, VersionedVariable left, VersionedVariable right)
        {
            if (this.CanBeSatisfiable)
            {
                this.currentState = areEqual
                        ? this.CurrentState.AssertEquality(left, right, this.context)
                        : this.CurrentState.AssertInequality(left, right, this.context);
            }
        }

        public Expression GetEqualityExpression(bool areEqual, VersionedVariable left, VersionedVariable right)
        {
            if (!this.CanBeSatisfiable)
            {
                // Doesn't matter, the path is unreachable anyway
                return ExpressionFactory.False;
            }

            (var newState, var expr) = this.CurrentState.GetEqualityExpression(areEqual, left, right, this.context);
            this.currentState = newState;

            return expr;
        }

        public void ReadField(VersionedVariable result, VersionedVariable reference, IFieldDefinition field)
        {
            if (this.CanBeSatisfiable)
            {
                this.currentState = this.CurrentState.ReadField(result, reference, field, this.context);
            }
        }

        public void WriteField(VersionedVariable reference, IFieldDefinition field, Expression value)
        {
            if (this.CanBeSatisfiable)
            {
                var newState = this.CurrentState.WriteField(reference, field, value, this.context);
            }
        }

        public void PushState()
        {
            this.stateStack.Push(this.currentState);
        }

        public void PopState(int levels = 1)
        {
            for (int i = 0; i < levels; i++)
            {
                this.currentState = this.stateStack.Pop();
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
