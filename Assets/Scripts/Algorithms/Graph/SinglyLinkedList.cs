namespace _Algorithms {
    using System.Collections;
    using System.Collections.Generic;

    public class SinglyLinkedList<T> : IEnumerable<T> {
        public SinglyLinkedList(){}
        public SinglyLinkedList(IEnumerable<T> items){ AppendRange(items); }
        private class LinkedListNode {
            public T item;
            public LinkedListNode next = null;
        }
        private LinkedListNode head = null;
        private LinkedListNode tail = null;
        public int Count{ get; private set; }
        public void Clear(){
            Count = 0;
            head = tail = null;
        }
        public void AddLast(T item){
            LinkedListNode node = new LinkedListNode(){ item = item };
            if(tail != null) tail.next = node;
            tail = node;
            if(head == null) head = node;
            Count++;
        }
        public void AddFirst(T item){
            LinkedListNode node = new LinkedListNode(){ item = item };
            node.next = head;
            head = node;
            if(tail == null) tail = node;
            Count++;
        }
        public void AddLast(SinglyLinkedList<T> list){
            if(list.Count == 0) return;
            if(tail != null) tail.next = list.head;
            tail = list.tail;
            if(head == null) head = list.head;
            Count += list.Count;
            list.Clear();
        }
        public void AddFirst(SinglyLinkedList<T> list){
            if(list.Count == 0) return;
            list.tail.next = head;
            head = list.head;
            if(tail == null) tail = list.tail;
            Count += list.Count;
            list.Clear();
        }
        public void PrependRange(IEnumerable<T> items){
            LinkedListNode first = head;
            LinkedListNode prev = null;
            foreach(T item in items){
                Count++;
                LinkedListNode node = new LinkedListNode(){ item = item };
                if(prev == null) head = node;
                else prev.next = node;
                prev = node;
            }
            if(prev != null) prev.next = first;
            if(tail == null) tail = prev;
        }
        public void AppendRange(IEnumerable<T> items){
            foreach(T item in items) AddLast(item);
        }
        public void Reverse(){
            LinkedListNode temp, prev = null, node = head;
            tail = head;
            while(node != null){
                temp = node.next;
                node.next = prev;
                prev = node;
                node = temp;
            }
            head = prev;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator(){
            for(LinkedListNode node = head; node != null; node = node.next)
                yield return node.item;
        }
    }
}