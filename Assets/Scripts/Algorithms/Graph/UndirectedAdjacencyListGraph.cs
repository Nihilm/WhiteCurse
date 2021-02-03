namespace _Algorithms {
    using System.Collections.Generic;

    public class UndirectedAdjacencyListGraph<T> : IGraph<T> {
        private readonly List<T> nodes = new List<T>();
        private readonly List<(int, int)> edges = new List<(int, int)>();
        private readonly List<LinkedList<int>> adjacencyList = new List<LinkedList<int>>();

        public int NodeCount => nodes.Count;
        public T this[int index] => nodes[index];
        public int this[int source, int target] => adjacencyList[source].Contains(target) ? 1 : 0;
        public ICollection<T> Nodes => nodes;
        public ICollection<(int source, int target)> Edges => edges;
        public ICollection<int> Neighbours(int node) => adjacencyList[node];
        public int AddNode(T node){
            nodes.Add(node);
            adjacencyList.Add(new LinkedList<int>());
            return nodes.Count - 1;
        }
        public void AddEdge(int source, int target){
            adjacencyList[source].AddLast(target);
            adjacencyList[target].AddLast(source);
            edges.Add((source, target));
        }
    }
}