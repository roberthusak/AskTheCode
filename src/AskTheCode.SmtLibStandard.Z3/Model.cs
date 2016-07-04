using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.Z3;

namespace AskTheCode.SmtLibStandard.Z3
{
    public class Model : IModel
    {
        private Microsoft.Z3.Model model;

        internal Model(Microsoft.Z3.Model z3model)
        {
            this.model = z3model;
        }

        // TODO: Handle also textual symbol names
        public Interpretation GetInterpretation(SymbolName variableName)
        {
            var constDecl = this.model.ConstDecls
                .First(decl => decl.Name.IsIntSymbol() && ((IntSymbol)decl.Name).Int == variableName.Number.Value);

            var interprExpr = this.model.ConstInterp(constDecl);

            if (interprExpr.IsBool && interprExpr.BoolValue != Z3_lbool.Z3_L_UNDEF)
            {
                bool value = (interprExpr.BoolValue == Z3_lbool.Z3_L_TRUE);
                return ExpressionFactory.BoolInterpretation(value);
            }
            else if (interprExpr.IsIntNum)
            {
                var intNum = (IntNum)interprExpr;
                return ExpressionFactory.IntInterpretation(intNum.Int64);
            }

            // TODO
            throw new NotImplementedException();
        }

        public double GetValue(RealHandle handle)
        {
            throw new NotImplementedException();
        }

        public string GetValue(StringHandle handle)
        {
            throw new NotImplementedException();
        }

        public long GetValue(IntHandle handle)
        {
            throw new NotImplementedException();
        }

        public bool GetValue(BoolHandle handle)
        {
            throw new NotImplementedException();
        }

        public object GetValue(Variable variable)
        {
            throw new NotImplementedException();
        }
    }
}
