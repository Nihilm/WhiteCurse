using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationGridView : MonoBehaviour {
    [SerializeField] private GameObject topLayer;
    //[SerializeField] public PlayerTemplate player;
    [SerializeField] private GameObject statusBarPrefab;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector2 tileSize;
    [HideInInspector] private GameObject[] tiles;
    //[HideInInspector] private Dictionary<UnitState, GameObject> units = new Dictionary<UnitState, GameObject>();
    //[HideInInspector] private Dictionary<UnitState, GameObject> bars = new Dictionary<UnitState, GameObject>();

    void Start(){
        //player.navigationEvent += OnNavigate;
        //player.selectTileEvent += OnSelectUnit;
        gameObject.SetActive(false);
    }
    void OnDestroy(){
        //player.navigationEvent -= OnNavigate;
    }
    void Update(){
        if(Input.GetKey(KeyCode.A)){
            Camera.main.transform.Translate(new Vector3(-5 * Time.deltaTime,0,0));
        }else if(Input.GetKey(KeyCode.D)){
            Camera.main.transform.Translate(new Vector3(5 * Time.deltaTime,0,0));
        }
    }
    void Clear(){
        // if(tiles != null) foreach(var tile in tiles) if(tile != null) Destroy(tile);
        // foreach(var unit in units) Destroy(unit.Value);
        // foreach(var bar in bars) Destroy(bar.Value);
        // units.Clear(); bars.Clear();
    }
    // void OnNavigate(AreaState prevArea, int prevLocation){
    //     Clear();
    //     if(prevArea != null) prevArea.locations[prevLocation].updateEvent -= OnAreaUpdate;
    //     LocationState area = player.ActiveLocation;
    //     area.updateEvent += OnAreaUpdate;

    //     tiles = new GameObject[area.tiles.Length];
    //     for(int i = 0; i < tiles.Length; i++){
    //         float x = tileSize.x * (i / area.template.tiles.rows);
    //         float y = tileSize.y * (i % area.template.tiles.rows) - tileSize.y * (0.5f * area.template.tiles.rows);
    //         tiles[i] = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
    //         tiles[i].transform.GetChild(0).GetComponent<TileView>().index = i;
    //         tiles[i].transform.GetChild(0).GetComponent<TileView>().parentView = this;
    //         if(area.units[i] != null){
    //             var unit = Instantiate(area.units[i].template.prefab, new Vector3(x, 0, y), Quaternion.identity, transform);
    //             var statusBar = Instantiate(statusBarPrefab, topLayer.transform);
    //             StatusBarView statusBarView = statusBar.GetComponent<StatusBarView>();
    //             statusBarView.target = unit;
    //             statusBarView.unit = area.units[i];
    //             statusBarView.CreateView();

    //             units[area.units[i]] = unit;
    //             bars[area.units[i]] = statusBar;
    //         }
    //     }
    // }
    // void OnAreaUpdate(){
    //     LocationState area = player.ActiveLocation;
    //     for(int i = 0; i < area.tiles.Length; i++){
    //         if(area.units[i] == null) continue;
    //         var unit = units[area.units[i]];
    //         var tile = tiles[i];
    //         unit.transform.position = tile.transform.position;
    //     }
    // }
    // void OnSelectUnit(int tile){
    //     var selectedUnit = tile != -1 ? player.ActiveLocation.units[tile] : null;
    //     foreach(var bar in bars){
    //         bar.Value.GetComponent<UnityEngine.UI.Image>().color = bar.Key == selectedUnit
    //         ? new Color(1,0,0,1)
    //         : new Color(0,0,0,1);
    //     }
    // }
}
