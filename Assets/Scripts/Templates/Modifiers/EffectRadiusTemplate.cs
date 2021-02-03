namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Radius", menuName = "Template/Action/Modifier/Radius")]
    public class EffectRadiusTemplate : ActionModifierTemplate {
        [SerializeField] public bool fallthrough;
        [SerializeField] public float[] tiers = new float[0];
        public override int TierCount => tiers.Length;
        public override string Description(int tier) => --tier < 0 ? "" : $"{tiers[tier]} Radius";

        public override IEnumerable<IAction> Apply(
            ModifierNode node, int tier,
            IAgent agent, ITarget source, IContainer target, int index
        ){
            if(--tier < 0){
                if(fallthrough) foreach(var action in node.Propagate(agent, source, target, index))
                    yield return action;
                yield break;
            }
            var unit = source.Parent?.Parent as UnitState;
            var location = target as LocationState;
            int step = unit.Index > index ? 1 : -1;
            int value = Mathf.FloorToInt(tiers[tier]);
            var center = location.GetTile(index);
            
            int radiusInt = Mathf.CeilToInt(value);
            float radiusSquared = value * value;
            for(int x = -radiusInt; x <= radiusInt; x++)
            for(int y = -radiusInt; y <= radiusInt; y++){
                if(x*x + y*y > radiusSquared) continue;
                int tile = y + x * location.template.rows;
                foreach(var action in node.Propagate(agent, source, location, tile))
                    yield return action;
            }
        }
    }
}