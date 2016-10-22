using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;
using Microsoft.Pex.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvaluationTests
{
    // TODO: Add the references to the papers to the methods
    [TestClass]
    [PexClass]
    public partial class IntegerArithmetic
    {
        [PexMethod]
        [ContractVerification(true)]
        public int Sign(int x)
        {
            if (x > 0)
            {
                return 1;
            }
            else if (x == 0)
            {
                return 0;
            }
            else
            {
                Evaluation.ValidAssert(x < 0);
                return -1;
            }
        }

        [PexMethod]
        [ContractVerification(true)]
        public void DotProductCommutative(int ax, int ay, int bx, int by)
        {
            int ab = DotProduct(ax, ay, bx, by);
            int ba = DotProduct(bx, by, ax, ay);

            Evaluation.ValidAssert(ab == ba);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void DotProductDistributive(int ax, int ay, int bx, int by, int cx, int cy)
        {
            int abPlusC = DotProduct(ax, ay, bx + cx, by + cy);
            int abac = DotProduct(ax, ay, bx, by) + DotProduct(ax, ay, cx, cy);

            Evaluation.ValidAssert(abPlusC == abac);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void DotProductScalarMultiplication(int ax, int ay, int bx, int by, int c1, int c2)
        {
            int c1aDotC2b = DotProduct(c1 * ax, c1 * ay, c2 * bx, c2 * by);
            int c1c2ab = c1 * c2 * DotProduct(ax, ay, bx, by);

            Evaluation.ValidAssert(c1aDotC2b == c1c2ab);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void AlmostAbsoluteValue(int x)
        {
            int y;
            if (x >= 0)
            {
                y = x;
            }
            else
            {
                y = -x;
            }

            Evaluation.InvalidAssert(y >= 0);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void CubicEquation(int x, int y)
        {
            int a = x * x * x;
            int b = y + 3;

            Evaluation.InvalidAssert(a != b);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void NonlinearSimple(int x, int y)
        {
            int z = x * y;
            if (x == z && x > 2)
            {
                Evaluation.InvalidUnreachable();
            }
        }

        [PexMethod]
        [ContractVerification(true)]
        public void NonlinearHarder(int x, int y)
        {
            int z = x * y;
            if (x == z && x > 2 && y != 1)
            {
                Evaluation.InvalidUnreachable();
            }
        }

        [PexMethod]
        [ContractVerification(true)]
        public void UnmodelledOperation(int x)
        {
            int sign = Math.Sign(x);

            Evaluation.InvalidAssert(x >= 0);
        }

        [Pure]
        private static int DotProduct(int ax, int ay, int bx, int by)
        {
            DotProductConditionalEnsures(ax, ay, bx, by);

            return (ax * bx) + (ay * by);
        }

        [Conditional(Evaluation.ContractsHintsSymbol)]
        [ContractAbbreviator]
        private static void DotProductConditionalEnsures(int ax, int ay, int bx, int by)
        {
            Contract.Ensures(Contract.Result<int>() == (ax * bx) + (ay * by));
        }
    }
}
