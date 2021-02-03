namespace _Template {
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Item", menuName = "Template/Item/Item")]
    public class ItemTemplate : ScriptableObject {
        [SerializeField] public ItemTemplate template;
        public ItemTemplate Root => template == null ? this : template.Root;
        [SerializeField] public string displayName;
        [SerializeField,TextArea(4,8)] public string description;
        [SerializeField] public Sprite icon;

        [SerializeField] public LifecycleTemplate lifecycle;
        [SerializeField] private InventoryTemplate[] inventories = new InventoryTemplate[0];
        [SerializeField] private AttributeRequirement[] requirements = new AttributeRequirement[0];

        public virtual bool Allowed(IContainer container, ITarget target){
            if(template != null) return template.Allowed(container, target);
            InventoryState inventory = container as InventoryState;
            if(inventories.Length > 0 && inventories.IndexOf(inventory.template) == -1)
                return false;
            UnitState unit = container.Parent as UnitState;
            if(unit != null) foreach(var requirement in requirements)
                if(!requirement.Match(unit)) return false;
            return true;
        }
        public virtual ItemState Create() => ItemState.Create(this).OnAfterDeserialize<ItemState>();
    }
    [Serializable] public class ItemState : NodeState {
        public static ItemState Create(ItemTemplate template) => new ItemState(){
            template = template
        };
        [SerializeField] public ItemTemplate template;
        public override ScriptableObject Template => template;
        public override string DisplayName => template.displayName;
        public virtual string Description => template.description;
        
        public override IAction Act(IAgent agent, IContainer target, int index) =>
            template.Root.lifecycle.Act(agent, this, target, index);
    }
}