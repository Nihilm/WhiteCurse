// private Texture2D RenderGeometryMap(_Algorithms.NodeGeometry geometry, List<Vector2Int> connectors, int padding){
//         var bounds = _Algorithms.RectilinearPolygon.CalculateBounds(geometry.rectangles);
//         Texture2D mapTexture = new Texture2D(bounds.width+2*padding, bounds.height+2*padding, TextureFormat.ARGB32, false);
//         mapTexture.wrapMode = TextureWrapMode.Clamp;
//         mapTexture.filterMode = FilterMode.Bilinear;
//         Color[] colors = new Color[mapTexture.width * mapTexture.height];
//         foreach(var rectangle in geometry.rectangles)
//         for(int x = rectangle.min.x; x < rectangle.max.x; x++)
//         for(int y = rectangle.min.y; y < rectangle.max.y; y++)
//         colors[(x - bounds.min.x + padding) + (y - bounds.min.y + padding) * mapTexture.width] = new Color(0,1,0,1);
//         foreach(var connector in connectors)
//         colors[(connector.x - bounds.min.x + padding) + (connector.y - bounds.min.y + padding) * mapTexture.width] = new Color(1,1,0,1);
//         mapTexture.SetPixels(colors);
//         mapTexture.Apply();
//         return mapTexture;
//     }
//     private Sprite RenderNodeGeometry(_Algorithms.NodeGeometry geometry, List<Vector2Int> connectors){
//         int tileSize = 64; int padding = 1;
//         //https://www.shadertoy.com/view/WdfBD7

//         Texture2D mapTexture = RenderGeometryMap(geometry, connectors, padding);
//         int width = mapTexture.width-2*padding, height = mapTexture.height-2*padding;

//         RenderTexture renderTexture = RenderTexture.GetTemporary(width*tileSize, height*tileSize, 0, RenderTextureFormat.ARGB32);
//         RenderTexture.active = renderTexture;
//         if(brushMaterial == null) brushMaterial = new Material(brushShader);

//         float scaleX = (float)width / (float)mapTexture.width;
//         float scaleY = (float)height / (float)mapTexture.height;
//         GL.LoadIdentity();
//         GL.LoadPixelMatrix(
//             tileSize * scaleX,
//             renderTexture.width - tileSize * scaleX,
//             renderTexture.height - tileSize * scaleY,
//             tileSize * scaleY
//         );
//         Graphics.DrawTexture(new Rect(0, 0, renderTexture.width, renderTexture.height), mapTexture, brushMaterial);

//         Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, true);
//         RenderTexture.active = renderTexture;
//         texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
//         texture.Apply();

//         RenderTexture.active = null;
//         renderTexture.DiscardContents();
//         RenderTexture.ReleaseTemporary(renderTexture);
        
//         return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, tileSize);
//     }








// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;

// public class MapView : MonoBehaviour {
//     [SerializeField] private GameObject token;

//     // [HideInInspector] public PlayerTemplate player;
//     // [HideInInspector] public AreaState area;
//     // private _Algorithms.LayoutState layoutState;
//     // private GameObject[] rooms;
//     // private GameObject token;

//     void Start(){

//         // token = transform.Find("Token").gameObject;
//         // var generator = new _Algorithms.ProceduralGenerator(){
//         //     random = new System.Random(area.seed)
//         // };
//         // using(var enumerator = generator.Generate(area.graph).GetEnumerator()){
//         //     enumerator.MoveNext();
//         //     layoutState = enumerator.Current;
//         //     Debug.Log("Rendering Map");
//         //     RectInt bounds = layoutState.CalculateBounds();
//         //     //int zoom = 1;
//         //     rooms = new GameObject[layoutState.geometries.Length];
//         //     for(int i = 0; i < layoutState.geometries.Length; i++){
//         //         var gameObject = new GameObject($"Room{i}");
//         //         rooms[i] = gameObject;
//         //         var _bounds = _Algorithms.RectilinearPolygon.CalculateBounds(layoutState.geometries[i].parent.rectangles);
//         //         gameObject.transform.SetParent(transform, false);
                
//         //         // gameObject.AddComponent<RectTransform>();
//         //         // gameObject.GetComponent<RectTransform>().position = new Vector3(
//         //         //     layout.geometries[i].position.x - bounds.min.x,
//         //         //     layout.geometries[i].position.y - bounds.min.y, 0) * zoom;

//         //         //gameObject.AddComponent<CanvasRenderer>();
//         //         //var image = gameObject.AddComponent<Image>();
//         //         var image = gameObject.AddComponent<SpriteRenderer>();
//         //         //TODO move doors handling elsewhere?
//         //         var doors = new List<Vector2Int>();
//         //         foreach(int neighbour in area.graph.Neighbours(i)){
//         //             var connectors = new List<_Algorithms.OrthogonalConnector>(layoutState.geometries[i].SampleConnectors(layoutState.geometries[neighbour]));
//         //             foreach(var connector in connectors){
//         //                 Vector2Int offset = new Vector2Int(
//         //                     connector.direction[0] == -1 ? -1 : 0,
//         //                     connector.direction[1] == 1 ? -1 : 0
//         //                 );
//         //                 doors.Add(connector.bounds.min - layoutState.geometries[i].position + offset);
//         //             }
//         //         }

//         //         var texture = RenderRoomGeometry(layoutState.geometries[i].parent, doors);

//         //         image.sprite = Sprite.Create(texture, new Rect(0, 0, _bounds.width, _bounds.height), Vector2.zero, 1f);

//         //         gameObject.transform.localPosition = new Vector3(
//         //             layoutState.geometries[i].position.x - bounds.min.x + _bounds.min.x,
//         //             layoutState.geometries[i].position.y - bounds.min.y + _bounds.min.y, 0);
//         //         gameObject.transform.localRotation = Quaternion.identity;



//         //         var label = new GameObject("label");
//         //         label.transform.SetParent(gameObject.transform, false);
//         //         var textMesh = label.AddComponent<TextMesh>();
//         //         //label.AddComponent<MeshRenderer>();
//         //         label.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
//         //         textMesh.text = area.locations[i].template.displayName;
//         //         label.transform.localPosition.Set(0, 0, -1);


//         //         // List<Vector3> vertices = new List<Vector3>();
//         //         // List<int> indices = new List<int>();
//         //         // foreach(var rectangle in layout.geometries[i].parent.rectangles){
//         //         //     int indexOffset = vertices.Count;
//         //         //     vertices.Add(new Vector3(rectangle.min.x, rectangle.min.y, 0) * zoom);
//         //         //     vertices.Add(new Vector3(rectangle.min.x, rectangle.max.y, 0) * zoom);
//         //         //     vertices.Add(new Vector3(rectangle.max.x, rectangle.max.y, 0) * zoom);
//         //         //     vertices.Add(new Vector3(rectangle.max.x, rectangle.min.y, 0) * zoom);
//         //         //     indices.AddRange(new int[]{ 0+indexOffset, 1+indexOffset, 2+indexOffset, 0+indexOffset, 2+indexOffset, 3+indexOffset });
//         //         // }
//         //         // Color[] colors = new Color[vertices.Count];
//         //         // colors.Fill(Random.ColorHSV());
//         //         // Mesh mesh = new Mesh(){
//         //         //     vertices = vertices.ToArray(),
//         //         //     triangles = indices.ToArray(),
//         //         //     colors = colors
//         //         // };
//         //         // var filter = gameObject.AddComponent<MeshFilter>();
//         //         // filter.sharedMesh = mesh;
//         //         // gameObject.AddComponent<CanvasRenderer>();
//         //         // gameObject.GetComponent<CanvasRenderer>().SetMesh(mesh);
//         //         // gameObject.GetComponent<CanvasRenderer>().SetMaterial(new Material(Shader.Find("Sprites/Default")), null);
                
//         //         gameObject.AddComponent<BoxCollider>();
//         //         var trigger = gameObject.AddComponent<EventTrigger>();
//         //         trigger.AddEventListener(EventTriggerType.PointerEnter, eventData => {
//         //             image.color = new Color(1, 0, 0, 1);
//         //         });
//         //         trigger.AddEventListener(EventTriggerType.PointerExit, eventData => {
//         //             image.color = new Color(1, 1, 1, 1);
//         //         });
//         //         int index = i;
//         //         trigger.AddEventListener(EventTriggerType.PointerUp, eventData => {
//         //             Navigate(index);
//         //         });
//         //     }
//         // }

//         // int currentLocation = player.activeLocation;
//         // GameObject currentRoom = rooms[currentLocation];
//         // RectInt __bounds = _Algorithms.RectilinearPolygon.CalculateBounds(layoutState.geometries[currentLocation].rectangles);
//         // token.transform.localPosition = currentRoom.transform.localPosition
//         //  + new Vector3(__bounds.width * 0.5f, __bounds.height * 0.5f, 0);
//     }
//     private Texture2D RenderRoomGeometry(_Algorithms.NodeGeometry geometry, List<Vector2Int> doors){
//         var bounds = _Algorithms.RectilinearPolygon.CalculateBounds(geometry.rectangles);
//         Texture2D texture = new Texture2D(bounds.width, bounds.height, TextureFormat.ARGB32, false, false);
//         texture.filterMode = FilterMode.Point;
//         foreach(var rectangle in geometry.rectangles){
//             for(int x = rectangle.min.x; x < rectangle.max.x; x++)
//             for(int y = rectangle.min.y; y < rectangle.max.y; y++){
//                 if(x == rectangle.min.x || x == rectangle.max.x - 1 || y == rectangle.min.y || y == rectangle.max.y - 1)
//                 texture.SetPixel(x - bounds.min.x, y - bounds.min.y, new Color(0, 0, 0, 1));
//                 else
//                 texture.SetPixel(x - bounds.min.x, y - bounds.min.y, new Color(1, 1, 1, 1));
//             }
//         }
//         foreach(var door in doors){
//             texture.SetPixel(door.x - bounds.min.x, door.y - bounds.min.y, new Color(0.7f,0.7f,0.7f,1));
//         }
//         texture.Apply();
//         return texture;

//         // Texture2D brush = new Texture2D(1, 1);
//         // brush.SetPixel(0, 0, new Color(0, 0, 0, 1));
        
//         // int width = 512; int height = 512;
//         // RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
//         // RenderTexture.active = renderTexture;

//         // GL.Clear(false, true, Color.clear);
//         // GL.PushMatrix();
//         // GL.LoadPixelMatrix(0, width, height, 0);


//         // // Vector2 coord = new Vector2(hit.textureCoord.x * _textureResolution, _textureResolution - hit.textureCoord.y * _textureResolution);
//         // // Graphics.DrawTexture(new Rect(coord.x - brushSize / 2, (coord.y - brushSize / 2), brushSize, brushSize), brush);
//         // GL.PopMatrix();

//         // Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, 0, true);
//         // texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//         // texture.Apply();

//         // RenderTexture.active = null;
//         // RenderTexture.ReleaseTemporary(renderTexture);

//         // return texture;
//     }
//     void Update(){
//         if(Input.GetKey(KeyCode.A)){
//             transform.Translate(new Vector3(5 * Time.deltaTime,0,0));
//         }else if(Input.GetKey(KeyCode.D)){
//             transform.Translate(new Vector3(-5 * Time.deltaTime,0,0));
//         }else if(Input.GetKey(KeyCode.W)){
//             transform.Translate(new Vector3(0,-5 * Time.deltaTime,0));
//         }else if(Input.GetKey(KeyCode.S)){
//             transform.Translate(new Vector3(0,5 * Time.deltaTime,0));
//         }
//     }
//     public void Navigate(int location){
//         // player.Navigate(location);

//         // int currentLocation = player.activeLocation;
//         // GameObject currentRoom = rooms[currentLocation];
//         // RectInt __bounds = _Algorithms.RectilinearPolygon.CalculateBounds(layoutState.geometries[currentLocation].rectangles);
//         // token.transform.localPosition = currentRoom.transform.localPosition
//         //  + new Vector3(__bounds.width * 0.5f, __bounds.height * 0.5f, 0);
//     }
// }
