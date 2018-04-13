using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.TypeSystem;
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

        public void AllocateNew(VersionedVariable result)
        {
            if (!this.CanBeSatisfiable)
            {
                this.stateStack.Push(AlgorithmState.ConflictState);
                return;
            }

            var newState = this.CurrentState.AllocateNew(result, this.context);
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

        public class VariableState
        {
            public const int NullId = 0;
            public const int NullValue = 0;
            public static readonly VariableState Null = new VariableState(NullId, NullValue, true, NullValue);

            private VariableState(int id, IntHandle representation, bool canBeNull, int? value)
            {
                this.Id = id;
                this.Representation = representation;
                this.CanBeNull = canBeNull;
                this.Value = value;
            }

            public int Id { get; }

            public IntHandle Representation { get; }

            public bool CanBeNull { get; }

            public int? Value { get; }

            public bool IsInput => this.Value == null;

            public static VariableState CreateInput(int id, NamedVariable namedVariable, bool canBeNull)
            {
                Contract.Requires(namedVariable.Sort == Sort.Int);

                return new VariableState(id, (IntHandle)namedVariable, canBeNull, null);
            }

            public static VariableState CreateValue(int id, int value)
            {
                return new VariableState(id, value, false, value);
            }

            public VariableState WithCanBeNull(bool canBeNull)
            {
                return new VariableState(this.Id, this.Representation, canBeNull, this.Value);
            }

            public override string ToString()
            {
                if (this == Null)
                {
                    return $"[{this.Id}] NULL";
                }
                else
                {
                    string nullInfo = (this.IsInput && !this.CanBeNull) ? ", NOT NULL" : "";
                    return $"[{this.Id}] {this.Representation}{nullInfo}";
                }
            }
        }

        private class AlgorithmState
        {
            public static readonly AlgorithmState ConflictState = new AlgorithmState(null, null, null, null, -1, -1);

            public static readonly AlgorithmState BasicState = ConstructBasicState();

            private readonly ImmutableSortedDictionary<int, VariableState> variableStates;
            private readonly ImmutableDictionary<VersionedVariable, int> variableToStateIdMap;
            private readonly ImmutableDictionary<int, ImmutableList<VersionedVariable>> stateIdToVariablesMap;
            private readonly ImmutableDictionary<IFieldDefinition, ArrayHandle<IntHandle, Handle>> fieldToVariableMap;
            private readonly int nextVariableStateId;
            private readonly int nextReferenceValue;

            private AlgorithmState(
                ImmutableSortedDictionary<int, VariableState> variableStates,
                ImmutableDictionary<VersionedVariable, int> variableToStateIdMap,
                ImmutableDictionary<int, ImmutableList<VersionedVariable>> stateIdToVariablesMap,
                ImmutableDictionary<IFieldDefinition, ArrayHandle<IntHandle, Handle>> fieldToVariableMap,
                int nextVariableStateId,
                int nextReferenceValue)
            {
                this.variableStates = variableStates;
                this.variableToStateIdMap = variableToStateIdMap;
                this.stateIdToVariablesMap = stateIdToVariablesMap;
                this.fieldToVariableMap = fieldToVariableMap;
                this.nextVariableStateId = nextVariableStateId;
                this.nextReferenceValue = nextReferenceValue;
            }

            public VariableState GetVariableState(VersionedVariable variable)
            {
                if (this.variableToStateIdMap.TryGetValue(variable, out int stateId))
                {
                    return this.variableStates[stateId];
                }
                else
                {
                    return null;
                }
            }

            public ArrayHandle<IntHandle, Handle> GetFieldArray(IFieldDefinition field)
            {
                return this.fieldToVariableMap[field];
            }

            public ImmutableArray<BoolHandle> GetAssumptions()
            {
                var refFieldHandles = this.fieldToVariableMap
                    .Where(kvp => kvp.Key.IsReference())
                    .Select(kvp => (ArrayHandle<IntHandle, IntHandle>)kvp.Value.Expression)
                    .ToArray();

                if (refFieldHandles.Length == 0)
                {
                    return ImmutableArray<BoolHandle>.Empty;
                }

                return this.variableStates.Values
                    .Where(s => s.IsInput)
                    .Select((s) =>
                    {
                        // TODO: Use only the fields present in the corresponding class
                        var readChecks = refFieldHandles
                            .Select(h => (h.Select(s.Representation) <= VariableState.NullValue).Expression)
                            .ToArray();

                        var readChecksAnd = (readChecks.Length == 1)
                            ? (BoolHandle)readChecks[0]
                            : (BoolHandle)ExpressionFactory.And(readChecks);

                        if (s.CanBeNull)
                        {
                            return s.Representation == VariableState.NullValue || readChecksAnd;
                        }
                        else
                        {
                            return readChecksAnd;
                        }
                    })
                    .ToImmutableArray();
            }

            public AlgorithmState AllocateNew(VersionedVariable result, ISymbolicHeapContext context)
            {
                (var state, var newVarState) = this.MapToNewValueVariableState(result);

                var origVarState = this.GetVariableOrNull(result);
                if (origVarState != null)
                {
                    // Note that allocating the same variable twice would result in a conflict in SMT solver,
                    // eg. (assert (= 2 3))
                    context.AddAssertion(origVarState.Representation == newVarState.Representation);
                }

                return state;
            }

            public AlgorithmState AssertEquality(
                VersionedVariable left,
                VersionedVariable right,
                ISymbolicHeapContext context)
            {
                var leftState = this.GetVariableOrNull(left);
                var rightState = this.GetVariableOrNull(right);

                if (leftState == null && rightState == null)
                {
                    // From now on, we will handle them together, no need to assert their equality
                    (var algState, var newVarState) = this.MapToNewInputVariableState(left, context);
                    return algState.MapToVariableState(right, newVarState);
                }

                if (leftState == null || rightState == null)
                {
                    Contract.Assert(leftState != null || rightState != null);

                    // Add the newly added variable to the existing one; again, no assertion needed
                    return (leftState == null)
                        ? this.MapToVariableState(left, rightState)
                        : this.MapToVariableState(right, leftState);
                }

                Contract.Assert(leftState != null && rightState != null);

                if (IsNullStateAndVarState(leftState, rightState, out var varState))
                {
                    if (!varState.CanBeNull)
                    {
                        // Variable said not to be null must be null, leading to a conflict
                        return ConflictState;
                    }
                    else
                    {
                        var versionedVar = (varState == leftState) ? left : right;

                        // Variable must be null
                        context.AddAssertion(varState.Representation == VariableState.Null.Representation);
                        return this.MapToVariableState(versionedVar, VariableState.Null);
                    }
                }

                // Assert the equality of the variables and unite them from now on
                // to reduce the number of generated assumptions
                context.AddAssertion(leftState.Representation == rightState.Representation);
                return this.MapToBetterVariableState(left, leftState, right, rightState);
            }

            public AlgorithmState AssertInequality(
                VersionedVariable left,
                VersionedVariable right,
                ISymbolicHeapContext context)
            {
                var leftState = this.GetVariableOrNull(left);
                var rightState = this.GetVariableOrNull(right);

                if (leftState == rightState && leftState != null)
                {
                    // Equal initialized variables are meant to be inequal, leading to a conflict
                    return ConflictState;
                }

                if (IsNullStateAndVarState(leftState, rightState, out var varState))
                {
                    if (varState.CanBeNull)
                    {
                        // Variable can't be null
                        context.AddAssertion(varState.Representation != VariableState.Null.Representation);
                        return this.UpdateVariableState(varState.WithCanBeNull(false));
                    }
                    else
                    {
                        // No more information provided, the variable is already known not to be null
                        return this;
                    }
                }

                AlgorithmState resultState = this;

                // Initialize left variable, if needed
                if (leftState == null)
                {
                    (resultState, leftState) = resultState.MapToNewInputVariableState(left, context);
                }

                // Initialize right variable, if needed
                if (rightState == null)
                {
                    (resultState, rightState) = resultState.MapToNewInputVariableState(right, context);
                }

                if (leftState.Value == null || rightState.Value == null)
                {
                    // In the general case, assert the inequality
                    context.AddAssertion(leftState.Representation != rightState.Representation);
                }
                else
                {
                    // Two different states must be of different values, hence no need to assert their inequality
                    Contract.Assert(leftState.Value.Value != rightState.Value.Value);
                }

                return resultState;
            }

            public (AlgorithmState newState, BoolHandle result) GetEqualityExpression(
                bool areEqual,
                VersionedVariable left,
                VersionedVariable right,
                ISymbolicHeapContext context)
            {
                (var stateWithLeft, var leftState) = this.GetOrAddVariable(left, context);
                (var resultState, var rightState) = this.GetOrAddVariable(right, context);

                BoolHandle result;
                if (leftState == rightState)
                {
                    // We know they are equal
                    result = areEqual;
                }
                else
                {
                    // We don't know directly, let the SMT solver decide it
                    result = areEqual
                        ? (leftState.Representation == rightState.Representation)
                        : (leftState.Representation != rightState.Representation);
                }

                return (resultState, result);
            }

            public AlgorithmState ReadField(
                VersionedVariable result,
                VersionedVariable reference,
                IFieldDefinition field,
                ISymbolicHeapContext context)
            {
                (var algState, var refState) = this.SecureDereference(reference, context);
                if (algState == ConflictState)
                {
                    return ConflictState;
                }

                Expression resultVar;
                if (result.Variable.IsReference)
                {
                    // Secure that the result variable is initialized
                    VariableState resultState;
                    (algState, resultState) = algState.GetOrAddVariable(result, context);
                    resultVar = resultState.Representation;
                }
                else
                {
                    // Don't store scalar values in the state
                    resultVar = result.Variable;
                }

                // Initialize the particular field
                ArrayHandle<IntHandle, Handle> fieldVar;
                (algState, fieldVar) = algState.GetOrAddFieldVariable(field, context);

                // Propagate the read to the SMT solver
                var selectAssert = (BoolHandle)ExpressionFactory.Equal(
                    resultVar,
                    fieldVar.Select(refState.Representation));
                context.AddAssertion(selectAssert);

                return algState;
            }

            public AlgorithmState WriteField(
                VersionedVariable reference,
                IFieldDefinition field,
                Expression value,
                ISymbolicHeapContext context)
            {
                (var algState, var refState) = this.SecureDereference(reference, context);
                if (algState == ConflictState)
                {
                    return ConflictState;
                }

                Expression valExpr;
                if (value.Sort == References.Sort)
                {
                    if (!(value is FlowVariable valVar))
                    {
                        throw new NotSupportedException("Only versioned flow variables supported as references");
                    }

                    // Secure that the result variable is initialized
                    var versionedVal = context.GetVersioned(valVar);
                    VariableState valState;
                    (algState, valState) = algState.GetOrAddVariable(versionedVal, context);
                    valExpr = valState.Representation;
                }
                else
                {
                    // Don't store scalar values in the state
                    valExpr = value;
                }

                // Get current and new version of the field
                ArrayHandle<IntHandle, Handle> oldFieldVar, newFieldVar;
                (algState, oldFieldVar) = algState.GetOrAddFieldVariable(field, context);
                (algState, newFieldVar) = algState.CreateNewFieldVariableVersion(field, context);

                // Propagate the write to the SMT solver
                var storeAssert = (oldFieldVar == newFieldVar.Store(refState.Representation, (Handle)valExpr));
                context.AddAssertion(storeAssert);

                return algState;
            }

            private static AlgorithmState ConstructBasicState()
            {
                var states = ImmutableSortedDictionary.CreateRange(new[]
                {
                    new KeyValuePair<int, VariableState>(VariableState.NullId, VariableState.Null)
                });
                var varStateMap = ImmutableDictionary.CreateRange(new[]
                {
                    new KeyValuePair<VersionedVariable, int>(VersionedVariable.Null, VariableState.NullId)
                });
                var stateVarsMap = ImmutableDictionary.CreateRange(new[]
                {
                    new KeyValuePair<int, ImmutableList<VersionedVariable>>(
                        VariableState.NullId,
                        ImmutableList.Create(VersionedVariable.Null))
                });

                return new AlgorithmState(
                    states,
                    varStateMap,
                    stateVarsMap,
                    ImmutableDictionary<IFieldDefinition, ArrayHandle<IntHandle, Handle>>.Empty,
                    1,
                    1);
            }

            private static bool IsNullStateAndVarState(
                VariableState leftState,
                VariableState rightState,
                out VariableState varState)
            {
                if (leftState == null || rightState == null)
                {
                    varState = null;
                    return false;
                }

                if (leftState == VariableState.Null && rightState != VariableState.Null)
                {
                    varState = rightState;
                    return true;
                }
                else if (rightState == VariableState.Null && leftState != VariableState.Null)
                {
                    varState = leftState;
                    return true;
                }
                else
                {
                    varState = null;
                    return false;
                }
            }

            private (AlgorithmState algState, VariableState refState)
                SecureDereference(
                    VersionedVariable reference,
                    ISymbolicHeapContext context)
            {
                var algState = this;

                // Secure that the reference variable is initialized and not null
                var refState = this.GetVariableOrNull(reference);
                if (refState == null)
                {
                    (algState, refState) = algState.MapToNewInputVariableState(
                        reference,
                        context,
                        canBeNull: false);

                    context.AddAssertion(refState.Representation != VariableState.Null.Representation);
                }
                else if (refState == VariableState.Null)
                {
                    // The statement wouldn't have executed due to null dereference
                    return (ConflictState, refState);
                }
                else if (refState.CanBeNull)
                {
                    context.AddAssertion(refState.Representation != VariableState.Null.Representation);

                    refState = refState.WithCanBeNull(false);
                    algState = algState.UpdateVariableState(refState);
                }

                return (algState, refState);
            }

            private (AlgorithmState algState, ArrayHandle<IntHandle, Handle> fieldVar)
                GetOrAddFieldVariable(
                    IFieldDefinition field,
                    ISymbolicHeapContext context)
            {
                if (this.fieldToVariableMap.TryGetValue(field, out var fieldVar))
                {
                    return (this, fieldVar);
                }
                else
                {
                    return this.CreateNewFieldVariableVersion(field, context);
                }
            }

            private (AlgorithmState algState, ArrayHandle<IntHandle, Handle> fieldVar)
                CreateNewFieldVariableVersion(
                    IFieldDefinition field,
                    ISymbolicHeapContext context)
            {
                var fieldSort = field.IsReference() ? Sort.Int : field.Sort;
                var newFieldVar = (ArrayHandle<IntHandle, Handle>)context.CreateVariable(
                    Sort.GetArray(Sort.Int, fieldSort),
                    field.ToString());

                var newFieldVarMap = this.fieldToVariableMap.SetItem(field, newFieldVar);
                var algState = new AlgorithmState(
                    this.variableStates,
                    this.variableToStateIdMap,
                    this.stateIdToVariablesMap,
                    newFieldVarMap,
                    this.nextVariableStateId,
                    this.nextReferenceValue);

                return (algState, newFieldVar);
            }

            private AlgorithmState UpdateVariableState(VariableState variableState)
            {
                var newVars = this.variableStates.SetItem(variableState.Id, variableState);
                return new AlgorithmState(
                    newVars,
                    this.variableToStateIdMap,
                    this.stateIdToVariablesMap,
                    this.fieldToVariableMap,
                    this.nextVariableStateId,
                    this.nextReferenceValue);
            }

            private AlgorithmState MapToVariableState(VersionedVariable variable, VariableState state)
            {
                Contract.Requires(this.variableStates[state.Id] == state);
                Contract.Requires(this.stateIdToVariablesMap.ContainsKey(state.Id));

                if (this.variableToStateIdMap.TryGetValue(variable, out int curStateId))
                {
                    if (curStateId == state.Id)
                    {
                        if (this.variableStates[curStateId] == state)
                        {
                            return this;
                        }
                        else
                        {
                            return this.UpdateVariableState(state);
                        }
                    }
                    else
                    {
                        // Update the states of all the variables pointing to the old one and erase it
                        var currentVars = this.stateIdToVariablesMap[curStateId];
                        var newVars = this.stateIdToVariablesMap[state.Id].AddRange(currentVars);
                        if (!currentVars.Contains(variable))
                        {
                            // TODO: Consider turning it into a set to make this more effective
                            newVars = newVars.Add(variable);
                        }

                        var newStates = this.variableStates.Remove(curStateId);
                        var newStateVarsMap = this.stateIdToVariablesMap
                            .Remove(curStateId)
                            .SetItem(state.Id, newVars);
                        var newVarStateMap = this.variableToStateIdMap.SetItems(
                            newVars.Select(v => new KeyValuePair<VersionedVariable, int>(v, state.Id)));

                        return new AlgorithmState(
                            newStates,
                            newVarStateMap,
                            newStateVarsMap,
                            this.fieldToVariableMap,
                            this.nextVariableStateId,
                            this.nextReferenceValue);
                    }
                }
                else
                {
                    var newVarStateMap = this.variableToStateIdMap.Add(variable, state.Id);
                    var currentVars = this.stateIdToVariablesMap.TryGetValue(state.Id, out var vars)
                        ? vars
                        : ImmutableList<VersionedVariable>.Empty;
                    var newStateVarsMap = this.stateIdToVariablesMap.SetItem(state.Id, currentVars.Add(variable));

                    return new AlgorithmState(
                        this.variableStates,
                        newVarStateMap,
                        newStateVarsMap,
                        this.fieldToVariableMap,
                        this.nextVariableStateId,
                        this.nextReferenceValue);
                }
            }

            private AlgorithmState MapToBetterVariableState(
                VersionedVariable left,
                VariableState leftState,
                VersionedVariable right,
                VariableState rightState)
            {
                Contract.Requires(this.variableToStateIdMap[left] == leftState.Id);
                Contract.Requires(this.variableToStateIdMap[right] == rightState.Id);
                Contract.Requires(this.variableStates[leftState.Id] == leftState);
                Contract.Requires(this.variableStates[rightState.Id] == rightState);
                Contract.Requires(
                    leftState == rightState
                    || (leftState != VariableState.Null && rightState != VariableState.Null));

                if (leftState == rightState)
                {
                    return this;
                }

                if (!leftState.IsInput && !rightState.IsInput)
                {
                    Contract.Assert(leftState.Value.Value != rightState.Value.Value);

                    return ConflictState;
                }

                if (leftState.IsInput && rightState.IsInput)
                {
                    if (!rightState.CanBeNull && leftState.CanBeNull)
                    {
                        // Map to the state with stronger condition
                        return this.MapToVariableState(left, rightState);
                    }
                    else
                    {
                        // By default, map to the left state
                        return this.MapToVariableState(right, leftState);
                    }
                }

                // Get rid of the input state
                if (leftState.IsInput)
                {
                    Contract.Assert(!rightState.IsInput);

                    return this.MapToVariableState(left, rightState);
                }
                else
                {
                    Contract.Assert(rightState.IsInput);

                   return this.MapToVariableState(right, leftState);
                }
            }

            private (AlgorithmState algState, VariableState refState)
                MapToNewInputVariableState(
                    VersionedVariable variable,
                    ISymbolicHeapContext context,
                    bool canBeNull = true)
            {
                var newVar = context.CreateVariable(Sort.Int, variable.ToString());
                var varState = VariableState.CreateInput(this.nextVariableStateId, newVar, canBeNull);
                var newVarStates = this.variableStates.Add(varState.Id, varState);
                var newStateVarsMap = this.stateIdToVariablesMap.SetItem(varState.Id, ImmutableList<VersionedVariable>.Empty);

                var algState = new AlgorithmState(
                    newVarStates,
                    this.variableToStateIdMap,
                    newStateVarsMap,
                    this.fieldToVariableMap,
                    this.nextVariableStateId + 1,
                    this.nextReferenceValue);

                algState = algState.MapToVariableState(variable, varState);

                return (algState, varState);
            }

            private (AlgorithmState state, VariableState result) MapToNewValueVariableState(VersionedVariable variable)
            {
                var varState = VariableState.CreateValue(this.nextVariableStateId, this.nextReferenceValue);
                var newVarStates = this.variableStates.Add(varState.Id, varState);
                var newStateVarsMap = this.stateIdToVariablesMap.SetItem(varState.Id, ImmutableList<VersionedVariable>.Empty);

                var algState = new AlgorithmState(
                    newVarStates,
                    this.variableToStateIdMap,
                    newStateVarsMap,
                    this.fieldToVariableMap,
                    this.nextVariableStateId + 1,
                    this.nextReferenceValue + 1);

                algState = algState.MapToVariableState(variable, varState);

                return (algState, varState);
            }

            private (AlgorithmState newState, VariableState varState) GetOrAddVariable(VersionedVariable variable, ISymbolicHeapContext context)
            {
                if (this.variableToStateIdMap.TryGetValue(variable, out var varStateId))
                {
                    return (this, this.variableStates[varStateId]);
                }
                else
                {
                    return this.MapToNewInputVariableState(variable, context);
                }
            }

            private VariableState GetVariableOrNull(VersionedVariable variable)
            {
                if (this.variableToStateIdMap.TryGetValue(variable, out var varStateId))
                {
                    return this.variableStates[varStateId];
                }
                else
                {
                    return null;
                }
            }
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
