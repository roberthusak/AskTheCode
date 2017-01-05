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
    /// Example program simulating a simple interactive banking application
    /// </summary>
    /// <remarks>
    /// Inspired by source code in the ASE08 paper from Xie Inkumsah.
    /// </remarks>
    [TestClass]
    [PexClass]
    public partial class BankAccount
    {
        [PexMethod]
        [ContractVerification(true)]
        public int ProcessOperations(int startBalance)
        {
            int balance = startBalance;
            int withdrawalsCount = 0;

            while (PexChoose.Value<bool>("operationLoop"))
            {
                bool operation = PexChoose.Value<bool>("operationType");
                int amount = PexChoose.Value<int>("amount");

                if (operation)
                {
                    // Deposit
                    if (amount > 0)
                    {
                        balance = balance + amount;
                    }
                    else
                    {
                        Evaluation.InvalidUnreachable();
                    }
                }
                else
                {
                    // Withdraw
                    if (amount > balance)
                    {
                        Evaluation.InvalidUnreachable();
                    }
                    else if (withdrawalsCount >= 10)
                    {
                        Evaluation.InvalidUnreachable();
                    }
                    else
                    {
                        balance = balance - amount;
                        withdrawalsCount = withdrawalsCount + 1;
                    }
                }
            }

            return balance;
        }
    }
}
