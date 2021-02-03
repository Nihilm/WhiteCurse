// [CustomEditor(typeof(A))]
// public class AEditor : Editor
// {
//     private A a;
 
//     void OnEnable()
//     {
//         a = (A) target;
//     }
 
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();
 
//         if (GUILayout.Button("Add"))
//         {
//             if (a.b == null)
//             {
//                 a.b = CreateInstance<B>();
//                 AssetDatabase.AddObjectToAsset(a.b, a);
//                 AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(a.b));
//             }
//         }
//     }
 
// }










// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using UnityEditor.Experimental.GraphView;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;

// namespace UnityEditor.Custom {
//     [CustomPropertyDrawer(typeof(NodeGraph),true)]
//     public class GraphPropertyDrawer : PropertyDrawer {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
//             EditorGUI.BeginProperty(position, label, property);
//             EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "Locations");
//             if(GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight), "Edit Graph"))
//             GraphEditor.ShowWindow(property);
//             EditorGUI.EndProperty();
//         }
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
//             return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
//         }
//     }
//     public class EditorGraphView : GraphView {
//         public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter){
//             List<Port> compatiblePorts = new List<Port>();
// 			foreach(Port candidatePort in ports.ToList()){
//                 if(startAnchor.direction == candidatePort.direction) continue;
//                 if(startAnchor.parent.parent == candidatePort.parent.parent) continue;
//                 compatiblePorts.Add(candidatePort);
// 			}
// 			return compatiblePorts;
//         }
//     }
//     public class GraphEditor : EditorWindow {
//         public static void ShowWindow(SerializedProperty property){
//             GraphEditor window = GetWindow<GraphEditor>();
//             window.titleContent = new GUIContent("Graph Editor");
//             window.Show();
//             window.Reload(property);
//         }
//         private SerializedProperty _property;
//         private SerializedProperty _nodes;
//         private SerializedProperty _links;
//         private SerializedProperty _positions;
//         private EditorGraphView graphView;
//         private void CreateGUI(){
//             rootVisualElement.Clear();
//             graphView = new EditorGraphView();
//             GridBackground gridBackground = new GridBackground();
//             graphView.Add(gridBackground);
//             gridBackground.SendToBack();

//             MiniMap miniMap = new MiniMap(){ anchored = true };
//             graphView.Add(miniMap);

//             graphView.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
//             graphView.AddManipulator(new ContentDragger(){ clampToParentEdges = true });
//             graphView.AddManipulator(new SelectionDragger());
//             graphView.AddManipulator(new RectangleSelector());
//             graphView.AddManipulator(new ClickSelector());
//             graphView.nodeCreationRequest = OnNodeCreationRequest;
//             graphView.graphViewChanged = OnGraphViewChanged;

//             rootVisualElement.Add(graphView);
//             graphView.StretchToParentSize();
//         }
//         void OnDisable(){
//             _property = _links = _nodes = null;
//             graphView.RemoveFromHierarchy();
//         }
//         void OnNodeCreationRequest(NodeCreationContext context){
//             Vector2 position = graphView.contentViewContainer.WorldToLocal(context.screenMousePosition - this.position.position);
//             _nodes.arraySize++;
//             _positions.arraySize++;
//             _positions.GetArrayElementAtIndex(_positions.arraySize - 1).vector2Value = position;
//             _nodes.serializedObject.ApplyModifiedProperties();
//             _nodes.serializedObject.Update();
//             Node node = CreateNode(_nodes.GetArrayElementAtIndex(_nodes.arraySize - 1), _nodes.arraySize - 1);
//             node.SetPosition(new Rect(position.x, position.y, 0, 0));
//             graphView.AddElement(node);
//         }
//         Node CreateNode(SerializedProperty property, int i){
//             Node node = new Node(){ name = $"{i}", title = $"Node {i}" };
//             Port input = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, null);
//             Port output = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, null);
//             input.portName = "Input";
//             output.portName = "Output";
//             node.inputContainer.Add(input);
//             node.outputContainer.Add(output);
//             PropertyField field = new PropertyField(property, "prefab");
//             field.Bind(property.serializedObject);
//             node.contentContainer.Add(field);
//             return node;
//         }
//         void Reload(SerializedProperty property){
//             this._property = property;
//             _nodes = property.serializedObject.FindProperty(property.propertyPath + ".nodes");
//             _links = property.serializedObject.FindProperty(property.propertyPath + ".links");
//             _positions = _property.serializedObject.FindProperty(_property.propertyPath + ".positions");

//             foreach(Edge edge in graphView.edges.ToList()) graphView.RemoveElement(edge);
//             foreach(Node node in graphView.nodes.ToList()) graphView.RemoveElement(node);

//             for(int i = 0; i < _nodes.arraySize; i++){
//                 Node node = CreateNode(_nodes.GetArrayElementAtIndex(i), i);
//                 Vector2 position = _positions.GetArrayElementAtIndex(i).vector2Value;
//                 node.SetPosition(new Rect(position.x, position.y, 0, 0));
//                 graphView.AddElement(node);
//             }
//             for(int i = 0; i < _links.arraySize; i++){
//                 Vector2Int pair = _links.GetArrayElementAtIndex(i).vector2IntValue;
//                 Node sourceNode = graphView.nodes.AtIndex(pair.x);
//                 Node targetNode = graphView.nodes.AtIndex(pair.y);
//                 Port sourcePort = sourceNode.inputContainer.ElementAt(0) as Port;
//                 Port targetPort = targetNode.outputContainer.ElementAt(0) as Port;
//                 Edge edge = sourcePort.ConnectTo<Edge>(targetPort);
//                 graphView.AddElement(edge);
//             }

//             rootVisualElement.MarkDirtyRepaint();
//             rootVisualElement.Bind(property.serializedObject);
//             graphView.FrameOrigin();
//         }
//         GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange){
//             if(graphViewChange.movedElements != null)
//             foreach(GraphElement element in graphViewChange.movedElements){
//                 //lement.GetPosition().position
//             }
//             if(graphViewChange.edgesToCreate != null)
//             foreach(Edge edge in graphViewChange.edgesToCreate){
//                 Node sourceNode = edge.input.node;
//                 Node targetNode = edge.output.node;
//                 Vector2Int pair = new Vector2Int(int.Parse(sourceNode.name), int.Parse(targetNode.name));
//                 _links.arraySize++;
//                 _links.GetArrayElementAtIndex(_links.arraySize - 1).vector2IntValue = pair;
//             }
//             bool[] removalFlags = new bool[_nodes.arraySize];
//             if(graphViewChange.elementsToRemove != null)
//             foreach(GraphElement element in graphViewChange.elementsToRemove)
//                 if(element is Node){
//                     removalFlags[int.Parse(element.name)] = true;
//                 }else if(element is Edge){
//                     Edge edge = element as Edge;
//                     Node sourceNode = edge.input.node;
//                     Node targetNode = edge.output.node;
//                     Vector2Int pair = new Vector2Int(int.Parse(sourceNode.name), int.Parse(targetNode.name));
//                     for(int i = _links.arraySize - 1; i >= 0; i--){
//                         Vector2Int link = _links.GetArrayElementAtIndex(i).vector2IntValue;
//                         if(link.x != pair.x || link.y != pair.y) continue;
//                         if(i == _links.arraySize - 1){
//                             _links.arraySize--;
//                         }else{
//                             _links.DeleteArrayElementAtIndex(i);
//                         }
//                         break;
//                     }
//                 }
//             for(int i = _nodes.arraySize - 1; i >= 0; i--){
//                 if(!removalFlags[i]) continue;
//                 if(_nodes.arraySize - 1 == i){
//                     _nodes.arraySize--;
//                 }else{
//                     _nodes.DeleteArrayElementAtIndex(i);
//                     foreach(Node node in graphView.nodes.ToList()){
//                         int index = int.Parse(node.name);
//                         if(index > i) node.name = $"{index-1}";
//                     }
//                     for(int j = 0; j < _links.arraySize; j++){
//                         Vector2Int link = _links.GetArrayElementAtIndex(j).vector2IntValue;
//                         if(link.x > i) link.x--;
//                         if(link.y > i) link.y--;
//                         _links.GetArrayElementAtIndex(j).vector2IntValue = link;
//                     }
//                 }
//             }
//             _positions.arraySize = _nodes.arraySize;
//             foreach(Node node in graphView.nodes.ToList()){
//                 int index = int.Parse(node.name);
//                 if(index >= _positions.arraySize) continue;
//                 _positions.GetArrayElementAtIndex(index).vector2Value = node.GetPosition().position;
//             }
//             _nodes.serializedObject.ApplyModifiedProperties();
//             return graphViewChange;
//         }
//     }
// }