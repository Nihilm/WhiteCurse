using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Template;

public class BattlefieldView : MonoBehaviour {
    public LocationState Location{get;set;}
    [SerializeField] public PlayerTemplate player;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject statusBarPrefab;
    [SerializeField] private Vector2 tileSize;

    [NonSerialized] public GameObject[] tiles;
    [NonSerialized] public Dictionary<UnitState, GameObject> units = new Dictionary<UnitState, GameObject>();
    [NonSerialized] public Dictionary<GroundEffectState, GameObject> ground = new Dictionary<GroundEffectState, GameObject>();
    [NonSerialized] public Dictionary<UnitState, GameObject> bars = new Dictionary<UnitState, GameObject>();
    private GameObject interior;
    private Queue<IAction> queue = new Queue<IAction>();
    private LocationState location;

    void Start(){
        player.navigationEvent += OnNavigation;
        player.pressTargetEvent += OnUnitSelect;
        player.state.world.actionEvent += OnAction;
        OnNavigation(null);
    }
    void OnEnable() => StartCoroutine(UpdateLoop());
    void OnDestroy(){
        player.navigationEvent -= OnNavigation;
        player.pressTargetEvent -= OnUnitSelect;
    }
    void OnAction(IAction action) => queue.Enqueue(action);
    void Update(){
        if(Input.GetKey(KeyCode.A)){
            Camera.main.transform.Translate(new Vector3(-5 * Time.deltaTime,0,0));
        }else if(Input.GetKey(KeyCode.D)){
            Camera.main.transform.Translate(new Vector3(5 * Time.deltaTime,0,0));
        }
    }
    void OnNavigation(LocationState previousLocation){
        if(previousLocation != null){
            previousLocation.updateEvent -= OnLocationUpdate;
            Destroy(interior);
            foreach(var tile in tiles) Destroy(tile);
            foreach(var entry in units) Destroy(entry.Value);
            foreach(var entry in ground) Destroy(entry.Value);
            foreach(var entry in bars) Destroy(entry.Value);
            ground.Clear();
            units.Clear();
            bars.Clear();
        }
        location = player.state.activeLocation;
        location.updateEvent += OnLocationUpdate;
        interior = Instantiate(location.template.prefab, transform);
        tiles = new GameObject[location.TileCount];
        for(int i = 0; i < tiles.Length; i++){
            float x = tileSize.x * (i / location.template.rows) + tileSize.x;
            float y = tileSize.y * (i % location.template.rows) - tileSize.y * (0.6f * location.template.rows);
            tiles[i] = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
            
            tiles[i].GetComponentInChildren<TileView>().index = i;
            tiles[i].GetComponentInChildren<TileView>().parentView = this;

            UnitState unit = location[i] as UnitState;
            if(unit == null) continue;
            var _unit = Instantiate(unit.template.prefab, new Vector3(x, 0, y), Quaternion.identity, transform);
            var statusBar = Instantiate(statusBarPrefab, overlay.transform);

            statusBar.GetComponent<StatusBarView>().target = _unit;
            statusBar.GetComponent<StatusBarView>().unit = unit;

            units[unit] = _unit;
            bars[unit] = statusBar;
        }
    }
    void OnLocationUpdate(int tile){
        for(int i = 0; i < location.TileCount; i++){
            UnitState unit = location[i] as UnitState;
            if(unit == null) continue;
            units[unit].transform.position = tiles[i].transform.position;
        }

        for(int i = 0; i < tiles.Length; i++){
            if(activeUnit == location[i] && activeUnit != null){
                tiles[i].GetComponentInChildren<TileView>().SetHighlight(true);
            }else{
                tiles[i].GetComponentInChildren<TileView>().SetHighlight(false);
            }
        }

        var remove = new Stack<GroundEffectState>();
        foreach(var groundEffect in ground)
            if(groundEffect.Key.Parent == null){
                Destroy(groundEffect.Value);
                remove.Push(groundEffect.Key);
            }
        while(remove.Count != 0) ground.Remove(remove.Pop());

        for(int i = 0; i < location.TileCount; i++){
            GroundEffectState groundEffect = location[location.TileCount + i] as GroundEffectState;
            if(groundEffect == null) continue;
            if(ground.ContainsKey(groundEffect)){
                ground[groundEffect].transform.position = tiles[i].transform.position;
            }else{
                ground[groundEffect] = Instantiate(groundEffect.template.prefab, Vector3.zero, Quaternion.identity, transform);
                ground[groundEffect].transform.position = tiles[i].transform.position;
            }
        }
    }
    IEnumerator UpdateLoop(){
        while(true){
            if(queue.Count != 0) yield return HandleAction(queue.Dequeue());
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator HandleAction(IAction action){
        UpdateStatusBars();
        switch(action){
            case EffectState effect:
                if(effect.template.prefab == null) yield break;
                var skillEffect = Instantiate(effect.template.prefab, transform);
                var effectView = skillEffect.GetComponentInChildren<EffectView>();
                effectView.Effect = effect;
                effectView.Battlefield = this;
                yield return new WaitForSeconds(1);
                break;
            case DeathAction death:
                var unit = units[death.target];
                units.Remove(death.target);
                unit.GetComponent<UnitView>().TriggerAnimation(UnitAnimation.DEATH);
                Destroy(bars[death.target]);
                bars.Remove(death.target);
                break;
            default: yield break;
        }
    }

    public void UpdateStatusBars(){
        foreach(var bar in bars) bar.Value.GetComponent<StatusBarView>().UpdateDisplay();
    }

    private UnitState activeUnit;
    void OnUnitSelect(IContainer container, int index, bool toggle){
        UnitState unit = container[index] as UnitState;
        if(!toggle || container != player.state.activeLocation) return;
        if(activeUnit != null){
            foreach(var inventory in activeUnit.GetNodes<InventoryState>()) inventory.template.OpenInventory(null);
            activeUnit = null;
            OnLocationUpdate(-1); //TODO just to update highlight
        }
        if(unit == null) return;

        if(unit.Agency is PlayerTemplate || unit.Agency is NPCTemplate)
        foreach(var inventory in unit.GetNodes<InventoryState>()) inventory.template.OpenInventory(inventory);
        if(unit.Agency != player) return;
        activeUnit = unit;
        OnLocationUpdate(-1); //TODO just to update highlight
    }
}