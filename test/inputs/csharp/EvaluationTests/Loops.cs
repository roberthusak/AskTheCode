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
    [TestClass]
    [PexClass(Timeout = 30, MaxRunsWithoutNewTests = 2000, MaxConditions = 2000)]
    public partial class Loops
    {
        private const int SimpleLoopTarget = 64;
        private const int LoopedModuloTarget = 512;

        /// <remarks>
        /// When unwinding the loop condition backwards, AskTheCode does not have any information about the initial value
        /// of i. Therefore, it leads to an infinite cycle stopping after the timeout has passed.
        /// </remarks>
        [PexMethod]
        [ContractVerification(true)]
        public static void DataIndependentLoop(int x)
        {
            int i = 0;
            while (i < 20)
            {
                x = 2 * x;
                i = i + 1;
            }

            Evaluation.InvalidAssert(x < 32);   // AskTheCode, Pex
        }

        [PexMethod]
        [ContractVerification(true)]
        [LinearlyParametrizedEvaluation(nameof(SimpleLoopTarget), 64, 64, 64)]
        public static void SimpleLoop(int i)
        {
            int j = 10;
            while (i > 1)
            {
                j = j + 1;
                i = i - 1;

                Evaluation.ValidAssert(j > 0);  // CC
            }

            Evaluation.InvalidAssert(j != 2048 /*SimpleLoopTarget*/);   // Nothing
        }

        [PexMethod]
        [PexAssertReachEventually("location")]
        [ContractVerification(true)]
        public static void LoopAndIf(int x)
        {
            int c = 0, p = 0;
            while (x > 0)
            {
                Evaluation.InvalidAssert(c != 50);  // AskTheCode, Pex

                c = c + 1;
                p = p + c;
                x = x - 1;
            }

            //Evaluation.InvalidAssert(c != 30);

            if (c == 30)
            {
                Evaluation.InvalidUnreachable(); //Evaluation.InvalidAssert(c != 30);  // AskTheCode, Pex
                PexAssert.ReachEventually("location");
            }
        }

        [PexMethod]
        [PexAssertReachEventually(1)]
        [ContractVerification(true)]
        public static void TwoLoops(int x, int y)
        {
            int c = 0, p = 0;
            while (x > 0)
            {
                Evaluation.ValidAssert(c >= 0);     // Nothing

                c = c + 1;
                x = x - 1;
            }

            while (y > 0)
            {
                if (p == 50 && c == 2)
                {
                    PexAssert.ReachEventually(0);
                    Evaluation.InvalidUnreachable();    // AskTheCode, Pex
                }

                p = p + 1;
                y = y - 1;
            }
        }

        [PexMethod]
        [ContractVerification(true)]
        [LinearlyParametrizedEvaluation(nameof(LoopedModuloTarget), 64, 64, 64)]
        public void LoopedModulo(int x)
        {
            int res = 0;
            int i = 0;
            while (i < x)
            {
                int tmp = i % 2;
                if (tmp == 0)
                {
                    res = res - 1;
                }
                else
                {
                    res = res + 17;
                }

                i = i + 1;
            }

            Evaluation.InvalidAssert(res != 2048 /*LoopedModuloTarget*/);   // Pex
        }

        [PexMethod]
        //[PexAssertReachEventually]
        [ContractVerification(true)]
        public void ConcolicInefficientLoop(int x)
        {
            int y = 0;
            int i = 0;
            while (i < x)
            {
                int tmp = i % 2;
                if (tmp == 0)
                {
                    y = y - 1;
                }
                else
                {
                    y = y + 17;
                }

                i = i + 1;
            }

            if (y > 2048)
            {
                Evaluation.ValidAssert(y != 0);     // AskTheCode, CC

                int count = CountOfSomething();
                Evaluation.ValidAssert(y + count != 0);     // AskTheCode, CC (with hints)

                //PexAssert.ReachEventually();
            }
        }

        private static int CountOfSomething()
        {
            CountOfSomethingConditionalEnsures();

            return 42;
        }

        [Conditional(Evaluation.ContractsHintsSymbol)]
        [ContractAbbreviator]
        private static void CountOfSomethingConditionalEnsures()
        {
            Contract.Ensures(Contract.Result<int>() > 0);
        }
    }
}
