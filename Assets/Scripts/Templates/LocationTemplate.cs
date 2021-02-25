namespace _Template {
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Location", menuName = "Template/Location")]
    public class LocationTemplate : ScriptableObject {
        [SerializeField] private LocationTemplate template;
        [SerializeField] public string displayName;
        [SerializeField] public GameObject prefab;

        [SerializeField] public RoomGeometry[] rooms = new RoomGeometry[0];
        [SerializeField] public Sprite icon;
        
        [SerializeField,Min(0)] public int columns;
        [SerializeField,Min(0)] public int rows;
        [SerializeField,HideInInspector] public UnitTemplate[] units = new UnitTemplate[0];

        public LocationState Create() => LocationState.Create(this).OnAfterDeserialize<LocationState>();
    }
    [System.Serializable] public class LocationState : NodeState {
        public static LocationState Create(LocationTemplate template) => new LocationState(){
            template = template,
            nodes = Array.ConvertAll(template.units, t => t?.Create())
        };

        [SerializeField] public LocationTemplate template;
        public override ScriptableObject Template => template;
        public override string DisplayName => template.displayName;
        [NonSerialized] public AreaState area;

        public int TileCount => template.columns * template.rows;
        public override IContainer Parent{get => area; set => area = (AreaState)value;}
        public override bool Allowed(ITarget target, int index){
            if(index < TileCount){
                var unit = target as UnitState;
                if(unit == null) return false;
                var tile = GetTile(index);
                var zone = GetZone(unit.Agency);
                return tile[0] >= zone[0] && tile[0] <= zone[1];
            }else if(index - TileCount < TileCount){
                var ground = target as GroundEffectState;
                if(ground == null) return false;
                return true;
            }
            return false;
        }
        public Vector2Int GetTile(int index) => new Vector2Int(
            index / template.rows,
            index % template.rows
        );
        public UnitState this[int column, int row] => this[row + column * template.rows] as UnitState;
        #region Zone Control
        [SerializeReference] public IAgent order;

        public Vector2Int GetZone(AgentTemplate agency){
            int min = 0, max = template.columns - 1;
            for(int i = 0; i < TileCount; i++){
                UnitState unit = this[i] as UnitState;
                if(unit == null) continue;
                if(unit.Agency != agency) min = i / template.rows + 1;
                else break;
            }
            for(int i = TileCount - 1; i >= 0; i--){
                UnitState unit = this[i] as UnitState;
                if(unit == null) continue;
                if(unit.Agency != agency) max = i / template.rows - 1;
                else break;
            }
            return new Vector2Int(min, max);
        }
        #endregion
    }
}