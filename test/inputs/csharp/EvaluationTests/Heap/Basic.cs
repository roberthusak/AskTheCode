using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;

namespace EvaluationTests.Heap
{
    /// <summary>
    /// Collection of straightforward scenarios.
    /// </summary>
    public static class Basic
    {
        /// <summary>
        /// Checks that a constructor successfully initializes a heap object.
        /// </summary>
        public static void SimpleConstructor()
        {
            var n = new Node(0, null);

            Evaluation.ValidAssert(n != null);
            Evaluation.ValidAssert(n.value == 0);
        }

        /// <summary>
        /// Checks that even an empty method on an object enforces it not to be null. Otherwise, it would have thrown a
        /// <see cref="NullReferenceException"/> and wouldn't reach the assertion.
        /// </summary>
        public static void EmptyMethodCall(Node a)
        {
            a.Nothing();

            Evaluation.ValidAssert(a != null);
        }

        /// <summary>
        /// Checks that an instance method cal is successfully modelled.
        /// </summary>
        public static void SimpleMethodCall(Node a, Node b)
        {
            a.SetNext(b);

            Evaluation.ValidAssert(a != null);
            Evaluation.ValidAssert(a.next == b);
        }

        /// <summary>
        /// Checks that a complicated sequence of field accesses does not cause an error in the algorithm and that they
        /// are modelled well.
        /// </summary>
        public static void FieldAccess(Node a, Node b)
        {
            int a_val = a.value;
            b.value = 10;
            Node c = a.next;
            b.next = c;

            Evaluation.ValidAssert(b.next == c);

            a.next = b.next;
            a.next.next = c;
            a = c.next.next;
            a.next.next = b.next.next;

            Evaluation.InvalidUnreachable();
            Evaluation.ValidAssert(a.next.next == b.next.next);
        }

        /// <summary>
        /// Checks that expected implications of reference comparison hold.
        /// </summary>
        public static void SimpleComparison(Node a, Node b)
        {
            if (a == b)
            {
                Evaluation.ValidAssert(a == b);
                Evaluation.ValidAssert(a.next == b.next);
            }
            else
            {
                Evaluation.ValidAssert(a != b);

                a.value = 5;
                b.value = 10;

                // Note that this statement is unreachable if a or b is null, hence valid
                Evaluation.ValidAssert(a.value != b.value);
            }
        }

        /// <summary>
        /// Checks that reference comparison works well even if its not directly constraining the current path.
        /// </summary>
        public static void DelayedComparison(Node a, Node b)
        {
            bool wereEqual = (a == b);
            a = new Node(0, b);

            if (wereEqual)
            {
                Evaluation.ValidAssert(a != b);
            }
        }

        /// <summary>
        /// Checks that a combination of multiple operations is modelled well.
        /// </summary>
        public static void SimpleBranching(Node n)
        {
            if (n == null)
            {
                n = new Node(0, n);
                Evaluation.ValidAssert(n.next == null);
            }
            else
            {
                int val = n.value;
                Evaluation.ValidAssert(n != null);
            }
        }

        /// <summary>
        /// Checks that a reference loaded from heap is not strongly constrained to be from the input heap. Instead,
        /// it must be constrained only conditionally.
        /// </summary>
        public static void ReadReferenceConstraints(Node b)
        {
            Node a = new Node(0, null);
            b.next = a;
            Node c = b.next;

            Evaluation.InvalidUnreachable();
        }
    }
}
