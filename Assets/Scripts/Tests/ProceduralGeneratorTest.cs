namespace _Algorithms {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;
    [TestFixture] public class ProceduralGeneratorTest {
        private static NodeGeometry BuildShape(int[,] filled, int[,] sockets){
            filled = filled.Transpose();
            sockets = sockets.Transpose();
            var geometry = new _Algorithms.NodeGeometry();
            var outline = _Algorithms.RectilinearPolygon.Contour(filled);
            geometry.rectangles = new List<RectInt>(_Algorithms.RectilinearPolygon.Decompose(outline));
            var bounds = new RectInt(0, 0, sockets.GetLength(0), sockets.GetLength(1));
            int[] _sockets = new int[bounds.width * bounds.height];
            for(int x = 0; x < bounds.width; x++)
            for(int y = 0; y < bounds.height; y++)
            _sockets[x + y * bounds.width] = sockets[x, y];
            geometry.connectors = RoomGeometry.AttachSockets(_sockets, bounds, outline);
            return geometry;
        }
        public static IEnumerable<TestCaseData> TestCases(){
            var shapeA = BuildShape(new int[,]{
                {0,0,1,1,0,0},
                {0,0,1,1,1,0},
                {0,1,1,1,1,0}
            }, new int[,]{
                {0,0,1,1,0,0},
                {0,0,0,0,0,0},
                {0,0,1,1,0,0}
            });
            var shapeB = BuildShape(new int[,]{
                {1,1,1,1},
                {1,1,1,0},
                {1,1,1,1},
                {0,1,1,1}
            }, new int[,]{
                {0,1,1,0},
                {1,0,0,0},
                {1,0,0,0},
                {0,0,1,0}
            });
            var shapeC = BuildShape(new int[,]{
                {0,1,1,1,0},
                {1,1,0,1,1},
                {1,1,0,1,1},
                {0,1,1,1,0},
                {0,1,1,0,0}
            }, new int[,]{
                {0,0,1,0,0},
                {0,0,0,0,0},
                {1,0,0,0,1},
                {0,1,0,1,0},
                {0,1,1,0,0}
            });
            var shapeD = BuildShape(new int[,]{
                {1,1,1,1},
                {1,1,1,1},
                {1,1,0,0},
                {1,1,0,0},
                {1,1,0,0}
            }, new int[,]{
                {0,1,1,0},
                {1,0,0,1},
                {1,0,0,0},
                {1,1,0,0},
                {0,1,0,0}
            });

            var graph0 = new UndirectedAdjacencyListGraph<NodeTemplate>();
            graph0.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeD }
            });
            graph0.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeC }
            });
            graph0.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeC }
            });
            graph0.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeC, shapeD }
            });
            graph0.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeC, shapeD }
            });
            graph0.AddEdge(0, 1); graph0.AddEdge(1, 2); graph0.AddEdge(2, 3); graph0.AddEdge(3, 4);
            graph0.AddEdge(0, 3);

            yield return new TestCaseData(graph0);

            var shapeE = BuildShape(new int[,]{
                {1,1,0,0,0},
                {1,1,0,0,0},
                {1,1,1,1,1},
                {1,1,1,1,1},
                {0,0,1,1,0}
            }, new int[,]{
                {1,1,0,0,0},
                {1,0,0,0,0},
                {1,0,0,1,1},
                {0,0,0,0,0},
                {0,0,1,1,0}
            });
            var shapeF = BuildShape(new int[,]{
                {1,1,1,1,1}
            }, new int[,]{
                {1,0,0,0,1}
            });
            var shapeG = BuildShape(new int[,]{
                {1,1,1,1},
                {0,1,1,1},
                {0,1,1,0},
                {0,1,1,0},
                {0,1,1,0},
                {0,1,1,0}
            }, new int[,]{
                {0,1,1,1},
                {0,0,0,1},
                {0,0,0,0},
                {0,0,0,0},
                {0,0,0,0},
                {0,1,1,0}
            });
            var shapeH = BuildShape(new int[,]{
                {1,1,1,1},
                {1,1,1,1}
            }, new int[,]{
                {1,1,1,1},
                {1,1,1,1}
            });

            var graph1 = new UndirectedAdjacencyListGraph<NodeTemplate>();
            graph1.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeD, shapeE, shapeH }
            });
            graph1.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeF, shapeG, shapeH }
            });
            graph1.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeC, shapeH }
            });
            graph1.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeC, shapeD, shapeE, shapeH }
            });
            graph1.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeC, shapeD, shapeF, shapeG, shapeH }
            });
            graph1.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeF, shapeG, shapeH }
            });
            graph1.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeF, shapeG, shapeH }
            });
            graph1.AddNode(new NodeTemplate(){
                geometries = new List<NodeGeometry>(){ shapeA, shapeB, shapeC, shapeD, shapeE, shapeH }
            });
            graph1.AddEdge(0, 1); graph1.AddEdge(1, 2); graph1.AddEdge(2, 3); graph1.AddEdge(3, 4);
            graph1.AddEdge(4, 5);
            graph1.AddEdge(4, 6); graph1.AddEdge(0, 6); graph1.AddEdge(6, 7); graph1.AddEdge(2, 7);

            yield return new TestCaseData(graph1);
        }
        [Test, TestCaseSource("TestCases")]
        public void TestRandomGraphs(IGraph<NodeTemplate> graph){
            var generator = new ProceduralGenerator(){
                random = new System.Random(1)
            };
            HashSet<string> keys = new HashSet<string>();
            foreach(var layout in generator.Generate(graph)){
                Assert.IsTrue(layout.Energy <= 0);
                List<string> boxes = new List<string>();
                foreach(var geometry in layout.geometries)
                    boxes.Add($"[{string.Join(",", geometry.rectangles).Replace("(","{").Replace(")","}")}]");
                string key = $"[{string.Join(",", boxes)}]";
                Assert.IsFalse(keys.Contains(key));
                keys.Add(key);
                Debug.Log($"var rects{keys.Count} = {key}");
            }
            Assert.IsNotEmpty(keys);
        }
    }
}