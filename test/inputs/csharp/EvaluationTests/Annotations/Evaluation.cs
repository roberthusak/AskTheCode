using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Pex.Framework;

namespace EvaluationTests.Annotations
{
    public static class Evaluation
    {
        /// <summary>
        /// Symbol used to conditionally add contract hints.
        /// </summary>
        public const string ContractsHintsSymbol = "EVALUATION_CC_HINTS";

        /// <remarks>
        /// During the contract checking, the precondition is propagated to the call site as an assertion.
        /// </remarks>
        public static void ValidAssert(bool condition)
        {
            Contract.Requires(condition);
        }

        /// <summary>
        /// Valid assertion skipped in tests.
        /// </summary>
        /// <remarks>
        /// During the contract checking, the precondition is propagated to the call site as an assertion.
        /// </remarks>
        public static void SkippedValidAssert(bool condition)
        {
            Contract.Requires(condition);
        }

        /// <remarks>
        /// During the contract checking, the precondition is propagated to the call site as an assertion.
        /// </remarks>
        public static void InvalidAssert(bool condition)
        {
            Contract.Requires(condition);
        }

        /// <summary>
        /// Invalid assertion skipped in tests.
        /// </summary>
        /// <remarks>
        /// During the contract checking, the precondition is propagated to the call site as an assertion.
        /// </remarks>
        public static void SkippedInvalidAssert(bool condition)
        {
            Contract.Requires(condition);
        }

        /// <remarks>
        /// This precondition is satisfied only if the call site is not reachable.
        /// </remarks>
        public static void ValidUnreachable()
        {
            Contract.Requires(false);
        }

        /// <remarks>
        /// This precondition is satisfied only if the call site is not reachable.
        /// </remarks>
        public static void SkippedValidUnreachable()
        {
            Contract.Requires(false);
        }

        /// <remarks>
        /// This precondition is satisfied only if the call site is not reachable.
        /// </remarks>
        public static void InvalidUnreachable()
        {
            Contract.Requires(false);
        }

        /// <remarks>
        /// This precondition is satisfied only if the call site is not reachable.
        /// </remarks>
        public static void SkippedInvalidUnreachable()
        {
            Contract.Requires(false);
        }
    }
}
