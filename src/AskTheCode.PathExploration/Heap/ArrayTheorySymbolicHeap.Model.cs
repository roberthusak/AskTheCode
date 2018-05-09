using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration.Heap
{
    public partial class ArrayTheorySymbolicHeap
    {

        private static int GetIdFromInterpretation(Interpretation interpretation)
        {
            return (int)(long)interpretation.Value;
        }

        private static HeapModelLocation ConvertInterpretationToLocation(Interpretation interpretation, int version)
        {
            Contract.Requires(interpretation.Sort == Sort.Int);
            int id = GetIdFromInterpretation(interpretation);
            return new HeapModelLocation(id, version);
        }

        private class ModelRecorder : IHeapModelRecorder
        {
            private readonly IModel smtModel;
            private readonly HeapState state;

            private Dictionary<int, LocationInfo> inputHeap = new Dictionary<int, LocationInfo>();
            private Dictionary<int, LocationInfo> currentHeap = new Dictionary<int, LocationInfo>();
            private List<FieldChange> changes = new List<FieldChange>();

            public ModelRecorder(IModel smtModel, HeapState state)
            {
                this.smtModel = smtModel;
                this.state = state;

                this.inputHeap.Add(HeapModelLocation.NullId, new LocationInfo() { LastModifiedVersion = 0 });
            }

            private int CurrentVersion => this.changes.Count;

            public HeapModelLocation AllocateNew(VersionedVariable result)
            {
                int id = GetIdFromInterpretation(this.GetInterpretation(result));

                // Increment the version by performing a change
                this.changes.Add(new FieldChange() { ReferenceId = id });

                this.currentHeap.Add(id, new LocationInfo() { LastModifiedVersion = this.CurrentVersion });

                return new HeapModelLocation(id, this.CurrentVersion);
            }

            public HeapModelLocation GetLocation(VersionedVariable reference)
            {
                int id = GetIdFromInterpretation(this.GetInterpretation(reference));

                if (this.currentHeap.TryGetValue(id, out var locationInfo)
                    || this.inputHeap.TryGetValue(id, out locationInfo))
                {
                    return new HeapModelLocation(id, locationInfo.LastModifiedVersion);
                }
                else
                {
                    this.inputHeap.Add(id, new LocationInfo() { LastModifiedVersion = 0 });
                    return new HeapModelLocation(id, 0);
                }
            }

            public void ReadField(VersionedVariable reference, IFieldDefinition field)
            {
                int refId = GetIdFromInterpretation(this.GetInterpretation(reference));

                if (this.currentHeap.TryGetValue(refId, out var locationInfo))
                {
                    if (locationInfo.ContainsField(field))
                    {
                        // It was read successfully
                        return;
                    }
                    else if (refId > VariableState.NullValue)
                    {
                        // Note: This can't happen in higher level languages, because they always
                        //       initialize the fields (e.g. to zero)
                        throw new NotSupportedException(
                            "Unable to model reading data of explicitly allocated but uninitialized objects");
                    }
                }

                // Fall back to input heap if not found in the current heap
                if (!this.inputHeap.TryGetValue(refId, out locationInfo))
                {
                    locationInfo = new LocationInfo() { LastModifiedVersion = 0 };
                    this.inputHeap.Add(refId, locationInfo);
                }

                if (!locationInfo.ContainsField(field))
                {
                    // Read the value from the particular input heap array
                    var fieldArray = this.state.GetFieldArray(field);
                    var valueIntr = this.smtModel.GetInterpretation(fieldArray.Select(refId));

                    // Store either the reference ID or the value interpretation to the field
                    locationInfo.SetField(field, valueIntr);
                }
            }

            public void WriteReferenceField(VersionedVariable reference, IFieldDefinition field, VersionedVariable value)
            {
                int refId = GetIdFromInterpretation(this.GetInterpretation(reference));
                var valIntr = this.GetInterpretation(value);

                this.WriteField(refId, field, valIntr);
            }

            public void WriteValueField(VersionedVariable reference, IFieldDefinition field, Interpretation value)
            {
                int refId = GetIdFromInterpretation(this.GetInterpretation(reference));

                this.WriteField(refId, field, value);
            }

            public IHeapModel GetModel()
            {
                var immutableInputHeap = this.inputHeap.ToImmutableDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToImmutable());

                var heapVersions = new List<ImmutableDictionary<int, Model.ImmutableLocationInfo>>()
                {
                    immutableInputHeap
                };

                var previousHeap = immutableInputHeap;
                int version = 1;
                foreach (var change in this.changes)
                {
                    var newLocationInfo = change.Field == null
                            ? new Model.ImmutableLocationInfo(version)
                            : previousHeap[change.ReferenceId].SetField(change.Field, change.Value, version);

                    var newHeap = previousHeap.SetItem(change.ReferenceId, newLocationInfo);
                    heapVersions.Add(newHeap);

                    Contract.Assert(heapVersions[version] == newHeap);

                    version++;
                    previousHeap = newHeap;
                }

                return new Model(heapVersions.ToImmutableArray());
            }

            private Interpretation GetInterpretation(VersionedVariable reference)
            {
                var varState = this.state.TryGetVariableState(reference);
                if (varState == null)
                {
                    // References with no operations or constraints imposed upon them will be null
                    return (Interpretation)VariableState.Null.Representation;
                }
                else if (varState.IsInput || varState.IsInputDerived)
                {
                    Contract.Assert(varState.Representation.Expression.Kind == ExpressionKind.Variable);

                    return this.smtModel.GetInterpretation(varState.Representation);
                }
                else
                {
                    Contract.Assert(varState.Representation.Expression.Kind == ExpressionKind.Interpretation);

                    return (Interpretation)varState.Representation;
                }
            }

            private void WriteField(int referenceId, IFieldDefinition field, Interpretation value)
            {
                // If the reference is from the input heap, add it there, knowing that we will access it later
                if (referenceId < VariableState.NullValue && !this.inputHeap.ContainsKey(referenceId))
                {
                    this.inputHeap.Add(referenceId, new LocationInfo() { LastModifiedVersion = 0 });
                }

                // Increment the version by performing a change
                this.changes.Add(new FieldChange() { ReferenceId = referenceId, Field = field, Value = value });

                if (!this.currentHeap.TryGetValue(referenceId, out var locationInfo))
                {
                    locationInfo = new LocationInfo();
                    this.currentHeap[referenceId] = locationInfo;
                }

                locationInfo.SetField(field, value);
                locationInfo.LastModifiedVersion = this.CurrentVersion;
            }

            private struct FieldChange
            {
                public int ReferenceId;
                public IFieldDefinition Field;
                public Interpretation Value;
            }

            private class LocationInfo
            {
                public int LastModifiedVersion { get; set; }

                public Dictionary<IFieldDefinition, int> ReferenceFields { get; } =
                    new Dictionary<IFieldDefinition, int>();

                public Dictionary<IFieldDefinition, Interpretation> ValueFields { get; } =
                    new Dictionary<IFieldDefinition, Interpretation>();

                public bool ContainsField(IFieldDefinition field)
                {
                    if (field.IsReference())
                    {
                        return this.ReferenceFields.ContainsKey(field);
                    }
                    else
                    {
                        return this.ValueFields.ContainsKey(field);
                    }
                }

                public void SetField(IFieldDefinition field, Interpretation interpretation)
                {
                    if (field.IsReference())
                    {
                        int id = GetIdFromInterpretation(interpretation);
                        this.ReferenceFields[field] = id;
                    }
                    else
                    {
                        this.ValueFields[field] = interpretation;
                    }
                }

                public Model.ImmutableLocationInfo ToImmutable()
                {
                    return new Model.ImmutableLocationInfo(
                        this.LastModifiedVersion,
                        this.ReferenceFields.ToImmutableDictionary(),
                        this.ValueFields.ToImmutableDictionary());
                }
            }
        }

        private class Model : IHeapModel
        {
            private ImmutableArray<ImmutableDictionary<int, ImmutableLocationInfo>> heapVersions;

            public Model(ImmutableArray<ImmutableDictionary<int, ImmutableLocationInfo>> heapVersions)
            {
                this.heapVersions = heapVersions;
            }

            public int MaxVersion => this.heapVersions.Length - 1;

            public IEnumerable<HeapModelLocation> GetLocations(int version)
            {
                return this.heapVersions[version]
                    .Select(kvp => new HeapModelLocation(kvp.Key, kvp.Value.LastModifiedVersion));
            }

            public IEnumerable<HeapModelReference> GetReferences(HeapModelLocation location)
            {
                return this.heapVersions[location.HeapVersion][location.Id].ReferenceFields
                    .Select(kvp => new HeapModelReference(kvp.Key, kvp.Value));
            }

            public IEnumerable<HeapModelValue> GetValues(HeapModelLocation location)
            {
                return this.heapVersions[location.HeapVersion][location.Id].ValueFields
                    .Select(kvp => new HeapModelValue(kvp.Key, kvp.Value));
            }

            public class ImmutableLocationInfo
            {
                public ImmutableLocationInfo(int lastModifiedVersion)
                    : this(
                        lastModifiedVersion,
                        ImmutableDictionary<IFieldDefinition, int>.Empty,
                        ImmutableDictionary<IFieldDefinition, Interpretation>.Empty)
                {
                }

                public ImmutableLocationInfo(
                    int lastModifiedVersion,
                    ImmutableDictionary<IFieldDefinition, int> referenceFields,
                    ImmutableDictionary<IFieldDefinition, Interpretation> valueFields)
                {
                    Contract.Requires(lastModifiedVersion >= 0);
                    Contract.Requires(referenceFields != null);
                    Contract.Requires(valueFields != null);

                    this.LastModifiedVersion = lastModifiedVersion;
                    this.ReferenceFields = referenceFields;
                    this.ValueFields = valueFields;
                }

                public static ImmutableLocationInfo Empty { get; } = new ImmutableLocationInfo(0);

                public int LastModifiedVersion { get; }

                public ImmutableDictionary<IFieldDefinition, int> ReferenceFields { get; }

                public ImmutableDictionary<IFieldDefinition, Interpretation> ValueFields { get; }

                public ImmutableLocationInfo SetField(IFieldDefinition field, Interpretation value, int newVersion)
                {
                    if (field.IsReference())
                    {
                        int id = GetIdFromInterpretation(value);
                        var newReferenceFields = this.ReferenceFields.SetItem(field, id);

                        return new ImmutableLocationInfo(newVersion, newReferenceFields, this.ValueFields);
                    }
                    else
                    {
                        var newValueFields = this.ValueFields.SetItem(field, value);

                        return new ImmutableLocationInfo(newVersion, this.ReferenceFields, newValueFields);
                    }
                }
            }
        }
    }
}
