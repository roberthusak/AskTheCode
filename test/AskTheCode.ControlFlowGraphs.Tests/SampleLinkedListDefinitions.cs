using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    public static class SampleLinkedListDefinitions
    {
        private static SampleNodeClassDefinition nodeClass = new SampleNodeClassDefinition();

        public static IClassDefinition Node => nodeClass;

        public static IFieldDefinition Next => nodeClass.Next;

        public static IFieldDefinition Value => nodeClass.Value;

        private class SampleNodeClassDefinition : IClassDefinition
        {
            public SampleNodeClassDefinition()
            {
                this.Value = new SampleFieldDefinition("value", Sort.Int, null);
                this.Next = new SampleFieldDefinition("next", References.Sort, this);

                this.Fields = new AsyncLazy<IEnumerable<IFieldDefinition>>(
                    () => Task.FromResult<IEnumerable<IFieldDefinition>>(
                        new IFieldDefinition[]
                        {
                            this.Value,
                            this.Next
                        }));
            }

            public SampleFieldDefinition Value { get; }

            public SampleFieldDefinition Next { get; }

            public AsyncLazy<IEnumerable<IFieldDefinition>> Fields { get; }

            public override string ToString() => "Node";
        }

        private class SampleFieldDefinition : IFieldDefinition
        {
            private readonly string name;

            public SampleFieldDefinition(string name, Sort sort, IClassDefinition referencedClass)
            {
                this.name = name;
                this.Sort = sort;
                this.ReferencedClass = referencedClass;
            }

            public Sort Sort { get; }

            public IClassDefinition ReferencedClass { get; }

            public override string ToString() => this.name;
        }
    }
}
