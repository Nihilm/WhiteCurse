namespace _Algorithms {
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;
    [TestFixture] public class RectilinearPolygonTest {
        public static IEnumerable<TestCaseData> TestCases(){
            yield return new TestCaseData(new (int,int)[][]{
                new (int,int)[]{ (0,0), (0,1), (0,2), (1,2), (1,0) }
            }, 1, true);
            yield return new TestCaseData(new (int,int)[][]{
                new (int,int)[]{ (0,0), (0,1), (1,1), (1,0) },
                new (int,int)[]{ (1,1), (1,2), (2,2), (2,1) }
            }, 2, true);
            yield return new TestCaseData(new (int,int)[][]{
                new (int,int)[]{ (0,0), (3,0), (3,3), (1,3), (1,2), (2,2), (2,1), (1,1), (1,2), (0,2) }
            }, 4, false);
            yield return new TestCaseData(new (int,int)[][]{
                new (int,int)[]{ (1,1), (1,2), (2,2), (2,1) },
                new (int,int)[]{ (0,0), (4,0), (4,4), (1,4), (1,3), (0,3) }
            }, 4, false);
            yield return new TestCaseData(new (int,int)[][]{
                new (int,int)[]{ (1,1),(0,1),(0,2),(1,2),(1,3),(2,3),(2,2),(3,2),(3,1),(2,1),(2,0),(1,0) }
            }, 3, true);
            yield return new TestCaseData(new (int,int)[][]{
                new (int,int)[]{ (1,1),(0,1),(0,2),(2,2),(2,1),(3,1),(3,0),(1,0) }
            }, 2, true);
            yield return new TestCaseData(new (int,int)[][]{
                new (int,int)[]{ (0,0),(0,1),(1,1),(1,2),(2,2),(2,1),(3,1),(3,0) }
            }, 2, true);
            yield return new TestCaseData(new (int,int)[][]{
                new (int,int)[]{ (0,0),(0,1),(1,1),(1,2),(2,2),(2,0) }
            }, 2, true);

            yield return new TestCaseData(new (int,int)[][]{new (int,int)[]{
                (0, 2), (0, 4), (2, 4), (2, 6),
				(4, 6), (4, 4), (6, 4), (6, 2),
				(4, 2), (4, 0), (2, 0), (2, 2)
            }}, 3, true);
            yield return new TestCaseData(new (int,int)[][]{new (int,int)[]{
                (0, 0), (0, 6), (3, 6),
				(3, 3), (7, 3), (7, 0)
            }}, 2, true);
            yield return new TestCaseData(new (int,int)[][]{new (int,int)[]{
                (0, 0), (0, 3), (-1, 3), (-1, 5),
				(5, 5), (5, 3), (4, 3), (4, 0),
				(5, 0), (5, -2), (-1, -2), (-1, 0)
            }}, 3, true);
            yield return new TestCaseData(new (int,int)[][]{new (int,int)[]{
                (2, 0), (2, 1), (1, 1), (1, 2),
				(0, 2), (0, 7), (1, 7), (1, 8),
				(2, 8), (2, 9), (7, 9), (7, 8),
				(8, 8), (8, 7), (9, 7), (9, 2),
				(8, 2), (8, 1), (7, 1), (7, 0)
            }}, 5, true);
            yield return new TestCaseData(new (int,int)[][]{new (int,int)[]{
                (1, 0), (2, 0), (3, 0), (4, 0), (4, 1), (4, 2), (4, 3), (4, 4),
                (3, 4), (2, 4), (1, 4), (1, 3), (2, 3), (2, 2), (3, 2), (3, 1),
                (2, 1), (2, 2), (1, 2), (1, 3), (0, 3), (0, 2), (0, 1), (1, 1)
            }}, 6, false);
            yield return new TestCaseData(new (int,int)[][]{new (int,int)[]{
                (2, 0), (2, 1), (3, 1), (3, 2), (2, 2), (2, 3), (1, 3), (1, 6), (0, 6),
                (0, 10), (1, 10), (1, 14), (2, 14), (2, 15), (1, 15), (1, 16), (2, 16),
                (2, 17), (4, 17), (4, 19), (3, 19), (3, 24), (4, 24), (4, 27), (5, 27),
                (5, 35), (6, 35), (6, 38), (7, 38), (7, 40), (8, 40), (8, 43), (9, 43),
                (9, 44), (11, 44), (11, 47), (12, 47), (12, 50), (13, 50), (13, 53),
                (14, 53), (14, 55), (16, 55), (16, 56), (19, 56), (19, 55), (21, 55),
                (21, 51), (20, 51), (20, 50), (19, 50), (19, 46), (18, 46), (18, 42),
                (16, 42), (16, 40), (15, 40), (15, 36), (14, 36), (14, 31), (13, 31),
                (13, 27), (12, 27), (12, 24), (11, 24), (11, 20), (10, 20), (10, 19),
                (11, 19), (11, 18), (12, 18), (12, 16), (10, 16), (10, 15), (8, 15),
                (8, 14), (7, 14), (7, 13), (8, 13), (8, 11), (7, 11), (7, 7), (8, 7),
                (8, 4), (9, 4), (9, 1), (10, 1), (10, 0)
            }}, 28, true);
        }
        [Test, TestCaseSource("TestCases")] public void DecomposeTest((int,int)[][] paths, int expected, bool ccw){
            List<List<RectInt>> rectangles = new List<List<RectInt>>();
            for(int rotation = 0; rotation < 4; rotation++){
                List<IList<Vector2Int>> _paths = new List<IList<Vector2Int>>();
                foreach(var path in paths){
                    List<Vector2Int> _path = new List<Vector2Int>();
                    foreach(var vertex in path){
                        var vector = new Vector2Int(vertex.Item1, vertex.Item2);
                        vector = vector.OrthogonalRotate(rotation);
                        _path.Add(vector);
                    };
                    _paths.Add(_path);
                }
                rectangles.Add(new List<RectInt>(RectilinearPolygon.Decompose(_paths, !ccw)));
            }
            for(int flipAxis = 0; flipAxis < 2; flipAxis++){
                List<IList<Vector2Int>> _paths = new List<IList<Vector2Int>>();
                foreach(var path in paths){
                    List<Vector2Int> _path = new List<Vector2Int>();
                    foreach(var vertex in path){
                        var vector = new Vector2Int(vertex.Item1, vertex.Item2);
                        vector[flipAxis] = -vector[flipAxis];
                        _path.Add(vector);
                    };
                    _paths.Add(_path);
                }
                rectangles.Add(new List<RectInt>(RectilinearPolygon.Decompose(_paths, !!ccw)));
            }
            int area = 0;
            foreach(var path in paths)
            for(int i = path.Length - 1, j = 0; i >= 0; j = i--)
            if(path[i].Item2 == path[j].Item2)
            area += (path[j].Item1 - path[i].Item1) * path[i].Item2;
            if(!ccw) area *= -1;

            foreach(var actual in rectangles){
                int actualArea = 0;
                for(int i = 0; i < actual.Count; i++){
                    actualArea += actual[i].width * actual[i].height;
                }
                Assert.AreEqual(expected, actual.Count);
                Assert.AreEqual(area, actualArea);
            }
        }

        [Test] public void TestOrthogonalConnectors(){
            {
                var line1 = new OrthogonalConnector(new RectInt(1, 1, 4, 0), Vector2Int.up);
                var line2 = new OrthogonalConnector(new RectInt(1, 2, 4, 0), Vector2Int.up);
                Assert.IsEmpty(OrthogonalConnector.Intersection(new List<RectInt>(){ line1.bounds }, new List<RectInt>(){ line2.bounds }));
                Assert.IsEmpty(OrthogonalConnector.Intersection(new List<RectInt>(){ line2.bounds }, new List<RectInt>(){ line1.bounds }));
            }
            {
                var line1 = new OrthogonalConnector(new RectInt(1, 1, 4, 0), Vector2Int.up);
                var line2 = new OrthogonalConnector(new RectInt(5, 1, 5, 0), Vector2Int.up);
                var result = new List<RectInt>(OrthogonalConnector.Intersection(new List<RectInt>(){ line1.bounds }, new List<RectInt>(){ line2.bounds }));
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(new RectInt(5, 1, 0, 0), result[0]);
            }
            {
                var line1 = new OrthogonalConnector(new RectInt(3, 2, 7, 0), Vector2Int.up);
                var line2 = new OrthogonalConnector(new RectInt(7, 2, 5, 0), Vector2Int.up);
                var result = new List<RectInt>(OrthogonalConnector.Intersection(new List<RectInt>(){ line1.bounds }, new List<RectInt>(){ line2.bounds }));
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(new RectInt(7, 2, 3, 0), result[0]);
            }
            {
                var lines1 = new List<RectInt>(){
                    new RectInt(new Vector2Int(2, 1), new Vector2Int(0, 6)),
                    new RectInt(new Vector2Int(3, 4), new Vector2Int(5, 0)),
                    new RectInt(new Vector2Int(6, 5), new Vector2Int(0, 3))
                };
                var lines2 = new List<RectInt>(){
                    new RectInt(new Vector2Int(1, 6), new Vector2Int(6, 0)),
                    new RectInt(new Vector2Int(4, 4), new Vector2Int(3, 0)),
                    new RectInt(new Vector2Int(8, 2), new Vector2Int(0, 4))
                };
                var expected = new List<RectInt>(){
                    new RectInt(new Vector2Int(2, 6), new Vector2Int(0, 0)),
                    new RectInt(new Vector2Int(4, 4), new Vector2Int(3, 0)),
                    new RectInt(new Vector2Int(6, 6), new Vector2Int(0, 0)),
                    new RectInt(new Vector2Int(8, 4), new Vector2Int(0, 0))
                };
                var intersection = OrthogonalConnector.Intersection(lines1, lines2);
                CollectionAssert.AreEquivalent(expected, intersection);
            }
        }
        [Test] public void UnionCrossTest(){
            var roomA = new NodeGeometry();
            roomA.rectangles.Add(new RectInt(0, 0, 50, 50));
            roomA.rectangles.Add(new RectInt(100, 0, 50, 50));
            roomA.rectangles.Add(new RectInt(0, 100, 50, 50));
            roomA.rectangles.Add(new RectInt(100, 100, 50, 50));
            roomA.connectors.Add(new OrthogonalConnector(new RectInt(0,50,50,0), Vector2Int.up));
            roomA.connectors.Add(new OrthogonalConnector(new RectInt(100,50,50,0), Vector2Int.up));
            roomA.connectors.Add(new OrthogonalConnector(new RectInt(100,100,50,0), Vector2Int.down));
            roomA.connectors.Add(new OrthogonalConnector(new RectInt(0,100,50,0), Vector2Int.down));
            roomA.connectors.Add(new OrthogonalConnector(new RectInt(50,100,0,50), Vector2Int.right));
            roomA.connectors.Add(new OrthogonalConnector(new RectInt(50,0,0,50), Vector2Int.right));
            roomA.connectors.Add(new OrthogonalConnector(new RectInt(100,0,0,50), Vector2Int.left));
            roomA.connectors.Add(new OrthogonalConnector(new RectInt(100,100,0,50), Vector2Int.left));
            var roomB = new NodeGeometry();
            roomB.rectangles.Add(new RectInt(0, 0, 50, 50));
            roomB.connectors.Add(new OrthogonalConnector(new RectInt(0,0,0,50), Vector2Int.left));
            roomB.connectors.Add(new OrthogonalConnector(new RectInt(0,50,50,0), Vector2Int.up));
            roomB.connectors.Add(new OrthogonalConnector(new RectInt(50,0,0,50), Vector2Int.right));
            roomB.connectors.Add(new OrthogonalConnector(new RectInt(0,0,50,0), Vector2Int.down));
            var spaces = new List<RectInt>(roomA.ConfigurationSpace(roomB));
            CollectionAssert.AreEquivalent(new List<RectInt>(){
                new RectInt(-50,50,99,0),
                new RectInt(51,50,99,0),
                new RectInt(50,-50,0,200)
            }, spaces);
        }
        [Test] public void RandomConfigurationSpaceTest(){
            System.Random random = new System.Random(123);
            for(int i = 100; i > 0; i--){
                var roomA = new NodeGeometry();
                roomA.position = new Vector2Int(random.Next(0, 100), random.Next(0, 100));
                for(int j = random.Next(1, 10); j > 0; j--){
                    var box = new RectInt(
                        random.Next(0, 40), random.Next(0, 40),
                        random.Next(10, 20), random.Next(10, 20)
                    );
                    roomA.rectangles.Add(box);
                    roomA.connectors.Add(new OrthogonalConnector(
                        new RectInt(box.xMin, box.yMin, box.width, 0), Vector2Int.up
                    ));
                    roomA.connectors.Add(new OrthogonalConnector(
                        new RectInt(box.xMax, box.yMin, 0, box.height), Vector2Int.left
                    ));
                    roomA.connectors.Add(new OrthogonalConnector(
                        new RectInt(box.xMin, box.yMax, box.width, 0), Vector2Int.down
                    ));
                    roomA.connectors.Add(new OrthogonalConnector(
                        new RectInt(box.xMin, box.yMin, 0, box.height), Vector2Int.right
                    ));
                }
                var roomB = new NodeGeometry();
                for(int j = random.Next(1, 10); j > 0; j--){
                    var box = new RectInt(
                        random.Next(0, 40), random.Next(0, 40),
                        random.Next(10, 20), random.Next(10, 20)
                    );
                    roomB.rectangles.Add(box);
                    roomB.connectors.Add(new OrthogonalConnector(
                        new RectInt(box.xMin, box.yMin, box.width, 0), Vector2Int.up
                    ));
                    roomB.connectors.Add(new OrthogonalConnector(
                        new RectInt(box.xMax, box.yMin, 0, box.height), Vector2Int.left
                    ));
                    roomB.connectors.Add(new OrthogonalConnector(
                        new RectInt(box.xMin, box.yMax, box.width, 0), Vector2Int.down
                    ));
                    roomB.connectors.Add(new OrthogonalConnector(
                        new RectInt(box.xMin, box.yMin, 0, box.height), Vector2Int.right
                    ));
                }

                var spaces = new List<RectInt>(roomA.ConfigurationSpace(roomB));
                var visited = new HashSet<(int,int)>();
                foreach(var space in spaces)
                for(int x = space.xMin; x <= space.xMax; x++)
                for(int y = space.yMin; y <= space.yMax; y++){
                    Assert.IsFalse(visited.Contains((x, y)));
                    visited.Add((x, y));
                    int minDistance = int.MaxValue;
                    foreach(var shapeA in roomA.rectangles)
                    foreach(var shapeB in roomB.rectangles){
                        var rect = new RectInt(shapeB.x + x, shapeB.y + y, shapeB.width, shapeB.height);
                        minDistance = Math.Min(minDistance, shapeA.ManhattanDistance(rect));
                    }
                    Assert.AreEqual(0, minDistance);
                }
            }
        }
        [Test] public void ContourTest(){
            CollectionAssert.AreEqual(
                new List<List<Vector2Int>>(){new List<Vector2Int>(){
                    new Vector2Int(1, 1), new Vector2Int(1, 3), new Vector2Int(3, 3), new Vector2Int(3, 1)
                }},
                RectilinearPolygon.Contour(new int[,]{
                    { 0,0,0,0 },
                    { 0,1,1,0 },
                    { 0,1,1,0 },
                    { 0,0,0,0 }
                })
            );
            CollectionAssert.AreEqual(
                new List<List<Vector2Int>>(){new List<Vector2Int>(){
                    new Vector2Int(0, 2), new Vector2Int(0, 4), new Vector2Int(2, 4), new Vector2Int(2, 2)
                }, new List<Vector2Int>(){
                    new Vector2Int(2, 0), new Vector2Int(2, 2), new Vector2Int(4, 2), new Vector2Int(4, 0)
                }},
                RectilinearPolygon.Contour(new int[,]{
                    { 0,0,1,1 },
                    { 0,0,1,1 },
                    { 1,1,0,0 },
                    { 1,1,0,0 }
                })
            );
            CollectionAssert.AreEqual(
                new List<List<Vector2Int>>(){new List<Vector2Int>(){
                    new Vector2Int(0, 0), new Vector2Int(0, 3), new Vector2Int(3, 3), new Vector2Int(3, 0)
                }, new List<Vector2Int>(){
                    new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(1, 2), new Vector2Int(1, 1)
                }},
                RectilinearPolygon.Contour(new int[,]{
                    { 1,1,1 },
                    { 1,0,1 },
                    { 1,1,1 }
                })
            );
            CollectionAssert.AreEqual(
                new List<List<Vector2Int>>(){new List<Vector2Int>(){
                    new Vector2Int(1, 2),new Vector2Int(1, 5),new Vector2Int(3, 5),
                    new Vector2Int(3, 4),new Vector2Int(2, 4),new Vector2Int(2, 2)
                }, new List<Vector2Int>(){
                    new Vector2Int(2, 0),new Vector2Int(2, 2),new Vector2Int(3, 2),
                    new Vector2Int(3, 1),new Vector2Int(4, 1),new Vector2Int(4, 0)
                }, new List<Vector2Int>(){
                    new Vector2Int(3, 2),new Vector2Int(3, 4),new Vector2Int(4, 4),new Vector2Int(4, 2)
                }},
                RectilinearPolygon.Contour(new int[,]{
                    { 0,0,0,0,0 },
                    { 0,0,1,1,1 },
                    { 1,1,0,0,1 },
                    { 1,0,1,1,0 },
                    { 0,0,0,0,0 }
                })
            );
        }
    }
}