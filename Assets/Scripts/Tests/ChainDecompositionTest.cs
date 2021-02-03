namespace _Algorithms {
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    [TestFixture] public class ChainDecompositionTest {
        private ChainDecomposition<int> decomposer = new ChainDecomposition<int>();
        [Test] public void TestReorder(){
            var array = new List<List<int>>(){
                new List<int>(){ -3, -8, 2, 0, 1, -1 },
                new List<int>(){ 5, 6, 4, 3 },
                new List<int>(){ -4, -2, 3 },
                new List<int>(){ 10 }
            };
            foreach(var subarray in array) subarray.Reorder((a, b) => b - a, 1);
            array.Reorder((a, b) => b == null ? a.Count : a.Count - b.Count, null);

            Assert.AreEqual(2, array.Count);
            CollectionAssert.AreEqual(array[0], new List<int>(){ -2, -4 });
            CollectionAssert.AreEqual(array[1], new List<int>(){ 0, -1, -3, -8 });

            var depth = new int[]{1, 1, 1, 0, 0};
            var order = new int[]{3, 3, 3, 4, 4};
            var indicesA = new List<int>(){1, 0, 2};
            indicesA.Reorder((int a, int b) => depth[b] == depth[a] ? order[a] - order[b] : depth[b] - depth[a], 1);
            Assert.AreEqual(0, indicesA.Count);
            var indicesB = new List<int>(){3, 1, 4};
            indicesB.Reorder((int a, int b) => depth[b] == depth[a] ? order[a] - order[b] : depth[b] - depth[a], 1);
            CollectionAssert.AreEqual(indicesB, new List<int>(){ 3, 4 });
        }
        [Test] public void OneStageDecomposition(){
            var graphC3_C3 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 5; i++) graphC3_C3.AddNode(i);
            graphC3_C3.AddEdge(0, 1); graphC3_C3.AddEdge(1, 2); graphC3_C3.AddEdge(2, 0);
            graphC3_C3.AddEdge(1, 3); graphC3_C3.AddEdge(3, 4); graphC3_C3.AddEdge(4, 1);
            var chainsC3_C3 = new List<(bool,List<int>)>(decomposer.Chains(graphC3_C3));

            Assert.AreEqual(2, chainsC3_C3.Count);
            CollectionAssert.AreEquivalent(chainsC3_C3[0].Item2, new List<int>(){ 0, 1, 2 });
            CollectionAssert.AreEquivalent(chainsC3_C3[1].Item2, new List<int>(){ 3, 4 });

            var graph7I = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 7; i++) graph7I.AddNode(i);
            graph7I.AddEdge(0, 1); graph7I.AddEdge(1, 2); graph7I.AddEdge(2, 3);
            graph7I.AddEdge(4, 1); graph7I.AddEdge(1, 5); graph7I.AddEdge(5, 6);
            var chains7I = new List<(bool,List<int>)>(decomposer.Chains(graph7I));
            
            Assert.AreEqual(1, chains7I.Count);
            CollectionAssert.AreEquivalent(chains7I[0].Item2, new List<int>(){ 0,1,2,3,4,5,6 });

            var graphM5 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 10; i++) graphM5.AddNode(i);
            graphM5.AddEdge(0, 1); graphM5.AddEdge(1, 2); graphM5.AddEdge(2, 3);
            graphM5.AddEdge(3, 4); graphM5.AddEdge(4, 5); graphM5.AddEdge(5, 6);
            graphM5.AddEdge(6, 7); graphM5.AddEdge(7, 0); graphM5.AddEdge(2, 8);
            graphM5.AddEdge(5, 8); graphM5.AddEdge(7, 8); graphM5.AddEdge(8, 9);
            var chainsM5 = new List<(bool,List<int>)>(decomposer.Chains(graphM5));

            Assert.AreEqual(3, chainsM5.Count);
            CollectionAssert.AreEquivalent(chainsM5[0].Item2, new List<int>(){ 6, 5, 8, 7 });
            CollectionAssert.AreEquivalent(chainsM5[1].Item2, new List<int>(){ 2, 4, 3 });
            CollectionAssert.AreEquivalent(chainsM5[2].Item2, new List<int>(){ 0, 9, 1 });

            var graphM20 = new UndirectedAdjacencyListGraph<int>();
            for(int i = 0; i < 20; i++) graphM20.AddNode(i);
            graphM20.AddEdge(0, 1); graphM20.AddEdge(1, 2); graphM20.AddEdge(1, 3);
            graphM20.AddEdge(2, 5); graphM20.AddEdge(3, 4); graphM20.AddEdge(4, 5);
            graphM20.AddEdge(4, 6); graphM20.AddEdge(6, 11); graphM20.AddEdge(5, 11);
            graphM20.AddEdge(5, 8); graphM20.AddEdge(5, 7); graphM20.AddEdge(7, 9);
            graphM20.AddEdge(7, 10); graphM20.AddEdge(9, 11); graphM20.AddEdge(8, 12);
            graphM20.AddEdge(12, 10); graphM20.AddEdge(10, 13); graphM20.AddEdge(11, 15);
            graphM20.AddEdge(15, 13); graphM20.AddEdge(13, 17); graphM20.AddEdge(13, 19);
            graphM20.AddEdge(17, 14); graphM20.AddEdge(15, 16); graphM20.AddEdge(16, 18);
            var chainsM20 = new List<(bool,List<int>)>(decomposer.Chains(graphM20));
        }
    }
}