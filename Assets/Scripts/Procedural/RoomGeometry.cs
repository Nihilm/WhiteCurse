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
}