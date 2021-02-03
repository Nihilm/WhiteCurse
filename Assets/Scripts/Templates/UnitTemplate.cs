namespace _Template {
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Unit", menuName = "Template/Unit")]
    public class UnitTemplate : ScriptableObject {
        [SerializeField] public UnitTemplate template;
        public UnitTemplate Root => template == null ? this : template.Root;
        [SerializeField] public string displayName;
        [SerializeField] public GameObject prefab;
        [SerializeField] public AgentTemplate agency;
        [SerializeField] public InventoryTemplate[] inventories = new InventoryTemplate[0];
        [SerializeField] public LifecycleTemplate lifecycle;

        [System.Serializable] public struct InitialAttribute {
            public int value;
            public AttributeTemplate attribute;
        }
        [SerializeField] public InitialAttribute[] attributes = new InitialAttribute[0];

        public virtual UnitState Create() => UnitState.Create(this).OnAfterDeserialize<UnitState>();
    }
    [System.Serializable] public class UnitState : NodeState, ActionReceiver {
        public static UnitState Create(UnitTemplate template){
            var state = new UnitState(){
                template = template,
                displayName = template.displayName
            };
            var nodes = new List<ITarget>();
            nodes.AddRange(System.Array.ConvertAll(template.inventories, t => t?.Create()));
            nodes.AddRange(System.Array.ConvertAll(template.attributes, t => t.attribute?.Create(t.value)));
            state.nodes = nodes.ToArray();
            return state;
        }

        [SerializeField] public UnitTemplate template;
        public override ScriptableObject Template => template;
        [SerializeField] public string displayName;
        public override string DisplayName => displayName;
        public override AgentTemplate Agency => template.agency;
        
        public override IAction Act(IAgent agent, IContainer target, int index) =>
            template.Root.lifecycle.Act(agent, this, target, index);
        public void PostAction(WorldState world, IAction action) =>
            template.Root.lifecycle.PostAction(world, action, this);
    }
}