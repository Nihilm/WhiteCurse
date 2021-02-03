namespace _Algorithms {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    //https://github.com/mikolalysenko/rectangle-decomposition/blob/master/decomp.js
    public static class RectilinearPolygon {
        public static IEnumerable<RectInt> Decompose(IList<Vector2Int> path, bool clockwise = false) =>
        Decompose(new List<IList<Vector2Int>>(1){ path }, clockwise);
        public static IEnumerable<RectInt> Decompose(IList<IList<Vector2Int>> paths, bool clockwise = false){
            List<Vertex> vertices = new List<Vertex>();
            List<Vertex> corners = new List<Vertex>();
            IntervalTree<int, Segment>[] axisTrees = new IntervalTree<int, Segment>[]{
                new IntervalTree<int, Segment>(),
                new IntervalTree<int, Segment>()
            };
            foreach(IList<Vector2Int> path in paths){
                int offset = vertices.Count;
                for(int i = path.Count - 1, j = 0, k = 1; i >= 0; k = j, j = i--){
                    bool concave = false;
                    if(path[i][0] == path[j][0]){
                        if(path[k][0] == path[j][0]) continue;
                        concave = path[i][1] < path[j][1] == path[j][0] > path[k][0];
                    }else{
                        if(path[k][1] == path[j][1]) continue;
                        concave = path[i][0] < path[j][0] != path[j][1] > path[k][1];
                    }
                    vertices.Add(new Vertex(){
                        concave = clockwise ? !concave : concave,
                        position = path[j]
                    });
                }
                for(int i = vertices.Count - 1, j = offset; i >= offset; j = i--){
                    Vertex prev = vertices[j], next = vertices[i];
                    if(prev.concave) corners.Add(prev);
                    if(clockwise){
                        prev.prev = next; next.next = prev;
                    }else{
                        prev.next = next; next.prev = prev;
                    }
                    Segment segment = new Segment(prev, next);
                    axisTrees[segment.direction].Add(segment, segment.range.min, segment.range.max);
                }
            }
            List<Segment>[] diagonals = new List<Segment>[]{
                new List<Segment>(FindDiagonals(corners, 1, axisTrees[0])),
                new List<Segment>(FindDiagonals(corners, 0, axisTrees[1]))
            };
            int searchDirection = diagonals[0].Count > diagonals[1].Count ? 0 : 1;
            IntervalTree<int, Segment> axisTree = new IntervalTree<int, Segment>();
            foreach(var segment in diagonals[searchDirection]) axisTree.Add(segment, segment.range.min, segment.range.max);
            IGraph<Segment> bipartiteGraph = new UndirectedAdjacencyListGraph<Segment>();
            foreach(var segment in diagonals[searchDirection]) bipartiteGraph.AddNode(segment);
            foreach(var segment in diagonals[searchDirection ^ 1]) bipartiteGraph.AddNode(segment);
            
            (int[],int[]) partition = (new int[diagonals[searchDirection].Count], new int[diagonals[1].Count]);
            for(int i = 0; i < diagonals[searchDirection].Count; i++) partition.Item1[i] = i;
            for(int i = 0; i < diagonals[searchDirection ^ 1].Count; i++) partition.Item2[i] = i + diagonals[searchDirection].Count;

            foreach(var diagonal in diagonals[searchDirection ^ 1])
            foreach(var tangent in axisTree.Query(diagonal.axisValue))
                if(diagonal.range.min <= tangent.axisValue && tangent.axisValue <= diagonal.range.max)
                    bipartiteGraph.AddEdge(tangent.index, diagonal.index + diagonals[searchDirection].Count);

            var selected = HopcroftKarp.MinimumVertexCover(bipartiteGraph, true, partition);
            foreach(var i in selected.left) bipartiteGraph[i].Split();
            foreach(var i in selected.right) bipartiteGraph[i].Split();

            SplitConcave(vertices);

            for(int i = 0; i < vertices.Count; i++){
                var vertex = vertices[i];
                if(vertex.visited) continue;
                int xmin = int.MaxValue; int ymin = int.MaxValue;
                int xmax = int.MinValue; int ymax = int.MinValue;
                while(!vertex.visited){
                    xmin = Math.Min(vertex.position[0], xmin); ymin = Math.Min(vertex.position[1], ymin);
                    xmax = Math.Max(vertex.position[0], xmax); ymax = Math.Max(vertex.position[1], ymax);
                    vertex.visited = true;
                    vertex = vertex.next;
                }
                if(xmin == xmax || ymin == ymax) continue;
                yield return new RectInt(xmin, ymin, xmax - xmin, ymax - ymin);
            }
        }
        private class Vertex {
            public Vector2Int position;
            public Vertex prev;
            public Vertex next;
            public bool concave;
            public bool visited;
            public static int CompareHorizontal(Vertex a, Vertex b) =>
            a.position[0] == b.position[0] ? a.position[1] - b.position[1] : a.position[0] - b.position[0];
            public static int CompareVertical(Vertex a, Vertex b) =>
            a.position[1] == b.position[1] ? a.position[0] - b.position[0] : a.position[1] - b.position[1];
        }
        private class Segment {
            public readonly (int min,int max) range;
            public int direction => prev.position[0] == next.position[0] ? 0 : 1;
            public int axisValue => prev.position[direction];
            public readonly Vertex prev;
            public readonly Vertex next;
            public int index;
            public Segment(Vertex prev, Vertex next){
                this.prev = prev; this.next = next;
                int left = prev.position[direction ^ 1];
                int right = next.position[direction ^ 1];
                range = left < right ? (left, right) : (right, left);
            }
            public void Split(){
                Vertex left = this.prev, right = this.next;
                Vertex prevLeft = left.prev, nextLeft = left.next;
                Vertex prevRight = right.prev, nextRight = right.next;
                left.concave = right.concave = false;
                bool directionLeft = prevLeft.position[this.direction] == left.position[this.direction];
                bool directionRight = prevRight.position[this.direction] == right.position[this.direction];
                if(directionLeft && directionRight){
                    left.prev = prevRight; prevRight.next = left;
                    right.prev = prevLeft; prevLeft.next = right;
                }else if(directionLeft){
                    left.prev = right; right.next = left;
                    prevLeft.next = nextRight; nextRight.prev = prevLeft;
                }else if(directionRight){
                    left.next = right; right.prev = left;
                    nextLeft.prev = prevRight; prevRight.next = nextLeft;
                }else{
                    left.next = nextRight; nextRight.prev = left;
                    right.next = nextLeft; nextLeft.prev = right;
                }
            }
        }
        private static IEnumerable<Segment> FindDiagonals(List<Vertex> corners, int direction, IntervalTree<int, Segment> tree){
            corners.Sort(direction == 0 ? (Comparison<Vertex>) Vertex.CompareHorizontal : Vertex.CompareVertical);
            for(int count = 0, i = 1; i < corners.Count; i++){
                Vertex prev = corners[i - 1], next = corners[i];
                if(prev.position[direction] != next.position[direction]) continue;
                if(prev.next == next || next.next == prev) continue;
                int min = prev.position[direction ^ 1];
                int max = next.position[direction ^ 1];
                bool diagonal = true;
                foreach(Segment segment in tree.Query(prev.position[direction])){
                    int value = segment.prev.position[direction ^ 1];
                    if(min < value && value < max){ diagonal = false; break; }
                }
                if(diagonal) yield return new Segment(prev, next){ index = count++ };
            }
        }
        private static void SplitConcave(List<Vertex> vertices){
            IntervalTree<int, Segment> leftTree = new IntervalTree<int, Segment>();
            IntervalTree<int, Segment> rightTree = new IntervalTree<int, Segment>();
            for(int i = 0; i < vertices.Count; i++){
                Vertex prev = vertices[i], next = prev.next;
                if(next.position[0] != prev.position[0]) continue;
                var segment = new Segment(prev, next);
                (next.position[1] > prev.position[1] ? leftTree : rightTree)
                .Add(segment, segment.range.min, segment.range.max);
            }
            for(int i = 0; i < vertices.Count; i++){
                var vertex = vertices[i];
                if(!vertex.concave) continue;
                bool direction = vertex.prev.position[0] == vertex.position[0]
                ? vertex.prev.position[1] < vertex.position[1]
                : vertex.next.position[1] > vertex.position[1];

                IntervalTree<int, Segment> tree = direction ? rightTree : leftTree;
                Segment closestSegment = null;
                var closestDistance = direction ? int.MaxValue : int.MinValue;
                foreach(var segment in tree.Query(vertex.position[1]))
                    if(direction ? (
                        segment.prev.position[0] > vertex.position[0] && segment.prev.position[0] < closestDistance
                    ) : (
                        segment.prev.position[0] < vertex.position[0] && segment.prev.position[0] > closestDistance
                    )){
                        closestDistance = segment.prev.position[0];
                        closestSegment = segment;
                    }
                var splitA = new Vertex(){ position = new Vector2Int(closestDistance, vertex.position[1]) };
                vertices.Add(splitA);
                var splitB = new Vertex(){ position = new Vector2Int(closestDistance, vertex.position[1]) };
                vertices.Add(splitB);
                vertex.concave = false;

                splitA.prev = closestSegment.prev;
                closestSegment.prev.next = splitA;
                splitB.next = closestSegment.next;
                closestSegment.next.prev = splitB;

                tree.Remove(closestSegment);
                var segmentA = new Segment(closestSegment.prev, splitA);
                var segmentB = new Segment(splitB, closestSegment.next);
                tree.Add(segmentA, segmentA.range.min, segmentA.range.max);
                tree.Add(segmentB, segmentB.range.min, segmentB.range.max);
                if(vertex.prev.position[0] == vertex.position[0]){
                    splitA.next = vertex.next;
                    splitB.prev = vertex;
                }else{
                    splitA.next = vertex;
                    splitB.prev = vertex.prev;
                }
                splitA.next.prev = splitA;
                splitB.prev.next = splitB;
            }
        }
        private static int[,] CayleyTableD4 = new int[8,8]{
            {1,2,3,4,5,6,7,8},
            {2,3,4,1,8,7,5,6},
            {3,4,1,2,6,5,8,7},
            {4,1,2,3,7,8,6,5},
            {5,7,6,8,1,3,2,4},
            {6,8,5,7,3,1,4,2},
            {7,6,8,5,4,2,1,3},
            {8,5,7,6,2,4,3,1}
        };
        private static int[][] matrixD4 = new int[][]{
            new int[]{ 1, 0, 0, 1 },
            new int[]{ 0, 1,-1, 0 },
            new int[]{-1, 0, 0, 1 },
            new int[]{ 0,-1, 1, 0 },
            new int[]{ 1, 0, 0,-1 },
            new int[]{-1, 0, 0, 1 },
            new int[]{ 0, 1, 1, 0 },
            new int[]{ 0,-1,-1, 0 }
        };
        public static DihedralGroup4 Combine(this DihedralGroup4 operation, DihedralGroup4 next) =>
        (DihedralGroup4) CayleyTableD4[(int) operation, (int) next];
        public enum DihedralGroup4: int {
            Identity, Rotate90CCW, Rotate180CCW, Rotate270CCW,
            ReflextVertical, ReflectHorizontal,
            ReflexMainDiagonal, ReflectReverseDiagonal
        }
        public static Vector2Int Transform(this Vector2Int vector, DihedralGroup4 operation) =>
        new Vector2Int(
            vector[0] * matrixD4[(int)operation][0] + vector[1] * matrixD4[(int)operation][1],
            vector[0] * matrixD4[(int)operation][2] + vector[1] * matrixD4[(int)operation][3]
        );
        private static readonly int[] _trigonometricTable = new int[]{ 0, 1, 0, -1 };
        public static Vector2Int OrthogonalRotate(this Vector2Int position, int quarters){
            int sin = _trigonometricTable[quarters & 0x3];
            int cos = _trigonometricTable[++quarters & 0x3];
            return new Vector2Int(
                position[0] * cos + position[1] * sin,
                position[0] * -sin + position[1] * cos
            );
        }
        public static RectInt CalculateBounds(this List<RectInt> rectangles){
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach(RectInt rectangle in rectangles){
                minX = Math.Min(minX, rectangle.min.x);
                minY = Math.Min(minY, rectangle.min.y);
                maxX = Math.Max(maxX, rectangle.max.x);
                maxY = Math.Max(maxY, rectangle.max.y);
            }
            return new RectInt(minX, minY, maxX - minX, maxY - minY);
        }
        public static int ManhattanDistance(this RectInt a, RectInt b) => Math.Max(
            Math.Max(a.xMin - b.xMax, b.xMin - a.xMax),
            Math.Max(a.yMin - b.yMax, b.yMin - a.yMax)
        );
        public static int OverlapArea(this RectInt a, RectInt b) =>
        Math.Max(0, Math.Min(a.xMax, b.xMax) - Math.Max(a.xMin, b.xMin)) *
        Math.Max(0, Math.Min(a.yMax, b.yMax) - Math.Max(a.yMin, b.yMin));
        
        private static uint CyclicShift(uint value, int shift, int size) => shift > 0 ? (
            (value << shift | value >> size-shift) & (((uint) 1 << size) - 1)
        ) : (
            (value >> -shift | value << size+shift) & (((uint) 1 << size) - 1)
        );
        public static IList<IList<Vector2Int>> Contour(int[,] matrix){
            const uint Left = 0x1, Top = 0x2, Right = 0x4, Bottom = 0x8;
            int columns = matrix.GetLength(0), rows = matrix.GetLength(1);
            var contours = new List<IList<Vector2Int>>();
            uint[,] edges = new uint[columns + 1, rows + 1];
            for(int c = 0; c < columns; c++)
            for(int r = 0; r < rows; r++){
                if(matrix[c,r] == 0) continue;
                if(c == 0 || matrix[c-1,r] == 0) edges[c,r] |= Left;
                if(r == rows - 1 || matrix[c,r+1] == 0) edges[c,r+1] |= Bottom;
                if(c == columns - 1 || matrix[c+1,r] == 0) edges[c+1,r+1] |= Right;
                if(r == 0 || matrix[c,r-1] == 0) edges[c+1,r] |= Top;
            }
            for(int c = 0; c <= columns; c++)
            for(int r = 0; r <= rows; r++){
                if((edges[c,r] & Left) == 0) continue;
                var contour = new List<Vector2Int>(){ new Vector2Int(c, r) };
                uint direction = Left;
                for(int x = c, y = r + 1; x != c || y != r;){
                    uint nextDirection = CyclicShift(direction, -1, 4);
                    if((edges[x,y] & nextDirection) != 0){
                    }else if((edges[x,y] & (nextDirection = direction)) != 0){
                    }else if((edges[x,y] & (nextDirection = CyclicShift(direction, 1, 4))) != 0){
                    }else break;

                    if(direction != nextDirection) contour.Add(new Vector2Int(x, y));
                    edges[x,y] -= nextDirection;
                    direction = nextDirection;
                    if(direction == Left) y++;
                    else if(direction == Top) x--;
                    else if(direction == Right) y--;
                    else if(direction == Bottom) x++;
                }
                contours.Add(contour);
            }
            return contours;
        }
        public static IEnumerable<Vector2Int> SupercoverLine(Vector2Int origin, Vector2Int target){
            int dx = target.x - origin.x, dy = target.y - origin.y;
            int countX = Math.Abs(dx), countY = Math.Abs(dy);
            int stepX = dx > 0 ? 1 : -1, stepY = dy > 0 ? 1 : -1;
            yield return origin;
            var point = origin;
            for(int x = 0, y = 0; x < countX || y < countY;){
                int direction = (1 + 2 * x) * countY - (1 + 2 * y) * countX;
                if(direction == 0){
                    point.x += stepX;
                    point.y += stepY;
                    x++;
                    y++;
                }else if(direction < 0){
                    point.x += stepX;
                    x++;
                }else{
                    point.y += stepY;
                    y++;
                }
                yield return point;
            }
        }
    }
}


