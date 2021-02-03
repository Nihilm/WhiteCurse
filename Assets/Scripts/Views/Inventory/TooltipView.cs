using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TooltipView : MonoBehaviour {
    [SerializeField] private _Template.PlayerTemplate player;
    void Start(){
        player.hoverTargetEvent += OnHover;
        gameObject.SetActive(false);
    }
    void OnDestroy(){
        player.hoverTargetEvent -= OnHover;
    }
    void OnHover(_Template.IContainer container, int index, bool toggle){
        if(!toggle && container[index] == null){
            gameObject.SetActive(false); return;
        }
        switch(container[index]){
            case _Template.UnitState unit:
                gameObject.SetActive(true);
                transform.Find("Label").GetComponent<Text>().text = unit.displayName;
                transform.Find("Description").GetComponent<Text>().text = "";
                transform.Find("Image").GetComponent<Image>().sprite = null;
                break;
            case _Template.ItemState item:
                gameObject.SetActive(true);
                transform.Find("Label").GetComponent<Text>().text = item.template.displayName;
                transform.Find("Description").GetComponent<Text>().text = item.Description;
                transform.Find("Image").GetComponent<Image>().sprite = item.template.icon;
                break;
            default:
                gameObject.SetActive(false);
                break;
        }
    }
}
