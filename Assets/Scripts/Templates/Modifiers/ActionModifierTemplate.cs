namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class ActionModifierTemplate : ScriptableObject, ITierEffect {
        public abstract IEnumerable<IAction> Apply(
            ModifierNode node, int tier,
            IAgent agent, ITarget source, IContainer target, int index
        );
        public abstract string Description(int tier);
        public abstract int TierCount{get;}
    }

    public interface IModifierNode {
        IEnumerable<IAction> Apply(IAgent agent, ITarget source, IContainer target, int index);
        void Connect(IModifierNode node, TargetType edge);
    }
    [Serializable] public class ModifierEntryNode : IModifierNode {
        [SerializeReference] private List<IModifierNode> neighbours = new List<IModifierNode>();
        [SerializeField] private List<TargetType> edges = new List<TargetType>();
        public void Connect(IModifierNode node, TargetType edge){
            edges.Add(edge);
            neighbours.Add(node);
        }
        public IEnumerable<IAction> Apply(IAgent agent, ITarget source, IContainer target, int index){
            for(int i = 0; i < neighbours.Count; i++)
                if(edges[i].Allow(agent, source.Root<UnitState>(), target as LocationState, index))
                    foreach(var action in neighbours[i].Apply(agent, source, target, index))
                        yield return action;
        }
    }
    //TODO snapshot properties for action
    [Serializable] public class ModifierSourceNode : IModifierNode {
        [SerializeField] public ActionTemplate template;
        [SerializeField] public int tier;
        [SerializeField] public TargetType filter = TargetType.All;
        public void Connect(IModifierNode node, TargetType edge) => throw new NotImplementedException();
        public IEnumerable<IAction> Apply(IAgent agent, ITarget source, IContainer target, int index){
            if(!filter.Allow(agent, source.Root<UnitState>(), target as LocationState, index))
                yield break;
            var action = template.Create(agent, source, target, index, tier);
            if(action == null) yield break;
            yield return action;
        }
    }
    [Serializable] public class ModifierNode : IModifierNode {
        [SerializeReference] private List<IModifierNode> neighbours = new List<IModifierNode>();
        [SerializeField] private List<TargetType> edges = new List<TargetType>();
        public void Connect(IModifierNode node, TargetType edge){
            edges.Add(edge);
            neighbours.Add(node);
        }
        [SerializeField] public ActionModifierTemplate template;
        [SerializeField] public int tier;
        public IEnumerable<IAction> Apply(IAgent agent, ITarget source, IContainer target, int index){
            return template.Apply(this, tier, agent, source, target, index);
        }
        public IEnumerable<IAction> Propagate(IAgent agent, ITarget source, IContainer target, int index){
            for(int i = 0; i < neighbours.Count; i++)
                if(edges[i].Allow(agent, source.Root<UnitState>(), target as LocationState, index))
                    foreach(var action in neighbours[i].Apply(agent, source, target, index))
                        yield return action;
        }
    }
}