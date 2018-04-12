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
        private Context context;
        private Microsoft.Z3.Model model;

        private Dictionary<int, FuncDecl> intConstDecls;
        private Dictionary<string, FuncDecl> stringConstDecls;

        internal Model(Context context, Microsoft.Z3.Model z3model)
        {
            this.context = context;
            this.model = z3model;
        }

        private Dictionary<int, FuncDecl> IntConstDecls
        {
            get
            {
                if (this.intConstDecls == null)
                {
                    this.intConstDecls = new Dictionary<int, FuncDecl>();
                    foreach (var decl in this.model.ConstDecls)
                    {
                        if (decl.Name.IsIntSymbol())
                        {
                            int key = ((IntSymbol)decl.Name).Int;
                            this.intConstDecls.Add(key, decl);
                        }
                    }
                }

                return this.intConstDecls;
            }
        }

        private Dictionary<string, FuncDecl> StringConstDecls
        {
            get
            {
                if (this.stringConstDecls == null)
                {
                    this.stringConstDecls = new Dictionary<string, FuncDecl>();
                    foreach (var decl in this.model.ConstDecls)
                    {
                        if (decl.Name.IsStringSymbol())
                        {
                            string key = ((StringSymbol)decl.Name).String;
                            this.stringConstDecls.Add(key, decl);
                        }
                    }
                }

                return this.stringConstDecls;
            }
        }

        public Interpretation GetInterpretation(SymbolName variableName)
        {
            FuncDecl constDecl;
            if (variableName.Number != null && variableName.Text == null)
            {
                this.IntConstDecls.TryGetValue(variableName.Number.Value, out constDecl);
            }
            else
            {
                this.StringConstDecls.TryGetValue(variableName.ToString(), out constDecl);
            }

            if (constDecl == null)
            {
                return null;
            }

            var interprExpr = this.model.ConstInterp(constDecl);

            return TranslateInterpretation(interprExpr);
        }

        public Interpretation GetInterpretation<TVariable>(
            INameProvider<TVariable> varNameProvider,
            Expression expression)
            where TVariable : Variable
        {
            var converter = this.context.ExpressionConverter;

            var expr = converter.Convert(expression, varNameProvider);
            var exprIntr = this.model.Eval(expr, completion: true);

            return TranslateInterpretation(exprIntr);
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

        private static Interpretation TranslateInterpretation(Expr interprExpr)
        {
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
            else
            {
                return null;
            }
        }
    }
}
