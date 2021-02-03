namespace _Template {
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;
    [TestFixture] public class SerializerTest {
        public interface ITestTarget {

        }
        public class TestTemplate : ScriptableObject {
            public int id;
        }
        [Serializable] public class TestItem : ITestTarget {
            [SerializeField] public string displayName;
            [SerializeField] public TestTemplate template;
        }
        [Serializable] public class TestUnit : ITestTarget {
            [SerializeField] public Vector2Int position;
            [SerializeReference] public ITestTarget target;
        }
        [Serializable] public class TestWorld {
            [SerializeReference] public ITestTarget[] targets;
        }
        [Test] public void TestJsonSerializer(){
            var world = new TestWorld(){
                targets = new ITestTarget[2]
            };
            var template = ScriptableObject.CreateInstance<TestTemplate>();
            template.id = 4;
            world.targets[0] = new TestItem(){
                displayName = "test",
                template = template
            };
            world.targets[1] = new TestUnit(){
                position = new Vector2Int(0, 2),
                target = world.targets[0]
            };

            string serialized = JsonUtility.ToJson(world, true);
            //Debug.Log($"JSON: {serialized}");

            var deserialized = new TestWorld();
            JsonUtility.FromJsonOverwrite(serialized, deserialized);

            Assert.AreEqual(2, deserialized.targets.Length);
            Assert.AreSame(deserialized.targets[0], ((TestUnit)deserialized.targets[1]).target);
            Assert.AreSame(((TestItem)deserialized.targets[0]).template, template);
        }
    }
}