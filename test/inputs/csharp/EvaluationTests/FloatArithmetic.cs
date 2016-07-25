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
    [TestClass]
    [PexClass]
    public partial class FloatArithmetic
    {
        [PexMethod]
        [ContractVerification(true)]
        public void Rosenbrock(double x1, double x2)
        {
            double f = Math.Pow((1 - x1), 2) + 100 * (Math.Pow((x2 - x1 * x1), 2));
            if (f == 0)
            {
                Evaluation.InvalidUnreachable();
            }
        }

        [PexMethod]
        [ContractVerification(true)]
        public void FreudensteinAndRoth(double x1, double x2)
        {
            if ((-13 + x1 + ((5 - x2) * x2 - 2) * x2) + (-29 + x1 + ((x2 + 1) * x2 - 14) * x2) == 0)
            {
                Evaluation.InvalidUnreachable();
            }
        }

        [PexMethod]
        [ContractVerification(true)]
        public void Powell(double x1, double x2)
        {
            if ((Math.Pow(10, 4) * x1 * x2 - 1) == 0 && (Math.Pow(Math.E, -x1) + Math.Pow(Math.E, -x2) - 1.0001) == 0)
            {
                Evaluation.InvalidUnreachable();
            }
        }

        [PexMethod]
        [ContractVerification(true)]
        public void Beale(double x1, double x2)
        {
            if ((1.5 - x1 * (1 - x2)) == 0)
            {
                Evaluation.InvalidUnreachable();
            }
        }

        [PexMethod]
        [ContractVerification(true)]
        public void WoodFunction(double x1, double x2, double x3, double x4)
        {
            if ((10 * (x2 - x1 * x1)) == 0
                && (1 - x1) == 0
                && (Math.Sqrt(90) * (x4 - x3 * x3)) == 0
                && (1 - x3) == 0
                && (Math.Sqrt(10) * (x2 + x4 - 2)) == 0
                && (Math.Pow(10, -0.5) * (x2 - x4)) == 0)
            {
                Evaluation.InvalidUnreachable();
            }
        }
    }
}
