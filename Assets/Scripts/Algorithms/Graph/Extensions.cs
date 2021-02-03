namespace _Algorithms {
    using System;
    using System.Collections.Generic;

    public static class Extensions {
        
        public enum NodeFlag{ NotDiscovered, Discovered, Closed } 
        public static IEnumerable<(int,int,bool)> TraverseDFS<T>(this IGraph<T> graph){
            IEnumerator<int>[] neighbours = new IEnumerator<int>[graph.NodeCount];
            NodeFlag[] flags = new NodeFlag[graph.NodeCount];
            Stack<int> stack = new Stack<int>();
            for(int i = 0; i < graph.NodeCount; i++){
                if(flags[i] != NodeFlag.NotDiscovered) continue;
                flags[i] = NodeFlag.Discovered;
                stack.Push(i);
                yield return (-1, i, false);
                while(stack.Count != 0){
                    int node = stack.Peek();
                    if(neighbours[node] == null) neighbours[node] = graph.Neighbours(node).GetEnumerator();
                    else yield return (neighbours[node].Current, node, true);
                    if(neighbours[node].MoveNext()){
                        int neighbour = neighbours[node].Current;
                        if(flags[neighbour] == NodeFlag.NotDiscovered){
                            flags[neighbour] = NodeFlag.Discovered;
                            stack.Push(neighbour);
                            yield return (node, neighbour, false);
                        }else if(flags[neighbour] == NodeFlag.Discovered){
                            yield return (node, neighbour, true);
                        }
                    }else{
                        stack.Pop();
                        neighbours[node].Dispose();
                        flags[node] = NodeFlag.Closed;
                        yield return (node, -1, true);
                    }
                }
            }
        }
        public static IEnumerable<(int,int)> TraversePlanarEmbedding(this IEnumerable<(int,int)> edges, (int,int)[][] embedding){
            var adjacency = new int[embedding.Length, embedding.Length];
            var visited = new bool[embedding.Length, embedding.Length];
			for(int i = 0; i < embedding.Length; i++)
			for(int j = 0; j < embedding[i].Length; j++){
				int prev = embedding[i][j].Sibling(i);
				int next = embedding[i][j != embedding[i].Length - 1 ? j + 1 : 0].Sibling(i);
				adjacency[i,prev] = next;
			}
            var selfLoops = new LinkedList<(int,int)>();
            foreach(var edge in edges){
                if(edge.Item1 == edge.Item2) selfLoops.AddLast(edge);
                var _edge = edge;
                foreach(var node in edge.ToEnumerable()){
                    int _node = node;
                    if(visited[_node, _edge.Sibling(_node)]) continue;
                    yield return (-1, _node);

                    while(!visited[_node, _edge.Sibling(_node)]){
                        visited[_node, _edge.Sibling(_node)] = true;
						int prev = _node;
						_node = _edge.Sibling(_node);
                        yield return (prev, _node);
						_edge = (_node, adjacency[_node, prev]);
                    }
                    yield return (_node, -1);
                }
            }
            foreach(var edge in selfLoops){
                yield return (-1, edge.Item1);
                yield return edge;
                yield return (edge.Item2, -1);
            }
        }
        public static int[] BucketSort(this IEnumerable<int> values, int bucketCount){
            LinkedList<int>[] buckets = new LinkedList<int>[bucketCount];
            int length = 0;
            foreach(int value in values){
                if(buckets[value] == null) buckets[value] = new LinkedList<int>();
                buckets[value].AddLast(length++);
            }
            int[] indices = new int[length];
            length = 0;
            foreach(LinkedList<int> bucket in buckets)
            if(bucket != null) foreach(int index in bucket)
            indices[length++] = index;
            return indices;
        }
        public static IEnumerable<T> ToEnumerable<T>(this (T,T) target){
            yield return target.Item1;
            yield return target.Item2;
        }
        public static int Sibling(this (int,int) pair, int target) => target == pair.Item1 ? pair.Item2 : pair.Item1;
        public static bool ScrambledEquals<T>(this IEnumerable<T> left, IEnumerable<T> right){
            var occurrences = new Dictionary<T, int>();
            int matching = 0;
            foreach(T item in left){
                matching++;
                if(occurrences.ContainsKey(item)) occurrences[item]++;
                else occurrences.Add(item, 1);
            }
            foreach(T item in right){
                matching--;
                if(!occurrences.ContainsKey(item)) return false;
                if(occurrences[item]-- <= 0) return false;
            }
            return matching == 0;
        }
        public static void Reorder<T>(this List<T> list, Comparison<T> comparer, T threshold){
            int shift = 0;
            for(int j, i = 0; i < list.Count; i++){
                T value = list[i];
                if(comparer(value, threshold) <= 0) shift++;
                else{
                    for(j = i - shift; j > 0 && comparer(value, list[j - 1]) < 0; j--)
                        list[j] = list[j - 1];
                    list[j] = value;
                }
            }
            if(shift != 0) list.RemoveRange(list.Count - shift, shift);
        }
        public static void Shuffle<T>(this IList<T> list, Random random){
            for(int i = list.Count - 1; i > 0; i--){
                int j = random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
        public static IEnumerable<int[]> Combinations(int items, int size){
            if(size > items) yield break;
            int[] indices = new int[size];
            for(int i = 0; true; indices[i]++){
                for(++i; i < size; i++) indices[i] = indices[i - 1] + 1;
                yield return indices;
                for(i = size - 1; i >= 0 && indices[i] == items - size + i; i--);
                if(i < 0) break;
            }
        }
        private static void Heapify<T>(IList<T> list, int i, int length, Comparison<T> comparer){
            for(int j; (j = 2 * i + 1) < length; i = j){
                j = j + 1 < length && comparer(list[j + 1], list[j]) > 0 ? j + 1 : j;
                if(comparer(list[j], list[i]) <= 0) break;
                T temp = list[i]; list[i] = list[j]; list[j] = temp;
            }
        }
        public static void Heapsort<T>(this IList<T> list, Comparison<T> comparer){
            for(int i = list.Count >> 1; i >= 0; i--)
                Heapify<T>(list, i, list.Count, comparer);
            for(int i = list.Count - 1; i > 0; i--){
                T temp = list[0]; list[0] = list[i]; list[i] = temp;
                Heapify(list, 0, i, comparer);
            }
        }
        public static T[,] Transpose<T>(this T[,] array){
            T[,] transposed = new T[array.GetLength(1), array.GetLength(0)];
            for(int i = array.GetLength(0) - 1; i >= 0; i--)
            for(int j = array.GetLength(1) - 1; j >= 0; j--)
            transposed[j,i] = array[i,j];
            return transposed;
        }
        public static T[,] Array2D<T>(this T[] input, int columns, int rows){
            T[,] output = new T[columns, rows];
            for(int c = 0; c < columns; c++)
            for(int r = 0; r < rows; r++)
            output[c, r] = input[c + r * columns];
            return output;
        }
    }
}