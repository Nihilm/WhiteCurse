// using System;
// using System.Collections.Generic;
// using UnityEngine;

// [System.Serializable] public class GridGeometry {
//     [HideInInspector] public List<Vector2Int> opaque;
//     [HideInInspector] public List<Vector2Int> sockets;

//     public static _Algorithms.NodeGeometry Build(int[,] filled, int[,] sockets){
//         var geometry = new _Algorithms.NodeGeometry();
//         var outline = _Algorithms.RectilinearPolygon.Contour(filled);
//         geometry.rectangles = new List<RectInt>(_Algorithms.RectilinearPolygon.Decompose(new List<IList<Vector2Int>>(outline)));
//         List<Vector2Int> socketPositions = new List<Vector2Int>();
//         for(int c = 0; c < sockets.GetLength(0); c++)
//         for(int r = 0; r < sockets.GetLength(1); r++)
//         if(sockets[c,r] == 1) socketPositions.Add(new Vector2Int(c, r));
//         geometry.connectors = Sockets(socketPositions, outline);
//         return geometry;
//     }
//     public static List<_Algorithms.OrthogonalConnector> Sockets(List<Vector2Int> sockets, IList<IList<Vector2Int>> outline){
//         var connectors = new List<_Algorithms.OrthogonalConnector>();
//         foreach(var path in outline)
//         for(int i = path.Count - 1, j = 0; i >= 0; j = i--){
//             Vector2Int prev = path[i], next = path[j];
//             int axis = prev.x == next.x ? 1 : 0;
//             int direction = prev[axis] < next[axis] ? 1 : -1;
//             int min = Math.Min(prev[axis], next[axis]);
//             int max = Math.Max(prev[axis], next[axis]);
//             Vector2Int position = new Vector2Int(prev.x, prev.y);
//             if(axis == 1 && direction == -1) position[axis ^ 1]-=1;
//             else if(axis == 0 && direction == 1) position[axis ^ 1]-=1;
//             bool merge = false;
//             for(int k = min; k < max; k++){
//                 position[axis] = k;
//                 int idx = sockets.FindIndex(v => v.x == position.x && v.y == position.y);
//                 if(idx != -1){
//                     Vector2Int _position = position;
//                     _position[axis ^ 1] = prev[axis ^ 1];
//                     if(merge){
//                         connectors[connectors.Count - 1].bounds.max = _position;
//                     }else{
//                         connectors.Add(new _Algorithms.OrthogonalConnector(
//                             new RectInt(_position.x, _position.y, 0, 0),
//                             new Vector2Int(){ [axis ^ 1] = direction }
//                         ));
//                     }
//                     merge = true;
//                 }else{
//                     merge = false;
//                 }
//             }
//         }
//         return connectors;
//     }
//     public static List<List<Vector2Int>> Outline(List<Vector2Int> filled){
//         Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
//         Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
//         foreach(var position in filled){
//             min = Vector2Int.Min(min, position);
//             max = Vector2Int.Max(max, position);
//         }
//         int[,] grid = new int[max.x - min.x + 1, max.y - min.y + 1];
//         foreach(var position in filled) grid[position.x - min.x, position.y - min.y] = 1;
//         return new List<IList<Vector2Int>>(_Algorithms.RectilinearPolygon.Contour(grid))
//         .ConvertAll(path => new List<Vector2Int>(path).ConvertAll(vertex => vertex + min));
//     }
// }