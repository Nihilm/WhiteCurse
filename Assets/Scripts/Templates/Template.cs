namespace _Template {
    using UnityEngine;

    public abstract class AgentTemplate : ScriptableObject {
        public abstract IAgent Create();
    }
    public interface IAgent {
        AgentTemplate Agency{get;}
        void Enter(WorldState world);
    }
    public interface IAction {
        void Apply(WorldState world);
    }
    public interface ITarget {
        ScriptableObject Template{get;}
        AgentTemplate Agency{get;}
        string DisplayName{get;}
        IAction Act(IAgent agent, IContainer target, int index);
        IContainer Parent{get;set;}
        int Index{get;set;}
    }
    public interface IContainer : ITarget {
        bool Allowed(ITarget target, int index);
        void Add(ITarget target, int index);
        void Remove(int index);
        int Count{get;}
        ITarget this[int index]{get;}

        event System.Action<int> updateEvent;
    }
    public interface IActiveEffect : ITarget {
        void Update(WorldState world, ActionTriggerType type, IAction action);
        int Remaining{get;}
    }
}