using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "Procedural/Room")]
public class RoomGeometry : ScriptableObject {
    public string displayName;
    public bool allowRotations;
    [HideInInspector] public RectInt bounds;
    [HideInInspector] public int[] filled = new int[0];
    [HideInInspector] public int[] sockets = new int[0];

    public _Algorithms.NodeGeometry BuildGeometry(){
        var geometry = new _Algorithms.NodeGeometry();
        var outline = Outline();
        geometry.rectangles = new List<RectInt>(_Algorithms.RectilinearPolygon.Decompose(outline));
        geometry.connectors = AttachSockets(sockets, bounds, outline);
        return geometry;
    }
    public static List<_Algorithms.OrthogonalConnector> AttachSockets(int[] sockets, RectInt bounds, IList<IList<Vector2Int>> outline){
        var connectors = new List<_Algorithms.OrthogonalConnector>();
        foreach(var path in outline)
        for(int i = path.Count - 1, j = 0; i >= 0; j = i--){
            Vector2Int prev = path[i], next = path[j];
            int axis = prev[0] == next[0] ? 1 : 0;
            int direction = prev[axis] <= next[axis] ? 1 : -1;
            if(direction < 0){ next = path[i]; prev = path[j]; }
            Vector2Int positionOffset = new Vector2Int(
                axis == 1 && direction == -1 ? -1 : 0,
                axis == 0 && direction == 1 ? -1 : 0
            ) - bounds.min;
            Vector2Int position = new Vector2Int(prev.x, prev.y);
            bool merge = false;
            for(int k = prev[axis]; k < next[axis]; k++){
                position[axis] = k;
                int x = position.x + positionOffset.x, y = position.y + positionOffset.y;
                if(x < 0 || y < 0 || x >= bounds.width || y >= bounds.height || sockets[x + bounds.width * y] == 0){
                    merge = false; continue;
                }
                if(merge) connectors[connectors.Count - 1].bounds.max = position;
                else connectors.Add(new _Algorithms.OrthogonalConnector(
                    new RectInt(position.x, position.y, 0, 0),
                    new Vector2Int(){ [axis ^ 1] = direction }
                ));
                merge = true;
            }
        }
        return connectors;
    }
    public void FromList(List<Vector2Int> filled, List<Vector2Int> sockets){
        Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
        foreach(var position in filled){
            min = Vector2Int.Min(min, position);
            max = Vector2Int.Max(max, position);
        }
        foreach(var position in sockets){
            min = Vector2Int.Min(min, position);
            max = Vector2Int.Max(max, position);
        }
        bounds = new RectInt(min.x, min.y,  max.x - min.x + 1, max.y - min.y + 1);
        this.filled = new int[bounds.width * bounds.height];
        this.sockets = new int[bounds.width * bounds.height];
        foreach(var position in filled) this.filled[position.x - min.x + bounds.width * (position.y - min.y)] = 1;
        foreach(var position in sockets) this.sockets[position.x - min.x + bounds.width * (position.y - min.y)] = 1;
    }
    public IList<IList<Vector2Int>> Outline(){
        var outline = _Algorithms.RectilinearPolygon.Contour(
            _Algorithms.Extensions.Array2D(this.filled, bounds.width, bounds.height)
        );
        foreach(var path in outline)
            for(int i = 0; i < path.Count; i++)
                path[i] += bounds.min;
        return outline;
    }
    public enum TileType { OuterCorner, InnerCorner, Wall, Door }
    private static IEnumerable<(Vector3 offset, Quaternion rotation, TileType type, int tile)> MarchingSquares(
        _Algorithms.LayoutState layout
    ){
        RectInt bounds = layout.CalculateBounds();
        int padding = 2;
        int width = bounds.width + 2*padding, height = bounds.height + 2*padding;
        int[,] tiles = new int[width, height];
        for(int i = 0; i < layout.geometries.Length; i++){
            var geometry = layout.geometries[i];
            foreach(var rectangle in geometry.rectangles)
            for(int x = rectangle.min.x; x < rectangle.max.x; x++)
            for(int y = rectangle.min.y; y < rectangle.max.y; y++)
            tiles[x - bounds.min.x + padding, y - bounds.min.y + padding] = (1+i) << 1;
            foreach(var connector in layout.SampleConnectors(i))
            tiles[connector.x - bounds.min.x + padding, connector.y - bounds.min.y + padding] |= 1;
        }

        for(int x = 1; x < width - 1; x++)
        for(int y = 1; y < height - 1; y++){
            Vector3 offset = new Vector3(x-padding+0.5f,y-padding+0.5f,0);
            int t00 = tiles[x-1,y-1] >> 1; int t10 = tiles[x+0,y-1] >> 1; int t20 = tiles[x+1,y-1] >> 1;
            int t01 = tiles[x-1,y+0] >> 1; int t11 = tiles[x+0,y+0] >> 1; int t21 = tiles[x+1,y+0] >> 1;
            int t02 = tiles[x-1,y+1] >> 1; int t12 = tiles[x+0,y+1] >> 1; int t22 = tiles[x+1,y+1] >> 1;
            bool door = (tiles[x,y] & 1) != 0;

            int s00 = t00 != t11 || t01 != t11 || t10 != t11 ? 0x01 : 0;
            int s10 = t20 != t11 || t21 != t11 || t10 != t11 ? 0x02 : 0;
            int s01 = t02 != t11 || t01 != t11 || t12 != t11 ? 0x08 : 0;
            int s11 = t22 != t11 || t21 != t11 || t12 != t11 ? 0x04 : 0;

            switch(s00 | s10 | s11 | s01){
                case 0: break;
                case 15: break;
                case 1: yield return (offset,Quaternion.identity,TileType.OuterCorner,t11); break;
                case 2: yield return (offset,Quaternion.Euler(0,0,90),TileType.OuterCorner,t11); break;
                case 4: yield return (offset,Quaternion.Euler(0,0,180),TileType.OuterCorner,t11); break;
                case 8: yield return (offset,Quaternion.Euler(0,0,270),TileType.OuterCorner,t11); break;

                case 3: yield return (offset,Quaternion.identity,door?TileType.Door:TileType.Wall,t11); break;
                case 6: yield return (offset,Quaternion.Euler(0,0,90),door?TileType.Door:TileType.Wall,t11); break;
                case 12: yield return (offset,Quaternion.Euler(0,0,180),door?TileType.Door:TileType.Wall,t11); break;
                case 9: yield return (offset,Quaternion.Euler(0,0,270),door?TileType.Door:TileType.Wall,t11); break;

                case 11: yield return (offset,Quaternion.identity,TileType.InnerCorner,t11); break;
                case 7: yield return (offset,Quaternion.Euler(0,0,90),TileType.InnerCorner,t11); break;
                case 14: yield return (offset,Quaternion.Euler(0,0,180),TileType.InnerCorner,t11); break;
                case 13: yield return (offset,Quaternion.Euler(0,0,270),TileType.InnerCorner,t11); break;
                case 5: break;
                case 10: break;
            }
        }
    }

    public static Mesh ConstructGeometry(_Algorithms.LayoutState layout){
        RectInt bounds = layout.CalculateBounds();

        GeometryBuilder builder = new GeometryBuilder();
        foreach(var geometry in layout.geometries)
        foreach(var rectangle in geometry.rectangles)
            builder.AddQuad(
                new Vector3(rectangle.min.x, rectangle.min.y, 0),
                new Vector3(rectangle.max.x, rectangle.min.y, 0),
                new Vector3(rectangle.max.x, rectangle.max.y, 0),
                new Vector3(rectangle.min.x, rectangle.max.y, 0)
            ).ApplyTransform(4, new Vector3(-bounds.min.x, -bounds.min.y, 0), Quaternion.identity);
        
        foreach(var (offset, rotation, type, tile) in MarchingSquares(layout)){
            float height = -1.0f, top = -0.4f, bottom = 0.0f;
            switch(type){
                case TileType.OuterCorner:
                    builder.AddQuad(new Vector3(-0.5f,-0.5f,height), new Vector3(top,-0.5f,height),
                        new Vector3(top,top,height), new Vector3(-0.5f,top,height));
                    builder.AddQuad(new Vector3(-0.5f,top,height), new Vector3(top,top,height),
                        new Vector3(bottom,bottom,0), new Vector3(-0.5f,bottom,0));
                    builder.AddQuad(new Vector3(top,-0.5f,height), new Vector3(bottom,-0.5f,0),
                        new Vector3(bottom,bottom,0), new Vector3(top,top,height));
                    builder.ApplyTransform(12, offset, rotation);
                    break;
                case TileType.InnerCorner:
                    builder.AddQuad(new Vector3(-0.5f,-0.5f,height), new Vector3(0.5f,-0.5f,height),
                        new Vector3(0.5f,top,height), new Vector3(top,top,height));
                    builder.AddQuad(new Vector3(-0.5f,-0.5f,height), new Vector3(top,top,height),
                        new Vector3(top,0.5f,height), new Vector3(-0.5f,0.5f,height));
                    builder.AddQuad(new Vector3(top,top,height), new Vector3(0.5f,top,height),
                        new Vector3(0.5f,bottom,0), new Vector3(bottom,bottom,0));
                    builder.AddQuad(new Vector3(top,top,height), new Vector3(bottom,bottom,0),
                        new Vector3(bottom,0.5f,0), new Vector3(top,0.5f,height));
                    builder.ApplyTransform(16, offset, rotation);
                    break;
                case TileType.Wall:
                    builder.AddQuad(new Vector3(-0.5f,-0.5f,height), new Vector3(0.5f,-0.5f,height),
                        new Vector3(0.5f,top,height), new Vector3(-0.5f,top,height));
                    builder.AddQuad(new Vector3(-0.5f,top,height), new Vector3(0.5f,top,height),
                        new Vector3(0.5f,bottom,0), new Vector3(-0.5f,bottom, 0));
                    builder.ApplyTransform(8, offset, rotation);
                    break;
                case TileType.Door:
                    builder.AddQuad(new Vector3(-0.5f,-0.5f,height), new Vector3(-0.5f,-0.5f,0),
                        new Vector3(-0.5f,bottom,0), new Vector3(-0.5f,top,height));
                    builder.AddQuad(new Vector3(0.5f,-0.5f,0), new Vector3(0.5f,-0.5f,height),
                        new Vector3(0.5f,top,height), new Vector3(0.5f,bottom,0));
                    builder.ApplyTransform(8, offset, rotation);
                    break;
            }
        }

        return builder.Mesh();
    }
}

public class GeometryBuilder {
    public int VertexCount => vertices.Count;
    List<Vector3> vertices = new List<Vector3>();
    List<int> indices = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public void ApplyTransform(int count, Vector3 offset, Quaternion rotation){
        for(int i = vertices.Count - 1; i >= vertices.Count - count; i--)
            vertices[i] = rotation * vertices[i] + offset;
    }

    public GeometryBuilder AddQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft){
        int indexOffset = vertices.Count;
        vertices.Add(topLeft); vertices.Add(topRight);
        vertices.Add(bottomRight); vertices.Add(bottomLeft);
        indices.AddRange(new int[]{
            0+indexOffset,3+indexOffset,1+indexOffset,
            3+indexOffset,2+indexOffset,1+indexOffset
        });

        uvs.Add(new Vector2(0,0)); uvs.Add(new Vector2(1,0));
        uvs.Add(new Vector2(1,1)); uvs.Add(new Vector2(0,1));
        return this;
    }

    public Mesh Mesh(){
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }
}