using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;

namespace EvaluationTests.Heap
{
    public static class Basic
    {
        public static void SimpleConstructor()
        {
            var n = new Node(0, null);

            Evaluation.ValidAssert(n != null);
            Evaluation.ValidAssert(n.value == 0);
        }

        public static void SimpleMethodCall(Node a, Node b)
        {
            a.SetNext(b);
            a.next.SetNext(b);
            a.next.SetNext(b.next);

            Evaluation.ValidAssert(a.next.next == b.next);
        }

        public static void SimpleFieldAccess(Node a, Node b)
        {
            int a_val = a.value;
            b.value = 10;
            Node c = a.next;
            b.next = c;

            a.next = b.next;
            a.next.next = c;
            a = c.next.next;
            a.next.next = b.next.next;

            Evaluation.ValidAssert(a.next.next == b.next.next);
        }

        public static void SimpleComparison(Node a, Node b)
        {
            if (a == b)
            {
                Evaluation.ValidAssert(a.next == b.next);
            }
            else
            {
                a.value = 5;
                b.value = 10;

                // Note that this statement is unreachable if a or b is null
                Evaluation.ValidAssert(a.value != b.value);
            }
        }

        public static void DelayedComparison(Node a, Node b)
        {
            bool wereEqual = (a == b);
            a = new Node(0, b);

            if (wereEqual)
            {
                Evaluation.ValidAssert(a != b);
            }
        }

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

        public static void ReadReferenceConstraints(Node b)
        {
            Node a = new Node(0, null);
            b.next = a;
            Node c = b.next;

            Evaluation.InvalidUnreachable();
        }
    }
}
