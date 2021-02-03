namespace _Template {
    using System;
    using UnityEngine;

    public interface IAttribute : ITarget {
        int Capacity{get;set;}
        int Value{get;}
        int Modify(int delta);
    }

    [CreateAssetMenu(fileName = "Attribute", menuName = "Template/Attribute/Attribute")]
    public class AttributeTemplate : ScriptableObject, IStatusTemplate {
        [SerializeField] public string displayName;
        [SerializeField] public GameObject iconPrefab;
        public GameObject IconPrefab => iconPrefab;
        [SerializeField] public bool allowNegative;
        [SerializeField] public bool allowOvercap;
        public virtual IAttribute Create(int value) => new AttributeState(){
            template = this,
            capacity = value,
            value = value
        };
    }
    [Serializable] public class AttributeState : IAttribute {
        [SerializeField] public AttributeTemplate template;
        [SerializeField] public int capacity;
        [SerializeField] public int value;
        public int Capacity{get => capacity; set => capacity = value;}
        public int Value => value;

        public ScriptableObject Template => template;
        public string DisplayName => template.displayName;
        public IContainer Parent{get;set;}
        public int Index{get;set;}
        public AgentTemplate Agency => null;
        public virtual IAction Act(IAgent agent, IContainer target, int index) => null;
        public virtual int Modify(int delta){
            value += delta;
            if(!template.allowNegative && value < 0){
                delta = value;
                value = 0;
            }else if(!template.allowOvercap && value > capacity){
                delta = value - capacity;
                value = capacity;
            }else delta = 0;
            return delta;
        }
    }

    [Serializable] public struct AttributeRequirement {
        [SerializeField] public AttributeTemplate attribute;
        [SerializeField] public int threshold;
        [SerializeField] public int consume;
        public bool Match(IContainer target){
            if(target == null) return false;
            foreach(var attribute in target.GetNodes<IAttribute>(attribute))
                if(attribute.Value >= threshold)
                    return true;
            return false;
        }
        public void Apply(IContainer target){
            if(consume == 0 || target == null) return;
            var attribute = target.GetNodes<IAttribute>(this.attribute).First();
            attribute.Modify(Math.Max(-consume, Math.Min(0, -attribute.Value)));
        }
    }
}