using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;
using Microsoft.Pex.Framework;
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
    [TestClass]
    [PexClass]
    public class TriangleClassification
    {
        [PexMethod]
        [ContractVerification(true)]
        public void CheckOriginal(int a, int b, int c)
        {
            int result = ClassifyOriginal(a, b, c);
            bool isValid = CheckResult(a, b, c, result);

            Evaluation.ValidAssert(isValid);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void CheckMutant1(int a, int b, int c)
        {
            int result = ClassifyMutant1(a, b, c);
            bool isValid = CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void CheckMutant2(int a, int b, int c)
        {
            int result = ClassifyMutant2(a, b, c);
            bool isValid = CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void CheckMutant3(int a, int b, int c)
        {
            int result = ClassifyMutant3(a, b, c);
            bool isValid = CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void CheckMutant4(int a, int b, int c)
        {
            int result = ClassifyMutant4(a, b, c);
            bool isValid = CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);
        }

        [PexMethod]
        [ContractVerification(true)]
        public void CheckMutant5(int a, int b, int c)
        {
            int result = ClassifyMutant5(a, b, c);
            bool isValid = CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);
        }

        private static bool CheckResult(int a, int b, int c, int result)
        {
            if (result == 1)
            {
                return (a != b && b != c && a != c);
            }
            else if (result == 2)
            {
                return (a == b && a != c) || (a == c && a != b)
                    || (b == c && b != a);
            }
            else if (result == 3)
            {
                return (a == b && b == c);
            }
            else if (result == 4)
            {
                return (a <= 0 || b <= 0 || c <= 0
                    || a <= b + c || b <= a + c || c <= b + c);
            }
            else
            {
                return false;
            }
        }

        private static int ClassifyOriginal(int a, int b, int c)
        {
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

        private static int ClassifyMutant1(int a, int b, int c)
        {
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

        private static int ClassifyMutant2(int a, int b, int c)
        {
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

        private static int ClassifyMutant3(int a, int b, int c)
        {
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

        private static int ClassifyMutant4(int a, int b, int c)
        {
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

        private static int ClassifyMutant5(int a, int b, int c)
        {
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
    }
}
