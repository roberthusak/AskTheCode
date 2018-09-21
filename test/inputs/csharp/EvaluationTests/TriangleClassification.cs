using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvaluationTests
{
    /// <summary>
    /// A nested algorithm of triangle type classification, often used to benchmark automated test generators.
    /// </summary>
    /// <remarks>
    /// The code was rewriten from Java version downloaded from the web pages of Darko Marinov:
    /// http://mir.cs.illinois.edu/~marinov/sp09-cs498dm/trityp.java.
    /// The mutants were created according to the ICFCA 2008 paper from Peggy Cellier.
    ///
    /// The meanings of the particular return values follow:
    /// 1: scalene
    /// 2: isosceles
    /// 3: equilateral
    /// 4: not a triangle
    /// </remarks>
    public partial class TriangleClassification
    {
        [Pure]
        [ContractVerification(true)]
        public static bool CheckResult(int a, int b, int c, int result)
        {
            CheckResultConditionalEnsures(a, b, c, result);

            if (result == 1)
            {
                return CheckScaleneTriangle(a, b, c);
            }
            else if (result == 2)
            {
                return CheckIsoscelesTriangle(a, b, c);
            }
            else if (result == 3)
            {
                return CheckEquilateralTriangle(a, b, c);
            }
            else if (result == 4)
            {
                return !CheckValidTriangle(a, b, c);
            }
            else
            {
                return false;
            }
        }

        [ContractVerification(true)]
        public static int ClassifyOriginal(int a, int b, int c)
        {
            ClassifyConditionalEnsures(a, b, c, Contract.Result<int>());

            int result;

            // After a quick confirmation that it's a legal triangle, detect any sides of equal length
            if (a <= 0 || b <= 0 || c <= 0)
            {
                result = 4;
                return result;
            }

            result = 0;

            if (a == b)
            {
                result = result + 1;
            }

            if (a == c)
            {
                result = result + 2;
            }

            if (b == c)
            {
                result = result + 3;
            }

            if (result == 0)
            {
                // Confirm it's a legal triangle before declaring it to be scalene
                if (a + b <= c || b + c <= a || a + c <= b)
                {
                    result = 4;
                }
                else
                {
                    result = 1;
                }

                return result;
            }

            // Confirm it's a legal triangle before declaring it to be isosceles or equilateral
            if (result > 3)
            {
                result = 3;
            }
            else if (result == 1 && a + b > c)
            {
                result = 2;
            }
            else if (result == 2 && a + c > b)
            {
                result = 2;
            }
            else if (result == 3 && b + c > a)
            {
                result = 2;
            }
            else
            {
                result = 4;
            }

            return result;
        }

        [ContractVerification(true)]
        public static int ClassifyMutant1(int a, int b, int c)
        {
            //ClassifyConditionalEnsures(a, b, c, Contract.Result<int>());

            int result;

            // After a quick confirmation that it's a legal triangle, detect any sides of equal length
            if (a <= 0 || b <= 0 || c <= 0)
            {
                result = 4;
                return result;
            }

            result = 0;

            if (a == b)
            {
                result = result + 1;
            }

            if (a == c)
            {
                result = result + 2;
            }

            if (b == c)
            {
                result = result + 3;
            }

            if (result == 0)
            {
                // Confirm it's a legal triangle before declaring it to be scalene
                if (a + b <= c || b + c <= a || a + c <= b)
                {
                    result = 4;
                }
                else
                {
                    result = 1;
                }

                return result;
            }

            // Confirm it's a legal triangle before declaring it to be isosceles or equilateral
            if (result > 3)
            {
                result = 3;
            }
            else if (result == 1 && a + b > c)
            {
                result = 2;
            }
            /* (result == 2 && a + c > b) in original */
            else if (result == 3 && a + c > b)
            {
                result = 2;
            }
            else if (result == 3 && b + c > a)
            {
                result = 2;
            }
            else
            {
                result = 4;
            }

            return result;
        }

        [ContractVerification(true)]
        public static int ClassifyMutant2(int a, int b, int c)
        {
            //ClassifyConditionalEnsures(a, b, c, Contract.Result<int>());

            int result;

            // After a quick confirmation that it's a legal triangle, detect any sides of equal length
            if (a <= 0 || b <= 0 || c <= 0)
            {
                result = 4;
                return result;
            }

            result = 0;

            if (a == b)
            {
                result = result + 1;
            }

            if (a == c)
            {
                result = result + 2;
            }

            if (b == c)
            {
                result = result + 3;
            }

            if (result == 0)
            {
                // Confirm it's a legal triangle before declaring it to be scalene
                if (a + b <= c || b + c <= a || a + c <= b)
                {
                    result = 4;
                }
                else
                {
                    result = 1;
                }

                return result;
            }

            // Confirm it's a legal triangle before declaring it to be isosceles or equilateral
            if (result > 3)
            {
                /* result = 3 in original */
                result = 0;
            }
            else if (result == 1 && a + b > c)
            {
                result = 2;
            }
            else if (result == 2 && a + c > b)
            {
                result = 2;
            }
            else if (result == 3 && b + c > a)
            {
                result = 2;
            }
            else
            {
                result = 4;
            }

            return result;
        }

        [ContractVerification(true)]
        public static int ClassifyMutant3(int a, int b, int c)
        {
            //ClassifyConditionalEnsures(a, b, c, Contract.Result<int>());

            int result;

            // After a quick confirmation that it's a legal triangle, detect any sides of equal length
            if (a <= 0 || b <= 0 || c <= 0)
            {
                result = 4;
                return result;
            }

            result = 0;

            if (a == b)
            {
                /* result = result + 1 in original */
                result = a + 1;
            }

            if (a == c)
            {
                result = result + 2;
            }

            if (b == c)
            {
                result = result + 3;
            }

            if (result == 0)
            {
                // Confirm it's a legal triangle before declaring it to be scalene
                if (a + b <= c || b + c <= a || a + c <= b)
                {
                    result = 4;
                }
                else
                {
                    result = 1;
                }

                return result;
            }

            // Confirm it's a legal triangle before declaring it to be isosceles or equilateral
            if (result > 3)
            {
                result = 3;
            }
            else if (result == 1 && a + b > c)
            {
                result = 2;
            }
            else if (result == 2 && a + c > b)
            {
                result = 2;
            }
            else if (result == 3 && b + c > a)
            {
                result = 2;
            }
            else
            {
                result = 4;
            }

            return result;
        }

        [ContractVerification(true)]
        public static int ClassifyMutant4(int a, int b, int c)
        {
            //ClassifyConditionalEnsures(a, b, c, Contract.Result<int>());

            int result;

            // After a quick confirmation that it's a legal triangle, detect any sides of equal length
            if (a <= 0 || b <= 0 || c <= 0)
            {
                result = 4;
                return result;
            }

            result = 0;

            if (a == b)
            {
                result = result + 1;
            }

            if (a == c)
            {
                result = result + 2;
            }

            if (b == c)
            {
                result = result + 3;
            }

            if (result == 0)
            {
                // Confirm it's a legal triangle before declaring it to be scalene
                if (a + b <= c || b + c <= a || a + c <= b)
                {
                    result = 4;
                }
                else
                {
                    result = 1;
                }

                return result;
            }

            // Confirm it's a legal triangle before declaring it to be isosceles or equilateral
            if (result > 3)
            {
                result = 3;
            }
            else if (result == 1 && a + b > c)
            {
                result = 2;
            }
            else if (result == 2 && a + c > b)
            {
                result = 2;
            }
            /* (result == 3 && b + c > a) in original */
            else if (result != 3 && b + c > a)
            {
                result = 2;
            }
            else
            {
                result = 4;
            }

            return result;
        }

        [ContractVerification(true)]
        public static int ClassifyMutant5(int a, int b, int c)
        {
            //ClassifyConditionalEnsures(a, b, c, Contract.Result<int>());

            int result;

            // After a quick confirmation that it's a legal triangle, detect any sides of equal length
            if (a <= 0 || b <= 0 || c <= 0)
            {
                result = 4;
                return result;
            }

            result = 0;

            if (a == b)
            {
                result = result + 1;
            }

            /* (a == c) in original */
            if (a >= c)
            {
                result = result + 2;
            }

            if (b == c)
            {
                result = result + 3;
            }

            if (result == 0)
            {
                // Confirm it's a legal triangle before declaring it to be scalene
                if (a + b <= c || b + c <= a || a + c <= b)
                {
                    result = 4;
                }
                else
                {
                    result = 1;
                }

                return result;
            }

            // Confirm it's a legal triangle before declaring it to be isosceles or equilateral
            if (result > 3)
            {
                result = 3;
            }
            else if (result == 1 && a + b > c)
            {
                result = 2;
            }
            else if (result == 2 && a + c > b)
            {
                result = 2;
            }
            else if (result == 3 && b + c > a)
            {
                result = 2;
            }
            else
            {
                result = 4;
            }

            return result;
        }

        [Conditional(Evaluation.ContractsHintsSymbol)]
        [ContractAbbreviator]
        public static void ClassifyConditionalEnsures(int a, int b, int c, int result)
        {
            Contract.Ensures(result >= 1 && result <= 4);
            Contract.Ensures((result != 1) || CheckScaleneTriangle(a, b, c));
            Contract.Ensures((result != 2) || CheckIsoscelesTriangle(a, b, c));
            Contract.Ensures((result != 3) || CheckEquilateralTriangle(a, b, c));
            Contract.Ensures((result != 4) || !CheckValidTriangle(a, b, c));
        }

        [Conditional(Evaluation.ContractsHintsSymbol)]
        [ContractAbbreviator]
        public static void CheckResultConditionalEnsures(int a, int b, int c, int result)
        {
            Contract.Ensures(Contract.Result<bool>() == (
                (result == 1 && CheckScaleneTriangle(a, b, c))
                || (result == 2 && CheckIsoscelesTriangle(a, b, c))
                || (result == 3 && CheckEquilateralTriangle(a, b, c))
                || (result == 4 && !CheckValidTriangle(a, b, c))));
        }

        [Pure]
        public static bool CheckValidTriangle(int a, int b, int c)
        {
            return a > 0 && b > 0 && c > 0
                && a < b + c && b < a + c && c < a + b;
        }

        [Pure]
        public static bool CheckScaleneTriangle(int a, int b, int c)
        {
            return CheckValidTriangle(a, b, c) && (a != b && b != c && a != c);
        }

        [Pure]
        public static bool CheckIsoscelesTriangle(int a, int b, int c)
        {
            return CheckValidTriangle(a, b, c)
                    && ((a == b && a != c) || (a == c && a != b) || (b == c && b != a));
        }

        [Pure]
        public static bool CheckEquilateralTriangle(int a, int b, int c)
        {
            return CheckValidTriangle(a, b, c) && (a == b && b == c);
        }
    }
}
