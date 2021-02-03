namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "Multi", menuName = "Template/Action/Modifier/Multi")]
    public class EffectMultiTemplate : ActionModifierTemplate {
        [SerializeField] public int[] tiers = new int[0];
        public override int TierCount => tiers.Length;
        public override string Description(int tier) => --tier < 0 ? "" : $"{tiers[tier]} times";

        public override IEnumerable<IAction> Apply(
            ModifierNode node, int tier,
            IAgent agent, ITarget source, IContainer target, int index
        ){
            if(--tier < 0) yield break;
            for(int i = tiers[tier]; i > 0; i--){
                foreach(var action in node.Propagate(agent, source, target, index))
                    yield return action;
            }
        }
    }
}