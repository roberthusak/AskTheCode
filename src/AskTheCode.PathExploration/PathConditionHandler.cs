using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.PathExploration
{
    internal class PathConditionHandler : PathVariableVersionHandler
    {
        private readonly SmtContextHandler contextHandler;
        private readonly ISolver smtSolver;
        private readonly VersionedNameProvider nameProvider;

        public PathConditionHandler(
            SmtContextHandler contextHandler,
            ISolver smtSolver,
            Path path,
            StartingNodeInfo startingNode)
            : this(contextHandler, smtSolver, path)
        {
            Contract.Requires(contextHandler != null);
            Contract.Requires(smtSolver != null);
            Contract.Requires(path != null);
            Contract.Requires(startingNode != null);

            this.smtSolver.Push();

            this.ProcessStartingNode(startingNode);
        }

        private PathConditionHandler(SmtContextHandler contextHandler, ISolver smtSolver, Path path)
            : base(path)
        {
            this.contextHandler = contextHandler;
            this.smtSolver = smtSolver;

            this.nameProvider = new VersionedNameProvider(this);
        }

        protected override void OnAfterPathRetracted(int popCount)
        {
            // It is done as batch for performance reasons
            if (popCount > 0)
            {
                this.smtSolver.Pop(popCount);
            }
        }

        protected override void OnBeforePathStepExtended()
        {
            this.smtSolver.Push();
        }

        protected override void OnConditionAsserted(BoolHandle condition)
        {
            this.smtSolver.AddAssertion(this.nameProvider, condition);
        }

        protected override void OnVariableAssigned(FlowVariable variable, int lastVersion, Expression value)
        {
            this.AssertEquals(variable, lastVersion, value);
        }

        private void AssertEquals(FlowVariable variable, int version, Expression value)
        {
            var symbolName = this.contextHandler.GetVariableVersionSymbol(variable, version);
            var symbolWrapper = new ConcreteVariableSymbolWrapper(variable, symbolName);

            var equal = (BoolHandle)ExpressionFactory.Equal(symbolWrapper, value);
            this.smtSolver.AddAssertion(this.nameProvider, equal);
        }

        private class ConcreteVariableSymbolWrapper : Variable
        {
            public ConcreteVariableSymbolWrapper(FlowVariable variable, SymbolName symbolName)
                : base(variable.Sort)
            {
                Contract.Requires(variable != null);
                Contract.Requires(symbolName.IsValid);

                this.Variable = variable;
                this.SymbolName = symbolName;
            }

            public override string DisplayName
            {
                get { return this.SymbolName.ToString(); }
            }

            public FlowVariable Variable { get; private set; }

            public SymbolName SymbolName { get; private set; }
        }

        private class VersionedNameProvider : INameProvider<Variable>
        {
            private PathConditionHandler owner;

            public VersionedNameProvider(PathConditionHandler owner)
            {
                this.owner = owner;
            }

            public SymbolName GetName(Variable variable)
            {
                var flowVariable = variable as FlowVariable;
                if (flowVariable != null)
                {
                    int version = this.owner.GetVariableVersion(flowVariable);
                    return this.owner.contextHandler.GetVariableVersionSymbol(flowVariable, version);
                }
                else
                {
                    var symbolWrapper = variable as ConcreteVariableSymbolWrapper;
                    if (symbolWrapper != null)
                    {
                        return symbolWrapper.SymbolName;
                    }
                }

                throw new InvalidOperationException();
            }
        }
    }
}
