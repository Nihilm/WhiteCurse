namespace _Template {
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "Inventory", menuName = "Template/Inventory")]
    public class InventoryTemplate : ScriptableObject {
        [SerializeField] private InventoryTemplate template;
        [SerializeField,Min(0)] public int capacity = 0;
        [SerializeField] public ItemTemplate[] items = new ItemTemplate[0];
        public InventoryState Create() => InventoryState.Create(this).OnAfterDeserialize<InventoryState>();

        public event System.Action<InventoryState> openInventoryEvent;
        [System.NonSerialized] private InventoryState activeInventory;
        public InventoryState ActiveInventory => template != null ? template.ActiveInventory : activeInventory;
        public void OpenInventory(InventoryState inventory){
            if(template != null) template.OpenInventory(inventory);
            else openInventoryEvent?.Invoke(activeInventory = inventory);
        }
    }

    [System.Serializable] public class InventoryState : NodeState {
        public static InventoryState Create(InventoryTemplate template) => new InventoryState(){
            template = template,
            nodes = System.Array.ConvertAll(template.items, t => t?.Create())
        };

        [SerializeField] public InventoryTemplate template;
        public override ScriptableObject Template => template;
        public override string DisplayName => "inventory";
        public override int Count => template.capacity > 0 ? template.capacity : base.Count;

        public override bool Allowed(ITarget target, int index){
            if(template.capacity > 0 && index >= template.capacity) return false;
            var item = target as ItemState;
            if(item == null) return false;
            if(template.capacity > 0)
            for(int i = 0, total = 0; i < Count; i++)
                if(this[i] != null && ++total >= template.capacity)
                    return false;
            if(!item.template.Allowed(this, item)) return false;
            return true;
        }
    }
}