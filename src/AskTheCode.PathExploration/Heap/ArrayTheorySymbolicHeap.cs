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
    /// <summary>
    /// Implements symbolic heap operation using the theory of arrays.
    /// </summary>
    public partial class ArrayTheorySymbolicHeap : ISymbolicHeap
    {
        private readonly ISymbolicHeapContext context;
        private readonly Stack<HeapState> stateStack = new Stack<HeapState>();

        private HeapState.Builder currentState;

        public ArrayTheorySymbolicHeap(ISymbolicHeapContext context)
        {
            this.context = context;
            this.currentState = HeapState.BasicState.ToBuilder();
        }

        private ArrayTheorySymbolicHeap(
            ISymbolicHeapContext context,
            Stack<HeapState> stateStack,
            HeapState currentState)
        {
            this.context = context;
            this.currentState = currentState.ToBuilder();

            foreach (var state in stateStack.Reverse())
            {
                this.stateStack.Push(state);
            }
        }

        public bool CanBeSatisfiable => !this.CurrentState.IsConflicting;

        private HeapState.Builder CurrentState => this.currentState;

        public ImmutableArray<BoolHandle> GetAssumptions() => this.CurrentState.GetAssumptions();

        public ISymbolicHeap Clone(ISymbolicHeapContext context)
        {
            return new ArrayTheorySymbolicHeap(context, this.stateStack, this.currentState.ToState());
        }

        public void AllocateNew(VersionedVariable result)
        {
            if (this.CanBeSatisfiable)
            {
                this.CurrentState.AllocateNew(result, this.context);
            }
        }

        public void AssignReference(VersionedVariable result, VersionedVariable value)
        {
            if (this.CanBeSatisfiable)
            {
                this.CurrentState.AssignReference(result, value, this.context);
            }
        }

        public void AssertEquality(bool areEqual, VersionedVariable left, VersionedVariable right)
        {
            if (this.CanBeSatisfiable)
            {
                if (areEqual)
                {
                    this.CurrentState.AssertEquality(left, right, this.context);
                }
                else
                {
                    this.CurrentState.AssertInequality(left, right, this.context);
                } 
            }
        }

        public Expression GetEqualityExpression(bool areEqual, VersionedVariable left, VersionedVariable right)
        {
            if (!this.CanBeSatisfiable)
            {
                // Doesn't matter, the path is unreachable anyway
                return ExpressionFactory.False;
            }

            var expr = this.CurrentState.GetEqualityExpression(areEqual, left, right, this.context);

            return expr;
        }

        public void ReadField(VersionedVariable result, VersionedVariable reference, IFieldDefinition field)
        {
            if (this.CanBeSatisfiable)
            {
                this.CurrentState.ReadField(result, reference, field, this.context);
            }
        }

        public void WriteField(VersionedVariable reference, IFieldDefinition field, Expression value)
        {
            if (this.CanBeSatisfiable)
            {
                this.CurrentState.WriteField(reference, field, value, this.context);
            }
        }

        public void PushState()
        {
            this.stateStack.Push(this.currentState.ToState());
        }

        public void PopState(int levels = 1)
        {
            for (int i = 0; i < levels; i++)
            {
                this.currentState = this.stateStack.Pop().ToBuilder();
            }
        }

        public IHeapModelRecorder GetModelRecorder(IModel smtModel)
        {
            return new ModelRecorder(smtModel, this.CurrentState.ToState());
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
