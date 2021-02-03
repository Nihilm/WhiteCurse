using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using _Template;

public class ExplorationView : MonoBehaviour, IAreaView {
    [SerializeField] private GameObject token;
    public AreaState Area{get;set;}
    public LocationState Location{get;set;}
    public event System.Action<LocationState> navigationEvent;
    private GameObject[] rooms;
    private _Algorithms.LayoutState layout;

    void Start(){
        Debug.Log($"Rendering Map: {Area.template.displayName}");
        layout = Area.GenerateMap();
        RectInt bounds = layout.CalculateBounds();
        rooms = new GameObject[layout.geometries.Length];
        for(int i = 0; i < layout.geometries.Length; i++){
            rooms[i] = new GameObject($"Room{i}");
            var roomBounds = _Algorithms.RectilinearPolygon.CalculateBounds(layout.geometries[i].parent.rectangles);
            rooms[i].transform.SetParent(transform, false);

            var image = rooms[i].AddComponent<SpriteRenderer>();
            //TODO move doors handling elsewhere?
            var doors = new List<Vector2Int>();
            foreach(int neighbour in Area.graph.Neighbours(i)){
                var connectors = new List<_Algorithms.OrthogonalConnector>(layout.geometries[i].SampleConnectors(layout.geometries[neighbour]));
                foreach(var connector in connectors){
                    Vector2Int offset = new Vector2Int(
                        connector.direction[0] == -1 ? -1 : 0,
                        connector.direction[1] == 1 ? -1 : 0
                    );
                    doors.Add(connector.bounds.min - layout.geometries[i].position + offset);
                }
            }
            
            var texture = RenderRoomGeometry(layout.geometries[i].parent, doors);

            image.sprite = Sprite.Create(texture, new Rect(0, 0, roomBounds.width, roomBounds.height), Vector2.zero, 1f);

            rooms[i].transform.localPosition = new Vector3(
                layout.geometries[i].position.x - bounds.min.x + roomBounds.min.x,
                layout.geometries[i].position.y - bounds.min.y + roomBounds.min.y, 0);
            rooms[i].transform.localRotation = Quaternion.identity;

            var label = new GameObject("label");
            label.transform.SetParent(rooms[i].transform, false);
            var textMesh = label.AddComponent<TextMesh>();
            label.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            textMesh.text = Area.locations[i].template.displayName;
            label.transform.localPosition.Set(0, 0, -1);

            rooms[i].AddComponent<BoxCollider>();
            var trigger = rooms[i].AddComponent<EventTrigger>();
            trigger.AddEventListener(EventTriggerType.PointerEnter, eventData => {
                image.color = new Color(1, 0, 0, 1);
            });
            trigger.AddEventListener(EventTriggerType.PointerExit, eventData => {
                image.color = new Color(1, 1, 1, 1);
            });
            int index = i;
            trigger.AddEventListener(EventTriggerType.PointerUp, eventData => {
                navigationEvent?.Invoke(Area.locations[index]);
                MoveToken();
            });
        }
        MoveToken();
    }
    void MoveToken(){
        GameObject room = rooms[Location.Index];
        RectInt roomBounds = _Algorithms.RectilinearPolygon.CalculateBounds(layout.geometries[Location.Index].rectangles);
        token.transform.localPosition = room.transform.localPosition
         + new Vector3(roomBounds.width * 0.5f, roomBounds.height * 0.5f, 0);
    }
    void Update(){
        if(Input.GetKey(KeyCode.A)){
            transform.Translate(new Vector3(5 * Time.deltaTime,0,0));
        }else if(Input.GetKey(KeyCode.D)){
            transform.Translate(new Vector3(-5 * Time.deltaTime,0,0));
        }else if(Input.GetKey(KeyCode.W)){
            transform.Translate(new Vector3(0,-5 * Time.deltaTime,0));
        }else if(Input.GetKey(KeyCode.S)){
            transform.Translate(new Vector3(0,5 * Time.deltaTime,0));
        }
    }
    private Texture2D RenderRoomGeometry(_Algorithms.NodeGeometry geometry, List<Vector2Int> doors){
        var bounds = _Algorithms.RectilinearPolygon.CalculateBounds(geometry.rectangles);
        Texture2D texture = new Texture2D(bounds.width, bounds.height, TextureFormat.ARGB32, false, false);
        texture.filterMode = FilterMode.Point;
        foreach(var rectangle in geometry.rectangles){
            for(int x = rectangle.min.x; x < rectangle.max.x; x++)
            for(int y = rectangle.min.y; y < rectangle.max.y; y++){
                if(x == rectangle.min.x || x == rectangle.max.x - 1 || y == rectangle.min.y || y == rectangle.max.y - 1)
                texture.SetPixel(x - bounds.min.x, y - bounds.min.y, new Color(0, 0, 0, 1));
                else
                texture.SetPixel(x - bounds.min.x, y - bounds.min.y, new Color(1, 1, 1, 1));
            }
        }
        foreach(var door in doors){
            texture.SetPixel(door.x - bounds.min.x, door.y - bounds.min.y, new Color(0.7f,0.7f,0.7f,1));
        }
        texture.Apply();
        return texture;
    }
}
