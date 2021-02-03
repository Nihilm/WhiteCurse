namespace _Algorithms {
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    [TestFixture] public class ExtensionsTest {
        [Test] public void IntervalTreeRandomTest(){
            System.Random random = new System.Random(12);
            for(int i = 50; i > 0; i--){
                var tree = new IntervalTree<int,int>();
                int size = random.Next(10, 100);
                (int,int)[] ranges = new (int, int)[size];
                for(int j = 0; j < size; j++){
                    int min = random.Next(-100, 100);
                    int max = random.Next(min, min + 100);
                    ranges[j] = (min,max);
                    tree.Add(j,min,max);
                }
                for(int j = random.Next(10, 20); j > 0; j--){
                    int min = random.Next(-100, 100);
                    int max = random.Next(min, min + 100);
                    int count = 0;
                    foreach(var index in tree.Query(min, max)){
                        Assert.IsTrue(ranges[index].Item1 <= max && ranges[index].Item2 >= min);
                        count++;
                    }
                    foreach(var range in ranges) if(range.Item1 <= max && range.Item2 >= min) count--;
                    Assert.IsTrue(count == 0);
                }
            }
        }
        [Test] public void HeapsortRandomTest(){
            System.Random random = new System.Random(125);
            for(int i = 10; i > 0; i--){
                int size = random.Next(100);
                int[] actual = new int[size];
                int[] expected = new int[size];
                for(int j = 0; j < size; j++)
                    actual[j] = expected[j] = random.Next(int.MinValue, int.MaxValue);
                CollectionAssert.AreEqual(expected, actual);
            }
        }
        [Test] public void TestCombinations(){
            var expected = new Queue<int[]>();
            expected.Enqueue(new int[]{ 0,1 });
            expected.Enqueue(new int[]{ 0,2 });
            expected.Enqueue(new int[]{ 1,2 });
            
            foreach(var combination in Extensions.Combinations(3, 2))
                CollectionAssert.AreEqual(combination, expected.Dequeue());
            Assert.AreEqual(0, expected.Count);
        }
    }
}