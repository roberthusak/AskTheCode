using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.ControlFlowGraphs.Tests;
using AskTheCode.SmtLibStandard;

namespace ControlFlowGraphViewer
{
    public class SampleHeapModel : IHeapModel
    {
        public int MaxVersion => 3;

        public IEnumerable<HeapModelLocation> GetLocations(int version)
        {
            var result = new List<HeapModelLocation>()
            {
                HeapModelLocation.Null,
                new HeapModelLocation(1, version <= 1 ? 0 : 2)
            };

            if (version >= 1)
            {
                result.Add(new HeapModelLocation(2, version <= 2 ? 1 : 3));
            }

            if (version >= 3)
            {
                result.Add(new HeapModelLocation(3, 3));
            }

            return result;
        }

        public IEnumerable<HeapModelReference> GetReferences(HeapModelLocation location)
        {
            switch (location.Id)
            {
                case 1:
                    if (location.HeapVersion <= 1)
                    {
                        return new[]
                        {
                            new HeapModelReference(SampleLinkedListDefinitions.Next, HeapModelLocation.NullId)
                        };
                    }
                    else
                    {
                        return new[]
                        {
                            new HeapModelReference(SampleLinkedListDefinitions.Next, 2)
                        };
                    }

                case 2:
                    if (location.HeapVersion <= 2)
                    {
                        return new HeapModelReference[0];
                    }
                    else
                    {
                        return new[]
                        {
                            new HeapModelReference(SampleLinkedListDefinitions.Next, 3)
                        };
                    }

                case 3:
                    return new[]
                    {
                        new HeapModelReference(SampleLinkedListDefinitions.Next, 3)
                    };

                default:
                    return new HeapModelReference[0];
            }
        }

        public IEnumerable<HeapModelValue> GetValues(HeapModelLocation location)
        {
            if (location.Id == HeapModelLocation.NullId || location.Id == 2)
            {
                return new HeapModelValue[0];
            }
            else
            {
                return new[]
                {
                    new HeapModelValue(
                        SampleLinkedListDefinitions.Value,
                        ExpressionFactory.IntInterpretation(location.Id))
                };
            }
        }
    }
}
