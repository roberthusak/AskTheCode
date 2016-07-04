using System;
using Microsoft.Z3;

namespace AskTheCode.SmtLibStandard.Z3
{
    public class Context : IContext
    {
        private readonly Microsoft.Z3.Context context;

        public Context()
        {
            // TODO: Add configuration options if necessary
            this.context = new Microsoft.Z3.Context();

            this.ExpressionConverter = new ExpressionConverter(this.context);
        }

        internal ExpressionConverter ExpressionConverter { get; private set; }

        //internal Microsoft.Z3.Context Z3Context
        //{
        //    get { return this.context; }
        //}

        // TODO: Take the boolean options into account
        public ISolver CreateSolver(bool areDeclarationsGlobal, bool isUnsatisfiableCoreProduced)
        {
            var z3solver = this.context.MkSimpleSolver();
            return new Solver(this, z3solver);
        }
    }
}
