namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable] public struct GroundEffectMergeStrategy {
        [SerializeField] public GroundEffectTemplate groundEffect;
    }

    [CreateAssetMenu(fileName = "Ground", menuName = "Template/Action/Modifier/Ground")]
    public class GroundEffectTemplate : ActionModifierTemplate {
        [SerializeField] public GameObject prefab;
        [SerializeField] public bool snapshot;
        [SerializeField] public GroundEffectMergeStrategy[] mergeStrategies = new GroundEffectMergeStrategy[0];

        [SerializeField] public int[] tiers = new int[0];
        public override int TierCount => tiers.Length;
        public override string Description(int tier) => --tier < 0 ? "" : $"ground for {tiers[tier]}";

        public override IEnumerable<IAction> Apply(
            ModifierNode node, int tier,
            IAgent agent, ITarget source, IContainer target, int index
        ){
            if(--tier < 0) yield break;
            var location = target as LocationState;
            var groundEffect = new GroundEffectState(){
                template = this,
                agent = agent,
                tier = tier,
                source = source,
                modifierNode = node
            };
            if(!location.Allowed(groundEffect, location.TileCount + index)) yield break;
            yield return new PlaceUnitAction(){
                source = source,
                target = location,
                index = index + location.TileCount,
                unit = groundEffect
            };
        }
    }
    [Serializable] public class PlaceUnitAction : IAction {
        //TODO check target.Allow first?
        [SerializeReference] public ITarget source;
        [SerializeReference] public LocationState target;
        [SerializeReference] public ITarget unit;
        [SerializeField] public int index;
        public virtual void Apply(WorldState world){
            Debug.Log($"{source.DisplayName} placed {unit.DisplayName} on {target.DisplayName}");
            var previous = target[index];
            if(previous != null) target.Remove(previous.Index);
            //TODO merge/stacking/override logic
            target.Add(unit, index);
        }
    }
    [Serializable] public class GroundEffectState : NodeState, IActiveEffect {
        [SerializeField] public GroundEffectTemplate template;
        [SerializeReference] public ITarget source;
        [SerializeReference] public IAgent agent;
        [SerializeField] public int tier;
        [SerializeField] public int elapsed;
        [SerializeField] public ModifierNode modifierNode;

        public override ScriptableObject Template => template;
        public override string DisplayName => source.DisplayName;
        public override AgentTemplate Agency => agent.Agency;
        public override IAction Act(IAgent agent, IContainer target, int index) => null;

        public int Remaining => template.tiers[tier] - elapsed;
        public virtual void Update(WorldState world, ActionTriggerType type, IAction action){
            if(ActionTriggerType.TurnEnd != type) return;
            elapsed++;

            LocationState location = (LocationState) Parent;
            int index = Index - location.TileCount;
            foreach(var triggeredAction in modifierNode.Propagate(agent, source, location, index))
                world.AddAction(triggeredAction);

            int duration = template.tiers[tier];
            if(elapsed >= duration) Debug.Log($"{DisplayName} on {Parent.DisplayName} dissipates");
            if(elapsed >= duration) Parent.Remove(Index);
        }
    }
}