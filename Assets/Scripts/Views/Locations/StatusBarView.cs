using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Template;

public interface IAttributeView {
    IAttribute Attribute{set;}
    void UpdateDisplay();
}

public class StatusBarView : MonoBehaviour {
    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject top;
    [SerializeField] private GameObject bottom;

    private Camera _camera;
    [HideInInspector] public GameObject target;
    [HideInInspector] public UnitState unit;

    private Dictionary<ITarget, GameObject> views = new Dictionary<ITarget, GameObject>();
    private Stack<ITarget> removed = new Stack<ITarget>();
    void Start(){
        _camera = Camera.main;
        UpdateDisplay();
    }
    public void UpdateDisplay(){
        foreach(var view in views)
            if(view.Key.Parent == null){
                Destroy(view.Value);
                removed.Push(view.Key);
            }
        while(removed.Count != 0) views.Remove(removed.Pop());

        foreach(var attribute in unit.GetNodes<IAttribute>()){
            IStatusTemplate attributeTemplate = attribute.Template as IStatusTemplate;
            if(attributeTemplate?.IconPrefab == null) continue;
            if(views.ContainsKey(attribute)){
                views[attribute].GetComponent<IAttributeView>().UpdateDisplay();
                continue;
            }
            GameObject attributeView = Instantiate(attributeTemplate.IconPrefab, top.transform);
            attributeView.GetComponent<IAttributeView>().Attribute = attribute;
            views.Add(attribute, attributeView);
        }
        foreach(var statusEffect in unit.GetNodes<IActiveEffect>()){
            IStatusTemplate statusTemplate = statusEffect.Template as IStatusTemplate;
            if(statusTemplate?.IconPrefab != null){
                if(views.ContainsKey(statusEffect)){
                    views[statusEffect].GetComponent<StatusEffectView>().UpdateDisplay();
                    continue;
                }
                GameObject statusEffectView = Instantiate(statusTemplate.IconPrefab, bottom.transform);
                statusEffectView.GetComponent<StatusEffectView>().Status = statusEffect;
                views.Add(statusEffect, statusEffectView);
            }
        }
    }
    void Update(){
        if(target == null) return;
        Transform targetTransform = target.transform.GetChild(0).transform;
        transform.position = _camera.WorldToScreenPoint(targetTransform.position + targetTransform.rotation * offset);
        //transform.position = _camera.WorldToViewportPoint(target.transform.position + offset);
    }
}
