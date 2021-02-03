namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    [Serializable] public class DeathAction : IAction {
        public static DeathAction Create(UnitState target){
            return new DeathAction(){
                target = target
            };
        }

        [SerializeReference] public UnitState target;
        public void Apply(WorldState world){
            Debug.Log($"{target.DisplayName} has died");
            List<ItemState> items = new List<ItemState>();
            foreach(var inventory in target.GetNodes<InventoryState>())
                for(int i = 0; i < inventory.Count; i++){
                    var item = inventory[i] as ItemState;
                    if(item == null) continue;
                    inventory.Remove(i);
                    items.Add(item);
                }
            for(int i = target.Count - 1; i >= 0; i--) target.Remove(i);
            target.Parent.Remove(target.Index);
        }
    }
}