using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    public class TestRoutineLocation : IRoutineLocation
    {
        public TestRoutineLocation(int value, bool isConstructor = false)
        {
            this.Name = $"TestRoutineLocation#{value}";
            this.IsConstructor = isConstructor;
        }

        public TestRoutineLocation(string name, bool isConstructor = false)
        {
            this.Name = name;
            this.IsConstructor = isConstructor;
        }

        public string Name { get; private set; }

        public bool CanBeExplored
        {
            get { return false; }
        }

        public bool IsConstructor { get; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
