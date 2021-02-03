namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using _Algorithms;

    [CreateAssetMenu(fileName = "Area", menuName = "Template/Area")]
    public class AreaTemplate : ScriptableObject {
        [SerializeField] private AreaTemplate template;
        [SerializeField] public string displayName;
        [SerializeField] public GameObject prefab;

        [System.Serializable] public class LocationGraph : TopologyGraph{[SerializeField] public LocationTemplate[] nodes;}
        [SerializeField] public LocationGraph locations;

        public AreaState Create() => new AreaState(){
            template = this,
            locations = Array.ConvertAll(locations.nodes, template => template == null ? null : template.Create())
        }.OnAfterDeserialize<AreaState>();
    }

    [Serializable] public class AreaState : NodeState {
        [SerializeField] public AreaTemplate template;
        public override ScriptableObject Template => template;
        public override string DisplayName => template.displayName;
        [SerializeField] public int seed;
        public LocationState[] locations{
            get{ return nodes as LocationState[]; }
            set{ nodes = value; }
        }
        public override bool Allowed(ITarget target, int index) => target is LocationState;

        [NonSerialized] private IGraph<NodeTemplate> _graph;
        public IGraph<NodeTemplate> graph{get{
            if(_graph == null){
                _graph = new _Algorithms.UndirectedAdjacencyListGraph<_Algorithms.NodeTemplate>();
                foreach(var node in locations) graph.AddNode(
                    new _Algorithms.NodeTemplate(){
                        geometries = new List<RoomGeometry>(node.template.rooms).ConvertAll(room => room.BuildGeometry())
                    }
                );
                foreach(var link in template.locations.links) graph.AddEdge(link[0], link[1]);
            }
            return _graph;
        }}

        public LayoutState GenerateMap(){
            var generator = new _Algorithms.ProceduralGenerator(){
                random = new System.Random(seed)
            };
            using(var enumerator = generator.Generate(this.graph).GetEnumerator()){
                if(!enumerator.MoveNext()) return null;
                return enumerator.Current;
            }
        }
    }
}