using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragItemView : MonoBehaviour {
    [SerializeField] private _Template.PlayerTemplate player;
    void Start(){
        player.pressTargetEvent += OnPress;
        gameObject.SetActive(false);
    }
    void OnDestroy(){
        player.pressTargetEvent -= OnPress;
    }
    void Update(){
        transform.position = Input.mousePosition;
    }
    void OnPress(_Template.IContainer container, int index, bool toggle){
        _Template.ItemState item = container[index] as _Template.ItemState;
        if(toggle && item != null){
            gameObject.SetActive(true);
            GetComponent<Image>().sprite = item.template.icon;
            transform.position = Input.mousePosition;
        }else gameObject.SetActive(false);
    }
}
