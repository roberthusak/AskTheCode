using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    public class TestRoutineLocation : IRoutineLocation
    {
        public TestRoutineLocation(int value)
        {
            this.Name = $"TestRoutineLocation#{value}";
        }

        public TestRoutineLocation(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public bool CanBeExplored
        {
            get { return false; }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
