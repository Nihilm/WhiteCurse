using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotView : MonoBehaviour ,
IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler,
IBeginDragHandler, IDragHandler, IEndDragHandler {

    [HideInInspector] public InventoryView parentView;
    [HideInInspector] public int index;
    public _Template.InventoryState Inventory => parentView?.openedInventory;
    public _Template.ItemState Item => Inventory?[index] as _Template.ItemState;

    public void UpdateState(){
        transform.Find("Icon").GetComponent<Image>().enabled = this.Item != null;
        if(this.Item != null){
            transform.Find("Icon").GetComponent<Image>().sprite = this.Item.template.icon;
        }
    }
    public void OnBeginDrag(PointerEventData eventData){}
    public void OnDrag(PointerEventData eventData){}
    public void OnEndDrag(PointerEventData eventData){}
    public void OnPointerUp(PointerEventData eventData){
        parentView.player.ReleaseTarget(Inventory, index);
        transform.Find("Icon").GetComponent<Image>().enabled = this.Item != null;
    }
    public void OnPointerDown(PointerEventData eventData){
        if(!parentView.player.SelectTarget(Inventory, index)) return;
        transform.Find("Icon").GetComponent<Image>().enabled = false;
    }
    public void OnPointerEnter(PointerEventData eventData){
        if(!parentView.player.HoverTarget(Inventory, index)) return;
        GetComponent<Image>().color = new Color(1, 1, 1, 1);
    }
    public void OnPointerExit(PointerEventData eventData){
        parentView.player.UnHoverTarget(Inventory, index);
        GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
}