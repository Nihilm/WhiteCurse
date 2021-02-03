namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Redirect", menuName = "Template/Action/Modifier/Redirect")]
    public class EffectRedirectTemplate : ActionModifierTemplate {
        [SerializeField] public bool self;
        public override int TierCount => 0;
        public override string Description(int tier) => --tier < 0 ? "" : $"self";
        public override IEnumerable<IAction> Apply(
            ModifierNode node, int tier,
            IAgent agent, ITarget source, IContainer target, int index
        ){
            if(--tier < 0) yield break;
            var unit = source.Root<UnitState>();
            var location = target as LocationState;
            foreach(var action in node.Propagate(agent, source, location, unit.Index))
                yield return action;
        }
    }
}