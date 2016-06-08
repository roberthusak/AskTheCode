using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public interface ISolverContext
    {
        IEnumerable<IAssertionStackLevel> Stack
        {
            get;
        }

        void Push(IEnumerable<Expression> assertions);

        void Push<TVariable>(INameProvider<TVariable> varNameProvider, IEnumerable<Expression> assertions);

        void Push(IEnumerable<NamedVariable> definedVars, IEnumerable<Expression> assertions);

        void Push<TVariable>(IEnumerable<TVariable> definedVars, INameProvider<TVariable> varNameProvider, IEnumerable<Expression> assertions);

        void Pop();

        SolverResult Solve(out IModel model);
    }
}
