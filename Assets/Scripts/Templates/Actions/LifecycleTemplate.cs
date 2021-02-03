namespace _Template {
    using System;
    using UnityEngine;

    public interface ActionReceiver {
        void PostAction(WorldState world, IAction action);
    }

    [CreateAssetMenu(fileName = "Lifecycle", menuName = "Template/Lifecycle")]
    public class LifecycleTemplate : ScriptableObject {
        //TODO add "move" actions back at some point? or have all logic here?
        [SerializeField] public AttributeRequirement[] moveRequirements = new AttributeRequirement[0];
        [SerializeField] public AttributeRequirement[] liveRequirements = new AttributeRequirement[0];

        public IAction Act(IAgent agent, ITarget source, IContainer target, int index){
            return MoveAction.Create(moveRequirements, agent, source, target, index, 0);
            // foreach(var actionTemplate in template.actions){
            //     var action = actionTemplate.Create(agent, this, target, index, 0);
            //     if(action != null) return action;
            // }
        }

        public void PostAction(WorldState world, IAction action, IContainer target){
            foreach(var requirement in liveRequirements)
                if(!requirement.Match(target)){
                    world.AddAction(DeathAction.Create(target as UnitState));
                    break;
                }
        }
    }
}