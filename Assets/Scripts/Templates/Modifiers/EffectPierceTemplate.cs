namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Pierce", menuName = "Template/Action/Modifier/Pierce")]
    public class EffectPierceTemplate : ActionModifierTemplate {
        [SerializeField] public bool fallthrough;
        [Tooltip("Absolute(tiles) or relative(units)."), SerializeField] public bool absolute;
        [SerializeField] public float[] tiers = new float[0];
        public override int TierCount => tiers.Length;
        public override string Description(int tier) => --tier < 0 ? "" : $"pierce {tiers[tier]} {(absolute ? "tiles" : "units")}";

        public override IEnumerable<IAction> Apply(
            ModifierNode node, int tier,
            IAgent agent, ITarget source, IContainer target, int index
        ){
            if(--tier < 0){
                if(fallthrough) foreach(var action in node.Propagate(agent, source, target, index))
                    yield return action;
                yield break;
            }
            var unit = source.Root<UnitState>();
            var location = target as LocationState;
            int step = unit.Index < index ? 1 : -1;
            var center = location.GetTile(unit.Index);
            int value = Mathf.FloorToInt(tiers[tier]);
            if(absolute){
                int distance = Math.Min(value, location.template.columns - center.y);
                for(int x = center.x + step; 0 <= x && x < location.template.columns; x+=step){
                    int tile = center.y + x * location.template.rows;
                    foreach(var action in node.Propagate(agent, source, target, tile))
                        yield return action;
                }
            }else{
                int remaining = value;
                for(int x = center.x + step; 0 <= x && x < location.template.columns && remaining >= 0; x+=step){
                    int tile = center.y + x * location.template.rows;
                    bool hit = false;
                    foreach(var action in node.Propagate(agent, source, target, tile)){
                        yield return action;
                        if(!hit){ hit = true; remaining--; }
                    }
                }
            }
        }
    }
}