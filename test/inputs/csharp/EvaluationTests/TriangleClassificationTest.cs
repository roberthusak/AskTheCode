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
    // TODO: Clean up the commented pieces of code (after finding out whether to use them or not)
    [TestClass]
    [PexClass(typeof(TriangleClassification))]
    public partial class TriangleClassificationTest
    {
        [PexMethod]
        [ContractVerification(true)]
        public int CheckOriginal(int a, int b, int c)
        {
            PexAssume.IsTrue(MaxValuesAssumptions(a, b, c));

            int result = TriangleClassification.ClassifyOriginal(a, b, c);
            bool isValid = TriangleClassification.CheckResult(a, b, c, result);

            Evaluation.ValidAssert(isValid);

            return result;
        }

        [PerformanceEvaluation("Trityp1")]
        [PexMethod]
        ////[PexAssertReachEventually]
        [ContractVerification(true)]
        public void CheckMutant1(int a, int b, int c)
        {
            //PexAssume.IsTrue(MaxValuesAssumptions(a, b, c));

            int result = TriangleClassification.ClassifyMutant1(a, b, c);
            bool isValid = TriangleClassification.CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);

            ////if (!isValid)
            ////{
            ////    PexAssert.ReachEventually();
            ////    Evaluation.InvalidUnreachable();
            ////}
        }

        [PerformanceEvaluation("Trityp2")]
        [PexMethod]
        ////[PexAssertReachEventually]
        [ContractVerification(true)]
        public void CheckMutant2(int a, int b, int c)
        {
            //PexAssume.IsTrue(MaxValuesAssumptions(a, b, c));

            int result = TriangleClassification.ClassifyMutant2(a, b, c);
            bool isValid = TriangleClassification.CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);

            ////if (!isValid)
            ////{
            ////    PexAssert.ReachEventually();
            ////    Evaluation.InvalidUnreachable();
            ////}
        }

        [PerformanceEvaluation("Trityp3")]
        [PexMethod]
        ////[PexAssertReachEventually]
        [ContractVerification(true)]
        public void CheckMutant3(int a, int b, int c)
        {
            //PexAssume.IsTrue(MaxValuesAssumptions(a, b, c));

            int result = TriangleClassification.ClassifyMutant3(a, b, c);
            bool isValid = TriangleClassification.CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);

            ////if (!isValid)
            ////{
            ////    PexAssert.ReachEventually();
            ////    Evaluation.InvalidUnreachable();
            ////}
        }

        [PerformanceEvaluation("Trityp4")]
        [PexMethod]
        ////[PexAssertReachEventually]
        [ContractVerification(true)]
        public void CheckMutant4(int a, int b, int c)
        {
            //PexAssume.IsTrue(MaxValuesAssumptions(a, b, c));

            int result = TriangleClassification.ClassifyMutant4(a, b, c);
            bool isValid = TriangleClassification.CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);

            ////if (!isValid)
            ////{
            ////    PexAssert.ReachEventually();
            ////    Evaluation.InvalidUnreachable();
            ////}
        }

        [PerformanceEvaluation("Trityp5")]
        [PexMethod]
        ////[PexAssertReachEventually]
        [ContractVerification(true)]
        public void CheckMutant5(int a, int b, int c)
        {
            //PexAssume.IsTrue(MaxValuesAssumptions(a, b, c));

            int result = TriangleClassification.ClassifyMutant5(a, b, c);
            bool isValid = TriangleClassification.CheckResult(a, b, c, result);

            Evaluation.InvalidAssert(isValid);

            ////if (!isValid)
            ////{
            ////    PexAssert.ReachEventually();
            ////    Evaluation.InvalidUnreachable();
            ////}
        }

        private static bool MaxValuesAssumptions(int a, int b, int c)
        {
            return (a < 10000) && (b < 10000) && (c < 10000);
        }
    }
}
