namespace _Algorithms {
    using System.Collections.Generic;

    //https://github.com/mikolalysenko/planar-dual/blob/master/loops.js
    public class ChainDecomposition<T> {
        public bool mergeChains = false;
        public bool ascending = true;
        public bool breadthFirstSearch = true;

        private int ReorderFace(int a, int b) => depth[b] == depth[a] ? order[a] - order[b] : depth[b] - depth[a];
        private int ReorderAscending(List<int> a, List<int> b) =>
        b == null ? a.Count : a.Count == b.Count ? order[a[0]] - order[b[0]] : a.Count - b.Count;
        private int ReorderDescending(List<int> a, List<int> b) =>
        b == null ? a.Count : a.Count == b.Count ? order[a[0]] - order[b[0]] : b.Count - a.Count;

        private IGraph<T> graph;
        private List<List<int>> faces;
        private List<(bool cycle, List<int> nodes)> chains = new List<(bool,List<int>)>();
        private int remainingCount;
        private int[] depth;
        private int[] order;
        private int[] cycle;

        public IEnumerable<(bool cycle, List<int> nodes)> Chains(IGraph<T> graph){
            if(graph.NodeCount == 1){ yield return (true, new List<int>(){0}); yield break; }
            if(graph.Connected() != 1) throw new System.ArgumentException($"Graph {nameof(graph)} is disconnected!");
            //IGraph<T> simplifiedGraph = null;
            List<List<int>> faces = this.faces = new BoyerMyrvold<T>().ExtractPlanarFaces(graph);
            if(faces == null) throw new System.ArgumentException($"Graph {nameof(graph)} is not planar!");
            this.graph = graph;

            chains.Clear();
            depth = new int[graph.NodeCount];
            order = new int[graph.NodeCount];
            cycle = new int[graph.NodeCount];
            remainingCount = graph.NodeCount;

            faces.Sort((a,b) => a.Count - b.Count);
            if(faces.Count != 0) faces.RemoveAt(faces.Count - 1);

            for(int i = 0; i < faces.Count; i++)
                faces[i].RemoveAll(vertex => cycle[vertex] > i ? true : (cycle[vertex] = i + 1) == 0);
            depth.Fill(int.MinValue);

            if(faces.Count != 0)
                yield return AddChain(new List<int>(ascending ? faces[0] : faces[faces.Count - 1]), true);
            else for(int i = 0; i < graph.NodeCount; i++)
                if(graph.Neighbours(i).Count == 1){
                    yield return AddChain(TraverseTree(new List<int>(){ i }), false);
                    break;
                }
            
            while(remainingCount > 0){
                for(int i = 0; i < faces.Count; i++){
                    if(depth[faces[i][0]] != 0) continue;
                    yield return AddChain(new List<int>(faces[i]), true);
                    i = -1;
                }
                List<int> path = new List<int>();
                for(int i = 0; i < chains.Count && path.Count == 0; i++)
                if(chains[i].cycle)
                foreach(int node in chains[i].nodes){
                    foreach(int neighbour in graph.Neighbours(node))
                        if(depth[neighbour] <= 0){
                            depth[neighbour] = 1;
                            path.Add(neighbour);
                            if(!mergeChains) break;
                        }
                    if(path.Count != 0) break;
                }
                if(path.Count == 0) break;
                yield return AddChain(TraverseTree(path), false);
            }
        }
        private List<int> TraverseTree(List<int> nodes){
            Queue<int> queue = new Queue<int>(nodes);
            while(queue.Count != 0){
                int node = queue.Dequeue();
                depth[node] = 1;
                foreach(int neighbour in graph.Neighbours(node)){
                    if(depth[neighbour] > 0 || cycle[neighbour] > 0) continue;
                    queue.Enqueue(neighbour);
                    nodes.Add(neighbour);
                }
            }
            return nodes;
        }
        private (bool,List<int>) AddChain(List<int> chain, bool cycle){
            chains.Add((cycle,chain));
            remainingCount -= chain.Count;
            var queue = new Queue<int>();
            foreach(int node in chain){
                depth[node] = 1;
                order[node] = chains.Count;
                queue.Enqueue(node);
            }
            while(queue.Count != 0){
                int node = queue.Dequeue();
                foreach(int neighbour in graph.Neighbours(node)){
                    if(depth[neighbour] >= depth[node] - 1) continue;
                    depth[neighbour] = depth[node] - 1;
                    order[neighbour] = order[node] + 1;
                    queue.Enqueue(neighbour);
                }
            }
            foreach(var face in faces) face.Reorder<int>(ReorderFace, chain[0]);
            faces.Reorder<List<int>>(ascending ? (System.Comparison<List<int>>) ReorderAscending : ReorderDescending, null);
            return chains[chains.Count - 1];
        }
        // public void SimplifyGraph(IGraph<T> graph){
        //     List<int> removed = new List<int>();
        //     int[] mapping = new int[graph.NodeCount];
        //     for(int i = 0; i < graph.NodeCount; i++){

        //     }
        //     for(int i = removed.Count - 1; i >= 0; i--){
        //         int node = removed[i]; bool attach = true;
        //         foreach(int neighbour in graph.Neighbours(node))
        //             if(depth[mapping[neighbour]] <= 0){
        //                 attach = false; break;
        //             }
        //         //if(attach) 
        //     }
        // }
    }
}