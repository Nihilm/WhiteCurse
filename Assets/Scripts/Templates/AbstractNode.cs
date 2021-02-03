namespace _Template {
    using System.Collections.Generic;
    using UnityEngine;

    public interface ITierEffect {
        string Description(int tier);
        int TierCount{get;}
    }

    public abstract class ActionTemplate : ScriptableObject, ITierEffect {
        [SerializeField] public string displayName;
        public abstract IAction Create(IAgent agent, ITarget source, IContainer target, int index, int tier);
        public abstract string Description(int tier);
        public abstract int TierCount{get;}
    }
    
    public static partial class Extensions {
        public static IEnumerable<T> GetNodes<T>(this IContainer container) where T : class, ITarget {
            for(int i = 0; i < container.Count; i++){
                T node = container[i] as T;
                if(node != null) yield return node;
            }
        }
        public static IEnumerable<T> GetNodes<T>(this IContainer container, ScriptableObject template) where T : class, ITarget {
            for(int i = 0; i < container.Count; i++){
                T node = container[i] as T;
                if(node != null && System.Object.ReferenceEquals(node.Template, template)) yield return node;
            }
        }
        public static T OnAfterDeserialize<T>(this T target) where T : ISerializationCallbackReceiver {
            target.OnAfterDeserialize(); return target;
        }
        public static ITarget Root(this ITarget target){
            while(target.Parent != null)
                target = target.Parent;
            return target;
        }
        public static T Root<T>(this ITarget target) where T : class, ITarget {
            do{
                T root = target as T;
                if(root != null) return root;
                target = target.Parent;
            }while(target != null);
            return null;
        }
        public static void Add(this IContainer container, ITarget target){
            int index;
            for(index = 0; index < container.Count; index++)
                if(container[index] == null) break;
            container.Add(target, index);
        }
    }

    [System.Serializable] public abstract partial class NodeState : IContainer, ISerializationCallbackReceiver {
        [SerializeReference] protected ITarget[] nodes = new ITarget[0];

        public abstract ScriptableObject Template{get;}
        public abstract string DisplayName{get;}
        public virtual int Count => nodes.Length;
        public virtual ITarget this[int index] => index >= nodes.Length ? null : nodes[index];
        public event System.Action<int> updateEvent;
        public virtual IContainer Parent{get;set;}
        public int Index{get;set;}

        public virtual AgentTemplate Agency => null;
        public virtual IAction Act(IAgent agent, IContainer target, int index) => null;

        public virtual void OnAfterDeserialize(){
            for(int index = 0; index < Count; index++)
                if(this[index] != null){
                    this[index].Index = index;
                    this[index].Parent = this;
                }
        }
        public virtual void OnBeforeSerialize(){}

        public virtual bool Allowed(ITarget target, int index) => false;
        public virtual void Add(ITarget target, int index){
            if(target.Parent != null) target.Parent.Remove(target.Index);
            if(index >= nodes.Length) System.Array.Resize<ITarget>(ref nodes, index + 1);
            if(nodes[index] != null) Remove(index);
            target.Parent = this;
            target.Index = index;
            nodes[index] = target;
            updateEvent?.Invoke(index);
        }
        public virtual void Remove(int index){
            if(nodes[index] == null) return;
            nodes[index].Parent = null;
            nodes[index].Index = -1;
            nodes[index] = null;
            updateEvent?.Invoke(index);
        }
    }
}