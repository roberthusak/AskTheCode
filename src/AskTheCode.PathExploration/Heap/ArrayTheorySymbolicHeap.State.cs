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
    public partial class ArrayTheorySymbolicHeap
    {
        internal class VariableState
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

            public bool IsNull => this.Id == NullId;

            public bool IsInput => this.Value == null;

            public bool IsExplicitlyAllocated => !this.IsInput && !this.IsNull;

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

        private class HeapState
        {
            public static readonly HeapState ConflictState = new HeapState(null, null, null, null, -1, -1);

            public static readonly HeapState BasicState = ConstructBasicState();

            private readonly ImmutableSortedDictionary<int, VariableState> variableStates;
            private readonly ImmutableDictionary<VersionedVariable, int> variableToStateIdMap;
            private readonly ImmutableDictionary<int, ImmutableList<VariableMappingInfo>> stateIdToVariablesMap;
            private readonly ImmutableDictionary<IFieldDefinition, ArrayHandle<IntHandle, Handle>> fieldToVariableMap;
            private readonly int nextVariableStateId;
            private readonly int nextReferenceValue;

            private HeapState(
                ImmutableSortedDictionary<int, VariableState> variableStates,
                ImmutableDictionary<VersionedVariable, int> variableToStateIdMap,
                ImmutableDictionary<int, ImmutableList<VariableMappingInfo>> stateIdToVariablesMap,
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

            private enum VariableMappingKind
            {
                Equality,
                Assignment
            }

            public Builder ToBuilder() => new Builder(this);

            public VariableState TryGetVariableState(VersionedVariable variable)
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

            private static HeapState ConstructBasicState()
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
                    new KeyValuePair<int, ImmutableList<VariableMappingInfo>>(
                        VariableState.NullId,
                        ImmutableList.Create(new VariableMappingInfo(VersionedVariable.Null, VariableMappingKind.Equality)))
                });

                return new HeapState(
                    states,
                    varStateMap,
                    stateVarsMap,
                    ImmutableDictionary<IFieldDefinition, ArrayHandle<IntHandle, Handle>>.Empty,
                    1,
                    1);
            }

            private struct VariableMappingInfo
            {
                public VersionedVariable Variable;
                public VariableMappingKind Kind;

                public VariableMappingInfo(VersionedVariable variable, VariableMappingKind kind)
                {
                    this.Variable = variable;
                    this.Kind = kind;
                }

                public VariableMappingInfo WithKind(VariableMappingKind kind)
                {
                    return new VariableMappingInfo(this.Variable, kind);
                }
            }

            public class Builder
            {
                private readonly ImmutableSortedDictionary<int, VariableState>.Builder variableStates;
                private readonly ImmutableDictionary<VersionedVariable, int>.Builder variableToStateIdMap;
                private readonly ImmutableDictionary<int, ImmutableList<VariableMappingInfo>>.Builder stateIdToVariablesMap;
                private readonly ImmutableDictionary<IFieldDefinition, ArrayHandle<IntHandle, Handle>>.Builder fieldToVariableMap;
                private int nextVariableStateId;
                private int nextReferenceValue;

                private HeapState cachedState;

                public Builder(HeapState state)
                {
                    this.variableStates = state.variableStates?.ToBuilder();
                    this.variableToStateIdMap = state.variableToStateIdMap?.ToBuilder();
                    this.stateIdToVariablesMap = state.stateIdToVariablesMap?.ToBuilder();
                    this.fieldToVariableMap = state.fieldToVariableMap?.ToBuilder();
                    this.nextVariableStateId = state.nextVariableStateId;
                    this.nextReferenceValue = state.nextReferenceValue;

                    this.cachedState = state;
                    this.IsConflicting = (state == ConflictState);
                }

                public bool IsConflicting { get; private set; }

                public HeapState ToState()
                {
                    if (this.IsConflicting)
                    {
                        return ConflictState;
                    }
                    else if (this.cachedState == null)
                    {
                        this.cachedState = new HeapState(
                            this.variableStates.ToImmutable(),
                            this.variableToStateIdMap.ToImmutable(),
                            this.stateIdToVariablesMap.ToImmutable(),
                            this.fieldToVariableMap.ToImmutable(),
                            this.nextVariableStateId,
                            this.nextReferenceValue);
                    }

                    return this.cachedState;
                }

                public ImmutableArray<BoolHandle> GetAssumptions()
                {
                    var refFieldHandles = this.fieldToVariableMap
                        .Where(kvp => kvp.Key.IsReference())
                        .Select(kvp => (ArrayHandle<IntHandle, IntHandle>)kvp.Value.Expression)
                        .ToArray();

                    return this.variableStates.Values
                        .Where(s => s.IsInput)
                        .Select((s) =>
                        {
                            // If there are no fields, only the object must be from the input heap
                            if (refFieldHandles.Length == 0)
                            {
                                if (s.CanBeNull)
                                {
                                    return s.Representation <= VariableState.NullValue;
                                }
                                else
                                {
                                    return s.Representation < VariableState.NullValue;
                                }
                            }

                            // Both the referenced object and all the objects referenced by it
                            // must be from the input heap (if not null)
                            var readConjuncts = new List<Expression>()
                            {
                                s.Representation < VariableState.NullValue
                            };

                            // TODO: Use only the fields present in the corresponding class
                            readConjuncts.AddRange(
                                refFieldHandles
                                    .Select(h => (h.Select(s.Representation) <= VariableState.NullValue).Expression));

                            var readAnd = (BoolHandle)ExpressionFactory.And(readConjuncts.ToArray());

                            if (s.CanBeNull)
                            {
                                return s.Representation == VariableState.NullValue || readAnd;
                            }
                            else
                            {
                                return readAnd;
                            }
                        })
                        .ToImmutableArray();
                }

                public void AllocateNew(
                    VersionedVariable result,
                    ISymbolicHeapContext context)
                {
                    this.cachedState = null;

                    var newState = VariableState.CreateValue(this.nextVariableStateId, this.nextReferenceValue);
                    this.nextVariableStateId++;
                    this.nextReferenceValue++;

                    if (this.variableToStateIdMap.TryGetValue(result, out int curStateId))
                    {
                        var curState = this.variableStates[curStateId];
                        Contract.Assert(curState.IsInput);

                        // Unify a previously known value with its current allocated number
                        context.AddAssertion(curState.Representation == newState.Representation);
                    }

                    this.MapToVariableState(result, newState);
                }

                public void AssignReference(
                    VersionedVariable result,
                    VersionedVariable value,
                    ISymbolicHeapContext context)
                {
                    this.cachedState = null;

                    var resultState = this.GetVariableStateOrNull(result);
                    var valueState = this.GetVariableStateOrNull(value);

                    if (resultState != null && valueState != null)
                    {
                        Contract.Assert(resultState.IsInput);

                        context.AddAssertion(resultState.Representation == valueState.Representation);
                        this.MapToVariableState(result, valueState);
                    }
                    else if (resultState == null && valueState == null)
                    {
                        var newVar = context.CreateVariable(Sort.Int, value.Variable.ToString());
                        var newState = VariableState.CreateInput(this.nextVariableStateId, newVar, true);
                        this.nextVariableStateId++;

                        this.MapToVariableState(result, newState);
                        this.MapToVariableState(value, newState);
                    }
                    else if (resultState != null)
                    {
                        Contract.Assert(valueState == null);

                        this.MapToVariableState(value, resultState);
                    }
                    else
                    {
                        Contract.Assert(valueState != null && resultState == null);

                        this.MapToVariableState(result, valueState);
                    }
                }

                public void AssertEquality(
                    VersionedVariable left,
                    VersionedVariable right,
                    ISymbolicHeapContext context)
                {
                    this.cachedState = null;

                    var leftState = this.GetOrCreateVariableState(left, context);
                    var rightState = this.GetOrCreateVariableState(right, context);

                    // TODO: Optimize by trying to get constant expression
                    context.AddAssertion(leftState.Representation == rightState.Representation);
                }

                public void AssertInequality(
                    VersionedVariable left,
                    VersionedVariable right,
                    ISymbolicHeapContext context)
                {
                    this.cachedState = null;

                    var leftState = this.GetOrCreateVariableState(left, context);
                    var rightState = this.GetOrCreateVariableState(right, context);

                    context.AddAssertion(leftState.Representation != rightState.Representation);
                }

                public BoolHandle GetEqualityExpression(
                    bool areEqual,
                    VersionedVariable left,
                    VersionedVariable right,
                    ISymbolicHeapContext context)
                {
                    this.cachedState = null;

                    var leftState = this.GetOrCreateVariableState(left, context);
                    var rightState = this.GetOrCreateVariableState(right, context);

                    return areEqual
                        ? leftState.Representation == rightState.Representation
                        : leftState.Representation != rightState.Representation;
                }

                public void ReadField(
                    VersionedVariable result,
                    VersionedVariable reference,
                    IFieldDefinition field,
                    ISymbolicHeapContext context)
                {
                    this.cachedState = null;

                    var refState = this.TrySecureNotNull(reference, context);
                    if (this.IsConflicting)
                    {
                        return;
                    }

                    Expression resultVar;
                    if (result.Variable.IsReference)
                    {
                        // Secure that the result variable is initialized
                        var resultState = this.GetOrCreateVariableState(result, context);
                        Contract.Assert(resultState.IsInput);

                        resultVar = resultState.Representation;

                        // TODO: Add also conditional constraints to secure input heap shape
                    }
                    else
                    {
                        // Don't store scalar values in the state
                        resultVar = context.GetNamedVariable(result);
                    }

                    // Initialize the particular field
                    var fieldVar = this.GetOrAddFieldVariable(field, context);

                    // Propagate the read to the SMT solver
                    var selectAssert = (BoolHandle)ExpressionFactory.Equal(
                        resultVar,
                        fieldVar.Select(refState.Representation));
                    context.AddAssertion(selectAssert);
                }

                public void WriteField(
                    VersionedVariable reference,
                    IFieldDefinition field,
                    Expression value,
                    ISymbolicHeapContext context)
                {
                    this.cachedState = null;

                    var refState = this.TrySecureNotNull(reference, context);
                    if (this.IsConflicting)
                    {
                        return;
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
                        var valState = this.GetOrCreateVariableState(versionedVal, context);
                        valExpr = valState.Representation;
                    }
                    else
                    {
                        // Don't store scalar values in the state
                        valExpr = value;
                    }

                    // Get current and new version of the field
                    var oldFieldVar = this.GetOrAddFieldVariable(field, context);
                    var newFieldVar = this.CreateNewFieldVariableVersion(field, context);

                    var storeAssert = (oldFieldVar == newFieldVar.Store(refState.Representation, (Handle)valExpr));
                    context.AddAssertion(storeAssert);
                }

                private ArrayHandle<IntHandle, Handle> GetOrAddFieldVariable(
                    IFieldDefinition field,
                    ISymbolicHeapContext context)
                {
                    if (this.fieldToVariableMap.TryGetValue(field, out var fieldVar))
                    {
                        return fieldVar;
                    }
                    else
                    {
                        return this.CreateNewFieldVariableVersion(field, context);
                    }
                }

                private ArrayHandle<IntHandle, Handle> CreateNewFieldVariableVersion(
                    IFieldDefinition field,
                    ISymbolicHeapContext context)
                {
                    var fieldSort = field.IsReference() ? Sort.Int : field.Sort;
                    var newFieldVar = (ArrayHandle<IntHandle, Handle>)context.CreateVariable(
                        Sort.GetArray(Sort.Int, fieldSort),
                        field.ToString());

                    this.fieldToVariableMap[field] = newFieldVar;

                    return newFieldVar;
                }

                private VariableState TrySecureNotNull(
                    VersionedVariable variable,
                    ISymbolicHeapContext context)
                {
                    // Secure that the variable is initialized and not null
                    var state = this.GetVariableStateOrNull(variable);
                    if (state == null)
                    {
                        state = this.CreateNewInputVariableState(variable, context, canBeNull: false);
                        this.MapToVariableState(variable, state);

                        context.AddAssertion(state.Representation != VariableState.Null.Representation);
                    }
                    else if (state == VariableState.Null)
                    {
                        this.IsConflicting = true;
                    }
                    else if (state.CanBeNull)
                    {
                        Contract.Assert(state.IsInput);

                        this.variableStates[state.Id] = state.WithCanBeNull(false);

                        context.AddAssertion(state.Representation != VariableState.Null.Representation);
                    }

                    return state;
                }

                private VariableState GetVariableStateOrNull(
                    VersionedVariable variable)
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

                private VariableState GetOrCreateVariableState(
                    VersionedVariable variable,
                    ISymbolicHeapContext context,
                    bool canBeNull = true)
                {
                    if (this.variableToStateIdMap.TryGetValue(variable, out var varStateId))
                    {
                        return this.variableStates[varStateId];
                    }
                    else
                    {
                        VariableState newState = this.CreateNewInputVariableState(variable, context, canBeNull);

                        this.MapToVariableState(variable, newState);

                        return newState;
                    }
                }

                private VariableState CreateNewInputVariableState(VersionedVariable variable, ISymbolicHeapContext context, bool canBeNull)
                {
                    var newVar = context.CreateVariable(Sort.Int, variable.ToString());
                    var newState = VariableState.CreateInput(this.nextVariableStateId, newVar, canBeNull);
                    this.nextVariableStateId++;
                    return newState;
                }

                private void MapToVariableState(
                    VersionedVariable variable,
                    VariableState state)
                {
                    this.variableStates[state.Id] = state;

                    if (this.variableToStateIdMap.TryGetValue(variable, out int curStateId))
                    {
                        if (curStateId == state.Id)
                        {
                            if (this.variableStates[curStateId] != state)
                            {
                                this.variableStates[curStateId] = state;
                            }
                        }
                        else
                        {
                            // Update the states of all the variables pointing to the old one and erase it
                            var currentVars = this.stateIdToVariablesMap[curStateId];
                            var newVars = this.stateIdToVariablesMap.GetValueOrDefault(state.Id)
                                ?? ImmutableList<VariableMappingInfo>.Empty;
                            newVars = newVars.AddRange(currentVars);
                            if (!currentVars.Any(v => v.Variable == variable))
                            {
                                // TODO: Consider turning it into a set to make this more effective
                                newVars = newVars.Add(new VariableMappingInfo(variable, VariableMappingKind.Assignment));
                            }

                            this.variableStates.Remove(curStateId);
                            this.stateIdToVariablesMap.Remove(curStateId);
                            this.stateIdToVariablesMap[state.Id] = newVars;

                            foreach (var newVar in newVars)
                            {
                                this.variableToStateIdMap[newVar.Variable] = state.Id;
                            }
                        }
                    }
                    else
                    {
                        this.variableToStateIdMap[variable] = state.Id;

                        var curMappedVars = this.stateIdToVariablesMap.GetValueOrDefault(state.Id)
                            ?? ImmutableList<VariableMappingInfo>.Empty;
                        VariableMappingInfo varInfo = new VariableMappingInfo(variable, VariableMappingKind.Assignment);
                        var newMappedVars = curMappedVars.Add(varInfo);
                        this.stateIdToVariablesMap[state.Id] = newMappedVars;
                    }
                }
            }
        }
    }
}
