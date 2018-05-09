using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
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
        /// Provides an indirect way to change <see cref="next"/> and demonstrates access to instance fields.
        /// </summary>
        public void SetNext(Node nextNode)
        {
            next = nextNode;
            int myValue = value;
            DoNothing(next);
        }

        /// <summary>
        /// Method with no operations to demonstrate proper calls.
        /// </summary>
        private void DoNothing(Node a)
        {
        }
    }

    /// <summary>
    /// Collection of straightforward scenarios.
    /// </summary>
    public static class Heap
    {
        /// <summary>
        /// Assignment of references.
        /// </summary>
        public static void SimpleReferencePassing(Node a, Node b, Node c)
        {
            a = b;
            c = null;
        }

        /// <summary>
        /// Shows a complicated sequence of field accesses.
        /// </summary>
        public static void FieldAccess(Node a, Node b)
        {
            int a_val = a.value;
            b.value = 10;
            Node c = a.next;
            b.next = c;

            a.next = b.next;
            a.next.next = c;
            a = c.next.next;
            a.next.next = b.next.next;
        }

        /// <summary>
        /// Shows a simple instance method call.
        /// </summary>
        public static void SimpleMethodCall(Node a, Node b)
        {
            a.SetNext(b);
        }

        /// <summary>
        /// Shows a simple constructor call.
        /// </summary>
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

        /// <summary>
        /// Shows a combination of multiple operations.
        /// </summary>
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

        /// <summary>
        /// Shows that reference comparison works well even if its not directly constraining the current path.
        /// </summary>
        public static void DelayedComparison(Node a, Node b)
        {
            bool wereEqual = (a == b);
            a = new Node(0, b);

            if (wereEqual)
            {
                Debug.Assert(a != b);
            }
        }
    }
}
