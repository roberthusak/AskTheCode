using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    public class TestLocation : ILocation
    {
        public TestLocation(int value)
        {
            this.Name = $"TestLocation#{value}";
        }

        public TestLocation(string name)
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
