using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.PathExploration
{
    public class SmtSolverHandler
    {
        internal SmtSolverHandler(Path path)
        {
            this.Path = path;
        }

        public Path Path { get; private set; }

        public SmtSolverHandler Clone()
        {
            throw new NotImplementedException();
        }

        public ExplorationResultKind Solve(Path path)
        {
            this.Path = path;

            throw new NotImplementedException();
        }

        public ExplorationResult GetFullResult()
        {
            throw new NotImplementedException();
        }
    }
}
