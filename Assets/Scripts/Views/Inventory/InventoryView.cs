using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour {
    [SerializeField] private GameObject view;
    [SerializeField] public _Template.PlayerTemplate player;
    [SerializeField] private _Template.InventoryTemplate inventory;
    [SerializeField] private GameObject slotPrefab;
    [System.NonSerialized, HideInInspector] public _Template.InventoryState openedInventory;
    private List<GameObject> slots = new List<GameObject>();
    void Start(){
        inventory.openInventoryEvent += OnOpenInventory;
        OnOpenInventory(inventory.ActiveInventory);
    }
    void onDestroy(){
        inventory.openInventoryEvent -= OnOpenInventory;
        if(openedInventory != null){
            openedInventory.updateEvent -= OnInventoryUpdate;
            openedInventory = null;
        }
    }
    void OnOpenInventory(_Template.InventoryState inventory){
        if(this.openedInventory != null) this.openedInventory.updateEvent -= OnInventoryUpdate;
        if(inventory == null){
            view.SetActive(false);
            this.openedInventory = inventory;
        }else{
            view.SetActive(true);
            this.openedInventory = inventory;
            inventory.updateEvent += OnInventoryUpdate;
            OnInventoryUpdate(-1);
        }
    }
    private void OnInventoryUpdate(int index){
        for(int i = 0; i < openedInventory.Count; i++){
            if(slots.Count - 1 < i){
                GameObject slot = Instantiate(slotPrefab, Vector3.zero, Quaternion.identity, transform);
                slot.GetComponent<SlotView>().parentView = this;
                slot.GetComponent<SlotView>().index = i;
                slots.Add(slot);
            }
            slots[i].GetComponent<SlotView>().UpdateState();
            slots[i].SetActive(true);
        }
        for(int i = openedInventory.Count; i < slots.Count; i++)
            slots[i].SetActive(false);
    }
}
