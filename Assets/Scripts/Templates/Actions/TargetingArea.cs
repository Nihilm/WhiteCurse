namespace _Template {
    using System;
    using UnityEngine;

    [Serializable, Flags] public enum TargetType {
        None = 0,
        Self = 1 << 0,
        Ally = 1 << 1,
        Enemy = 1 << 2,
        Ground = 1 << 3,
        All = ~0,
    }

    public static partial class Extensions {
        public static bool Allow(this TargetType type, IAgent agent, UnitState source, LocationState location, int tile){
            var targetUnit = location[tile] as UnitState;
            if(type == TargetType.None) return false;
            if((type & TargetType.Self) == 0 && source.Index == tile) return false;
            if((type & TargetType.Ally) == 0 && targetUnit != null && source.Index != tile && targetUnit.Agency == source.Agency) return false;
            if((type & TargetType.Enemy) == 0 && targetUnit != null && targetUnit.Agency != source.Agency) return false;
            if((type & TargetType.Ground) == 0 && targetUnit == null) return false;
            return true;
        }
    }
    //TODO that can be refactored into another "Template" but we need to know the list of possible options.
    [Serializable] public struct TargetingArea {
        [SerializeField, Min(0)] public float range;
        [SerializeField, Range(0, 2 * Mathf.PI)] public float arc;
        [SerializeField] public bool raycast;
        [SerializeField] public TargetType filter;
        public int GetTile(IAgent agent, UnitState source, LocationState location, int index){
            int columns = location.template.columns, rows = location.template.rows;
            var origin = location.GetTile(source.Index);
            var target = location.GetTile(index);
            float rangeSquared = range * range;
            if(raycast){
                float angle = Mathf.Atan2(target.y - origin.y, target.x - origin.x);
                if(Math.Abs(angle) > arc) return -1;
                foreach(var tile in _Algorithms.RectilinearPolygon.SupercoverLine(origin, target)){
                    float distanceSquared = (tile - origin).sqrMagnitude;
                    if(distanceSquared > rangeSquared) break;
                    int tileIndex = tile[1] + tile[0] * rows;
                    if(!filter.Allow(agent, source, location, tileIndex)) continue;
                    return tileIndex;
                }
                return -1;
            }else{
                float distanceSquared = (target - origin).sqrMagnitude;
                if(distanceSquared > rangeSquared) return -1;
                if(!filter.Allow(agent, source, location, index)) return -1;
                return index;
            }
        }
    }
}



