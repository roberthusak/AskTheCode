using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;

namespace EvaluationTests.Heap
{
    public class Node
    {
        public int value;
        public Node next;

        public Node()
        {
        }

        public Node(int value, Node next)
        {
            this.value = value;
            this.next = next;
        }

        public void SetNext(Node nextNode)
        {
            // Test that the field of the current instance is successfully matched
            next = nextNode;
        }

        public void Nothing()
        {
        }

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

        public static void Test(Node a)
        {
            var b = new Node(0, null);
            b.SetNext(a);

            Evaluation.ValidAssert(b.next == a);
        }
    }
}
