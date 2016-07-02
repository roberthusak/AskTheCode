using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public interface ISolver
    {
        IContext Context { get; }

        IEnumerable<IAssertionStackLevel> Stack { get; }

        bool AreDeclarationsGlobal { get; }

        bool IsUnsatisfiableCoreProduced { get; }

        IModel Model { get; }

        IReadOnlyList<IUnsatisfiableCoreElement> UnsatisfiableCore { get; }

        ISolver Clone();

        void AddAssertion(Expression assertion);

        void AddAssertion<TVariable>(INameProvider<TVariable> varNameProvider, Expression assertion)
            where TVariable : Variable;

        void AddAssertions(IEnumerable<Expression> assertions);

        void AddAssertions<TVariable>(INameProvider<TVariable> varNameProvider, IEnumerable<Expression> assertions)
            where TVariable : Variable;

        void Push();

        void Pop(int levels);

        SolverResult Solve();
    }
}
