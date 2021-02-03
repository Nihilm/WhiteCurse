using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Template;

public class SettlementView : MonoBehaviour, IAreaView {
    public AreaState Area{get;set;}
    public LocationState Location{get;set;}
    public event System.Action<LocationState> navigationEvent;
    void Start(){
        
    }

    void Update(){
        
    }
}
