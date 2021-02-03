using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TileView : MonoBehaviour,
IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {
    private enum State { IDLE, HOVER, ACTIVE }
    private State state = State.IDLE;

    public GameObject selected;

    [HideInInspector] public BattlefieldView parentView;
    [HideInInspector] public int index;

    public _Template.LocationState Location => parentView.player.state.activeLocation;
    public _Template.UnitState Unit => Location[index] as _Template.UnitState;
    void Start(){
        
    }
    public void SetHighlight(bool toggle){
        selected.SetActive(toggle);
    }
    //TODO move logic outisde, only emit events and display player state as is.
    public void OnPointerDown(PointerEventData eventData){
        if(!parentView.player.SelectTarget(Location, index)) return;
        this.state = State.ACTIVE;
        GetComponent<SpriteRenderer>().color = new Color(1,0,0,1);
    }
    public void OnPointerUp(PointerEventData eventData){
        if(state != State.ACTIVE) return;
        state = State.IDLE;
        parentView.player.ReleaseTarget(Location, index);
        GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
    public void OnPointerEnter(PointerEventData eventData){
        if(!parentView.player.HoverTarget(Location, index)) return;
        this.state = State.HOVER;
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1);
    }
    public void OnPointerExit(PointerEventData eventData){
        if(state != State.HOVER) return;
        state = State.IDLE;
        parentView.player.UnHoverTarget(Location, index);
        GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
}
