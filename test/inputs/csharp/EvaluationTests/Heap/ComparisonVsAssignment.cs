using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;

namespace EvaluationTests.Heap
{
    /// <summary>
    /// Scenarios demonstrating that it is not possible to treat a comparison the same way as an assignment.
    /// </summary>
    public static class ComparisonVsAssignment
    {
        /// <summary>
        /// Checks that a comparison constraints two references to point to the same location, but an initialization of
        /// just one of them causes a conflict.
        /// </summary>
        public static void SimpleConflict(Node a)
        {
            Node b = new Node(0, a);
            if (a == b)
            {
                Evaluation.ValidUnreachable();
            }
        }

        /// <summary>
        /// Checks that a conflict is caused even when an additional assigned reference variable is introduced.
        /// </summary>
        public static void IndirectConflict(Node a)
        {
            Node c = new Node(0, a);
            Node b = c;
            if (a == b)
            {
                Evaluation.ValidUnreachable();
            }
        }

        /// <summary>
        /// Checks that even a more complicated situation causes a conflict.
        /// </summary>
        public static void IndirectNestedConflict(Node e, Node f)
        {
            Node a = new Node(0, null);
            Node b = f;
            if (a == b && e == f)
            {
                Evaluation.ValidUnreachable();
            }
        }

        /// <summary>
        /// Checks that a situation without a conflict is reachable.
        /// </summary>
        public static void AssignedEquivalence()
        {
            Node c = new Node(0, null);
            Node b = c;
            Node a = b;
            if (a == b)
            {
                Evaluation.InvalidUnreachable();
            }
            else
            {
                Evaluation.ValidUnreachable();
            }
        }
    }
}
