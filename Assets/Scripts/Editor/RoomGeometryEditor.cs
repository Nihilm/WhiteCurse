using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomGeometry))]
public class RoomGeometryInspector : Editor {
    public override void OnInspectorGUI(){
        serializedObject.Update();
        DrawDefaultInspector();

        if(GUILayout.Button("Open Room Editor"))
            RoomGeometryEditor.OpenWindow((RoomGeometry) target);

        serializedObject.ApplyModifiedProperties();
    }
    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line){
        var assetPath = AssetDatabase.GetAssetPath(instanceID);
        var assetData = AssetDatabase.LoadAssetAtPath<RoomGeometry>(assetPath);
        if(assetData == null) return false;
        RoomGeometryEditor.OpenWindow(assetData);
        return true;
    }
}

public class RoomGeometryEditor : EditorWindow {
    public static void OpenWindow(RoomGeometry room){
        var window = EditorWindow.GetWindow<RoomGeometryEditor>("Room Editor", Type.GetType("UnityEditor.GameView,UnityEditor"));
        window.Load(room);
        window.Show();
    }
    private int pickerControlID = -1;
    private Vector2 pan = Vector2.zero;
    private float zoom = 1;
    private int gridSize = 64;
    private int mode;

    private RoomGeometry room;
    private List<Vector2Int> filled = new List<Vector2Int>();
    private List<Vector2Int> sockets = new List<Vector2Int>();
    private IList<IList<Vector2Int>> outline = new List<IList<Vector2Int>>();
    private List<_Algorithms.OrthogonalConnector> connectors = new List<_Algorithms.OrthogonalConnector>();

    void OnSelectionChange(){Close();}
    public void Clear(){
        filled.Clear(); sockets.Clear(); outline.Clear(); connectors.Clear(); room = null;
    }
    public void Load(RoomGeometry room){
        this.Clear();
        if(room == null) return;
        this.room = room;
        for(int x = 0; x < room.bounds.width; x++)
        for(int y = 0; y < room.bounds.height; y++){
            int index = x + y * room.bounds.width;
            if(room.filled[index] != 0) filled.Add(new Vector2Int(x, y) + room.bounds.min);
            if(room.sockets[index] != 0) sockets.Add(new Vector2Int(x, y) + room.bounds.min);
        }
        outline = room.Outline();
        connectors = RoomGeometry.AttachSockets(room.sockets, room.bounds, outline);
    }
    public void SaveUpdate(){
        this.room.FromList(filled, sockets);
        outline = room.Outline();
        connectors = RoomGeometry.AttachSockets(room.sockets, room.bounds, outline);
        EditorUtility.SetDirty(room);
    }
    private void OnDisable(){ rootVisualElement.Clear(); Clear(); }
    public void OnGUI(){
        DrawGrid(gridSize, new Color(0.5f,0.5f,0.5f,0.2f));
        DrawGrid(gridSize * 5, new Color(0.5f,0.5f,0.5f,0.5f));
        DrawTiles();

        GUILayout.BeginArea(new Rect(0, 0, position.width, 20), EditorStyles.toolbar);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Selected: {(room == null ? "None" : room.displayName)}", new GUIStyle(EditorStyles.label){ fixedWidth = 100 });
        if(room == null) GUI.enabled = false;
        mode = GUILayout.SelectionGrid(mode, new string[]{
            "pan", "walls", "doors", "clear"
        }, 4);
        GUI.enabled = true;
        if(GUILayout.Button(new GUIContent("Reset"), EditorStyles.toolbarButton, GUILayout.Width(100))){
            zoom = 1; pan = Vector2.zero; mode = 0;
        }
        if(GUILayout.Button(new GUIContent("Select in inspector"), EditorStyles.toolbarButton, GUILayout.Width(150)))
            Selection.activeObject = room;
        if(GUILayout.Button(new GUIContent("Select"), EditorStyles.toolbarButton, GUILayout.Width(150)))
            EditorGUIUtility.ShowObjectPicker<RoomGeometry>(null, false, string.Empty, pickerControlID =  GUIUtility.GetControlID(this.GetInstanceID(), FocusType.Passive));
        if(Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == pickerControlID){
            pickerControlID = -1;
            Load(EditorGUIUtility.GetObjectPickerObject() as RoomGeometry);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        HandleEvents();
    }
    private void HandleEvents(){
        Vector2 localPosition = Event.current.mousePosition / zoom - pan;
        switch(Event.current.type){
            case EventType.MouseDown: {
                Vector2Int tile = new Vector2Int(
                    Mathf.RoundToInt(localPosition.x / (float) gridSize - 0.5f),
                    Mathf.RoundToInt(localPosition.y / (float) gridSize - 0.5f)
                );
                if(mode == 1){
                    if(!filled.Contains(tile)) filled.Add(tile);
                    SaveUpdate();
                }else if(mode == 2){
                    if(!sockets.Contains(tile)) sockets.Add(tile);
                    SaveUpdate();
                }else if(mode == 3){
                    int index0 = sockets.IndexOf(tile);
                    int index1 = filled.IndexOf(tile);
                    if(index0 != -1) sockets.RemoveAt(index0);
                    else if(index1 != -1) filled.RemoveAt(index1);
                    SaveUpdate();
                }
                GUI.changed = true;
                break;
            }
            case EventType.MouseDrag: {
                if(Event.current.button == 0 && mode == 0) pan += Event.current.delta / zoom;
                GUI.changed = true;
                break;
            }
            case EventType.ScrollWheel: {
                float prevZoom = zoom;
                zoom = Mathf.Clamp(zoom - (Mathf.Sign(Event.current.delta.y) * zoom * 0.1f), 0.1f, 5f);
                pan += -(zoom * (Event.current.mousePosition - pan * prevZoom) - Event.current.mousePosition * prevZoom) / (zoom * prevZoom) - pan;
                GUI.changed = true;
                break;
            }
        }
        if(GUI.changed) Repaint();
    }
    private void DrawGrid(float spacing, Color color){
        Handles.BeginGUI();
        Color prevColor = Handles.color;
        Handles.color = color;
        spacing *= zoom;
        Vector3 offset = new Vector3((zoom * pan.x) % spacing, (zoom * pan.y) % spacing, 0);
        for(int i = Mathf.CeilToInt(position.width / spacing) - 1; i >= 0; i--)
            Handles.DrawLine(new Vector3(spacing * i, -spacing, 0) + offset, new Vector3(spacing * i, position.height + spacing, 0) + offset);
        for(int i = Mathf.CeilToInt(position.height / spacing) - 1; i >= 0; i--)
            Handles.DrawLine(new Vector3(-spacing, spacing * i, 0) + offset, new Vector3(position.width + spacing, spacing * i, 0) + offset);
        Handles.EndGUI();
        Handles.color = prevColor;
    }
    private void DrawTiles(){
        Vector3 offset = new Vector3(zoom * pan.x, zoom * pan.y, 0);
        foreach(var tile in filled){
            EditorGUI.DrawRect(new Rect(
                (tile.x * gridSize + pan.x) * zoom,
                (tile.y * gridSize + pan.y) * zoom,
                gridSize * zoom, gridSize * zoom
            ), Color.grey);
        }
        foreach(var socket in sockets){
            EditorGUI.DrawRect(new Rect(
                ((socket.x + 0.25f) * gridSize + pan.x) * zoom,
                ((socket.y + 0.25f) * gridSize + pan.y) * zoom,
                gridSize * 0.5f * zoom, gridSize * 0.5f * zoom
            ), Color.red);
        }
        Color prevColor = Handles.color;
        Handles.color = Color.green;
        foreach(var path in outline)
        for(int i = path.Count - 1, j = 0; i >= 0; j = i--){
            Handles.DrawLine(
                new Vector3(path[i].x * gridSize, path[i].y * gridSize, 0) * zoom + offset,
                new Vector3(path[j].x * gridSize, path[j].y * gridSize, 0) * zoom + offset);
        }
        Handles.color = prevColor;
        foreach(var socket in connectors)
            Handles.DrawSolidRectangleWithOutline(new Rect(
                new Vector2(socket.bounds.position.x, socket.bounds.position.y) * gridSize * zoom + new Vector2(offset.x, offset.y),
                new Vector2(Math.Max(socket.bounds.size.x, 0.1f), Math.Max(socket.bounds.size.y, 0.1f)) * gridSize * zoom
            ), new Color(1f,1f,0.0f,0.5f), Color.clear);
    }
}