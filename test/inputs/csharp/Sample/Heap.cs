using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Node
    {
        public int value;
        public Node next;

        public Node(int value, Node next)
        {
            this.value = value;
            this.next = next;
        }
    }

    public static class Heap
    {
        public static void SimpleConstructor()
        {
            var n = new Node(0, null);

            Debug.Assert(n != null);
            Debug.Assert(n.value == 0);
        }

        public static void SimpleBranching(Node n)
        {
            if (n == null)
            {
                n = new Node(0, n);
                Debug.Assert(n.next == null);
            }
            else
            {
                int val = n.value;
                Debug.Assert(n != null);
            }
        }

        public static void SimpleComparison(Node a, Node b)
        {
            if (a == b)
            {
                Debug.Assert(a.next == b.next);
            }
            else
            {
                a.value = 5;
                b.value = 10;

                // Note that this statement is unreachable if a or b is null
                Debug.Assert(a.value != b.value);
            }
        }

        public static void DelayedComparison(Node a, Node b, int i)
        {
            bool wereEqual = (a == b);
            a = new Node(0, b);
            i = 2 * i;
            bool cond = wereEqual || i < 10;

            if (!cond)
            {
                Debug.Assert(a == b);
            }
        }
    }
}
