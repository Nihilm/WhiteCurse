namespace _Algorithms {
    using System.Collections.Generic;

    public interface IGraph<T> {
        int NodeCount{ get; }
        T this[int index]{ get; }
        int this[int source, int target]{ get; }
        ICollection<T> Nodes{ get; }
        ICollection<(int source, int target)> Edges{ get; }
        ICollection<int> Neighbours(int node);
        int AddNode(T node);
        void AddEdge(int source, int target);
    }

    public static class GraphExtensions {
        public static int Connected<T>(this IGraph<T> graph){
            int[] visited = new int[graph.NodeCount];
            Stack<int> stack = new Stack<int>();
            int subgraphs = 0;
            for(int i = 0; i < visited.Length; i++){
                if(visited[i] != 0) continue;
                subgraphs++;
                stack.Push(i);
                while(stack.Count != 0){
                    int node = stack.Pop();
                    visited[node] = subgraphs;
                    foreach(int neighbour in graph.Neighbours(node)){
                        if(visited[neighbour] != 0) continue;
                        stack.Push(neighbour);
                    }
                }
            }
            return subgraphs;
        }
        public static (int[] left, int[] right)? Bipartite<T>(this IGraph<T> graph){
			var coloring = new int[graph.NodeCount];
			var queue = new Queue<int>();
            int opposite = 0;
			for(int i = 0; i < graph.NodeCount; i++){
				if(coloring[i] != 0) continue;
				queue.Enqueue(i);
				coloring[i] = 1;
				while(queue.Count != 0){
					int node = queue.Dequeue();
                    opposite += coloring[node] & 1;
					foreach(int neighbour in graph.Neighbours(node))
						if(coloring[neighbour] == 0){
							coloring[neighbour] = coloring[node] ^ 3;
							queue.Enqueue(neighbour);
						}else if(coloring[neighbour] == coloring[node]) return null;
				}
			}
			int[] left = new int[opposite];
			int[] right = new int[coloring.Length - opposite];
			for(int l = 0, r = 0, i = 0; i < coloring.Length; i++)
                if(coloring[i] == 1) left[l++] = i;
                else right[r++] = i;
			return (left, right);
        }
    }
}