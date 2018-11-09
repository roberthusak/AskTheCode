using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;

namespace EvaluationTests.Heap
{
    public class DoublyLinkedList
    {
        private class ListNode
        {
            public int value;
            public ListNode previous;
            public ListNode next;

            public ListNode(int value)
            {
                this.value = value;
            }
        }

        private ListNode head;
        private int count;

        public int GetCount()
        {
            return this.count;
        }

        public DoublyLinkedList(DoublyLinkedList sth)
        {
            ListNode n = new ListNode(0);
            this.head = n;
            this.head.next = n;
            this.head.previous = n;
            this.count = 0;
        }

        public void Add(int value)
        {
            var added = new ListNode(value);
            added.previous = this.head.previous;
            added.next = this.head;

            this.head.previous.next = added;
            this.head.previous = added;
        }

        public void RemoveLast()
        {
            if (this.head.previous == this.head)
            {
                throw new InvalidOperationException();
            }
            else
            {
                ListNode prevLast = this.head.previous.previous;
                prevLast.next = this.head;
                this.head.previous = prevLast;
            }
        }

        public bool Contains(int value)
        {
            ListNode node = this.head.next;

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            if (node == this.head)
            {
                return false;
            }
            else
            {
                if (node.value == value)
                {
                    return true;
                }
                else
                {
                    node = node.next;
                }
            }

            return false;
        }

        public static DoublyLinkedList CreateSample()
        {
            DoublyLinkedList list = new DoublyLinkedList(null);
            list.Add(1);
            list.Add(8);
            list.Add(5);
            list.Add(20);
            list.Add(11);
            list.Add(30);
            list.Add(80);
            list.Add(41);
            list.Add(8);
            list.Add(42);

            return list;
        }

        public static void TestContains()
        {
            var list = CreateSample();

            bool a1 = list.Contains(42);
            Evaluation.ValidAssert(a1);

            bool a2 = !list.Contains(0);
            Evaluation.ValidAssert(a2);
        }

        public static void TestRemoveLast()
        {
            var list = CreateSample();
            list.RemoveLast();

            bool a = !list.Contains(42);
            Evaluation.ValidAssert(a);
        }
    }
}
