using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    /// <summary>
    /// Models a reference in a heap model.
    /// </summary>
    public class ReferenceValueModel : ReferenceModel, IValueModel
    {
        private readonly HeapModelLocation location;
        private readonly IHeapModel heap;

        private string lazyText;

        internal ReferenceValueModel(ReferenceModelFactory factory, HeapModelLocation location, IHeapModel heap)
            : base(factory)
        {
            this.location = location;
            this.heap = heap;
        }

        public override IReadOnlyList<Variable> AssignmentLeft => throw new NotSupportedException();

        public override IReadOnlyList<Expression> AssignmentRight => throw new NotSupportedException();

        public override bool IsLValue => false;

        public string ValueText
        {
            get
            {
                if (this.lazyText == null)
                {
                    if (this.location.IsNull)
                    {
                        this.lazyText = "null";
                    }
                    else
                    {
                        var fieldValues = new List<string>();

                        // TODO: Use only the fields of the current type and print them using type models
                        foreach (var valueField in this.heap.GetValues(this.location))
                        {
                            fieldValues.Add($"{valueField.Field} = {valueField.Value}");
                        }

                        foreach (var referenceField in this.heap.GetReferences(this.location))
                        {
                            fieldValues.Add($"{referenceField.Field} = #{referenceField.LocationId}");
                        }

                        this.lazyText = $"[#{this.location.Id}] {{{string.Join(", ", fieldValues)}}}";
                    }
                }

                return this.lazyText;
            }
        }

        public override string ToString() => this.ValueText;
    }
}
