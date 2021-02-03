using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Template;

public class AttributeView : MonoBehaviour, IAttributeView {
    public IAttribute Attribute{get;set;}

    private Text amount;
    void Start(){
        amount = transform.Find("Amount").GetComponent<Text>();
        UpdateDisplay();
    }
    public void UpdateDisplay(){
        amount.text = $"{Attribute.Value}/{Attribute.Capacity}";
    }
}
