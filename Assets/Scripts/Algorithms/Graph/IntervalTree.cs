namespace _Algorithms {
    using System;
    using System.Collections.Generic;

    public class IntervalTree<TValue, TKey> {
        private Comparison<TValue> compare = Comparer<TValue>.Default.Compare;
        private List<TValue> values = new List<TValue>();
        private List<TKey> keys = new List<TKey>();
        private List<(int left, int right, List<int> indices)> nodes = new List<(int,int,List<int>)>();
        private Stack<int> stack = new Stack<int>();
        private Queue<(int min, int max)> queue = new Queue<(int min, int max)>();
        private bool dirtyFlag;
        public IList<TKey> Keys => keys;

        private int Compare(int a, int b){
            int difference = this.compare(values[a], values[b]);
            return difference == 0 ? this.compare(values[a | 1], values[b | 1]) : difference;
        }
        public void Add(TKey key, TValue min, TValue max){
            if(compare(min, max) > 0) throw new ArgumentOutOfRangeException($"{nameof(min)} < {nameof(max)}");
            dirtyFlag = true;
            keys.Add(key); values.Add(min); values.Add(max);
        }
        public void Remove(TKey key){
            dirtyFlag = true;
            int index = keys.IndexOf(key);
            keys.RemoveAt(index);
            values.RemoveAt(2 * index + 1); values.RemoveAt(2 * index);
        }
        public void Clear(){
            dirtyFlag = false;
            keys.Clear(); values.Clear(); nodes.Clear();
        }
        private void Rebuild(){
            if(!dirtyFlag) return;
            dirtyFlag = false;
            nodes.Clear();

            int[] indices = new int[values.Count];
            for(int i = 0; i < indices.Length; indices[i] = i++);
            Array.Sort(indices, Compare);

            queue.Enqueue((0, indices.Length));
            while(queue.Count != 0){
                var node = queue.Dequeue();
                int middle = (node.min + node.max) >> 1;
                var list = new List<int>(){ indices[middle] };
                TValue median = values[list[0]];
                int shift = 0, left = 0, right = 0;
                for(int i = node.min; i < node.max; i++){
                    int index = indices[i] >> 1;
                    if(i < middle && compare(values[2 * index + 1], median) < 0) left++;
                    else if(i > middle && compare(values[2 * index], median) > 0) right++;
                    else{
                        if((indices[i] & 1) == 0) list.Add(index);
                        shift++; continue;
                    }
                    indices[i - shift] = indices[i];
                }
                int nextIndex = nodes.Count + queue.Count;
                nodes.Add((left > 0 ? ++nextIndex : 0, right > 0 ? ++nextIndex : 0, list));
                if(left > 0) queue.Enqueue((node.min, node.min + left));
                if(right > 0) queue.Enqueue((node.min + left, node.min + left + right));
            }
        }
        public IEnumerable<TKey> Query(TValue middle) => Query(middle, middle);
        public IEnumerable<TKey> Query(TValue min, TValue max){
            if(dirtyFlag) Rebuild();
            if(nodes.Count == 0) yield break;
            stack.Push(0);
            while(stack.Count != 0){
                var (left, right, indices) = nodes[stack.Pop()];
                for(int i = 1; i < indices.Count; i++)
                    if(compare(values[2 * indices[i]], max) > 0) break;
                    else if(compare(max, values[2 * indices[i]]) >= 0 && compare(min, values[2 * indices[i] + 1]) <= 0)
                        yield return keys[indices[i]];
                if(left != 0 && compare(min, values[indices[0]]) < 0) stack.Push(left);
                if(right != 0 && compare(max, values[indices[0]]) > 0) stack.Push(right);
            }   
        }
    }
}