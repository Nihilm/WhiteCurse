namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable] public enum ActionTriggerType {
        TurnStart,
        TurnEnd,
        Hit,
        Death,
        Attack
    }

    [CreateAssetMenu(fileName = "Trigger", menuName = "Template/Action/Modifier/Trigger")]
    public class EffectTriggerTemplate : ActionModifierTemplate, IStatusTemplate {
        [SerializeField] public GameObject iconPrefab;
        public GameObject IconPrefab => iconPrefab;
        [Tooltip("Target or self."), SerializeField] public bool applyTarget;
        [Tooltip("Target or self."), SerializeField] public bool triggerTarget;
        [SerializeField] public ActionTriggerType triggerType;
        [SerializeField] public TargetingArea targetingArea;
        [SerializeField] public int[] tiers = new int[0];
        public override int TierCount => tiers.Length;
        public override string Description(int tier) => --tier < 0 ? "" : $"{tiers[tier]} times on {triggerType.ToString()}";

        public override IEnumerable<IAction> Apply(
            ModifierNode node, int tier,
            IAgent agent, ITarget source, IContainer target, int index
        ){
            if(--tier < 0) yield break;
            UnitState unit = applyTarget ? target[index] as UnitState : source.Root<UnitState>();
            if(unit == null) yield break;
            yield return new AttachEffectAction(){
                source = source,
                target = unit,
                effect = new EffectTriggerState(){
                    template = this,
                    agent = agent,
                    tier = tier,
                    source = source,
                    modifierNode = node
                }
            };
        }
    }
    [Serializable] public class AttachEffectAction : IAction {
        [SerializeReference] public ITarget source;
        [SerializeReference] public IContainer target;
        [SerializeReference] public ITarget effect;
        public virtual void Apply(WorldState world){
            Debug.Log($"{source.DisplayName} applied {effect.DisplayName} to {target.DisplayName}");
            var previous = target.GetNodes<ITarget>(effect.Template).First();
            if(previous != null) target.Remove(previous.Index);
            target.Add(effect); //TODO update on attached so it could handle merging itself?
        }
    }
    [Serializable] public class EffectTriggerState : IActiveEffect {
        [SerializeField] public EffectTriggerTemplate template;
        [SerializeField] public int elapsed;
        [SerializeField] public int tier;
        [SerializeReference] public ITarget source;
        [SerializeReference] public IAgent agent;
        [SerializeField] public ModifierNode modifierNode;

        public ScriptableObject Template => template;
        public string DisplayName => source.DisplayName;
        public IContainer Parent{get;set;}
        public int Index{get;set;}
        public AgentTemplate Agency => agent.Agency;
        public virtual IAction Act(IAgent agent, IContainer target, int index) => null;

        public int Remaining => template.tiers[tier] - elapsed;
        public virtual void Update(WorldState world, ActionTriggerType type, IAction action){
            if(template.triggerType != type) return;

            if(template.triggerTarget){
                Debug.Log($"{DisplayName} on {Parent.DisplayName} has triggered");
                elapsed++;
                foreach(var triggeredAction in modifierNode.Propagate(agent, source, Parent.Parent, Parent.Index))
                    world.AddAction(triggeredAction);
            }else if(type == ActionTriggerType.Hit){
                var damageAction = action as DamageAction;
                ITarget damageSource = damageAction.source;
                int tile = template.targetingArea.GetTile(agent, Parent as UnitState, damageSource.Parent as LocationState, damageSource.Index);
                if(tile == damageSource.Index){
                    Debug.Log($"{DisplayName} on {Parent.DisplayName} has triggered");
                    elapsed++;
                    foreach(var triggeredAction in modifierNode.Propagate(agent, source, damageSource.Parent, damageSource.Index))
                        world.AddAction(triggeredAction);
                }
            }

            int duration = template.tiers[tier];
            if(elapsed >= duration) Debug.Log($"{DisplayName} on {Parent.DisplayName} dissipates");
            if(elapsed >= duration) Parent.Remove(Index);
        }
    }
}