using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    public class TestRoutineLocation : IRoutineLocation
    {
        public TestRoutineLocation(MethodInfo generator, bool isConstructor = false)
        {
            Contract.Requires(generator.IsStatic);
            Contract.Requires(!generator.ContainsGenericParameters);
            Contract.Requires(generator.GetParameters().Length == 1);
            Contract.Requires(generator.GetParameters()[0].ParameterType == typeof(FlowGraphId));

            this.Name = generator.Name;
            this.Generator = generator;
            this.IsConstructor = isConstructor;
        }

        public TestRoutineLocation(string name, bool isConstructor = false)
        {
            Contract.Requires(name != null);

            this.Name = name;
            this.IsConstructor = isConstructor;
        }

        public string Name { get; }

        public MethodInfo Generator { get; }

        public bool CanBeExplored => this.Generator != null;

        public bool IsConstructor { get; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
