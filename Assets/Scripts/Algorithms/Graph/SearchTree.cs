namespace _Algorithms {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    class OverlapConstraint {
        public static float Calculate(NodeGeometry[] state, int target, int loose, IGraph<NodeTemplate> graph){
            //TODO cache area
            int area = 0; foreach(var rectangle in state[loose].rectangles) area += rectangle.width * rectangle.height;
            int overlap = 0;
            foreach(var rectangleA in state[loose].rectangles)
            foreach(var rectangleB in state[target].rectangles)
            overlap += rectangleA.OverlapArea(rectangleB);
            return (float) overlap / area;
        }
    }
    class NeighbourConstraint {
        public static float Calculate(NodeGeometry[] state, int target, int loose, IGraph<NodeTemplate> graph){
            if(graph[target, loose] == 0) return 0;
            //TODO cache bounds
            RectInt boundsA = state[loose].rectangles.CalculateBounds();
            RectInt boundsB = state[target].rectangles.CalculateBounds();

            List<RectInt> space = new List<RectInt>(state[target].ConfigurationSpace(state[loose]));
            List<Vector2Int> positions = new List<Vector2Int>(NodeGeometry.SampleConfigurationSpace(space));
            int distance = int.MaxValue;
            foreach(var position in positions)
                distance = Math.Min(distance, Math.Abs(position.x) + Math.Abs(position.y));
            return (float) distance / ((boundsB.width + boundsB.height + boundsA.width + boundsA.height) * 0.25f);
        }
    }

    public class LayoutState : IStateNode<LayoutState> {
        private IGraph<NodeTemplate> graph;
        public NodeGeometry[] geometries;
        public float[] energy;
        public int count;
        public int offset;
        public NodeGeometry this[int index] => geometries[index];
        public LayoutState(IGraph<NodeTemplate> graph){
            this.graph = graph;
            geometries = new NodeGeometry[graph.NodeCount];
            energy = new float[2 * graph.NodeCount];
        }
        public LayoutState(LayoutState prev){
            this.graph = prev.graph;
            this.count = prev.count;
            this.offset = prev.offset;
            geometries = Array.ConvertAll(prev.geometries, geometry => geometry == null ? null : geometry.Clone());
            energy = (float[]) prev.energy.Clone();
        }
        public void RecalculateEnergy(int node, int delta = 1){
            if(geometries[node] == null) return;
            energy[node * 2 + 0] = 0;
            energy[node * 2 + 1] = 0;
            for(int i = 0; i < geometries.Length; i++){
                if(geometries[i] == null || i == node) continue;
                energy[i * 2 + 0] += delta * OverlapConstraint.Calculate(geometries, node, i, graph);
                energy[i * 2 + 1] += delta * NeighbourConstraint.Calculate(geometries, node, i, graph);
                if(delta == -1) continue;
                energy[node * 2 + 0] += OverlapConstraint.Calculate(geometries, i, node, graph);
                energy[node * 2 + 1] += NeighbourConstraint.Calculate(geometries, i, node, graph);
            }
        }
        public void ApplyMutation(int node, int geometry, Vector2Int position){
            int index = geometries[node] == null ? count++ : geometries[node].index;
            RecalculateEnergy(node, -1);
            geometries[node] = graph[node].geometries[geometry].Clone();
            geometries[node].index = index;
            geometries[node].variant = geometry;
            geometries[node].UpdateTransform(position);
            RecalculateEnergy(node, 1);
        }
        public List<(int node, int geometry, Vector2Int position)> candidates;
        public LayoutState NextState(System.Random random){
            if(candidates == null) candidates = new List<(int node, int geometry, Vector2Int position)>(Perturb());
            if(candidates.Count == 0) return null;
            candidates.Shuffle(random);
            var (node, geometry, position) = candidates[candidates.Count - 1];
            candidates.RemoveAt(candidates.Count - 1);

            LayoutState nextState = new LayoutState(this);
            nextState.ApplyMutation(node, geometry, position);
            return nextState;
        }
        public IEnumerable<(int node, int geometry, Vector2Int position)> Perturb(int node){
            //TODO optimize
            List<NodeGeometry> neighbours = new List<NodeGeometry>();
            foreach(int neighbour in graph.Neighbours(node))
            if(this[neighbour] != null) neighbours.Add(this[neighbour]);
            neighbours.Sort((a,b) => a.index - b.index);
            //TODO end
            for(int i = 0; i < graph[node].geometries.Count; i++){
                NodeGeometry geometry = graph[node].geometries[i];
                List<RectInt> intersection = null;
                List<RectInt> _intersection = new List<RectInt>();
                foreach(var neighbour in neighbours){
                    var space = new List<RectInt>(neighbour.ConfigurationSpace(geometry));
                    var nextIntersection = intersection == null ? space :
                    new List<RectInt>(OrthogonalConnector.Intersection(intersection, space));
                    if(nextIntersection.Count == 0) continue;
                    if(neighbour.index < offset) intersection = nextIntersection;
                    else _intersection.AddRange(nextIntersection);
                }
                if(_intersection.Count > 0){
                    intersection = _intersection;
                    NodeGeometry.MergeConfigurationSpace(intersection);
                }
                if(intersection == null || intersection.Count == 0) continue;

                List<Vector2Int> positions = new List<Vector2Int>(NodeGeometry.SampleConfigurationSpace(intersection));
                foreach(var position in positions){
                    if(this[node] != null && this[node].parent == geometry && this[node].position == position) continue;
                    yield return (node, i, position);
                }
            }
        }
        public IEnumerable<(int node, int geometry, Vector2Int position)> Perturb(){
            for(int node = 0; node < geometries.Length; node++){
                if(geometries[node] == null || geometries[node].index < offset) continue;
                foreach(var mutation in Perturb(node)) yield return mutation;
            }
        }
        public double Energy{get{
            //TODO cache
            double total = 0;
            foreach(float value in energy) total += Mathf.Exp(value) - 1f;
            return total;
        }}
        public RectInt CalculateBounds(){
            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
            for(int i = 0; i < geometries.Length; i++){
                if(geometries[i] == null) continue;
                RectInt bounds = geometries[i].rectangles.CalculateBounds();
                min = Vector2Int.Min(min, bounds.min);
                max = Vector2Int.Max(max, bounds.max);
            }
            return new RectInt(min, max - min);
        }
    }
    public class ProceduralGenerator {
        public int branchingFactor = 4;
        public System.Random random;
        private ChainDecomposition<NodeTemplate> chainDecomposition = new ChainDecomposition<NodeTemplate>();
        private SimulatedAnnealing<LayoutState> simulatedAnnealing = new SimulatedAnnealing<LayoutState>(){
            iterations = 10000
        };
        public IEnumerable<LayoutState> Generate(IGraph<NodeTemplate> graph){
            var chains = new List<(bool, List<int>)>(chainDecomposition.Chains(graph));
            var stack = new Stack<IEnumerator<LayoutState>>();
            stack.Push(Extend(new LayoutState(graph), graph, chains[0]).GetEnumerator());
            while(stack.Count != 0){
                var enumerator = stack.Peek();
                if(enumerator.MoveNext()){
                    if(stack.Count == chains.Count) yield return enumerator.Current;
                    else stack.Push(Extend(enumerator.Current, graph, chains[stack.Count]).GetEnumerator());
                }else stack.Pop().Dispose();
            }
        }
        private IEnumerable<LayoutState> Extend(LayoutState state, IGraph<NodeTemplate> graph, (bool, List<int> nodes) chain){            
            LayoutState nextState = new LayoutState(state);
            nextState.offset = nextState.count;
            for(int index = 0; index < chain.nodes.Count; index++){
                int node = chain.nodes[index];
                double minEnergy = double.MaxValue;
                int minGeometry = index == 0 && state.count == 0 ? 0 : -1;
                Vector2Int minPosition = Vector2Int.zero;

                foreach(var mutation in nextState.Perturb(node)){
                    nextState.ApplyMutation(node, mutation.geometry, mutation.position);
                    if(nextState.Energy >= minEnergy) continue;
                    minEnergy = nextState.Energy;
                    minGeometry = mutation.geometry;
                    minPosition = mutation.position;
                }
                if(minGeometry == -1) yield break;
                nextState.ApplyMutation(node, minGeometry, minPosition);
            }
            
            HashSet<int> visited = new HashSet<int>();
            int remaining = branchingFactor;
            foreach(var perturbedState in simulatedAnnealing.Simulate(nextState, random)){
                int hash = 0;
                for(int i = 0; i < perturbedState.geometries.Length; i++){
                    if(perturbedState[i] == null) continue;
                    hash = ((hash << 5) - hash) + perturbedState[i].position.x;
                    hash = ((hash << 5) - hash) + perturbedState[i].position.y;
                    hash = ((hash << 5) - hash) + perturbedState[i].variant;
                }
                if(visited.Contains(hash)) continue;
                visited.Add(hash);
                yield return perturbedState;
                if(--remaining <= 0) break;
            }
        }
    }
}