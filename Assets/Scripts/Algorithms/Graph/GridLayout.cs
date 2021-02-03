namespace _Algorithms {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

//TODO move to level generator
    public class GridLayout {
        
    }
    
    public class NodeTemplate {
        public List<NodeGeometry> geometries = new List<NodeGeometry>();
    }
    public class NodeGeometry {
        public NodeGeometry parent;
        public int index;
        public int variant;
        public Vector2Int position;
        public RectInt bounds => rectangles.CalculateBounds();
        public List<OrthogonalConnector> connectors = new List<OrthogonalConnector>();
        public List<RectInt> rectangles = new List<RectInt>();
        public void UpdateTransform(Vector2Int position){
            Vector2Int delta = position - this.position;
            this.position = position;
            foreach(var connector in connectors)
                connector.bounds = new RectInt(delta + connector.bounds.position, connector.bounds.size);
            for(int i = 0; i < rectangles.Count; i++)
                rectangles[i] = new RectInt(delta + rectangles[i].position, rectangles[i].size);
        }
        public NodeGeometry Clone() => new NodeGeometry(){
            parent = this.parent == null ? this : this.parent,
            position = this.position,
            connectors = connectors.ConvertAll(connector => new OrthogonalConnector(connector.bounds, connector.direction)),
            rectangles = rectangles.ConvertAll(rectangle => new RectInt(rectangle.position, rectangle.size)),
            index = this.index,
            variant = this.variant
        };
        public IEnumerable<RectInt> ConfigurationSpace(NodeGeometry loose){
            var spaces = new List<RectInt>();
            foreach(var fixedConnector in connectors)
            foreach(var looseConnector in loose.connectors){
                if(fixedConnector.direction != looseConnector.direction * -1) continue;
                spaces.Add(new RectInt(){
                    xMin = fixedConnector.bounds.min.x - looseConnector.bounds.max.x,
                    yMin = fixedConnector.bounds.min.y - looseConnector.bounds.max.y,
                    xMax = fixedConnector.bounds.max.x - looseConnector.bounds.min.x,
                    yMax = fixedConnector.bounds.max.y - looseConnector.bounds.min.y
                });
            }
            OverlapSpace(spaces, loose);
            MergeConfigurationSpace(spaces);
            return spaces;
        }
        public void OverlapSpace(List<RectInt> spaces, NodeGeometry loose){
            foreach(var fixedRectangle in rectangles)
            foreach(var looseRectangle in loose.rectangles)
            for(int i = spaces.Count - 1; i >= 0; i--){
                var min = looseRectangle.min + spaces[i].min;
                var max = looseRectangle.max + spaces[i].max;
                if(min.x >= fixedRectangle.max.x || max.x <= fixedRectangle.min.x ||
                min.y >= fixedRectangle.max.y || max.y <= fixedRectangle.min.y) continue;
                if(spaces[i].width == 0){
                    int y0 = fixedRectangle.min.y - min.y - looseRectangle.height;
                    int y1 = max.y - fixedRectangle.max.y - looseRectangle.height;
                    if(y0 >= 0) spaces.Add(new RectInt(spaces[i].min.x, spaces[i].min.y, 0, y0));
                    if(y1 >= 0) spaces.Add(new RectInt(spaces[i].min.x, spaces[i].max.y - y1, 0, y1));
                    spaces.RemoveAt(i);
                }else{
                    int x0 = fixedRectangle.min.x - min.x - looseRectangle.width;
                    int x1 = max.x - fixedRectangle.max.x - looseRectangle.width;
                    if(x0 >= 0) spaces.Add(new RectInt(spaces[i].min.x, spaces[i].min.y, x0, 0));
                    if(x1 >= 0) spaces.Add(new RectInt(spaces[i].max.x - x1, spaces[i].min.y, x1, 0));
                    spaces.RemoveAt(i);
                }
            }
        }
        public static void MergeConfigurationSpace(List<RectInt> spaces){
            for(int i = spaces.Count - 1; i > 0; i--)
            for(int j = i - 1; j >= 0; j--){
                var lineA = spaces[i];
                var lineB = spaces[j];
                if(lineA.max.x < lineB.min.x || lineB.max.x < lineA.min.x ||
                lineA.max.y < lineB.min.y || lineB.max.y < lineA.min.y) continue;
                var directionA = lineA.width == 0;
                var directionB = lineB.width == 0;
                var emptyA = directionA && lineA.height == 0;
                var emptyB = directionB && lineB.height == 0;
                if(directionA == directionB || emptyA || emptyB){
                    spaces[j] = new RectInt(){
                        xMin = Math.Min(lineA.min.x, lineB.min.x),
                        yMin = Math.Min(lineA.min.y, lineB.min.y),
                        xMax = Math.Max(lineA.max.x, lineB.max.x),
                        yMax = Math.Max(lineA.max.y, lineB.max.y)
                    };
                    spaces.RemoveAt(i);
                    break;
                }else{
                    var vertical = directionA ? lineA : lineB;
                    var horizontal = directionA ? lineB : lineA;
                    int splitIndex = directionA ? j : i;
                    var left = new RectInt(){
                        xMin = horizontal.min.x, xMax = vertical.min.x - 1,
                        yMin = horizontal.min.y, yMax = horizontal.max.y
                    };
                    var right = new RectInt(){
                        xMin = vertical.max.x + 1, xMax = horizontal.max.x,
                        yMin = horizontal.min.y, yMax = horizontal.max.y
                    };
                    if(left.width >= 0 && right.width >= 0){
                        spaces[splitIndex] = left;
                        spaces.Insert(splitIndex, right);
                        i++;
                    }else spaces[splitIndex] = left.width >= 0 ? left : right;
                }
            }
        }
        public static IEnumerable<Vector2Int> SampleConfigurationSpace(List<RectInt> space){
            foreach(RectInt area in space)
            for(int x = area.min.x; x <= area.max.x; x++)
            for(int y = area.min.y; y <= area.max.y; y++)
            yield return new Vector2Int(x, y);
        }
        public IEnumerable<OrthogonalConnector> SampleConnectors(NodeGeometry neighbour){
            foreach(var sourceConnector in connectors)
            foreach(var targetConnector in neighbour.connectors){
                if(sourceConnector.direction != targetConnector.direction * -1) continue;
                var intersection = OrthogonalConnector.Intersection(sourceConnector.bounds, targetConnector.bounds);
                if(intersection.width < 0 || intersection.height < 0) continue;
                for(int x = intersection.min.x; x <= intersection.max.x; x++)
                for(int y = intersection.min.y; y <= intersection.max.y; y++)
                yield return new OrthogonalConnector(new RectInt(x, y, 0, 0), sourceConnector.direction);
            }
        }
    }
    public class OrthogonalConnector {
        public RectInt bounds;
        public Vector2Int direction;
        public OrthogonalConnector(RectInt bounds, Vector2Int direction){
            this.bounds = bounds; this.direction = direction;
        }
        public static IEnumerable<RectInt> Intersection(List<RectInt> boundsA, List<RectInt> boundsB){
            foreach(var boundA in boundsA)
            foreach(var boundB in boundsB){
                int minX = Math.Max(boundA.xMin, boundB.xMin);
                int minY = Math.Max(boundA.yMin, boundB.yMin);
                int maxX = Math.Min(boundA.xMax, boundB.xMax);
                int maxY = Math.Min(boundA.yMax, boundB.yMax);
                if(minX > maxX || minY > maxY) continue;
                yield return new RectInt(){ xMin = minX, yMin = minY, xMax = maxX, yMax = minY };
            }
        }
        public static RectInt Intersection(RectInt boundA, RectInt boundB){
            int minX = Math.Max(boundA.min.x, boundB.min.x);
            int minY = Math.Max(boundA.min.y, boundB.min.y);
            int maxX = Math.Min(boundA.max.x, boundB.max.x);
            int maxY = Math.Min(boundA.max.y, boundB.max.y);
            return new RectInt(minX, minY, maxX - minX, maxY - minY);
        }
    }
}