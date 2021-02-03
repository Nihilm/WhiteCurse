namespace _Algorithms {
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture] public class BoyerMyrvoldTest {
        private BoyerMyrvold<int> boyerMyrvold = new BoyerMyrvold<int>();
        [Test] public void CheckConnectivity(){
			var graphEmpty = new UndirectedAdjacencyListGraph<int>();
            Assert.AreEqual(0, graphEmpty.Connected());

            var graphC3 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 3; i++) graphC3.AddNode(i);
            graphC3.AddEdge(0, 1); graphC3.AddEdge(1, 2); graphC3.AddEdge(2, 0);
            Assert.AreEqual(1, graphC3.Connected());

            var graphC3_C3 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 6; i++) graphC3_C3.AddNode(i);
            graphC3_C3.AddEdge(0, 1); graphC3_C3.AddEdge(1, 2); graphC3_C3.AddEdge(2, 0);
			graphC3_C3.AddEdge(3, 4); graphC3_C3.AddEdge(4, 5); graphC3_C3.AddEdge(5, 3);
			graphC3_C3.AddEdge(0, 3);
			Assert.AreEqual(1, graphC3_C3.Connected());

            var graphC3x2 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 6; i++) graphC3x2.AddNode(i);
            graphC3x2.AddEdge(0, 1); graphC3x2.AddEdge(1, 2); graphC3x2.AddEdge(2, 0);
			graphC3x2.AddEdge(3, 4); graphC3x2.AddEdge(4, 5); graphC3x2.AddEdge(5, 3);
			Assert.AreEqual(2, graphC3x2.Connected());

            var graphD20 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 20; i++) graphD20.AddNode(i);
            Assert.AreEqual(20, graphD20.Connected());
		}
        [Test] public void CheckPlanarity(){
            var graphEmpty = new UndirectedAdjacencyListGraph<int>();
            Assert.IsNotNull(boyerMyrvold.ExtractPlanarFaces(graphEmpty));

            var graphC3 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 3; i++) graphC3.AddNode(i);
            graphC3.AddEdge(0, 1); graphC3.AddEdge(1, 2); graphC3.AddEdge(2, 0);
            Assert.IsNotNull(boyerMyrvold.ExtractPlanarFaces(graphC3));

            var graphK5 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5; i++) graphK5.AddNode(i);
            graphK5.AddEdge(0, 1); graphK5.AddEdge(0, 2); graphK5.AddEdge(0, 3);
			graphK5.AddEdge(0, 4); graphK5.AddEdge(1, 2); graphK5.AddEdge(1, 3);
			graphK5.AddEdge(1, 4); graphK5.AddEdge(2, 3); graphK5.AddEdge(2, 4); graphK5.AddEdge(3, 4);
            Assert.IsNull(boyerMyrvold.ExtractPlanarFaces(graphK5));
        }
        private void AreFacesEqual(List<List<int>> expected, List<List<int>> actual){
            Assert.AreEqual(expected.Count, actual.Count);
            foreach(var face in actual)
            foreach(var potential in expected)
                if(face.ScrambledEquals<int>(potential)){
                    expected.Remove(potential);
                    break;
                }
            Assert.AreEqual(0, expected.Count);
        }
		[Test] public void CheckFaces(){
            var graphC3 = new UndirectedAdjacencyListGraph<int>();
            graphC3.AddNode(0); graphC3.AddNode(1); graphC3.AddNode(2);
            graphC3.AddEdge(0, 1); graphC3.AddEdge(1, 2); graphC3.AddEdge(2, 0);
            AreFacesEqual(new List<List<int>>() {
                new List<int>() {0, 1, 2},
                new List<int>() {0, 1, 2}
            }, boyerMyrvold.ExtractPlanarFaces(graphC3));

            var graphC3_C3 = new UndirectedAdjacencyListGraph<int>();
            graphC3_C3.AddNode(0); graphC3_C3.AddNode(1); graphC3_C3.AddNode(2);
			graphC3_C3.AddNode(3); graphC3_C3.AddNode(4);
            graphC3_C3.AddEdge(0, 1); graphC3_C3.AddEdge(1, 2); graphC3_C3.AddEdge(2, 0);
            graphC3_C3.AddEdge(1, 3); graphC3_C3.AddEdge(3, 4); graphC3_C3.AddEdge(4, 1);
            AreFacesEqual(new List<List<int>>() {
                new List<int>() {0, 1, 2},
                new List<int>() {1, 3, 4},
                new List<int>() {0, 1, 3, 4, 1, 2}
            }, boyerMyrvold.ExtractPlanarFaces(graphC3_C3));

            var graphK33 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 6; i++) graphK33.AddNode(i);
            for(int i = 0; i < 3; i++)
			for(int j = 3; j < 6; j++) graphK33.AddEdge(i, j);
            Assert.IsNull(boyerMyrvold.ExtractPlanarFaces(graphK33));

            var graphK5 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5; i++) graphK5.AddNode(i);
            for(int i = 0; i < 4; i++)
			for(int j = i; j < 5; j++) graphK5.AddEdge(i, j);
            Assert.IsNull(boyerMyrvold.ExtractPlanarFaces(graphK5));

            var graphM5 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 10; i++) graphM5.AddNode(i);
            graphM5.AddEdge(0, 1); graphM5.AddEdge(1, 2); graphM5.AddEdge(2, 3);
            graphM5.AddEdge(3, 4); graphM5.AddEdge(4, 5); graphM5.AddEdge(5, 6);
            graphM5.AddEdge(6, 7); graphM5.AddEdge(7, 0); graphM5.AddEdge(2, 8);
            graphM5.AddEdge(5, 8); graphM5.AddEdge(7, 8); graphM5.AddEdge(8, 9);
            var faces = boyerMyrvold.ExtractPlanarFaces(graphM5);

            AreFacesEqual(new List<List<int>>() {
                new List<int>() {0, 1, 2, 3, 4, 5, 6, 7},
                new List<int>() {1, 0, 7, 8, 9, 8, 2},
                new List<int>() {3, 2, 8, 5, 4},
                new List<int>() {6, 5, 8, 7}
            }, faces);
		}
    }
}

