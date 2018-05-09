using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.PathExploration.Heap
{
    /// <summary>
    /// Abstracts creation of a heap model.
    /// </summary>
    public interface IHeapModelRecorder
    {
        HeapModelLocation GetLocation(VersionedVariable reference);

        HeapModelLocation AllocateNew(VersionedVariable result);

        void ReadField(VersionedVariable reference, IFieldDefinition field);

        void WriteReferenceField(VersionedVariable reference, IFieldDefinition field, VersionedVariable value);

        void WriteValueField(VersionedVariable reference, IFieldDefinition field, Interpretation value);

        IHeapModel GetModel();
    }
}
