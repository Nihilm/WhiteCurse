using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Template;

public class StatusEffectView : MonoBehaviour {
    public IActiveEffect Status{get;set;}

    private Text amount;
    void Start(){
        UpdateDisplay();
    }
    public void UpdateDisplay(){
        if(amount == null) amount = transform.Find("Amount").GetComponent<Text>();
        amount.text = $"{(Status.Remaining)}";
        // switch(Status){
        //     case StatusState statusEffect:
        //     amount.text = $"{(statusEffect.duration - statusEffect.elapsed)}";
        //     break;
        //     case EffectTriggerState triggerState:
        //     amount.text = $"{(triggerState.template.tiers[triggerState.tier] - triggerState.elapsed)}";
        //     break;
        // }
    }
}
