using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Overlays;
using AskTheCode.PathExploration.Heap;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration
{
    // TODO: Keep the list of solvers (already in the documentation...)
    public class SmtContextHandler
    {
        private IContextFactory contextFactory;
        private IContext context;

        private int lastVariableNumber = 0;
        private FlowGraphsVariableOverlay<List<SymbolName>> variableVersionSymbols =
            new FlowGraphsVariableOverlay<List<SymbolName>>(() => new List<SymbolName>());

        private List<NamedVariable> customVariables = new List<NamedVariable>();

        public SmtContextHandler(IContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;

            // TODO: Consider recreating the context and solvers when it starts to slow down the solving
            //       (http://stackoverflow.com/questions/23026243/can-i-use-same-z3-context-to-solve-independent-problems/23031822#comment43374632_23031822)
            this.context = this.contextFactory.CreateContext();
        }

        public SmtSolverHandler CreateEmptySolver(Path rootPath, StartingNodeInfo startingNode, ISymbolicHeapFactory heapFactory)
        {
            Contract.Requires(rootPath != null);
            Contract.Requires(startingNode != null);
            Contract.Requires(rootPath.Depth == 0);
            Contract.Requires(rootPath.Node == startingNode.Node);

            var solver = this.context.CreateSolver(areDeclarationsGlobal: true, isUnsatisfiableCoreProduced: true);

            return new SmtSolverHandler(this, solver, rootPath, startingNode, heapFactory);
        }

        internal NamedVariable CreateVariable(Sort sort, string name = null)
        {
            Contract.Requires(sort != null);

            var symbolName = new SymbolName(name, this.lastVariableNumber++);
            this.lastVariableNumber++;

            var variable = ExpressionFactory.NamedVariable(sort, symbolName);
            this.customVariables.Add(variable);

            return variable;
        }

        internal SymbolName GetVariableVersionSymbol(FlowVariable variable, int version)
        {
            Contract.Requires(variable != null);
            Contract.Requires(version >= 0);

            var versionsList = this.variableVersionSymbols[variable];
            for (int i = versionsList.Count; i <= version; i++)
            {
                // TODO: Make configurable without the dependence on the Debug/Release configuration
#if DEBUG
                var symbolName = new SymbolName($"{variable.ToString()}_{version}", this.lastVariableNumber);
#else
                var symbolName = new SymbolName(null, this.lastVariableNumber);
#endif
                this.lastVariableNumber++;
                versionsList.Add(symbolName);
            }

            return versionsList[version];
        }
    }
}
