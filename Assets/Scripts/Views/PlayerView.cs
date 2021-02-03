using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Template;
public class PlayerView : MonoBehaviour {
    [SerializeField] private PlayerTemplate player;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject battlefield;
    private GameObject activeArea;
    private GameObject activeLocation;

    void Start(){
        player.navigationEvent += OnNavigation;
        battlefield.SetActive(false);
        OnNavigation(null);
    }
    void OnDestroy(){
        player.navigationEvent -= OnNavigation;
    }
    void OnNavigation(LocationState previousLocation){
        if(previousLocation == null || previousLocation.area != player.state.ActiveArea){
            if(activeArea != null) Destroy(activeArea);
            activeArea = Instantiate(player.state.ActiveArea.template.prefab);
            activeArea.GetComponent<IAreaView>().Area = player.state.ActiveArea;
            activeArea.GetComponent<IAreaView>().navigationEvent += player.Navigate;
        }
        if(previousLocation == null || previousLocation != player.state.activeLocation){
            if(activeLocation != null) Destroy(activeLocation);
            activeArea.GetComponent<IAreaView>().Location = player.state.activeLocation;
            //battlefield.GetComponent<ILocationView>().Location = player.state.activeLocation;
            //activeLocation = Instantiate(player.state.activeLocation.template.prefab, battlefield.transform);
            //activeLocation.GetComponent<ILocationView>().Location = player.state.activeLocation;
        }
    }
    public void ToggleInventory(){
        player.state.shared.template.OpenInventory(
            player.state.shared.template.ActiveInventory == null ?
            player.state.shared : null);
    }
    public void ToggleMap(){
        activeArea.SetActive(!activeArea.activeSelf);
        battlefield.SetActive(!activeArea.activeSelf);
    }
    public void EndTurn() => player.EndTurn();
}
