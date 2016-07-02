using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.PathExploration
{
    public enum ExplorationResultKind
    {
        Unknown,
        Unreachable,
        Reachable
    }

    public class ExplorationResult
    {
        private ExplorationResult(
            ExplorationResultKind kind,
            ExecutionModel executionModel,
            PathCounterExample pathCounterExample)
        {
            this.Kind = kind;
            this.ExecutionModel = executionModel;
            this.PathCounterExample = pathCounterExample;
        }

        public ExplorationResultKind Kind { get; private set; }

        public ExecutionModel ExecutionModel { get; private set; }

        public PathCounterExample PathCounterExample { get; private set; }

        public static ExplorationResult CreateUnknown()
        {
            return new ExplorationResult(ExplorationResultKind.Unknown, null, null);
        }

        public static ExplorationResult CreateUnreachable(PathCounterExample pathCounterExample)
        {
            return new ExplorationResult(ExplorationResultKind.Unreachable, null, pathCounterExample);
        }

        public static ExplorationResult CreateReachable(ExecutionModel executionModel)
        {
            return new ExplorationResult(ExplorationResultKind.Reachable, executionModel, null);
        }
    }
}
