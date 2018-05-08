using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;

namespace EvaluationTests.Heap
{
    public static class ComparisonVsAssignment
    {
        public static void SimpleConflict(Node a)
        {
            Node b = new Node(0, a);
            if (a == b)
            {
                Evaluation.ValidUnreachable();
            }
        }

        public static void IndirectConflict(Node a)
        {
            Node c = new Node(0, a);
            Node b = c;
            if (a == b)
            {
                Evaluation.ValidUnreachable();
            }
        }

        public static void IndirectNestedConflict(Node e, Node f)
        {
            Node a = new Node(0, null);
            Node b = f;
            if (a == b && e == f)
            {
                Evaluation.ValidUnreachable();
            }
        }

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
