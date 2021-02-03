namespace _Algorithms {
    using System.Collections.Generic;

    public static class HopcroftKarp {
        public static List<(int left, int right)> MaximumMatching<T>(IGraph<T> graph, (int[], int[])? partitions = null){
            var (left, right) = partitions ?? graph.Bipartite() ?? throw new System.ArgumentException($"Graph {nameof(graph)} is not bipartite!");
            int Nil = left.Length;

            var queue = new Queue<int>();
            var stack = new Stack<int>();
            var iterators = new Stack<IEnumerator<int>>();
            var depth = new int[left.Length + 1];
            var pairs = new int[graph.NodeCount];
            pairs.Fill(Nil);

            while(true){
                for(int l = 0; l < left.Length; l++)
                    if(pairs[left[l]] == Nil){
                        depth[l] = 0;
                        queue.Enqueue(l);
                    }else depth[l] = int.MaxValue;

                depth[Nil] = int.MaxValue;
                while(queue.Count != 0){
                    int l = queue.Dequeue();
                    if(depth[l] >= depth[Nil]) continue;
                    foreach(int r in graph.Neighbours(left[l]))
                        if(depth[pairs[r]] == int.MaxValue){
                            depth[pairs[r]] = depth[l] + 1;
                            queue.Enqueue(pairs[r]);
                        }
                }
                if(depth[Nil] == int.MaxValue) break;
                
                for(int l = 0; l < left.Length; l++){
                    if(pairs[left[l]] != Nil) continue;
                    stack.Push(l);
                    bool augment = false;
                    while(stack.Count != 0){
                        int next = stack.Pop();
                        if(next == Nil){ augment = true; continue; }
                        var iterator = iterators.Count > stack.Count
                        ? iterators.Pop() : graph.Neighbours(left[next]).GetEnumerator();

                        if(augment){
                            pairs[iterator.Current] = next;
                            pairs[left[next]] = Nil + 1 + iterator.Current;
                        }else if(iterator.MoveNext()){
                            iterators.Push(iterator); stack.Push(next);
                            if(depth[pairs[iterator.Current]] == depth[next] + 1)
                                stack.Push(pairs[iterator.Current]);
                            continue;
                        }else depth[next] = int.MaxValue;
                        iterator.Dispose();
                    }
                }
            }
            var matching = new List<(int,int)>();
            for(int l = 0; l < left.Length; l++)
                if(pairs[left[l]] != Nil)
                    matching.Add((left[l], pairs[left[l]] - Nil - 1));
			return matching;
        }
        //https://stackoverflow.com/questions/42836016/algorithm-for-minimum-vertex-cover-in-bipartite-graph
        //Given bipartite graph G, separated between X and Y find maximum matching set of edges M
        //Find set U of unmatched vertices in X (not connected to any edge in M)
        //Build set Z of vertices in U or connected to U via alternating (edge not in M, edge in M)
        //Minimum Vertex Cover = (X \ Z) U (Y ∩ Z)
        //https://ali-ibrahim137.github.io/competitive/programming/2020/01/02/maximum-independent-set-in-bipartite-graphs.html
        public static (List<int> left, List<int> right) MinimumVertexCover<T>(IGraph<T> graph, bool independentSet = false, (int[] left, int[] right)? partitions = null){
            var parts = partitions ?? graph.Bipartite() ?? throw new System.ArgumentException($"Graph {nameof(graph)} is not bipartite!");
            int[] pairs = new int[graph.NodeCount];
            bool[] visited = new bool[graph.NodeCount];
            pairs.Fill(-1);
            foreach(var edge in MaximumMatching(graph, parts)){
                pairs[edge.left] = edge.right;
                pairs[edge.right] = edge.left;
            }
            Stack<int> stack = new Stack<int>();
            for(int i = 0; i < parts.left.Length; i++){
                int node = parts.left[i];
                if(visited[node] || pairs[node] != -1) continue;
                stack.Push(node);
                while(stack.Count != 0){
                    node = stack.Pop();
                    visited[node] = true;
                    foreach(int neighbour in graph.Neighbours(node)){
                        if(visited[neighbour]) continue;
                        visited[neighbour] = true;
                        if(pairs[neighbour] != -1 && pairs[neighbour] != node)
                            stack.Push(pairs[neighbour]);
                    }
                }
            }
            List<int> left = new List<int>();
            List<int> right = new List<int>();
            foreach(int i in parts.left) if(visited[i] == independentSet) left.Add(i);
            foreach(int i in parts.right) if(visited[i] == !independentSet) right.Add(i);
            return (left, right);
        }
    }
}