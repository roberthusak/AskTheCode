using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration
{
    public class ExecutionModelEventArgs : EventArgs
    {
        public ExecutionModelEventArgs(ExecutionModel executionModel)
        {
            this.ExecutionModel = executionModel;
        }

        public ExecutionModel ExecutionModel { get; private set; }
    }
}
