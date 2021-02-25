using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using _Template;

public class ExplorationView : MonoBehaviour, IAreaView {
    [SerializeField] private MeshFilter levelMesh;
    [SerializeField] private GameObject layer;
    [SerializeField] private GameObject token;
    [SerializeField] private Sprite maskSprite;
    public AreaState Area{get;set;}
    public LocationState Location{get;set;}
    public event System.Action<LocationState> navigationEvent;
    private GameObject[] rooms;
    private _Algorithms.LayoutState layout;

    void Start(){
        Debug.Log($"Rendering Map: {Area.template.displayName}");
        layout = Area.GenerateMap();

        levelMesh.mesh = RoomGeometry.ConstructGeometry(layout);
        var globalBounds = layout.CalculateBounds();
        rooms = new GameObject[layout.geometries.Length];
        for(int i = 0; i < layout.geometries.Length; i++){
            var bounds = _Algorithms.RectilinearPolygon.CalculateBounds(layout.geometries[i].rectangles);

            var room = new GameObject($"Room {i}");
            room.layer = LayerMask.NameToLayer("PostProcessing");
            room.transform.SetParent(layer.transform, false);

            var image = room.AddComponent<SpriteRenderer>();
            image.sprite = Area.template.locations.nodes[i].icon;

            room.transform.localPosition = new Vector3(
                0.5f * (bounds.min.x + bounds.max.x) - globalBounds.min.x,
                0.5f * (bounds.min.y + bounds.max.y) - globalBounds.min.y,-1);
            rooms[i] = room;

            var mask = new GameObject("Mask");
            mask.layer = LayerMask.NameToLayer("Mask");
            var maskImage = mask.AddComponent<SpriteRenderer>();
            maskImage.color = Color.clear;
            maskImage.sprite = maskSprite;
            maskImage.transform.localScale = new Vector3(
                (2+bounds.width) / maskSprite.rect.width * maskSprite.pixelsPerUnit,
                (2+bounds.height) / maskSprite.rect.height * maskSprite.pixelsPerUnit, 1);
            mask.transform.SetParent(room.transform, false);
            GetComponentInChildren<MaskPostEffect>().Add(mask);

            var collider = room.AddComponent<PolygonCollider2D>();
            collider.pathCount = 1;
            collider.SetPath(0, new[]{
                new Vector2(-0.5f*bounds.width,-0.5f*bounds.height),
                new Vector2(0.5f*bounds.width,-0.5f*bounds.height),
                new Vector2(0.5f*bounds.width,0.5f*bounds.height),
                new Vector2(-0.5f*bounds.width,0.5f*bounds.height),
            });
            var trigger = room.AddComponent<EventTrigger>();
            trigger.AddEventListener(EventTriggerType.PointerEnter, eventData => {
                image.color = new Color(1, 0, 0, 1);
            });
            trigger.AddEventListener(EventTriggerType.PointerExit, eventData => {
                image.color = new Color(1, 1, 1, 1);
            });
            int index = i;
            trigger.AddEventListener(EventTriggerType.PointerUp, eventData => {
                if(Area.graph[index, Location.Index] == 0) return;
                navigationEvent?.Invoke(Area.locations[index]);
                OnLocationChange(index);
            });
        }

        token.transform.localPosition = rooms[Location.Index].transform.localPosition;
        for(int i = 0; i < Area.graph.NodeCount; i++)
            if(i == Location.Index || Area.graph[i, Location.Index] != 0)
                rooms[i].transform.Find("Mask").gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }
    private void OnLocationChange(int index){
        StartCoroutine(MoveToken(index, 0.5f));
        foreach(var neighbour in Area.graph.Neighbours(index))
            StartCoroutine(RevealRoom(neighbour, 2.4f));
    }
    private IEnumerator MoveToken(int index, float duration){
        GameObject room = rooms[index];
        Vector3 start = token.transform.localPosition, end = room.transform.localPosition;
        for(float elapsed = 0f; elapsed < duration;){
            token.transform.localPosition = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator RevealRoom(int index, float duration){
        GameObject room = rooms[index];
        GameObject mask = room.transform.Find("Mask")?.gameObject;
        SpriteRenderer spriteRenderer = mask.GetComponent<SpriteRenderer>();
        if(spriteRenderer.color.a != 0) yield break;
        Vector3 scale = mask.transform.localScale;
        for(float elapsed = 0f; elapsed < duration;){
            spriteRenderer.color = Color.Lerp(Color.clear, Color.white, elapsed / duration);
            mask.transform.localScale = Vector3.Lerp(Vector3.zero, scale, Mathf.Min(1, 2*elapsed / duration));
            elapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}