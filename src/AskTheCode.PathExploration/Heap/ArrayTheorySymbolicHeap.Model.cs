using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.TypeSystem;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.PathExploration.Heap
{
    public partial class ArrayTheorySymbolicHeap
    {
        private class EmptyModelRecorder : IHeapModelRecorder
        {
            public HeapModelLocation AllocateNew(VersionedVariable result)
            {
                return HeapModelLocation.Null;
            }

            public HeapModelLocation GetLocation(VersionedVariable reference)
            {
                return HeapModelLocation.Null;
            }

            public IHeapModel GetModel()
            {
                return new EmptyModel();
            }

            public void ReadField(VersionedVariable reference, IFieldDefinition field)
            {
            }

            public void WriteReferenceField(VersionedVariable reference, IFieldDefinition field, VersionedVariable value)
            {
            }

            public void WriteValueField(VersionedVariable reference, IFieldDefinition field, Interpretation value)
            {
            }
        }

        private class EmptyModel : IHeapModel
        {
            public int MaxVersion => 0;

            public IEnumerable<HeapModelLocation> GetLocations(int version)
            {
                return ImmutableArray<HeapModelLocation>.Empty;
            }

            public IEnumerable<HeapModelReference> GetReferences(HeapModelLocation location)
            {
                return ImmutableArray<HeapModelReference>.Empty;
            }

            public IEnumerable<HeapModelValue> GetValues(HeapModelLocation location)
            {
                return ImmutableArray<HeapModelValue>.Empty;
            }
        }
    }
}
