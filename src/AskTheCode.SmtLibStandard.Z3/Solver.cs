using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.Z3;

namespace AskTheCode.SmtLibStandard.Z3
{
    /// <summary>
    /// Implementation of <see cref="ISolver"/> for Microsoft Z3.
    /// </summary>
    public class Solver : ISolver
    {
        private Context context;
        private Microsoft.Z3.Solver solver;

        private Model model;
        private ImmutableArray<IUnsatisfiableCoreElement> unsatisfiableCore;

        internal Solver(Context context, Microsoft.Z3.Solver z3solver)
        {
            this.context = context;
            this.solver = z3solver;
        }

        // TODO: Make configurable or readonly from the context
        public bool AreDeclarationsGlobal
        {
            get { return true; }
        }

        // TODO: Make configurable
        public bool IsUnsatisfiableCoreProduced
        {
            get { return false; }
        }

        public IContext Context
        {
            get { return this.context; }
        }

        public IModel Model
        {
            get
            {
                if (this.model == null)
                {
                    this.model = new Model(this.solver.Model);
                }

                return this.model;
            }
        }

        // TODO: Enable
        public IReadOnlyList<IUnsatisfiableCoreElement> UnsatisfiableCore
        {
            get { return ImmutableArray<IUnsatisfiableCoreElement>.Empty; }
        }

        public void AddAssertion(BoolHandle assertion)
        {
            throw new NotImplementedException();
        }

        public void AddAssertion<TVariable>(INameProvider<TVariable> varNameProvider, BoolHandle assertion)
            where TVariable : Variable
        {
            // It is expected to be reused by multiple solvers
            var converter = this.context.ExpressionConverter;

            var expr = (BoolExpr)converter.Convert(assertion, varNameProvider);
            this.solver.Assert(expr);
        }

        public void AddAssertions(IEnumerable<BoolHandle> assertions)
        {
            foreach (var assertion in assertions)
            {
                this.AddAssertion(assertion);
            }
        }

        public void AddAssertions<TVariable>(
            INameProvider<TVariable> varNameProvider,
            IEnumerable<BoolHandle> assertions)
            where TVariable : Variable
        {
            foreach (var assertion in assertions)
            {
                this.AddAssertion(varNameProvider, assertion);
            }
        }

        public ISolver Clone()
        {
            throw new NotImplementedException();
        }

        public void Pop(int levels)
        {
            this.solver.Pop((uint)levels);
        }

        public void Push()
        {
            this.solver.Push();
        }

        public SolverResult Solve()
        {
            this.model = null;
            var status = this.solver.Check();

            switch (status)
            {
                case Status.UNSATISFIABLE:
                    return SolverResult.Unsat;
                case Status.UNKNOWN:
                    return SolverResult.Unknown;
                case Status.SATISFIABLE:
                default:
                    Contract.Assert(status == Status.SATISFIABLE);
                    return SolverResult.Sat;
            }
        }
    }
}
