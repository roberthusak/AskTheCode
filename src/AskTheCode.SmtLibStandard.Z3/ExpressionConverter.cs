using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeContractsRevival.Runtime;
using Microsoft.Z3;

namespace AskTheCode.SmtLibStandard.Z3
{
    // TODO: Move the visitor itself to a private nested class making only Convert() public
    internal class ExpressionConverter : ExpressionVisitor<Expr>
    {
        private Microsoft.Z3.Context context;

        private INameProvider nameProvider;

        public ExpressionConverter(Microsoft.Z3.Context z3context)
        {
            this.context = z3context;
        }

        public Expr Convert(Expression expression, INameProvider nameProvider = null)
        {
            this.nameProvider = nameProvider;
            var result = this.Visit(expression);
            this.nameProvider = null;

            return result;
        }

        public override Expr VisitInterpretation(Interpretation interpretation)
        {
            if (interpretation.Sort == Sort.Bool)
            {
                Contract.Assert(interpretation.Value is bool);

                return this.context.MkBool((bool)interpretation.Value);
            }
            else if (interpretation.Sort == Sort.Int)
            {
                Contract.Assert(interpretation.Value is long);

                return this.context.MkInt((long)interpretation.Value);
            }
            else
            {
                // TODO: Implement also other sorts
                throw new InvalidOperationException();
            }
        }

        public override Expr VisitVariable(Variable variable)
        {
            SymbolName symbolName;

            var namedVariable = variable as NamedVariable;
            if (namedVariable != null)
            {
                symbolName = namedVariable.SymbolName;
            }
            else
            {
                symbolName = this.nameProvider.GetName(variable);
            }

            if (!symbolName.IsValid)
            {
                throw new InvalidOperationException();
            }

            // TODO: Consider caching if needed
            Symbol z3symbol;
            if (symbolName.Text != null)
            {
                z3symbol = this.context.MkSymbol(symbolName.ToString());
            }
            else
            {
                Contract.Assert(symbolName.Number != null);

                z3symbol = this.context.MkSymbol(symbolName.Number.Value);
            }

            var z3sort = this.TranslateSort(variable.Sort);

            return this.context.MkConst(z3symbol, z3sort);
        }

        public override Expr VisitFunction(Function function)
        {
            switch (function.Kind)
            {
                case ExpressionKind.Not:
                    return this.context.MkNot(this.VisitBoolChild(function, 0));
                case ExpressionKind.And:
                    return this.context.MkAnd(this.VisitBoolChildren(function));
                case ExpressionKind.Or:
                    return this.context.MkOr(this.VisitBoolChildren(function));
                case ExpressionKind.Xor:
                    return this.context.MkXor(
                        this.VisitBoolChild(function, 0),
                        this.VisitBoolChild(function, 1));
                case ExpressionKind.Implies:
                    return this.context.MkImplies(
                        this.VisitBoolChild(function, 0),
                        this.VisitBoolChild(function, 1));

                case ExpressionKind.Negate:
                    return this.context.MkUnaryMinus(this.VisitArithChild(function, 0));
                case ExpressionKind.Multiply:
                    return this.context.MkMul(this.VisitArithChildren(function));
                case ExpressionKind.DivideReal:
                case ExpressionKind.DivideInteger:
                    // TODO: Properly distinguish and handle these
                    return this.context.MkDiv(
                        this.VisitArithChild(function, 0),
                        this.VisitArithChild(function, 1));
                case ExpressionKind.Modulus:
                    return this.context.MkMod(
                        this.VisitIntChild(function, 0),
                        this.VisitIntChild(function, 1));
                case ExpressionKind.Remainder:
                    return this.context.MkRem(
                        this.VisitIntChild(function, 0),
                        this.VisitIntChild(function, 1));
                case ExpressionKind.Add:
                    return this.context.MkAdd(this.VisitArithChildren(function));
                case ExpressionKind.Subtract:
                    return this.context.MkSub(
                        this.VisitArithChild(function, 0),
                        this.VisitArithChild(function, 1));
                case ExpressionKind.LessThan:
                    return this.context.MkLt(
                        this.VisitArithChild(function, 0),
                        this.VisitArithChild(function, 1));
                case ExpressionKind.GreaterThan:
                    return this.context.MkGt(
                        this.VisitArithChild(function, 0),
                        this.VisitArithChild(function, 1));
                case ExpressionKind.LessThanOrEqual:
                    return this.context.MkLe(
                        this.VisitArithChild(function, 0),
                        this.VisitArithChild(function, 1));
                case ExpressionKind.GreaterThanOrEqual:
                    return this.context.MkGe(
                        this.VisitArithChild(function, 0),
                        this.VisitArithChild(function, 1));

                case ExpressionKind.Equal:
                    return this.context.MkEq(
                        this.VisitChild(function, 0),
                        this.VisitChild(function, 1));
                case ExpressionKind.Distinct:
                    return this.context.MkDistinct(this.VisitChildren<Expr>(function));
                case ExpressionKind.IfThenElse:
                    return this.context.MkITE(
                        this.VisitBoolChild(function, 0),
                        this.VisitChild(function, 1),
                        this.VisitChild(function, 2));

                case ExpressionKind.Select:
                    return this.context.MkSelect(
                        this.VisitArrayChild(function, 0),
                        this.VisitChild(function, 1));
                case ExpressionKind.Store:
                    return this.context.MkStore(
                        this.VisitArrayChild(function, 0),
                        this.VisitChild(function, 1),
                        this.VisitChild(function, 2));

                case ExpressionKind.Interpretation:
                case ExpressionKind.Variable:
                    throw new InvalidOperationException();
                default:
                    throw new NotImplementedException();
            }
        }

        private Microsoft.Z3.Sort TranslateSort(Sort sort)
        {
            if (sort == Sort.Bool)
            {
                return this.context.BoolSort;
            }
            else if (sort == Sort.Int)
            {
                return this.context.IntSort;
            }
            else if (sort.IsArray)
            {
                return this.context.MkArraySort(
                    this.TranslateSort(sort.SortArguments[0]),
                    this.TranslateSort(sort.SortArguments[1]));
            }
            else
            {
                // TODO: Implement also other sorts
                throw new NotImplementedException();
            }
        }

        private BoolExpr VisitBoolChild(Expression expression, int childIndex)
        {
            this.CheckVisitChildArguments(expression, childIndex);
            Contract.Requires(expression.GetChild(childIndex).Sort == Sort.Bool);

            return (BoolExpr)this.Visit(expression.GetChild(childIndex));
        }

        private BoolExpr[] VisitBoolChildren(Expression expression)
        {
            Contract.Requires(expression != null);
            Contract.Requires(Contract.ForAll(expression.Children, child => child.Sort == Sort.Bool));

            return this.VisitChildren<BoolExpr>(expression);
        }

        private ArithExpr VisitArithChild(Expression expression, int childIndex)
        {
            this.CheckVisitChildArguments(expression, childIndex);
            Contract.Requires(expression.GetChild(childIndex).Sort.IsNumeric);

            return (ArithExpr)this.Visit(expression.GetChild(childIndex));
        }

        private ArithExpr[] VisitArithChildren(Expression expression)
        {
            Contract.Requires(expression != null);
            Contract.Requires(Contract.ForAll(expression.Children, child => child.Sort.IsNumeric));

            return this.VisitChildren<ArithExpr>(expression);
        }

        private IntExpr VisitIntChild(Expression expression, int childIndex)
        {
            this.CheckVisitChildArguments(expression, childIndex);
            Contract.Requires(expression.GetChild(childIndex).Sort == Sort.Int);

            return (IntExpr)this.Visit(expression.GetChild(childIndex));
        }

        private ArrayExpr VisitArrayChild(Expression expression, int childIndex)
        {
            this.CheckVisitChildArguments(expression, childIndex);
            Contract.Requires(expression.GetChild(childIndex).Sort.IsArray);

            return (ArrayExpr)this.Visit(expression.GetChild(childIndex));
        }

        private Expr VisitChild(Expression expression, int childIndex)
        {
            this.CheckVisitChildArguments(expression, childIndex);

            return this.Visit(expression.GetChild(childIndex));
        }

        private T[] VisitChildren<T>(Expression expression)
            where T : Expr
        {
            Contract.Requires(expression != null);

            var result = new T[expression.ChildrenCount];
            for (int i = 0; i < expression.ChildrenCount; i++)
            {
                result[i] = (T)this.Visit(expression.GetChild(i));
            }

            return result;
        }

        [ContractAbbreviator]
        private void CheckVisitChildArguments(Expression expression, int childIndex)
        {
            Contract.Requires(expression != null);
            Contract.Requires(childIndex >= 0 && childIndex < expression.ChildrenCount);
        }
    }
}
