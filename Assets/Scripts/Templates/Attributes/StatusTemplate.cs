namespace _Template {
    using System;
    using UnityEngine;

    public interface IStatusTemplate {
        GameObject IconPrefab{get;}
    }

    [Serializable] public enum EffectMergeStrategy {
        None,
        Override,
        Refresh,
        Stack
    }
    
    [CreateAssetMenu(fileName = "Status", menuName = "Template/Action/Status")]
    public class StatusTemplate : ActionTemplate, IStatusTemplate {
        [SerializeField] public GameObject iconPrefab;
        public GameObject IconPrefab => iconPrefab;
        [SerializeField] public GameObject effectPrefab;
        [SerializeField] public int[] tiers = new int[0];
        [SerializeField] public EffectMergeStrategy mergeStrategy;

        public override int TierCount => tiers.Length;
        public override string Description(int tier) => --tier < 0 ? "" : $"over {tiers[tier]}";

        public override IAction Create(IAgent agent, ITarget source, IContainer target, int index, int tier){
            UnitState targetUnit = target?[index] as UnitState;
            UnitState sourceUnit = source?.Parent?.Parent as UnitState;
            if(targetUnit == null || sourceUnit == null) return null;
            return new AttachEffectAction(){
                source = source,
                target = targetUnit,
                effect = new StatusState(){
                    template = this,
                    tier = tier,
                    agent = agent,
                    source = source
                }
            };
        }
    }

    [Serializable] public class StatusState : IActiveEffect {
        [SerializeField] public StatusTemplate template;
        [SerializeField] public int tier;
        [SerializeField] public int elapsed;
        [SerializeReference] public ITarget source;
        [SerializeReference] public IAgent agent;

        public ScriptableObject Template => template;
        public string DisplayName => template.displayName;
        public IContainer Parent{get;set;}
        public int Index{get;set;}
        public AgentTemplate Agency => agent.Agency;
        public virtual IAction Act(IAgent agent, IContainer target, int index) => null;

        public int Remaining => template.tiers[tier] - elapsed;
        public virtual void Update(WorldState world, ActionTriggerType type, IAction action){
            // if(template.triggerType != type) return;
            // Apply(world);
        }
        // public virtual void Apply(WorldState world){
        //     int duration = template.duration[tier];
        //     elapsed++;
        //     //TODO snapshot proxy item?
            
        //     for(int i = 0; i < template.effects.Length; i++){
        //         var actionTemplate = template.effects[i];
        //         int actionTier = template.tierMatrix[tier + i * template.effects.Length];
        //         if(actionTier <= 0) continue;

        //         //TODO maybe apply modifiers as well
        //         var action = template.effects[i].Create(agent, source, Parent.Parent, Parent.Index, actionTier);
        //         if(action != null) world.AddAction(action);
        //     }

        //     if(elapsed >= duration) Parent.Remove(Index);
        // }
    }
}