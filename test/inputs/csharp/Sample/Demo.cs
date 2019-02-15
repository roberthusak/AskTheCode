using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    public class Node
    {
        public int val;
        public Node next;

        public Node(int val, Node next)
        {
            this.val = val;
            this.next = next;
        }
    }

    public static class A
    {
        private static bool LastNode(Node n)
        {
            Debug.Assert(n.next != null || n.val == 0);
            return n.next == null;
        }
        public static void CheckedUse(Node n)
        {
            if (n.val == 0 && LastNode(n)) { /*...*/ }
        }
        public static Node UncheckedUse(Node n)
        {
            Node gen = RandomNode();
            if (LastNode(gen)) { gen.next = n; }
            return gen;
        }
        private static Node RandomNode()
        {
            int v = GetRandomNumber();
            if (v == 0) return new Node(0, null);
            if (v == 1) return new Node(10, null);
            return TooComplicatedOperation();
        }

        private static int GetRandomNumber()
        {
            int n = Console.Read();
            return n;
        }

        private static Node TooComplicatedOperation()
        {
            Node next = new Node(0, null);

            int x = Console.Read();
            int res = 0;
            int i = 0;
            while (i < x)
            {
                int tmp = i % 2;
                if (tmp == 0)
                {
                    res = res - 1;
                }
                else
                {
                    res = res + 17;
                }

                i = i + 1;
            }

            Node result = new Node(i, next);
            return result;
        }
    }
}
