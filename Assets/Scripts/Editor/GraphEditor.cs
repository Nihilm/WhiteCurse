namespace _Algorithms {
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    [CustomPropertyDrawer(typeof(TopologyGraph),true)]
    public class GraphPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "Graph");
            if(GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight), "Edit Graph")){
                GraphEditor.OpenWindow(new SerializedObject(property.serializedObject.targetObject).FindProperty(property.propertyPath));
                //var assetPath = AssetDatabase.GetAssetPath(property.serializedObject.targetObject.GetInstanceID());
                //var levelGraph = AssetDatabase.LoadAssetAtPath(assetPath);
                
                //.LoadAssetAtPath<LevelGraph>(assetPath);
            }
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
        }
    }
    public class GraphEditor : EditorWindow {
        private SerializedObject _object;
        private SerializedProperty _nodes;
        private SerializedProperty _positions;
        private SerializedProperty _links;
        private List<Vector2> positions = new List<Vector2>();
        private List<Vector2Int> links = new List<Vector2Int>();

        private Vector2 pan = Vector2.zero;
        private Vector2 prevPosition = Vector2.zero;
        private int selected = -1;
        private float zoom = 1;
        private int gridSize = 16;
        private bool snapToGrid;

        //[MenuItem ("Window/Level/Graph Editor")]
        public static void OpenWindow(SerializedProperty property = null){
            GraphEditor window = GetWindow<GraphEditor>("Graph Editor", Type.GetType("UnityEditor.SceneView,UnityEditor"));
            window.Clear();
            if(property != null) window.Load(property);
            window.Show();
        }
        void OnSelectionChange(){Close();}
        public void Load(SerializedProperty property){
            this.Clear();
            _object = property.serializedObject;
            _nodes = property.serializedObject.FindProperty(property.propertyPath + ".nodes");
            _links = property.serializedObject.FindProperty(property.propertyPath + ".links");
            _positions = property.serializedObject.FindProperty(property.propertyPath + ".positions");

            for(int i = 0; i < _nodes.arraySize; i++)
                positions.Add(_positions.GetArrayElementAtIndex(i).vector2Value);
            for(int i = 0; i < _links.arraySize; i++)
                links.Add(_links.GetArrayElementAtIndex(i).vector2IntValue);

            rootVisualElement.MarkDirtyRepaint();
        }
        private void ToggleLink(int prevNode, int nextNode){
            int index = links.FindIndex(pair => pair.x == prevNode && pair.y == nextNode || pair.x == nextNode && pair.y == prevNode);
            int l0 = links.IndexOf(new Vector2Int(prevNode, nextNode));
            int l1 = links.IndexOf(new Vector2Int(nextNode, prevNode));
            if(index == -1){
                _links.arraySize++;
                _links.GetArrayElementAtIndex(_links.arraySize - 1).vector2IntValue = new Vector2Int(prevNode, nextNode);
                links.Add(new Vector2Int(prevNode, nextNode));
            }else{
                links.RemoveAt(index);
                if(index == _links.arraySize - 1) _links.arraySize--;
                else _links.DeleteArrayElementAtIndex(index);
            }
            _links.serializedObject.ApplyModifiedProperties();
            _links.serializedObject.Update();
        }
        private void DeleteNode(int node){
            for(int i = links.Count - 1; i >= 0; i--){
                var pair = links[i];
                if(pair[0] == node || pair[1] == node){
                    links.RemoveAt(i);
                    if(i == _links.arraySize - 1) _links.arraySize--;
                    else _links.DeleteArrayElementAtIndex(i);
                }else{
                    if(pair[0] > node) pair[0]--;
                    if(pair[1] > node) pair[1]--;
                    links[i] = pair;
                    _links.GetArrayElementAtIndex(i).vector2IntValue = pair;
                }
            }
            positions.RemoveAt(node);
            if(node == _nodes.arraySize - 1) _nodes.arraySize--;
            else _nodes.DeleteArrayElementAtIndex(node);
            if(node == _positions.arraySize - 1) _positions.arraySize--;
            else _positions.DeleteArrayElementAtIndex(node);
            _links.serializedObject.ApplyModifiedProperties();
            _links.serializedObject.Update();
        }
        private void AddNode(Vector2 position){
            positions.Add(position);
            _nodes.arraySize++;
            _positions.arraySize++;
            _positions.GetArrayElementAtIndex(_positions.arraySize - 1).vector2Value = position;
            _links.serializedObject.ApplyModifiedProperties();
            _links.serializedObject.Update();
        }
        private void MoveNode(int node, Vector2 position){
            positions[node] = position;
            _positions.GetArrayElementAtIndex(node).vector2Value = position;
            _links.serializedObject.ApplyModifiedProperties();
            _links.serializedObject.Update();
        }
        public void OnEnable(){}
        public void OnGUI(){
            if(_object != null && _object.targetObject != Selection.activeObject) Clear();
            HandleEvents();
            DrawGrid(gridSize, new Color(0.5f,0.5f,0.5f,0.2f));
            DrawGrid(gridSize * 5, new Color(0.5f,0.5f,0.5f,0.5f));
            DrawNodes();

            GUILayout.BeginArea(new Rect(0, 0, position.width, 20), EditorStyles.toolbar);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{(_object == null ? "Nothing selected" : "Selected")}", new GUIStyle(EditorStyles.label){ fixedWidth = 100 });
            snapToGrid = GUILayout.Toggle(snapToGrid, "Snap to grid", GUILayout.Width(120));
            if(GUILayout.Button(new GUIContent("Reset"), EditorStyles.toolbarButton, GUILayout.Width(100))){
                zoom = 1; pan = Vector2.zero;
            }

            // if(GUILayout.Button(new GUIContent("Select in inspector"), EditorStyles.toolbarButton, GUILayout.Width(150))){
            //     Selection.activeObject = LevelGraph;

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        private void OnDisable(){ rootVisualElement.Clear(); Clear(); }
        private void Clear(){ _object = null; _links = _nodes = _positions = null; positions.Clear(); links.Clear(); }
        private void HandleEvents(){
            Vector2 localPosition = Event.current.mousePosition / zoom - pan;
            switch(Event.current.type){
                case EventType.MouseDown: {
                    selected = OnHover(Event.current.mousePosition);
                    prevPosition = selected == -1 ? localPosition : positions[selected] - localPosition;
                    GUI.changed = true;
                    break;
                }
                case EventType.MouseUp: {
                    int nextSelected = OnHover(Event.current.mousePosition);
                    if(selected != -1 && nextSelected != -1 && selected != nextSelected && Event.current.button == 1)
                        ToggleLink(selected, nextSelected);
                    else if(Event.current.button == 1){
                        var genericMenu = new GenericMenu();
                        if(nextSelected == -1) genericMenu.AddItem(new GUIContent("Create Node"), false, () => {
                            if(snapToGrid) localPosition = new Vector2(
                            Mathf.Round(localPosition.x / (float) gridSize) * gridSize,
                            Mathf.Round(localPosition.y / (float) gridSize) * gridSize);
                            AddNode(localPosition);
                        });
                        else genericMenu.AddItem(new GUIContent("Delete Node"), false, () => {
                            DeleteNode(nextSelected);
                        });
                        genericMenu.ShowAsContext();
                    }
                    selected = -1;
                    GUI.changed = true;
                    break;
                }
                case EventType.MouseDrag: {
                    if(Event.current.button == 0 && selected == -1) pan += Event.current.delta / zoom;
                    else if(Event.current.button == 0){
                        Vector2 nodePosition = prevPosition + localPosition;
                        if(snapToGrid) nodePosition = new Vector2(
                        Mathf.Round(nodePosition.x / (float) gridSize) * gridSize,
                        Mathf.Round(nodePosition.y / (float) gridSize) * gridSize);
                        MoveNode(selected, nodePosition); 
                    }
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
        private void DrawNodes(){
            for(int i = 0; i < links.Count; i++){
                var rect0 = CalculateNodePosition(links[i][0]);
                var rect1 = CalculateNodePosition(links[i][1]);
                Handles.DrawBezier(rect0.center, rect1.center,
                new Vector3(0.5f * (rect1.center.x + rect0.center.x), rect0.center.y, 0),
                new Vector3(0.5f * (rect0.center.x + rect1.center.x), rect1.center.y, 0), Color.white, Texture2D.whiteTexture, 2);
            }
            if(selected != -1 && Event.current.button == 1)
            Handles.DrawLine(CalculateNodePosition(selected).center, Event.current.mousePosition);

            var style = new GUIStyle();
            style.normal.background = Texture2D.whiteTexture;
            style.normal.textColor = Color.white;
            style.fontSize = (int) (12 * zoom);
            style.alignment = TextAnchor.MiddleCenter;
            Color prevColor = GUI.backgroundColor;
            for(int i = 0; i < positions.Count; i++){
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                var rectangle = CalculateNodePosition(i);
                GUI.Box(CalculateNodePosition(i), $"Node {i}", style);
                GUI.backgroundColor = prevColor;
                EditorGUI.PropertyField(
                    new Rect(rectangle.position + new Vector2(0, 36 * zoom), rectangle.size),
                    _nodes.GetArrayElementAtIndex(i), GUIContent.none, true);
            }
            _nodes.serializedObject.ApplyModifiedProperties();
            _nodes.serializedObject.Update();
        }
        private Rect CalculateNodePosition(int node) => new Rect(
            (positions[node].x + pan.x) * zoom,
            (positions[node].y + pan.y) * zoom, (EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth) * zoom, 36 * zoom);
        private int OnHover(Vector2 position){
            for(int i = 0; i < positions.Count; i++)
                if(CalculateNodePosition(i).Contains(position)) return i;
            return -1;
        }
    }
}