using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.SmtLibStandard
{
    /// <summary>
    /// A unified interface for all SMT solvers.
    /// </summary>
    // TODO: Base on IDisposable, as the underlying solvers might use it (such as Z3)
    public interface ISolver
    {
        IContext Context { get; }

        bool AreDeclarationsGlobal { get; }

        bool IsUnsatisfiableCoreProduced { get; }

        IModel Model { get; }

        IReadOnlyList<IUnsatisfiableCoreElement> UnsatisfiableCore { get; }

        ISolver Clone();

        void AddAssertion(BoolHandle assertion);

        void AddAssertion<TVariable>(INameProvider<TVariable> varNameProvider, BoolHandle assertion)
            where TVariable : Variable;

        void AddAssertions(IEnumerable<BoolHandle> assertions);

        void AddAssertions<TVariable>(INameProvider<TVariable> varNameProvider, IEnumerable<BoolHandle> assertions)
            where TVariable : Variable;

        void Push();

        void Pop(int levels);

        SolverResult Solve();

        SolverResult Solve(IEnumerable<BoolHandle> assumptions);

        // TODO: Consider solving these overloads by default parameters and/or extension methods
        SolverResult Solve<TVariable>(INameProvider<TVariable> varNameProvider, IEnumerable<BoolHandle> assumptions)
             where TVariable : Variable;
    }
}
