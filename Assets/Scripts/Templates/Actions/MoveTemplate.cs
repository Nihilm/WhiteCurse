namespace _Template {
    using System;
    using UnityEngine;

    [Serializable] public class MoveAction : IAction {
        public static MoveAction Create(
            AttributeRequirement[] requirements,
            IAgent agent, ITarget source, IContainer target, int index, int tier
        ){
            if(agent == null || source == null || target == null || source.Parent == null || index == -1) return null;
            if(!target.Allowed(source, index)) return null;
            if(target[index] != null && !source.Parent.Allowed(target[index], source.Index)) return null;

            foreach(var requirement in requirements) if(!requirement.Match(source as IContainer)) return null;
            if(target[index] != null)
            foreach(var requirement in requirements) if(!requirement.Match(target[index] as IContainer)) return null;

            return new MoveAction(){
                requirements = requirements,
                source = source,
                targetParent = target,
                targetIndex = index
            };
        }

        [SerializeField] public AttributeRequirement[] requirements;
        [SerializeReference] public ITarget source;
        [SerializeReference] public IContainer targetParent;
        [SerializeField] public int targetIndex;

        public void Apply(WorldState world){
            int sourceIndex = source.Index;
            IContainer sourceParent = source.Parent;
            ITarget target = targetParent[targetIndex];
            sourceParent.Remove(sourceIndex);
            if(target != null){
                targetParent.Remove(targetIndex);
                sourceParent.Add(target, sourceIndex);
            }
            targetParent.Add(source, targetIndex);

            foreach(var requirement in requirements){
                requirement.Apply(source as IContainer);
                requirement.Apply(target as IContainer);
            }
        }
    }
    
}