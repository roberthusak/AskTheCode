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
            this.Value = value;
        }

        public int Value { get; private set; }

        public override string ToString()
        {
            return $"TestLocation #{this.Value}";
        }
    }
}
