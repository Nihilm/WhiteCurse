namespace _Algorithms {
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture] public class HopcroftKarpTest {
        [Test] public void MaximumMatching(){
            var graph4x1 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 4; i++) graph4x1.AddNode(i);
            graph4x1.AddEdge(0, 1); graph4x1.AddEdge(0, 2); graph4x1.AddEdge(0, 3);
            var matching4x1 = HopcroftKarp.MaximumMatching(graph4x1);
			Assert.AreEqual(1, matching4x1.Count);
			AreGraphsMatching(graph4x1, matching4x1);

            var graph8x4 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 8; i++) graph8x4.AddNode(i);
            graph8x4.AddEdge(0, 5); graph8x4.AddEdge(0, 6); graph8x4.AddEdge(1, 4);
			graph8x4.AddEdge(2, 5); graph8x4.AddEdge(3, 5); graph8x4.AddEdge(3, 7);
            var matching8x4 = HopcroftKarp.MaximumMatching(graph8x4);
			Assert.AreEqual(4, matching8x4.Count);
			AreGraphsMatching(graph8x4, matching8x4);

            var graph5 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5+6; i++) graph5.AddNode(i);
            for(int i = 0; i < 5; i++)
            for(int j = 5; j < 5+6; j++) graph5.AddEdge(i, j);
            var matching5 = HopcroftKarp.MaximumMatching(graph5);
			Assert.AreEqual(5, matching5.Count);
			AreGraphsMatching(graph5, matching5);
        }

        private void AreGraphsMatching(IGraph<int> graph, IEnumerable<(int,int)> matching){
            bool[] visited = new bool[graph.NodeCount];
            foreach(var edge in matching){
                Assert.IsFalse(visited[edge.Item1]);
				Assert.IsFalse(visited[edge.Item2]);
				Assert.AreEqual(1, graph[edge.Item1,edge.Item2]);
                visited[edge.Item1] = visited[edge.Item2] = true;
            }
        }

        private void ArePartsEqual((int[],int[]) parts, IGraph<int> graph){
            Assert.NotNull(parts);
            var vertices = new List<int>();
            vertices.AddRange(parts.Item1);
            vertices.AddRange(parts.Item2);
            Assert.IsTrue(graph.Nodes.ScrambledEquals(vertices));
            var coloring = new Dictionary<int,int>();
            foreach(int node in parts.Item1) coloring[node] = 1;
            foreach(int node in parts.Item2) coloring[node] = 2;
            foreach(int node in graph.Nodes)
            foreach(int neighbour in graph.Neighbours(node))
                Assert.AreNotEqual(coloring[node], coloring[neighbour]);
        }
        [Test] public void Bipartite(){
            var graph3 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 3; i++) graph3.AddNode(i);
            for(int i = 0; i < 3; i++) graph3.AddEdge(i, (i+1) % graph3.NodeCount);
            Assert.IsNull(graph3.Bipartite());

            var graph11 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 11; i++) graph11.AddNode(i);
            for(int i = 0; i < 11; i++) graph11.AddEdge(i, (i+1) % graph11.NodeCount);
            Assert.IsNull(graph11.Bipartite());

            var graph25 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 25; i++) graph25.AddNode(i);
            for(int i = 0; i < 25; i++) graph25.AddEdge(i, (i+1) % graph25.NodeCount);
            Assert.IsNull(graph25.Bipartite());

            var graph5 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5; i++) graph5.AddNode(i);
            graph5.AddEdge(0, 1); graph5.AddEdge(1, 2); graph5.AddEdge(2, 3);
			graph5.AddEdge(2, 4); graph5.AddEdge(3, 4);
            Assert.IsNull(graph5.Bipartite());

            var graph3x4 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 3+4; i++) graph3x4.AddNode(i);
            for(int i = 0; i < 3; i++)
            for(int j = 3; j < 3+4; j++) graph3x4.AddEdge(i, j);
            ArePartsEqual(graph3x4.Bipartite().Value, graph3x4);

            var graph10x1 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 10+1; i++) graph10x1.AddNode(i);
            for(int i = 0; i < 10; i++)
            for(int j = 10; j < 10+1; j++) graph10x1.AddEdge(i, j);
            ArePartsEqual(graph10x1.Bipartite().Value, graph10x1);

            var graph10x21 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 10+21; i++) graph10x21.AddNode(i);
            for(int i = 0; i < 10; i++)
            for(int j = 10; j < 10+21; j++) graph10x21.AddEdge(i, j);
            ArePartsEqual(graph10x21.Bipartite().Value, graph10x21);

            var graphEdgeless = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5; i++) graphEdgeless.AddNode(i);
            ArePartsEqual(graphEdgeless.Bipartite().Value, graphEdgeless);

            var graph5N = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5; i++) graph5N.AddNode(i);
            graph5N.AddEdge(0, 1); graph5N.AddEdge(1, 2); 
            graph5N.AddEdge(2, 0); graph5N.AddEdge(3, 4);
            Assert.IsNull(graph5N.Bipartite());

            var graph5T = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5; i++) graph5T.AddNode(i);
            graph5T.AddEdge(0, 1); graph5T.AddEdge(1, 2); graph5T.AddEdge(3, 4);
            ArePartsEqual(graph5T.Bipartite().Value, graph5T);
        }
        public static IEnumerable<TestCaseData> TestCases(){
            var graph4x4 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 4+4; i++) graph4x4.AddNode(i);
            graph4x4.AddEdge(0, 5); graph4x4.AddEdge(1, 4); graph4x4.AddEdge(1, 5);
            graph4x4.AddEdge(1, 6); graph4x4.AddEdge(2, 5); graph4x4.AddEdge(3, 6);
            graph4x4.AddEdge(3, 7);
            yield return new TestCaseData(graph4x4, (4,4), (2,1));

            var graph2x3 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 2+3; i++) graph2x3.AddNode(i);
            graph2x3.AddEdge(0, 2); graph2x3.AddEdge(0, 3); graph2x3.AddEdge(1, 3); graph2x3.AddEdge(1, 4);
            yield return new TestCaseData(graph2x3, (2,3), (2,0));

            var graph4x2 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 4+2; i++) graph4x2.AddNode(i);
            graph4x2.AddEdge(0, 4); graph4x2.AddEdge(1, 4); graph4x2.AddEdge(1, 5);
            graph4x2.AddEdge(2, 5); graph4x2.AddEdge(3, 5);
            yield return new TestCaseData(graph4x2, (4,2), (0,2));

            var graph5x5 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5+5; i++) graph5x5.AddNode(i);
            graph5x5.AddEdge(0, 5); graph5x5.AddEdge(0, 6); graph5x5.AddEdge(0, 7);
            graph5x5.AddEdge(1, 5); graph5x5.AddEdge(2, 6); graph5x5.AddEdge(3, 7);
            graph5x5.AddEdge(4, 8); graph5x5.AddEdge(4, 9);
            yield return new TestCaseData(graph5x5, (5,5), (1,3));

            var graph66x65 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 66+65; i++) graph66x65.AddNode(i);
            (int,int)[] edges = new (int, int)[]{
                 (62,0),(1,1),(37,2),(0,3),(64,3),(29,4),(43,5),(10,6),(4,7),(6,10),(8,12),
                 (8,59),(16,13),(65,14),(9,15),(9,17),(10,20),(11,21),(12,16),(12,18),(13,15),
                 (14,16),(13,17),(14,17),(15,19),(13,20),(16,21),(13,22),(17,25),(17,55),(54,24),
                 (18,25),(19,58),(20,27),(23,27),(24,27),(21,34),(21,35),(22,28),(23,30),(49,28),
                 (24,30),(24,31),(24,32),(58,33),(25,37),(25,45),(25,54),(26,36),(59,36),(26,60),
                 (27,45),(27,54),(31,38),(29,39),(29,40),(42,41),(30,64),(32,50),(33,42),(44,42),
                 (34,43),(36,43),(48,43),(46,44),(36,45),(48,45),(53,45),(41,46),(38,47),(39,47),
                 (41,48),(45,49),(43,50),(43,51),(44,62),(45,51),(46,52),(47,53),(48,53),(47,54),
                 (48,54),(50,55),(51,56),(52,57),(58,59),(59,60),(60,61),(62,63),(63,63),
                 (1,8),(3,9),(13,23),(44,26),(14,11),(50,29),(0,29),(14,23)
            };
            foreach(var edge in edges) graph66x65.AddEdge(edge.Item1, edge.Item2 + 66);
            yield return new TestCaseData(graph66x65, (66,65), (41,10));
        }
        [Test, TestCaseSource("TestCases")]
        public void MinimumVertexCoverTest(IGraph<int> graph, (int,int) size, (int,int) expected){
            var cover = HopcroftKarp.MinimumVertexCover(graph);
            int[] counts = new int[graph.NodeCount];
            foreach(int vertex in cover.left) Assert.AreEqual(0, counts[vertex]++);
            foreach(int vertex in cover.right) Assert.AreEqual(0, counts[vertex]++);
            foreach(var edge in graph.Edges)
                Assert.AreNotEqual(0, counts[edge.source] + counts[edge.target]);
            Assert.AreEqual(expected.Item1 + expected.Item2, cover.left.Count + cover.right.Count);
            Assert.AreEqual(expected.Item1, cover.left.Count);
            Assert.AreEqual(expected.Item2, cover.right.Count);
        }
        [Test, TestCaseSource("TestCases")]
        public void BipartiteIndependentSetTest(IGraph<int> graph, (int,int) size, (int,int) expected){
            var set = HopcroftKarp.MinimumVertexCover(graph, true);
            foreach(var edge in graph.Edges)
                Assert.IsTrue(!set.Item1.Contains(edge.source) || !set.Item2.Contains(edge.target));
            int[] counts = new int[graph.NodeCount];
            foreach(var node in set.Item1) Assert.AreEqual(0, counts[node]++);
            foreach(var node in set.Item2) Assert.AreEqual(0, counts[node]++);
            Assert.AreEqual((size.Item1 - expected.Item1) + (size.Item2 - expected.Item2), set.left.Count + set.right.Count);
            Assert.AreEqual(size.Item1 - expected.Item1, set.left.Count);
            Assert.AreEqual(size.Item2 - expected.Item2, set.right.Count);
        }
    }
}