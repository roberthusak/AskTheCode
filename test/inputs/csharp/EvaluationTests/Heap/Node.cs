using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;

namespace EvaluationTests.Heap
{
    /// <summary>
    /// Simple heap structure to demonstrate heap operations: a node of a linked list.
    /// </summary>
    public class Node
    {
        public int value;
        public Node next;

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        public Node(int value, Node next)
        {
            this.value = value;
            this.next = next;
        }

        /// <summary>
        /// Provides an indirect way to change <see cref="next"/>.
        /// </summary>
        public void SetNext(Node nextNode)
        {
            // Enables testing that the field of the current instance is successfully matched
            next = nextNode;
        }

        /// <summary>
        /// Method with no operations to demonstrate proper calls.
        /// </summary>
        public void Nothing()
        {
        }

        /// <summary>
        /// Swaps this node with the following one if the <see cref="value"/> of the following one is lower than that
        /// of the current one. Serves to demonstrate a more complicated example of heap manipulation.
        /// </summary>
        public Node SwapNode()
        {
            Node result = this;
            if (this.next != null)
            {
                if (this.value > this.next.value)
                {
                    Node t = this.next;
                    this.next = t.next;
                    t.next = this;
                    result = t;

                    Evaluation.InvalidUnreachable();
                }

                Evaluation.ValidAssert(result != null);
                Evaluation.ValidAssert(result.next != null);
                Evaluation.ValidAssert(result.value <= result.next.value);
                Evaluation.InvalidUnreachable();
            }

            return result;
        }

        /// <summary>
        /// Checks that <see cref="Node.Node(int, Node)"/> and <see cref="SetNext(Node)"/> work correctly.
        /// </summary>
        public static void Test(Node a)
        {
            var b = new Node(0, null);
            b.SetNext(a);

            Evaluation.ValidAssert(b.next == a);
        }

        public Node GetByIndex(int i)
        {
            Node r = this;
            while (i > 0)
            {
                r = r.next;
                i = i - 1;
            }

            return r;
        }

        public static void EmbarrasinglyBackward(Node n1, Node n2, Node n3, int a, int b, int c)
        {
            Node na = n1.GetByIndex(a);
            Node nb = n2.GetByIndex(b);
            Node nc = n3.GetByIndex(c);
            if (a == 8 && b == 10 && c == 4)
            {
                Evaluation.InvalidUnreachable();
            }
        }
    }
}
